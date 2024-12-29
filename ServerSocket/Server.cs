using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SuperTank.Objects;


namespace ServerSocket
{
    public partial class Server : Form
    {
        private static List<PlayerTank> connectedPlayers = new List<PlayerTank>();
        private static List<Lobby> lobbies = new List<Lobby>();
        private static List<Skin> usedSkins = new List<Skin>();
        private static readonly int port = 8989;
        private TcpListener server;
        

        //private const int MAP_WIDTH = 1024;
        //private const int MAP_HEIGHT = 768;
        //private const int SPAWN_INTERVAL = 30000; // 30 seconds

        private Dictionary<string, System.Threading.Timer> lobbyTimers = new Dictionary<string, System.Threading.Timer>();
        //private Dictionary<string, System.Threading.Timer> zombieSpawnTimers = new Dictionary<string, System.Threading.Timer>();
        const int fixedHeight = 620;

        // Thêm hàng đợi để lưu trữ các thông điệp
        private static BlockingCollection<(PlayerTank, string)> messageQueue
            = new BlockingCollection<(PlayerTank, string)>(new ConcurrentQueue<(PlayerTank, string)>(), 1000); // Giới hạn kích thước

        public Server()
        {
            InitializeComponent();
            StartServer();
            Thread processingThread = new Thread(ProcessMessages);
            processingThread.IsBackground = true;
            processingThread.Start();
        }
        private void StartServer()
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            UpdateInfo($"Server is running on port {port}...");

