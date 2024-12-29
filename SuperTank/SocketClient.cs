
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using SuperTank.Objects;
using SuperTank.General;
using static SuperTank.SocketClient;

namespace SuperTank
{
    public class SocketClient
    {
        public static Socket clientSocket;
        public static Thread receiveThread;
        private static bool stopThread = false;
        public static List<PlayerTank> players = new List<PlayerTank>(); // Sử dụng PlayerTank từ SuperTank.Objects
        public static PlayerTank localPlayer; // Sử dụng PlayerTank từ SuperTank.Objects
        private static readonly object playersLock = new object(); // Đối tượng lock


        public static bool isStartGame = false;

        public static bool isCreateRoom = true;
        public static bool isJoinRoom = true;

        public static List<Lobby> lobbies = new List<Lobby>();
        public static string joinedRoom = null;
        public static Lobby joinedLobby = null;

        public static List<string> messages = new List<string>();

        //update position
        public static event Action<SuperTank.Objects.PlayerTank> OnPlayerPositionUpdated;

        //update bullet
        public static event Action<SuperTank.Objects.PlayerTank, string, string> OnPlayerShoot;

        public static event Action<string> OnReceiveMessage;
        public static event Action<bool> OnGameOver;


        // Thêm hàng đợi an toàn luồng để lưu trữ thông điệp
        private static int maxQueueSize = 1000; // Giới hạn kích thước hàng đợi
        private static ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
        private static AutoResetEvent messageReceivedEvent = new AutoResetEvent(false);


        public static void SetLocalPlayer(string playerName)
        {
            localPlayer = new PlayerTank { Name = playerName };
        }
        // Kết nối đến server
        public static void ConnectToServer(System.Net.IPEndPoint serverEP)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(serverEP);
            receiveThread = new Thread(ReceiveData);
            receiveThread.Start();

            // Bắt đầu luồng xử lý dữ liệu riêng
            Thread processThread = new Thread(ProcessMessageQueue);
            processThread.Start();
        }

        // Gửi dữ liệu đến server
        public static void SendData(string data)
        {
            data += "\n";
            byte[] sendData = Encoding.UTF8.GetBytes(data);
            if (clientSocket != null && clientSocket.Connected)
            {
                clientSocket.Send(sendData);
            }
            else
            {
                Debug.WriteLine("Cannot send data. Client socket is not connected.");
            }
        }

        // Nhận dữ liệu từ server
        private static void ReceiveData()
        {
            byte[] buffer = new byte[1024];
            while (clientSocket.Connected && !stopThread)
            {
                try
                {
                    int receivedBytes = clientSocket.Receive(buffer);
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                    // Thêm thông điệp vào hàng đợi và kích hoạt sự kiện xử lý
                    // Kiểm tra kích thước hàng đợi trước khi thêm
                    if (messageQueue.Count < maxQueueSize)
                    {
                        messageQueue.Enqueue(receivedData);
                        messageReceivedEvent.Set();
                    }
                }
                catch (SocketException ex)
                {
                    Debug.WriteLine($"Socket error: {ex.Message}");
                }
            }
        }


        // Luồng xử lý hàng đợi thông điệp
        private static void ProcessMessageQueue()
        {
            while (!stopThread)
            {
                messageReceivedEvent.WaitOne(); // Chờ sự kiện để bắt đầu xử lý

                while (messageQueue.TryDequeue(out string data))
                {
                    ProcessReceivedData(data);
                }
            }
        }