            Thread serverThread = new Thread(() =>
            {
                while (true)
                {
                    var client = server.AcceptTcpClient();
                    UpdateInfo($"Client connected: {client.Client.RemoteEndPoint}");
                    PlayerTank newPlayer = new PlayerTank { PlayerSocket = client };
                    connectedPlayers.Add(newPlayer);
                    ThreadPool.QueueUserWorkItem(HandleClient, newPlayer);
                }
            });
            serverThread.IsBackground = true;
            serverThread.Start();
        }
        private void HandleClient(object obj)
        {
            var player = (PlayerTank)obj;
            var client = player.PlayerSocket;
            var buffer = new byte[1024];
            StringBuilder dataBuffer = new StringBuilder();

            try
            {
                while (client.Connected)
                {
                    var stream = client.GetStream();
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        dataBuffer.Append(receivedData);

                        if (!connectedPlayers.Contains(player))
                        {
                            connectedPlayers.Add(player);
                            Random rnd = new Random();
                            List<Skin> availableSkins = Enum.GetValues(typeof(Skin)).Cast<Skin>().Except(usedSkins).ToList();

                            if (availableSkins.Count > 0)
                            {
                                player.SkinTank = availableSkins[rnd.Next(availableSkins.Count)];
                                usedSkins.Add(player.SkinTank);
                            }
                            else
                            {
                                // Xử lý trường hợp đã sử dụng hết skin (có thể gán lại từ đầu)
                                // Hoặc thông báo cho người chơi rằng không còn skin nào khả dụng
                                // Ví dụ:
                                usedSkins.Clear();
                                player.SkinTank = (Skin)rnd.Next(Enum.GetNames(typeof(Skin)).Length);
                                usedSkins.Add(player.SkinTank);
                                UpdateInfo("All skins have been used. Resetting used skins list.");
                            }
                        }
                        // Kiểm tra và tách thông điệp dựa trên ký tự phân tách '\n'
                        while (dataBuffer.ToString().Contains("\n"))
                        {
                            int newlineIndex = dataBuffer.ToString().IndexOf("\n");
                            string message = dataBuffer.ToString(0, newlineIndex);
                            dataBuffer.Remove(0, newlineIndex + 1);

                            //ACTIVATE THIS LINE TO TRACK PLAYERS//
                            //UpdateInfo($"Received message from {client.Client.RemoteEndPoint}: {message}");

                            // Thêm thông điệp vào hàng đợi
                            if (!messageQueue.TryAdd((player, message)))
                            {
                                UpdateInfo("Message queue is full. Dropping message.");
                            }
                        }
                    }
                    Thread.Sleep(10); // Giảm tải CPU
                }
            }
            catch (ObjectDisposedException ex)
            {
                // Xử lý khi đối tượng đã bị dispose
                UpdateInfo($"Client {player.PlayerName} error: {ex.Message} Socket closed.");
            }
            catch (IOException ex)
            {
                // Xử lý khi mất kết nối
                UpdateInfo($"Client {player.PlayerName} error: {ex.Message} Lost connection.");
            }
            catch (Exception ex)
            {
                UpdateInfo($"Client error: {ex.Message}");
            }
            finally
            {
                client.Close();
                connectedPlayers.Remove(player);
                UpdateInfo($"Client {player.PlayerName} disconnected.");

                // Broadcast tin nhắn ngắt kết nối (nếu cần thiết)
                string disconnectMessage = $"PLAYER_DISCONNECTED;{player.PlayerName}";
                BroadcastMessage(disconnectMessage, player);

                // Xóa player khỏi lobby (nếu cần thiết)
                var lobby = FindLobbyByPlayer(player);
                if (lobby != null)
                {
                    lobby.Players.RemoveAll(p => p.PlayerName == player.PlayerName);
                }
            }
        }

        // Luồng xử lý thông điệp từ hàng đợi
        private void ProcessMessages()
        {
            foreach (var (player, message) in messageQueue.GetConsumingEnumerable())
            {
                AnalyzingMessage(message, player);
            }
        }
        private void AnalyzingMessage(string message, PlayerTank player)
        {
            string[] arrPayload = message.Split(';');
            UpdateInfo($"Received message: {message} from {player.PlayerName}"); // Log tin nhắn nhận được
            switch (arrPayload[0])
            {
                case "CONNECT":
                    player.PlayerName = arrPayload[1];
                    if (connectedPlayers.Count == 1)
                    {
                        player.IsHost = true;
                        UpdateInfo($"{player.PlayerName} is the host.");
                    }
                    break;
                case "DISCONNECT":
                    HandleDisconnect(player);
                    break;
                case "CREATE_ROOM":
                    CreateRoom(player, arrPayload[1]);
                    break;
                case "SEND_ROOM_LIST":
                    SendRoomList(player);
                    break;
                case "JOIN_ROOM":
                    JoinRoom(player, arrPayload[1]);
                    break;
                case "SEND_LOBBY":
                    SendLobbyInfoToAll(player, arrPayload[1]);
                    UpdateInfo($"Sending LOBBY_INFO for room {arrPayload[1]} to all connected clients.");
                    break;
                case "READY":
                    var lobby = FindLobbyByPlayer(player);
                    lobby.Players.SingleOrDefault(r => r.PlayerName == player.PlayerName).IsReady = true;
                    string readyInfo = $"READY_INFO;{player.PlayerName}";
                    foreach (var _player in lobby.Players)
                    {
                        SendMessageToPlayer(_player, readyInfo);
                    }
                    break;
                case "MESSAGE":
                    string content = $"SEND_MESSAGE;{arrPayload[1]}";
                    BroadcastMessage(content, player);
                    break;
                case "START":
                    StartGameForLobby(player);
                    var lobbyy = FindLobbyByPlayer(player);
                    if (lobbyy != null)
                        UpdateInfo($"Game start signal received, lobby IsStart is: {lobbyy.IsStart} for lobby hosted by {lobbyy.Host.PlayerName}");
                    break;
                case "STATS":
                    int killCount = int.Parse(arrPayload[1]);
                    int scoreGained = int.Parse(arrPayload[2]);
/*                    player.KillCount += killCount;
                    player.Score += scoreGained;*/
                    UpdatePlayerStats(player);
                    break;
                case "GAMEOVER":
                    CheckGameOver(player);
                    break;
                case "RANKING":
                    var playerLobby = FindLobbyByPlayer(player);
                    if (playerLobby != null)
                    {
                        BroadcastRanking(playerLobby);
                    }
                    break;
                case "CLEAR_LOBBY":
                    var _lobby = FindLobbyByPlayer(player);
                    if (_lobby != null)
                    {
                        _lobby.Players.Clear();
                        _lobby.Host = null;
                        lobbies.RemoveAll(l => l.RoomId == _lobby.RoomId);
                    }
                    string clearLobbyMessage = "CLEAR_LOBBY";
                    SendMessageToPlayer(player, clearLobbyMessage);
                    break;
                case "DISCONNECTGameRoom":
                    CloseAllGameRooms();
                    break;
                case "UPDATE_POSITION":
                    string playerName = arrPayload[1];
                    string direction = arrPayload[2];
                    int x = int.Parse(arrPayload[3]);
                    int y = int.Parse(arrPayload[4]);
                    int frx_tank = int.Parse(arrPayload[5]);
                    int skinTank = int.Parse(arrPayload[6]); // Thêm skinTank

                    player = connectedPlayers.FirstOrDefault(p => p.PlayerName == playerName);

                    if (player != null)
                    {
                        UpdatePlayerPosition(player, direction, x, y, frx_tank, (Skin)skinTank); // Sửa
                        BroadcastPlayerPosition(player, direction, x, y, frx_tank);
                    }
                    else
                    {
                        UpdateInfo($"Player not found: {playerName}");
                    }
                    break;

                case "UPDATE_GUN":
                    string playerName1 = arrPayload[1];
                    string gunName = arrPayload[2];
                    string gunUpdateMessage = $"UPDATE_GUN;{playerName1};{gunName}";
                    BroadcastMessage(gunUpdateMessage, player);
                    break;
                case "PLAYER_SHOOT":
                    string shooterName = arrPayload[1];
                    string shootDirection = arrPayload[2];
                    string gunName2 = arrPayload[3];
                    string shootMessage = $"PLAYER_SHOOT;{shooterName};{shootDirection};{gunName2}";
                    BroadcastMessage(shootMessage, player);
                    break;
                case "UPDATE_WALL_HEALTH": //Not used
                    double health = double.Parse(arrPayload[1]);
                    //UpdateWallHealth(health);
                    break;

               
                default:
                    UpdateInfo($"Unknown command received: {arrPayload[0]} from {player.PlayerName}");
                    break;
            }
        }

        private void HandleDisconnect(PlayerTank player)
        {
            connectedPlayers.Remove(player);
            player.PlayerSocket.Close(); // Đảm bảo đóng kết nối
            UpdateInfo($"{player.PlayerName} has disconnected.");

            string disconnectMessage = $"PLAYER_DISCONNECTED;{player.PlayerName}";
            BroadcastMessage(disconnectMessage, player);

            // Xóa player khỏi lobby
            var lobby = FindLobbyByPlayer(player);
            if (lobby != null)
            {
                lobby.Players.RemoveAll(p => p.PlayerName == player.PlayerName);
            }
        }

        private void CreateRoom(PlayerTank player, string id)
        {
            var lobby = lobbies.SingleOrDefault(r => r.RoomId == id);
            if (lobby == null)
            {
                string roomId;
                if (!string.IsNullOrEmpty(id))
                {
                    roomId = id;
                }
                else
                {
                    roomId = GenerateRoomId();
                }
                Lobby newLobby = new Lobby
                {
                    RoomId = roomId,
                    Host = player,
                    Players = new List<PlayerTank> { player }
                };
                lobbies.Add(newLobby);
                UpdateInfo($"Lobby {roomId} has been created by {player.PlayerName}.");
                SendRoomList(player);
                string joinMessage = $"JOINED;{roomId}";
                SendMessageToPlayer(player, joinMessage);
            }
            else
            {
                string errorMessage = $"ERROR_CREATE;{id}";
                SendMessageToPlayer(player, errorMessage);
            }
        }

        private void JoinRoom(PlayerTank player, string roomId)
        {
            var lobby = lobbies.SingleOrDefault(r => r.RoomId == roomId);
            if (lobby != null && lobby.Players.Count < 4 && !lobby.IsStart)
            {
                lobby.Players.Add(player);
                string joinMessage = $"JOINED;{roomId}";
                SendMessageToPlayer(player, joinMessage);
            }
            else
            {
                string errorMessage = $"ERROR_JOIN;{roomId}";
                SendMessageToPlayer(player, errorMessage);
            }
        }
        private void StartGameForLobby(PlayerTank player)
        {
            Lobby lobby = FindLobbyByPlayer(player);
            if (lobby != null)
            {
                lobby.IsStart = true;

                // Tạo danh sách tên người chơi bao gồm cả host
                List<string> playerNames = lobby.Players.Select(p => p.PlayerName).ToList();

                // Tạo thông điệp START
                string startMessage = $"START;{lobby.Host.PlayerName};{string.Join(",", playerNames)}";

                int playerIndex = 0;
                foreach (var p in lobby.Players)
                {
                    int xPos = 2;
                    int yPos = 0;
                    switch (playerIndex)
                    {
                        case 0: yPos = 3; break;
                        case 1: yPos = 16; break;
                        case 2: yPos = 29; break;
                        case 3: yPos = 42; break;
                    }

                    p.SetLocation(xPos, yPos);
                    SendMessageToPlayer(p, startMessage); // Gửi message đến TẤT CẢ player, kể cả host
                    playerIndex++;
                }

                // Gửi thông tin về tất cả xe tăng đến người chơi mới
                foreach (var p in lobby.Players)
                {
                    foreach (var otherPlayer in lobby.Players)
                    {
                        if (p != otherPlayer)
                        {
                            // Gửi thông tin vị trí của otherPlayer cho p
                            string positionMessage = $"UPDATE_POSITION;{otherPlayer.PlayerName};{(int)otherPlayer.DirectionTank};{otherPlayer.RectX};{otherPlayer.RectY};{otherPlayer.frx_tank};{(int)otherPlayer.SkinTank}"; SendMessageToPlayer(p, positionMessage);
                        }
                    }
                }
                UpdateInfo("Game started for lobby hosted by " + lobby.Host.PlayerName);
            }
            else
            {
                UpdateInfo("Lobby not found for player: " + player.PlayerName);
            }
        }
        private void CheckGameOver(PlayerTank player)
        {
            bool check = true;
            var lobby = FindLobbyByPlayer(player);
            if (lobby != null)
            {
                foreach (var _player in lobby.Players)
                {
                    if (!_player.IsGameOver)
                    {
                        check = false;
                        break;
                    }
                }
                string GameOverInfo = $"GAMEOVER;{check.ToString()}";
                SendMessageToPlayer(player, GameOverInfo);
            }
        }

        private void CloseAllGameRooms()
        {
            foreach (var player in connectedPlayers)
            {
                string closeMessage = "CLOSE_ALL";
                SendMessageToPlayer(player, closeMessage);
            }
            UpdateInfo("All game rooms have been closed.");
        }

        private void UpdatePlayerStats(PlayerTank player)
        {
           string updateMessage = $"UPDATE_STATS;{player.PlayerName};";
            player.IsGameOver = true;
            Lobby lobby = FindLobbyByPlayer(player);
            if (lobby != null)
            {
                foreach (var _player in lobby.Players)
                {
                    SendMessageToPlayer(_player, updateMessage);
                }
            }

            //UpdateInfo($"Updated stats for {player.PlayerName} - Kills: {player.KillCount}, Score: {player.Score}");
        }
        private void UpdatePlayerPosition(PlayerTank player, string direction, int x, int y, int frx_tank, Skin skinTank)
        {
            // Cập nhật direction, x, y như bạn đã có
            if (int.TryParse(direction, out int directionInt))
            {
                if (Enum.IsDefined(typeof(Direction), directionInt))
                {
                    player.DirectionTank = (Direction)directionInt;
         
                }
                else
                {
                    UpdateInfo($"Invalid direction value: {directionInt}");
                    player.DirectionTank = Direction.eUp; // Giá trị mặc định
                }

                player.SetLocation(x, y); // Lưu tọa độ dạng số ô
                if (player.frx_tank != frx_tank)
                {
                    player.frx_tank = frx_tank;
                }
            }
            else
            {
                UpdateInfo($"Invalid direction format: {direction}");
            }

            // Cập nhật frx_tank
            // Mỗi khi nhận được UPDATE_POSITION (xe tăng di chuyển), frx_tank sẽ giảm đi 1
            // Khi frx_tank < 0, nó sẽ quay trở lại giá trị ban đầu (7)
            player.frx_tank--;
            if (player.frx_tank < 0)
            {
                player.frx_tank = 7;
            }
        }


        private void BroadcastMessage(string message, PlayerTank sender)
        {
            byte[] msgBuffer = Encoding.UTF8.GetBytes(message);
            Lobby lobby = FindLobbyByPlayer(sender);
            if (lobby != null)
            {
                foreach (var player in lobby.Players)
                {
                    if (player.PlayerSocket != sender.PlayerSocket)
                    {
                        SendMessageToPlayer(player, message);
                    }
                }
            }
        }

        private void SendMessageToPlayer(PlayerTank player, string message)
        {
            try
            {
                if (player.PlayerSocket.Connected) // Kiểm tra kết nối
                {
                    var stream = player.PlayerSocket.GetStream();
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    stream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    UpdateInfo($"Player {player.PlayerName} is not connected. Cannot send message.");
                }
            }
            catch (IOException ex)
            {
                // Xử lý khi mất kết nối
                Console.WriteLine("Mất kết nối với player: ");
                // Có thể thêm code để xóa player khỏi danh sách connected players
            }
            catch (SocketException ex)
            {
                // Xử lý lỗi socket
                Console.WriteLine("Lỗi kết nối socket với player: ");
            }
            catch (ObjectDisposedException ex)
            {
                // Xử lý khi đối tượng đã bị dispose
                Console.WriteLine($"Không thể gửi tin nhắn đến player {player.PlayerName} đã bị đóng.");
            }
        }
        private static string GenerateRoomId()
        {
            return Guid.NewGuid().ToString().Substring(0, 6);
        }

        private void SendRoomListToAll()
        {
            foreach (var player in connectedPlayers)
            {
                SendRoomList(player);
            }
        }

        private void SendRoomList(PlayerTank player)
        {
            StringBuilder roomList = new StringBuilder("ROOMLIST;");
            foreach (var lobby in lobbies)
            {
                roomList.Append($"{lobby.RoomId};{lobby.Host.PlayerName};");
            }

            SendMessageToPlayer(player, roomList.ToString());
        }

        private Lobby FindLobbyByPlayer(PlayerTank player)
        {
            return lobbies.FirstOrDefault(lobby => lobby.Players.Contains(player));
        }

        private void ShowingInfo_TextChanged(object sender, EventArgs e) { }

        private void SendLobbyInfoToAll(PlayerTank player, string roomId)
        {
            var lobby = lobbies.FirstOrDefault(r => r.RoomId == roomId);
            if (lobby != null)
            {
                string lobbyInfo = $"LOBBY_INFO;{lobby.RoomId};{lobby.Players.Count};" +
                    $"{string.Join(",", lobby.Players.Select(p => p.PlayerName))};" +
                    $"{string.Join(",", lobby.Players.Select(p => p.IsReady.ToString()))}";

                var playersCopy = connectedPlayers.ToList(); // Create a copy of the collection
                foreach (var _player in playersCopy)
                {
                    SendMessageToPlayer(_player, lobbyInfo);
                }
            }
        }

        public void UpdateInfo(string message)
        {
            if (ShowingInfo.InvokeRequired)
            {
                ShowingInfo.Invoke(new Action(() => UpdateInfo(message)));
            }
            else
            {
                ShowingInfo.AppendText(message + Environment.NewLine);
                ShowingInfo.SelectionStart = ShowingInfo.Text.Length;
                ShowingInfo.ScrollToCaret();
            }
        }

        /*private void BroadcastRanking(Lobby lobby)
        {
            var rankingData = new StringBuilder("RANKING;");

            var sortedPlayers = lobby.Players.OrderByDescending(p => p.Score).ToList();

            foreach (var player in sortedPlayers)
            {
                rankingData.Append($"{player.PlayerName};{player.KillCount};{player.Score};");
            }

            string rankingMessage = rankingData.ToString().TrimEnd(';');

            foreach (var player in lobby.Players)
            {
                SendMessageToPlayer(player, rankingMessage);
            }
        }*/

        private void BroadcastRanking(Lobby lobby)
        {
            var rankingData = new StringBuilder("RANKING;");

            /*var sortedPlayers = lobby.Players.OrderByDescending(p => p.Score).ToList();

            foreach (var player in sortedPlayers)
            {
                rankingData.Append($"{player.PlayerName};{player.KillCount};{player.Score};");
            }*/

            string rankingMessage = rankingData.ToString().TrimEnd(';');

            foreach (var player in lobby.Players)
            {
                SendMessageToPlayer(player, rankingMessage);
            }
        }

        private void BroadcastPlayerPosition(PlayerTank player, string direction, int x, int y, int frx_tank)
        {
            // Gửi giá trị int của Direction
            string positionMessage = $"UPDATE_POSITION;{player.PlayerName};{(int)player.DirectionTank};{x};{y};{player.frx_tank};{(int)player.SkinTank}";
            Lobby lobby = FindLobbyByPlayer(player);

            if (lobby != null)
            {
                foreach (var otherPlayer in lobby.Players)
                {
                    if (otherPlayer != player)
                    {
                        SendMessageToPlayer(otherPlayer, positionMessage);
                    }
                }
            }
        }


        private void BroadcastToLobby(Lobby lobby, string message)
        {
            foreach (var player in lobby.Players)
            {
                SendMessageToPlayer(player, message);
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }

        
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        #region các class dùng chung 
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
            public bool IsWallCollision(List<Wall> walls, Direction directionTank)
            {
                foreach (Wall wall in walls)
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
        public class PlayerTank : Tank
        {
            public TcpClient PlayerSocket { get; set; }

            /* private bool isShield;
             private Bitmap bmpShield;*/
            public bool IsGameOver { get; set; } = false;
            public int PositionX { get; set; }
            public int PositionY { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public bool IsHost { get; internal set; }
            public bool isHost { get; set; }
            public string Id { get; set; }
            public string PlayerName { get; set; }
            public bool IsReady { get; set; } = false;
            public new Direction DirectionTank { get; set; }
            public int Frame { get; set; } // Thêm thuộc tính lưu frame (nếu cần)
            public int frx_tank { get; set; } = 7; // Thêm thuộc tính frx_tank và khởi tạo giá trị ban đầu


            public PlayerTank()
            {
                this.moveSpeed = 10;
                this.tankBulletSpeed = 20;
                this.energy = 100;
                this.SetLocation(2, 2);
                this.DirectionTank = Direction.eUp;
                //this.SkinTank = Skin.eYellow;
                //this.frx_tank = 7;
                /*bmpEffect = new Bitmap(Common.path + @"\Images\effect1.png");
                bmpShield = new Bitmap(Common.path + @"\Images\shield.png");*/

            }

            // cập nhật vị trí xe tăng player
            public void SetLocation(int i, int j)
            {
                this.RectX = i * Common.STEP;
                this.RectY = j * Common.STEP;
            }
            // cập nhật vị trí xe tăng player



        #region properties
        /*public bool IsShield
        {
            get { return isShield; }
            set { isShield = value; }
        }*/


        #endregion
    }
        public class Lobby
        {
            public string RoomId { get; set; }
            public PlayerTank Host { get; set; } // Sử dụng PlayerTank từ SuperTank.Objects
            public List<PlayerTank> Players { get; set; } = new List<PlayerTank>(); // Sử dụng PlayerTank từ SuperTank.Objects
            public bool IsStart { get; set; } = false;
        }
        #endregion
    }

}