        private static void ProcessReceivedData(string data)
        {
            Debug.WriteLine($"Received data: {data}");
            string[] payload = data.Split(';');
            string messageType = payload[0];

            switch (messageType)
            {
                case "ROOMLIST":
                    AddRoomList(payload);
                    Debug.WriteLine("Received ROOMLIST.");
                    break;
                case "JOINED":
                    joinedRoom = payload[1];
                    Debug.WriteLine($"Joined room: {joinedRoom}");
                    var lobby = lobbies.SingleOrDefault(r => r.RoomId == payload[1]);
                    if (lobby != null)
                    {
                        joinedLobby = lobby;
                    }
                    break;
                case "LOBBY_INFO":
                    UpdateLobby(payload);
                    Debug.WriteLine($"Received LOBBY_INFO for room: {payload[1]}");
                    break;
                case "READY_INFO":
                    UpdateReadyInfo(payload);
                    Debug.WriteLine($"Received READY_INFO for player: {payload[1]}");
                    break;
                case "SEND_MESSAGE":
                    OnReceiveMessage?.Invoke(payload[1]);
                    Debug.WriteLine($"Received message: {payload[1]}");
                    UpdateMessage(payload[1]);
                    break;
                case "START":
                    Debug.WriteLine($"isStartGame set to: {SocketClient.isStartGame} - Player: {SocketClient.localPlayer.Name} - Time: {DateTime.Now}");
                    UpdatePlayInfo(payload);
                    isStartGame = true;
                    Debug.WriteLine("Game started. isStartGame set to true");
                    break;
                case "UPDATE_STATS":
                    UpdateStats(payload);
                    Debug.WriteLine("Updated Stats");
                    break;
                case "GAMEOVER":
                    if (true)
                    {
                        isStartGame = false;
                        OnGameOver?.Invoke(false);
                    }
                    break;
                case "PLAYER_DISCONNECTED":
                    HandleDisconnect(payload[1]);
                    Debug.WriteLine($"Player Disconnected: {payload[1]}");
                    break;
                case "CLEAR_LOBBY":
                    ClearLobby();
                    Debug.WriteLine($"Lobby cleared");
                    break;
                case "ERROR_JOIN":
                    isJoinRoom = false;
                    Debug.WriteLine($"Error joining room");
                    break;
                case "ERROR_CREATE":
                    isCreateRoom = false;
                    Debug.WriteLine($"Error creating room");
                    break;
                case "UPDATE_POSITION":
                    if (isStartGame)
                    {
                        HandlePlayerPosition(payload);
                    }
                    else
                    {
                        Debug.WriteLine("Ignoring UPDATE_POSITION because game hasn't started yet.");
                    }
                    break;
                default:
                    Debug.WriteLine($"Received unknown command: {messageType}");
                    break;
            }
        }

        private static void UpdatePlayInfo(string[] payload)
        {
            if (payload.Length < 4) return;
            lock (playersLock) // Khóa trước khi truy cập players
            {
                players.Clear();
                // ... (thêm người chơi vào players)
            }
            string[] playerNames = payload[2].Split(',');
            int index = 0;
            foreach (string playerName in playerNames)
            {
                int xPos = 2;
                int yPos = 0;
                switch (index)
                {
                    case 0: yPos = 3; break;
                    case 1: yPos = 16; break;
                    case 2: yPos = 29; break;
                    case 3: yPos = 42; break;
                }
                if (players.All(p => p.Name != playerName))
                {

                    PlayerTank newPlayer = new PlayerTank { Name = playerName, Position = new PointF(xPos * Common.STEP, yPos * Common.STEP) }; //Sử dụng PlayerTank từ SuperTank.Objects
                    newPlayer.SetLocation(xPos, yPos);
                    players.Add(newPlayer);
                    index++;
                }
            }
            // Kiểm tra giá trị của joinedLobby.Host.Name
            Debug.WriteLine("joinedLobby.Host.Name: " + (joinedLobby != null && joinedLobby.Host != null ? joinedLobby.Host.Name : "null"));
            localPlayer = players.SingleOrDefault(p => p.Name == joinedLobby.Host.Name);
        }

        //Cập nhập danh sách phòng hiện có vào list lobbies
        private static void AddRoomList(string[] payload)
        {
            for (int i = 1; i < payload.Length - 1; i += 2)
            {
                var lobby = lobbies.SingleOrDefault(r => r.RoomId == payload[i]);
                if (lobby == null)
                {
                    Lobby newLobby = new Lobby()
                    {
                        RoomId = payload[i],
                        HostName = payload[i + 1],
                        Host = new SuperTank.Objects.PlayerTank { Name = payload[i + 1] },
                        PlayersName = new List<string> { payload[i + 1] },
                        Players = new List<SuperTank.Objects.PlayerTank>
                        {
                            new SuperTank.Objects.PlayerTank() {Name = payload[i + 1]}
                        }
                    };
                    lobbies.Add(newLobby);
                }
            }
        }
        //Xử lý ngắt kết nối
        private static void HandleDisconnect(string playerName)
        {
            if (joinedLobby != null && joinedLobby.PlayersName.Contains(playerName))
            {
                joinedLobby.Players.RemoveAll(p => p.Name == playerName);
                joinedLobby.PlayersName.Remove(playerName);

                if (joinedLobby.HostName == playerName)
                {
                    joinedLobby.HostName = joinedLobby.PlayersName[0];
                    joinedLobby.Host = new SuperTank.Objects.PlayerTank { Name = joinedLobby.PlayersName[0] };
                }
            }
        }

        // UpdateLobby method
        private static void UpdateLobby(string[] payload)
        {
            var lobby = lobbies.SingleOrDefault(r => r.RoomId == payload[1]);
            if (lobby != null)
            {
                joinedLobby = lobby;
                int playerCount = Convert.ToInt32(payload[2]);
                string[] playerList = payload[3].Split(',');
                string[] readyPlayerList = payload[4].Split(',');
                for (int i = 0; i < playerCount; i++)
                {
                    if (!lobby.PlayersName.Contains(playerList[i]))
                    {
                        lobby.PlayersName.Add(playerList[i]);
                        lobby.Players.Add(new SuperTank.Objects.PlayerTank()
                        {
                            Name = playerList[i],
                            IsReady = bool.Parse(readyPlayerList[i])
                        });
                    }
                    else
                    {
                        lobby.Players[i].IsReady = bool.Parse(readyPlayerList[i]);
                    }
                }
            }
        }
        //Cập nhập trạng thái sẵn sàng của người chơi khác
        public static void UpdateReadyInfo(string[] payload)
        {
            var player = joinedLobby.Players.SingleOrDefault(r => r.Name == payload[1]);
            if (player != null)
                player.IsReady = true;
        }
        //Kiểm tra xem người chơi có "name" đã sẵn sàng chưa
        public static bool CheckIsReady(string name)
        {
            var player = joinedLobby.Players.SingleOrDefault(r => r.Name == name);
            if (player == null) return false;
            return player.IsReady;
        }
        //Kiểm tra xem tất cả player đã sẵn sàng chưa
        public static bool CheckIsReadyForAll()
        {
            foreach (var player in joinedLobby.Players)
            {
                if (!player.IsReady) return false;
            }
            return true;
        }
        //Kiểm tra xem còn người chơi khác trong trận không
        public static bool CheckGameOver()
        {
            if (joinedLobby.IsGameOver)
                return true;
            return false;
        }
        //Tin nhắn từ người chơi khác
        public static void UpdateMessage(string content)
        {
            messages.Add(content);
        }

        public static string GetMessageFromPlayers()
        {
            if (messages.Count == 0) return null;
            string content = messages[0];
            messages.Clear();
            return content;
        }
        //Cập nhập thông số (score và kill) vào class lobby và player
        public static void UpdateStats(string[] payload)
        {
            foreach (var player in joinedLobby.Players)
            {
                if (player.Name == payload[1])
                {
                    //player.Kill += int.Parse(payload[2]);
                    //player.Score += int.Parse(payload[3]);
                }
            }
        }
        // Ngắt kết nối từ server
        public static void Disconnect()
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                try
                {
                    SendData("DISCONNECT");
                    stopThread = true;

                    if (receiveThread != null && receiveThread.IsAlive)
                    {
                        receiveThread.Join();
                    }

                    if (clientSocket.Connected)
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                    }
                    clientSocket.Close();
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Error disconnecting: {ex.Message}");
                }
                finally
                {
                    clientSocket = null;
                }
            }
            stopThread = false;
            localPlayer = null;
            isCreateRoom = true;
            isJoinRoom = true;
            isStartGame = false;
        }
        //Xóa lobby
        public static void ClearLobby()
        {
            foreach (var lobby in lobbies)
            {
                lobby.Players.Clear();
                lobby.PlayersName.Clear();
                lobby.Host = null;
                lobby.RoomId = null;
                lobby.HostName = null;
                lobby.IsGameOver = false;
            }
            lobbies.Clear();
            joinedRoom = null;
            joinedLobby = null;
            isCreateRoom = true;
            isJoinRoom = true;
            isStartGame = false;
        }

        public static void SendPlayerPosition(string playerName, Direction direction, int X, int Y, int frx_tank, Skin skin)
        {
            // Gửi giá trị int của Direction thay vì string
            string message = $"UPDATE_POSITION;{playerName};{(int)direction};{X / Common.STEP};{Y / Common.STEP};{frx_tank};{(int)skin}"; SendData(message);
        }

        private static void HandlePlayerPosition(string[] payload)
        {
            if (payload.Length < 7) return;// Kiểm tra số lượng phần tử (ít nhất là 6: UPDATE_POSITION, playerName, direction, X, Y, frx_tank)

            string playerName = payload[1];

            if (int.TryParse(payload[2], out int direction) &&
                int.TryParse(payload[3], out int X) &&
                int.TryParse(payload[4], out int Y) &&
                int.TryParse(payload[5], out int frx_tank) &&
                int.TryParse(payload[6], out int skinTank)) // Thêm xử lý frx_tank
            {
                PlayerTank playerToUpdate;

                lock (playersLock)
                {
                    playerToUpdate = players.FirstOrDefault(p => p.Name == playerName);

                    if (playerToUpdate == null)
                    {
                        playerToUpdate = new PlayerTank { Name = playerName };
                        players.Add(playerToUpdate);
                    }

                    // Cập nhật hướng di chuyển
                    if (Enum.IsDefined(typeof(Direction), direction))
                    {
                        playerToUpdate.DirectionTank = (General.Direction)direction;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid direction received: {direction}. Using default.");
                        playerToUpdate.DirectionTank = General.Direction.eUp; // Giá trị mặc định
                    }
                    if (Enum.IsDefined(typeof(Skin), skinTank))
                    {
                        playerToUpdate.SkinTank = (General.Skin)skinTank;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid skinTank received: {skinTank}. Using default.");
                        playerToUpdate.SkinTank = General.Skin.eYellow; // Giá trị mặc định
                    }

                    // Cập nhật frx_tank
                    playerToUpdate.frx_tank = frx_tank;
                }

                // Cập nhật vị trí
                playerToUpdate.Position = new PointF(X * Common.STEP, Y * Common.STEP);

                // Kích hoạt sự kiện để thông báo cho frmGameMulti cập nhật
                OnPlayerPositionUpdated?.Invoke(playerToUpdate);
            }
            else
            {
                Debug.WriteLine($"Invalid format for direction, X, or Y. Data received: {string.Join(";", payload)}");
                // Xử lý trường hợp dữ liệu nhận được không đúng định dạng
            }

        }

        //internal static void SendPlayerPosition(string name, General.Direction directionTank, int left, int top)
        //{
        //    throw new NotImplementedException();
        //}
        #region lớp đối tượng xử lí chung

        public partial class Lobby
        {
            public bool IsGameOver { get; set; } = false;
            public bool IsStart { get; set; } = false;
            public string RoomId { get; set; }
            public SuperTank.Objects.PlayerTank Host { get; set; }
            public string HostName { get; set; }
            public List<SuperTank.Objects.PlayerTank> Players { get; set; } = new List<SuperTank.Objects.PlayerTank>();
            public List<string> PlayersName { get; set; } = new List<string>();

        }
        public class BaseObject
        {
            protected Rectangle rect;
            protected Bitmap bmpObject;

            #region properties
            public Rectangle Rect
            {
                get
                {
                    return rect;
                }

                set
                {
                    rect = value;
                }
            }
            public int RectX
            {
                get
                {
                    return rect.X;
                }

                set
                {
                    rect.X = value;
                }
            }
            public int RectY
            {
                get
                {
                    return rect.Y;
                }

                set
                {
                    rect.Y = value;
                }
            }
            public int RectWidth
            {
                get
                {
                    return rect.Width;
                }

                set
                {
                    rect.Width = value;
                }
            }
            public int RectHeight
            {
                get
                {
                    return rect.Height;
                }

                set
                {
                    rect.Height = value;
                }
            }
            public Bitmap BmpObject
            {
                get
                {
                    return bmpObject;
                }

                set
                {
                    bmpObject = value;
                }
            }
            #endregion

            // load ảnh đối tượng
            public void LoadImage(string path)
            {
                this.bmpObject = new Bitmap(path);
            }

            // vẽ đối tượng vào bitmap nền
            public virtual void Show(Bitmap bmpBack)
            {
                Common.PaintObject(bmpBack, this.bmpObject, rect.X, rect.Y,
                    0, 0, this.RectWidth, this.RectHeight);
            }
        }

        public class Common
        {
            #region Hằng số các thông số cố định
            public const int SCREEN_WIDTH = 1100;
            public const int SCREEN_HEIGHT = 900;
            public const int NUMBER_OBJECT_WIDTH = 45;
            public const int NUMBER_OBJECT_HEIGHT = 40;
            public const int MAX_LEVEL = 10;
            public const int STEP = 20;
            public const int tankSize = 40;
            #endregion
            public static string path;

            // load hình ảnh
            public static Bitmap LoadImage(string fileName)
            {
                return new Bitmap(Common.path + fileName);
            }

            // vẽ lên bitmap
            public static void PaintObject(Bitmap bmpBack, Bitmap front, int x, int y, int xFrame, int yFrame, int wFrame, int hFrame)
            {
                Graphics g = Graphics.FromImage(bmpBack);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(front, x, y, new Rectangle(xFrame, yFrame, wFrame, hFrame), GraphicsUnit.Pixel);
                //g.DrawRectangle(new Pen(Color.Yellow, 2)
                //    , x, y, wFrame, hFrame);
                g.Dispose();
            }

            // clear bitmap
            public static void PaintClear(Bitmap bmpBack)
            {
                Graphics g = Graphics.FromImage(bmpBack);
                g.Clear(Color.Black);
                g.Dispose();
            }

            // đọc map vào ma trận
            public static int[,] ReadMap(string path, int numberObjectHeight, int numberObjectWidth)
            {
                int[,] arrayObject;
                string s = "";
                using (StreamReader reader = new StreamReader(path))
                {
                    arrayObject = new int[numberObjectHeight, numberObjectWidth];
                    for (int i = 0; i < numberObjectHeight; i++)
                    {
                        s = reader.ReadLine();
                        for (int j = 0; j < numberObjectWidth; j++)
                            arrayObject[i, j] = int.Parse(s[j].ToString());
                    }
                    return arrayObject;
                }
            }

            // kiểm tra va chạm giữa hai hình chữ nhật
            public static bool IsCollision(Rectangle rect1, Rectangle rect2)
            {
                // góc dưới phải
                if (rect1.Left > rect2.Left && rect1.Left < rect2.Right)
                {
                    if (rect1.Top > rect2.Top && rect1.Top < rect2.Bottom)
                    {
                        return true;
                    }
                }
                // góc trên phải
                if (rect1.Left > rect2.Left && rect1.Left < rect2.Right)
                {
                    if (rect1.Bottom > rect2.Top && rect1.Bottom < rect2.Bottom)
                    {
                        return true;
                    }
                }
                // góc dưới trái
                if (rect1.Right > rect2.Left && rect1.Right < rect2.Right)
                {
                    if (rect1.Top > rect2.Top && rect1.Top < rect2.Bottom)
                    {
                        return true;
                    }
                }
                // góc trên trái
                if (rect1.Right > rect2.Left && rect1.Right < rect2.Right)
                {
                    if (rect1.Bottom > rect2.Top && rect1.Bottom < rect2.Bottom)
                    {
                        return true;
                    }
                }
                //=============================================================
                // góc dưới phải
                if (rect2.Left > rect1.Left && rect2.Left < rect1.Right)
                {
                    if (rect2.Top > rect1.Top && rect2.Top < rect1.Bottom)
                    {
                        return true;
                    }
                }
                // góc trên phải
                if (rect2.Left > rect1.Left && rect2.Left < rect1.Right)
                {
                    if (rect2.Bottom > rect1.Top && rect2.Bottom < rect1.Bottom)
                    {
                        return true;
                    }
                }
                // góc dưới trái
                if (rect2.Right > rect1.Left && rect2.Right < rect1.Right)
                {
                    if (rect2.Top > rect1.Top && rect2.Top < rect1.Bottom)
                    {
                        return true;
                    }
                }
                // góc trên trái
                if (rect2.Right > rect1.Left && rect2.Right < rect1.Right)
                {
                    if (rect2.Bottom > rect1.Top && rect2.Bottom < rect1.Bottom)
                    {
                        return true;
                    }
                }
                //=============================================================
                if (rect1.Left == rect2.Left && rect1.Right == rect2.Right &&
                    rect1.Top == rect2.Top && rect1.Bottom == rect2.Bottom)
                    return true;
                return false;
            }

        }

        #region
        public enum Direction
        {
            eLeft, eRight, eUp, eDown
        }
        public enum BulletType
        {
            eTriangleBullet, eRocketBullet
        }
        public enum ExplosionSize
        {
            eSmallExplosion, eBigExplosion
        }
        public enum EnemyTankType
        {
            // 0, 1
            eNormal, eMedium
        }
        public enum Skin
        {
            // 0, 1, 2, 3, 4, 5, 6, 7
            eGreen, eRed, eYellow, eBlue, ePurple, eLightBlue, eOrange, ePink
        }
        public enum ItemType
        {
            eItemHeart, eItemShield, eItemGrenade, eItemRocket
        }
        public enum InforStyle
        {
            eGameOver, eGameNext, eGameWin
        }
        #endregion
        public class Bullet : BaseObject
        {
            private Direction directionBullet;
            private int speedBullet;
            private int power;

            // viên đạn di chuyển
            public void BulletMove()
            {
                switch (directionBullet)
                {
                    case Direction.eLeft:
                        this.RectX -= speedBullet;
                        break;
                    case Direction.eRight:
                        this.RectX += speedBullet;
                        break;
                    case Direction.eUp:
                        this.RectY -= speedBullet;
                        break;
                    case Direction.eDown:
                        this.RectY += speedBullet;
                        break;
                }
            }

            #region properties
            public Direction DirectionBullet
            {
                get
                {
                    return directionBullet;
                }

                set
                {
                    directionBullet = value;
                }
            }
            public int SpeedBullet
            {
                get
                {
                    return speedBullet;
                }

                set
                {
                    speedBullet = value;
                }
            }

            public int Power
            {
                get
                {
                    return power;
                }
                set
                {
                    power = value;
                }
            }
            #endregion
        }

        public class Wall : BaseObject
        {
            private int wallNumber;

            public int WallNumber
            {
                get
                {
                    return wallNumber;
                }

                set
                {
                    wallNumber = value;
                }
            }
        }

        public class Tank : BaseObject
        {
            #region số frame lớn nhất
            protected const int MAX_NUMBER_SPRITE_TANK = 7;
            protected const int MAX_NUMBER_SPRITE_EFFECT = 6;
            #endregion
            #region Số làm việc với frame (tank: có 8 frame 0-7; effect: có 6 frame 0-5)
            protected int frx_tank = 7;
            protected int frx_effect = 0;
            protected int fry_effect = 0;
            #endregion
            #region các thông số cố định
            protected int moveSpeed;
            protected int tankBulletSpeed;
            protected int energy;
            private BulletType bulletType;
            protected Skin skinTank;
            protected bool isMove;
            private bool isActivate;
            protected bool left, right, up, down;
            protected Direction directionTank;
            private List<Bullet> bullets;
            protected Bitmap bmpEffect;
            #endregion
            public List<Bullet> Bullets { get; }
            public BulletType BulletTypes { get; }

            // contructor
            public Tank()
            {
                this.isActivate = false;
                this.RectWidth = Common.tankSize;
                this.RectHeight = Common.tankSize;
                this.Bullets = new List<Bullet>();
                this.BulletType = BulletType.eTriangleBullet;

            }

            // hiển thị xe tăng
            public override void Show(Bitmap background)
            {
                // nếu xe tăng đang bật chế độ hoạt động sẽ hiển thị xe tăng, 
                // ngược lại hiện thị hiệu ứng xuất hiện
                if (IsActivate)
                {
                    switch (directionTank)
                    {
                        case Direction.eUp:
                            Common.PaintObject(background, this.bmpObject, rect.X, rect.Y,
                                   (int)skinTank * Common.tankSize, frx_tank * Common.tankSize, this.RectWidth, this.RectHeight);
                            break;
                        case Direction.eDown:
                            Common.PaintObject(background, this.bmpObject, rect.X, rect.Y,
                                   (MAX_NUMBER_SPRITE_TANK - (int)skinTank) * Common.tankSize, frx_tank * Common.tankSize, this.RectWidth, this.RectHeight);
                            break;
                        case Direction.eLeft:
                            Common.PaintObject(background, this.bmpObject, rect.X, rect.Y,
                                     frx_tank * Common.tankSize, (MAX_NUMBER_SPRITE_TANK - (int)skinTank) * Common.tankSize, this.RectWidth, this.RectHeight);
                            break;
                        case Direction.eRight:
                            Common.PaintObject(background, this.bmpObject, rect.X, rect.Y,
                                frx_tank * Common.tankSize, (int)skinTank * Common.tankSize, this.RectWidth, this.RectHeight);
                            break;
                    }
                    // nếu xe tăng được di chuyển bánh xe sẽ xoay
                    if (this.isMove)
                    {
                        frx_tank--;
                        if (frx_tank == -1)
                            frx_tank = MAX_NUMBER_SPRITE_TANK;
                    }
                }
                else
                {
                    // hiển thị hiệu ứng xuất hiện
                    Common.PaintObject(background, this.bmpEffect, this.RectX, this.RectY,
                           frx_effect * this.RectWidth, fry_effect * this.RectHeight, this.RectWidth, this.RectHeight);
                    frx_effect++;
                    if (frx_effect == MAX_NUMBER_SPRITE_EFFECT)
                    {
                        frx_effect = 0;
                        fry_effect++;
                        if (fry_effect == MAX_NUMBER_SPRITE_EFFECT)
                        {
                            fry_effect = 0;
                            // hiệu ứng kết thúc, bật lại hoạt động của xe
                            IsActivate = true;
                        }
                    }
                }
            }

            // xoay frame xe tăng
            public void RotateFrame()
            {
                // xoay ảnh phù hợp với frame xe tăng
                if ((left == true && this.DirectionTank == Direction.eDown) ||
                    (right == true && this.DirectionTank == Direction.eUp) ||
                    (up == true && this.DirectionTank == Direction.eLeft) ||
                    (down == true && this.DirectionTank == Direction.eRight))
                {
                    this.bmpObject.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
                else if ((left == true && this.DirectionTank == Direction.eUp) ||
                   (right == true && this.DirectionTank == Direction.eDown) ||
                   (up == true && this.DirectionTank == Direction.eRight) ||
                   (down == true && this.DirectionTank == Direction.eLeft))
                {
                    this.bmpObject.RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
                else if ((left == true && this.DirectionTank == Direction.eRight) ||
                   (right == true && this.DirectionTank == Direction.eLeft) ||
                   (up == true && this.DirectionTank == Direction.eDown) ||
                   (down == true && this.DirectionTank == Direction.eUp))
                {
                    this.bmpObject.RotateFlip(RotateFlipType.Rotate180FlipNone);
                }
                else
                {
                    this.bmpObject.RotateFlip(RotateFlipType.RotateNoneFlipNone);
                }
                // cập nhật hướng của xe tăng
                if (left)
                    directionTank = Direction.eLeft;
                else if (right)
                    directionTank = Direction.eRight;
                else if (up)
                    directionTank = Direction.eUp;
                else if (down)
                    directionTank = Direction.eDown;
            }

            // tạo đạn cho xe tăng
            public void CreatBullet(string pathRoundBullet, string pathRocketBullet)
            {
                if (this.bullets.Count == 0 && this.IsActivate)
                {
                    // đạn
                    Bullet bullet;
                    bullet = new Bullet();
                    bullet.SpeedBullet = this.TankBulletSpeed;

                    // set loại bullet
                    switch (this.bulletType)
                    {
                        case BulletType.eTriangleBullet:
                            bullet.LoadImage(Common.path + pathRoundBullet);
                            // đạn tam giác có kích thước 8x8
                            bullet.RectWidth = 8;
                            bullet.RectHeight = 8;
                            // năng lượng của đạn tam giác mặc định là 10
                            bullet.Power = 10;
                            break;
                        case BulletType.eRocketBullet:
                            bullet.LoadImage(Common.path + pathRocketBullet);
                            // đạn rocket có kích thước 12x12
                            bullet.RectWidth = 12;
                            bullet.RectHeight = 12;
                            // năng lượng của đạn rocket mặc định là 40
                            bullet.Power = 30;
                            break;
                    }
                    // hướng của xe tăng
                    switch (directionTank)
                    {
                        case Direction.eLeft:
                            bullet.DirectionBullet = Direction.eLeft;
                            bullet.BmpObject.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            bullet.RectX = this.RectX + bullet.RectWidth;
                            bullet.RectY = this.RectY + this.RectHeight / 2 - bullet.RectHeight / 2;
                            break;
                        case Direction.eRight:
                            bullet.DirectionBullet = Direction.eRight;
                            bullet.BmpObject.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            bullet.RectX = this.RectX + this.RectWidth - bullet.RectWidth;
                            bullet.RectY = this.RectY + this.RectHeight / 2 - bullet.RectHeight / 2;
                            break;
                        case Direction.eUp:
                            bullet.DirectionBullet = Direction.eUp;
                            bullet.RectY = this.RectY + bullet.RectHeight;
                            bullet.RectX = this.RectX + this.RectWidth / 2 - bullet.RectWidth / 2;
                            break;
                        case Direction.eDown:
                            bullet.DirectionBullet = Direction.eDown;
                            bullet.BmpObject.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            bullet.RectY = this.RectY + this.RectHeight - bullet.RectHeight;
                            bullet.RectX = this.RectX + this.RectWidth / 2 - bullet.RectWidth / 2;
                            break;
                    }
                    this.bullets.Add(bullet);
                    bullet = null;
                }
            }

            // hủy một viên đạn
            public void RemoveOneBullet(int index)
            {
                this.bullets[index] = null;
                this.bullets.RemoveAt(index);
            }

            // di chuyển và hiển thị đạn xe tăng
            public void ShowBulletAndMove(Bitmap background)
            {
                for (int i = 0; i < this.Bullets.Count; i++)
                {
                    this.Bullets[i].BulletMove();
                    this.Bullets[i].Show(background);
                }
            }

            // kiểm tra va chạm của xe tăng với một đối tượng
            public bool IsObjectCollision(Rectangle rectObj)
            {
                switch (this.directionTank)
                {
                    case Direction.eLeft:
                        if (this.Rect.Left == rectObj.Right)
                            if (this.Rect.Top >= rectObj.Top && this.Rect.Top < rectObj.Bottom ||
                                this.Rect.Bottom > rectObj.Top && this.Rect.Bottom <= rectObj.Bottom ||
                                this.Rect.Bottom > rectObj.Bottom && this.Rect.Top < rectObj.Top)
                            {
                                return true;
                            }
                        break;
                    case Direction.eRight:
                        if (this.Rect.Right == rectObj.Left)
                            if (this.Rect.Top >= rectObj.Top && this.Rect.Top < rectObj.Bottom ||
                                this.Rect.Bottom > rectObj.Top && this.Rect.Bottom <= rectObj.Bottom ||
                                this.Rect.Bottom > rectObj.Bottom && this.Rect.Top < rectObj.Top)
                            {
                                return true;
                            }
                        break;
                    case Direction.eUp:
                        if (this.Rect.Top == rectObj.Bottom)
                            if (this.Rect.Left < rectObj.Right && this.Rect.Left >= rectObj.Left ||
                                this.Rect.Right > rectObj.Left && this.Rect.Right <= rectObj.Right ||
                                this.Rect.Right > rectObj.Right && this.Rect.Left < rectObj.Left)
                            {
                                return true;
                            }
                        break;
                    case Direction.eDown:
                        if (this.Rect.Bottom == rectObj.Top)
                            if (this.Rect.Left < rectObj.Right && this.Rect.Left >= rectObj.Left ||
                                this.Rect.Right > rectObj.Left && this.Rect.Right <= rectObj.Right ||
                                this.Rect.Right >= rectObj.Right && this.Rect.Left <= rectObj.Left)
                            {
                                return true;
                            }
                        break;
                }
                return false;
            }

            // kiểm tra xe tăng chạm tường
            public bool IsWallCollision(List<SuperTank.Objects.Wall> walls, Direction directionTank)
            {
                foreach (SuperTank.Objects.Wall wall in walls)
                    // nếu không phải bụi cây thì xét va chạm
                    if (wall.WallNumber != 4)
                        if (IsObjectCollision(wall.Rect))
                            return true;
                return false;
            }

            // xe tăng di chuyển
            public void Move()
            {
                if (this.IsActivate)
                {
                    if (left)
                    {
                        this.RectX -= this.MoveSpeed;
                    }
                    else if (right)
                    {
                        this.RectX += this.MoveSpeed;
                    }
                    else if (up)
                    {
                        this.RectY -= this.MoveSpeed;
                    }
                    else if (down)
                    {
                        this.RectY += this.MoveSpeed;
                    }
                }
            }
            #region properties
            public int MoveSpeed
            {
                get
                {
                    return moveSpeed;
                }

                set
                {
                    moveSpeed = value;
                }
            }
            public int TankBulletSpeed
            {
                get
                {
                    return tankBulletSpeed;
                }

                set
                {
                    tankBulletSpeed = value;
                }
            }
            public int Energy
            {
                get
                {
                    return energy;
                }

                set
                {
                    energy = value;
                }
            }
            public bool Left
            {
                get
                {
                    return left;
                }

                set
                {
                    left = value;
                }
            }
            public bool Right
            {
                get
                {
                    return right;
                }

                set
                {
                    right = value;
                }
            }
            public bool Up
            {
                get
                {
                    return up;
                }

                set
                {
                    up = value;
                }
            }
            public bool Down
            {
                get
                {
                    return down;
                }

                set
                {
                    down = value;
                }
            }
            public Direction DirectionTank
            {
                get
                {
                    return directionTank;
                }

                set
                {
                    directionTank = value;
                }
            }
            public bool IsMove
            {
                get
                {
                    return isMove;
                }

                set
                {
                    isMove = value;
                }
            }
            private List<Bullet> bulletsList; // Renamed to bulletsList to avoid ambiguity

            public List<Bullet> bulletList
            {
                get
                {
                    return bulletsList;
                }

                set
                {
                    bulletsList = value;
                }
            }
            public Skin SkinTank
            {
                get
                {
                    return skinTank;
                }

                set
                {
                    skinTank = value;
                }
            }

            public bool IsActivate
            {
                get
                {
                    return isActivate;
                }

                set
                {
                    isActivate = value;
                }
            }

            public BulletType BulletType
            {
                get
                {
                    return bulletType;
                }
                set
                {
                    bulletType = value;
                }
            }
            #endregion property
        }



        #endregion

    }



}
