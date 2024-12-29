using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SuperTank.General;
using SuperTank.Objects;
using SuperTank.WindowsForms;

namespace SuperTank
{
    public partial class frmGameMulti : Form
    {
        #region Đồ họa graphics
        private Graphics graphics;
        #endregion Đồ họa graphics

        #region các thuộc tính chung
        private Bitmap background;
        private Bitmap bmpCastle;
        private int[,] map;
        #endregion các thuộc tính chung

        #region Đối tượng
        private WallManagement wallManager;
        private ExplosionManagement explosionManager;
        private PlayerTank playerTank;
        private EnemyTankManagement enemyTankManager;
        private Item item;
        public ChatRoom _chatRoom;
        #endregion Đối tượng

        #region thuộc tính thông tin
        private PictureBox[] picNumberEnemyTanks;
        private int level;
        private int scores;
        private int killed;
        private InforStyle inforStyle;
        #endregion thuộc tính thông tin

        #region thuộc tính thời gian
        //private int timeItem = 50;
        //private int timeItemActive = 15;
        //private bool isTimeItemActive;
        //private int time_delay = 0;
        #endregion thuộc tính thời gian

        string lastShooter;
        public frmMenu formMenu;
        private DateTime lastSentPositionTime = DateTime.Now;
        private const int POSITION_UPDATE_INTERVAL_MS = 1; // Thời gian tối thiểu giữa các lần gửi (ms)
        public frmGameMulti()
        {
            this.level = 10;
            this.Size = new Size(500, 640);
            InitializeComponent();
            SocketClient.OnGameOver += HandleGameOver;
        }

        private void HandleGameOver(bool isWin)
        {
            bmpCastle = (Bitmap)Image.FromFile(Common.path + @"\Images\ruinedcastle.png");
            tmrGameLoop.Stop();

      
                Lose l = new Lose(this);
                l.Show();

       
            this.Close();

        }

        private void frmGameMulti_Load(object sender, EventArgs e)
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false; // Tắt tự động thay đổi kích thước




            // load ảnh heart cho hp playertank
            picHeart.Image = Image.FromFile(Common.path + @"\Images\heart.png");
            // add picture box vào mảng hiển thị số lượng địch
            picNumberEnemyTanks = new PictureBox[]{picTank00, picTank01, picTank02,
            picTank03, picTank04, picTank05, picTank06, picTank07, picTank08, picTank09, picTank10,
            picTank11, picTank12, picTank13, picTank14, picTank15, picTank16, picTank17, picTank18, picTank19};
            // khởi tạo graphics
            graphics = pnMainGame.CreateGraphics();
            // khỏi tạo background
            background = new Bitmap(Common.SCREEN_WIDTH, Common.SCREEN_HEIGHT);
            // khởi tạo bitmap castle
            bmpCastle = new Bitmap(Common.STEP * 3, Common.STEP * 3);
            // khởi tạo map
            map = new int[Common.NUMBER_OBJECT_HEIGHT, Common.NUMBER_OBJECT_WIDTH];
            // tạo đối tượng quản lí tường
            wallManager = new WallManagement();
            // tạo đối tượng quản lí vụ nổ
            explosionManager = new ExplosionManagement();
            // tạo đối tượng xe tăng player
            playerTank = new PlayerTank();
            playerTank.LoadImage(Common.path + @"\Images\tank0.png");
            // khởi tạo danh sách địch
            enemyTankManager = new EnemyTankManagement();
            // khởi tạo vật phẩm
            item = new Item();

            // khởi tạo game
            this.GameStart();
        }

        // hàm khởi tạo game mới
        private void GameStart()
        {
            // phát âm thanh
            Sound.PlayStartSound();
            // load map
            Array.Copy(Common.ReadMap(String.Format("{0}{1:00}.txt", Common.path + @"\Maps\Map", this.level),
                Common.NUMBER_OBJECT_HEIGHT, Common.NUMBER_OBJECT_WIDTH),
            this.map, Common.NUMBER_OBJECT_HEIGHT * Common.NUMBER_OBJECT_WIDTH);
            // giải phóng danh sách tường cũ
            wallManager.WallsClear();
            // giải phóng danh sách địch
            enemyTankManager.EnemyTanksClear();
            // giải phóng tất cả vụ nổ
            explosionManager.Explosions.Clear();
            GC.Collect();
            // tạo danh sách tường
            wallManager.CreatWall(this.map, this.level);
            // khởi tạo danh sách địch

            enemyTankManager.Init_EnemyTankManagement(String.Format("{0}{1:00}.txt",
                Common.path + @"\EnemyTankParameters\EnemyParameter", this.level));
            // hiển thị thông tin level hiện tại
            lblLevel.Text = String.Format("LEVEL {0}", this.level);
            // hiển thị số lượng xe tăng địch cần tiêu diệt bên bảng thông tin
            ShowNumberEnemyTankDestroy(enemyTankManager.NumberEnemyTank());
            // cập nhật vị trí xe tăng player
            playerTank.SetLocation(2,2);
            // cập nhật năng lượng xe tăng player 
            playerTank.Energy = 100;
            // cập nhật khiên bảo vệ
            playerTank.IsShield = false;
            // cập nhật loại đạn
            playerTank.BulletType = BulletType.eTriangleBullet;
            // cập nhật thông tin máu hiển thị của xe tăng player
            this.lblHpTankPlayer.Width = playerTank.Energy;
            // cập nhật thông tin máu hiển thị của thành
            this.lblCastleBlood.Width = 60;
            // cập nhật thông tin vật phẩm đang ăn
            this.picItem.Image = null;
            this.lblItemActive.Text = "";
            // load hình castle 
            bmpCastle = (Bitmap)Image.FromFile(Common.path + @"\Images\castle.png");
            // điểm và số lượng địch tiêu diệt được là 0
            this.scores = 0;
            this.killed = 0;
            // hủy hình ảnh item
            item.BmpObject = null;
            item.IsOn = false;
            //// bật biến hoạt động của item về false
            //isTimeItemActive = false;
            // bật các nút chức năng trên game 
            this.LabelEnableOn();
            // set thời gian item và chạy item
            //timeItem = 50;
            //timeItemActive = 15;

            tmrGameLoop.Start();
        }

        #region Vòng lặp game
        private void tmrGameLoop_Tick(object sender, EventArgs e)
        {
            // xóa background
            Common.PaintClear(this.background);
            // hiển thị castle
            Common.PaintObject(this.background, bmpCastle,
                420, 700, 0, 0, 60, 60);
            // vẽ và di chuyển đạn player
            playerTank.ShowBulletAndMove(this.background);
            //tạo và di chuyển đạn của địch
            foreach (var player in SocketClient.players)
            {
                if (player.Name == SocketClient.localPlayer.Name)
                {
                    if (!playerTank.IsWallCollision(wallManager.Walls, playerTank.DirectionTank))
                    {
                        playerTank.Move();

                        // GỬI THÔNG TIN VỀ SOCKETCLIENT SAU KHI DI CHUYỂN
                        if ((DateTime.Now - lastSentPositionTime).TotalMilliseconds >= POSITION_UPDATE_INTERVAL_MS)
                        {
                            playerTank.frx_tank--;
                            if (playerTank.frx_tank < 0) playerTank.frx_tank = 7;

                            SocketClient.SendPlayerPosition(SocketClient.localPlayer.Name, (SocketClient.Direction)playerTank.DirectionTank, playerTank.Rect.Left, playerTank.Rect.Top, playerTank.frx_tank, (SocketClient.Skin)playerTank.SkinTank);
                            lastSentPositionTime = DateTime.Now;
                        }
                    }
                    playerTank.Show(this.background);
                }
                else
                {
                    // Vẽ xe tăng của người chơi khác
                    DrawOtherPlayerTank(player);
                }
            }

            foreach (EnemyTank enemyTank in enemyTankManager.EnemyTanks)
            {
                enemyTank.CreatBullet(@"\Images\triangleBullet2.png", @"\Images\rocketBullet2.png");
                enemyTank.ShowBulletAndMove(this.background);
            }

            #region đạn player và đạn địch trúng tường
            for (int i = wallManager.Walls.Count - 1; i >= 0; i--)
            {
                // chạy danh sách đạn player và kiểm tra
                for (int j = 0; j < playerTank.Bullets.Count; j++)
                {
                    // nếu đạn xe tăng player trúng tường 
                    if (Common.IsCollision(playerTank.Bullets[j].Rect, wallManager.Walls[i].Rect))
                    {
                        // viên đạn bị hủy nếu nó trúng, không phải bụi cây(4)
                        if (wallManager.Walls[i].WallNumber != 4 &&
                            wallManager.Walls[i].WallNumber != 5)
                        {
                            // thêm vụ nổ vào danh sách
                            explosionManager.CreateExplosion(ExplosionSize.eSmallExplosion, playerTank.Bullets[j].Rect);
                            // viên đạn xe tăng player này bị hủy
                            playerTank.RemoveOneBullet(j);
                        }

                        // hủy viên gạch đi khi nó là gạch có thể phá hủy
                        if (wallManager.Walls[i].WallNumber == 1)
                        {
                            //Console.WriteLine("Ta bắn trúng tường có thể phá.");
                            wallManager.RemoveOneWall(i);
                        }
                        else
                        // player tự bắn trúng boss của player
                        if (wallManager.Walls[i].WallNumber == 6)
                        {
                            //Console.WriteLine("player bắn trúng boss player!");
                            lblCastleBlood.Width -= 6;
                            if (lblCastleBlood.Width == 0)
                            {
                                // game over
                                inforStyle = InforStyle.eGameOver;
                                // lâu đài bị hỏng
                                bmpCastle = (Bitmap)Image.FromFile(Common.path + @"\Images\ruinedcastle.png");
                                lastShooter = playerTank.Name; // Giả sử playerTank là người bắn
                                                               // Gửi tín hiệu GAMEOVER đến server
                                Win winForm = new Win();
                                winForm.Show();
                                SocketClient.SendData($"GAMEOVER;{false}");

                            }
                        }
                    }
                }

                // chạy danh sách đạn của từng kẻ địch 
                foreach (EnemyTank enemyTank in enemyTankManager.EnemyTanks)
                {
                    for (int h = 0; h < enemyTank.Bullets.Count; h++)
                    {
                        // nếu đạn xe tăng địch trúng tường 
                        if (Common.IsCollision(enemyTank.Bullets[h].Rect, wallManager.Walls[i].Rect))
                        {
                            // viên đạn dừng di chuyển nếu nó trúng, không phải bụi cây(4)
                            if (wallManager.Walls[i].WallNumber != 4 &&
                                wallManager.Walls[i].WallNumber != 5)
                            {
                                // thêm vụ nổ vào danh sách
                                explosionManager.CreateExplosion(ExplosionSize.eSmallExplosion, enemyTank.Bullets[h].Rect);
                                // viên đạn xe tăng địch này bị hủy
                                enemyTank.RemoveOneBullet(h);
                            }
                            // hủy viên gạch đi khi nó là gạch có thể phá hủy
                            if (wallManager.Walls[i].WallNumber == 1)
                            {
                                //Console.WriteLine("Địch bắn trúng tường có thể phá.");
                                wallManager.RemoveOneWall(i);
                            }
                            else
                             // địch bắn trúng boss của player
                             if (wallManager.Walls[i].WallNumber == 6)
                            {
                                //Console.WriteLine("địch bắn trúng boss player!");
                                lblCastleBlood.Width -= 6;
                                if (lblCastleBlood.Width == 0)
                                {
                                    // game over
                                    inforStyle = InforStyle.eGameOver;
                                    // lâu đài bị hỏng
                                    bmpCastle = (Bitmap)Image.FromFile(Common.path + @"\Images\ruinedcastle.png");
                                    // dừng timer show vật phẩm

                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region đạn địch trúng xe tăng hoặc trúng đạn của xe tăng player
            // chạy danh sách xe tăng địch
            for (int i = 0; i < enemyTankManager.EnemyTanks.Count; i++)
            {
                // chạy danh sách đạn địch kiểm tra có trúng xe tăng player
                for (int j = 0; j < enemyTankManager.EnemyTanks[i].Bullets.Count; j++)
                {
                    // đạn của xe tăng địch bắn trúng xe tăng player
                    if (Common.IsCollision(enemyTankManager.EnemyTanks[i].Bullets[j].Rect, playerTank.Rect)
                        && playerTank.IsActivate)
                    {
                        //Console.WriteLine("Địch bắn trúng ta");
                        // phát âm thanh
                        Sound.PlayHitByBulletsSound();
                        // thêm vụ nổ vào danh sách
                        explosionManager.CreateExplosion(ExplosionSize.eBigExplosion, enemyTankManager.EnemyTanks[i].Bullets[j].Rect);
                        // nếu xe tăng player không có vật phẩm khiêng chắn
                        if (!playerTank.IsShield)
                        {
                            // cập nhật lại thông tin vị trí cho xe tăng player
                            playerTank.SetLocation(2,2);
                            playerTank.IsActivate = false;
                            // cập nhật năng lượng của xe tăng player
                            playerTank.Energy -= enemyTankManager.EnemyTanks[i].Bullets[j].Power;
                            this.lblHpTankPlayer.Width = playerTank.Energy;
                            // xe tăng player hết năng lượng sẽ thua
                            if (playerTank.Energy == 0)
                            {
                                // Gameover
                                inforStyle = InforStyle.eGameOver;
                                // dừng timer show vật phẩm

                            }
                        }
                        // viên đạn này của địch bị hủy
                        enemyTankManager.EnemyTanks[i].RemoveOneBullet(j);
                    }

                }

                // chạy danh sách đạn địch kiểm tra va chạm với ds đạn xe tăng player
                for (int j = 0; j < enemyTankManager.EnemyTanks[i].Bullets.Count; j++)
                {
                    // chạy danh sách đạn xe tăng player
                    for (int h = 0; h < playerTank.Bullets.Count; h++)
                    {
                        // đạn của xe tăng địch va chạm đạn của xe tăng player
                        if (Common.IsCollision(enemyTankManager.EnemyTanks[i].Bullets[j].Rect, playerTank.Bullets[h].Rect))
                        {
                            //Console.WriteLine("hai viên đạn trúng nhau");
                            enemyTankManager.EnemyTanks[i].RemoveOneBullet(j);
                            playerTank.RemoveOneBullet(h);
                        }
                    }
                }

                //chạy danh sách đạn xe tăng player
                for (int k = 0; k < playerTank.Bullets.Count; k++)
                {
                    // xe tăng player bắn trúng địch
                    if (Common.IsCollision(enemyTankManager.EnemyTanks[i].Rect, playerTank.Bullets[k].Rect) &&
                        enemyTankManager.EnemyTanks[i].IsActivate)
                    {
                        //Console.WriteLine("Địch bị trúng đạn");

                        // thêm vụ nổ vào danh sách
                        explosionManager.CreateExplosion(ExplosionSize.eBigExplosion, playerTank.Bullets[k].Rect);

                        #region kiểm tra cập nhật vị trí xe tăng địch
                        // trừ năng lượng của địch
                        enemyTankManager.EnemyTanks[i].Energy -= playerTank.Bullets[k].Power;
                        if (enemyTankManager.EnemyTanks[i].Energy > 0)
                        {
                            // phát âm thanh
                            Sound.PlayLowAmmoEnergySound();
                            // đổi màu skin
                            enemyTankManager.EnemyTanks[i].SkinTank = enemyTankManager.SkinEnemyTank(enemyTankManager.EnemyTanks[i]);
                        }
                        else
                        {
                            enemyTankManager.EnemyTankParameters[i].maxNumberEnemyTank--;
                            if (enemyTankManager.EnemyTankParameters[i].maxNumberEnemyTank > 0)
                            {
                                enemyTankManager.UpdateParameter(enemyTankManager.EnemyTanks[i], enemyTankManager.EnemyTankParameters[i]);
                                enemyTankManager.EnemyTanks[i].IsActivate = false;
                            }
                            else
                            {
                                enemyTankManager.EnemyTanks.RemoveAt(i);
                                enemyTankManager.EnemyTankParameters.RemoveAt(i);
                            }
                            // tiêu diệt được một kẻ địch
                            enemyTankManager.NumberEnemyTankDestroy--;
                            killed++;
                            scores += 100;
                            // phát âm thanh
                            Sound.PlayHitByBulletsSound();
                            // cập nhật lại thông tin số địch còn lại lên pic
                            picNumberEnemyTanks[enemyTankManager.NumberEnemyTankDestroy].Image = null;
                            // đã tiêu diệt toàn bộ kẻ địch
                            if (enemyTankManager.NumberEnemyTankDestroy == 0 && this.level == Common.MAX_LEVEL)
                            {
                                // Gamewin
                                inforStyle = InforStyle.eGameWin;
                                // dừng timer show vật phẩm

                            }
                            else
                                if (enemyTankManager.NumberEnemyTankDestroy == 0)
                            {
                                // Gamenext
                                inforStyle = InforStyle.eGameNext;
                                // dừng timer show vật phẩm

                            }
                        }
                        #endregion kiểm tra cập nhật vị trí xe tăng địch

                        // viên đạn này của player bị hủy
                        playerTank.RemoveOneBullet(k);
                    }
                }
            }
            #endregion đạn địch trúng xe tăng, đạn của xe tăng player

            // xe tăng player di chuyển
            if (!playerTank.IsWallCollision(wallManager.Walls, playerTank.DirectionTank))
            {
                playerTank.Move();

                // GỬI THÔNG TIN VỀ SOCKETCLIENT SAU KHI DI CHUYỂN
                if ((DateTime.Now - lastSentPositionTime).TotalMilliseconds >= POSITION_UPDATE_INTERVAL_MS)
                {
                    playerTank.frx_tank--;
                    if (playerTank.frx_tank < 0) playerTank.frx_tank = 7;

                    SocketClient.SendPlayerPosition(SocketClient.localPlayer.Name, (SocketClient.Direction)playerTank.DirectionTank, playerTank.Rect.Left, playerTank.Rect.Top, playerTank.frx_tank, (SocketClient.Skin)playerTank.SkinTank);
                    lastSentPositionTime = DateTime.Now;
                }
            }
            // hiển thị xe tăng của player
            playerTank.Show(this.background);
            // hiển thị tất cả tường lên background
            wallManager.ShowAllWall(this.background);
            //hiển thị vụ nổ
            explosionManager.ShowAllExplosion(this.background);

            // vật phẩm được phép hiển thị

            //vẽ lại Bitmap background lên form
            graphics.DrawImageUnscaled(this.background, 0, 0);
        }

        private void DrawOtherPlayerTank(PlayerTank otherPlayer)
        {
            // Load hình ảnh nếu chưa load
            if (otherPlayer.BmpObject == null)
            {
                otherPlayer.LoadImage(Common.path + @"\Images\tank1.png");
            }

            // Tạo bản sao của bitmap để xoay (nếu cần)
            Bitmap rotatedBmp = (Bitmap)otherPlayer.BmpObject.Clone();

            //Xoay bản sao dựa vào DirectionTank
            switch (otherPlayer.DirectionTank)
            {
                case Direction.eUp:
                    // Không cần xoay
                    break;
                case Direction.eDown:
                    rotatedBmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case Direction.eLeft:
                    rotatedBmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                case Direction.eRight:
                    rotatedBmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
            }

            // Lấy frame hiện tại dựa vào frx_tank (0-7)
            int currentFrame = otherPlayer.frx_tank;

            // Tính toán xFrame dựa vào skin và hướng di chuyển
            int xFrame = 0;
            switch (otherPlayer.DirectionTank)
            {
                case Direction.eUp:
                    xFrame = (int)otherPlayer.SkinTank * Common.tankSize;
                    break;
                case Direction.eDown:
                    xFrame = (7 - (int)otherPlayer.SkinTank) * Common.tankSize;
                    break;
                case Direction.eLeft:
                    xFrame = currentFrame * Common.tankSize;
                    break;
                case Direction.eRight:
                    xFrame = currentFrame * Common.tankSize;
                    break;
            }

            // Tính toán yFrame dựa vào hướng di chuyển
            int yFrame = 0;
            switch (otherPlayer.DirectionTank)
            {
                case Direction.eUp:
                    yFrame = currentFrame * Common.tankSize;
                    break;
                case Direction.eDown:
                    yFrame = currentFrame * Common.tankSize;
                    break;
                case Direction.eLeft:
                    yFrame = (7 - (int)otherPlayer.SkinTank) * Common.tankSize;
                    break;
                case Direction.eRight:
                    yFrame = (int)otherPlayer.SkinTank * Common.tankSize;
                    break;
            }

            // Vẽ xe tăng của người chơi khác với skin và hướng tương ứng
            Common.PaintObject(this.background, rotatedBmp, (int)otherPlayer.Position.X, (int)otherPlayer.Position.Y,
                               xFrame, yFrame, Common.tankSize, Common.tankSize);

            rotatedBmp.Dispose(); // Giải phóng tài nguyên
        }
        private void RotateTankImage(PlayerTank player)
        {
            switch (player.DirectionTank)
            {
                case Direction.eUp:
                    // Không cần xoay
                    break;
                case Direction.eDown:
                    player.BmpObject.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case Direction.eLeft:
                    player.BmpObject.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                case Direction.eRight:
                    player.BmpObject.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
            }

        }        
        // hàm delay vòng lặp game sau khi game kết thúc


            #endregion Vòng lặp game

            #region sự kiện phím
            // nhấn phím di chuyển 
        private void frmGameMulti_KeyDown(object sender, KeyEventArgs e)
        {
            // biến kiểm tra xem có phải ấn nút di chuyển hay không
            bool isMove_local;

            isMove_local = false;
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.A:
                    playerTank.Left = true;
                    playerTank.Right = playerTank.Up = playerTank.Down = false;
                    isMove_local = true;
                    break;
                case Keys.Right:
                case Keys.D:
                    playerTank.Right = true;
                    playerTank.Left = playerTank.Up = playerTank.Down = false;
                    isMove_local = true;
                    break;
                case Keys.Up:
                case Keys.W:
                    playerTank.Up = true;
                    playerTank.Right = playerTank.Left = playerTank.Down = false;
                    isMove_local = true;
                    break;
                case Keys.Down:
                case Keys.S:
                    playerTank.Down = true;
                    playerTank.Up = playerTank.Right = playerTank.Left = false;
                    isMove_local = true;
                    break;
            }
            if (isMove_local)
            {
                playerTank.IsMove = true;
                playerTank.RotateFrame();
                isMove_local = false;
            }
            e.SuppressKeyPress = true;
        }
        // dừng di chuyển; bắn đạn
        private void frmGameMulti_KeyUp(object sender, KeyEventArgs e)
        {
            bool isMove_local = true;

            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.A:
                    isMove_local = false;
                    playerTank.Left = false;
                    break;
                case Keys.Right:
                case Keys.D:
                    isMove_local = false;
                    playerTank.Right = false;
                    break;
                case Keys.Up:
                case Keys.W:
                    isMove_local = false;
                    playerTank.Up = false;
                    break;
                case Keys.Down:
                case Keys.S:
                    isMove_local = false;
                    playerTank.Down = false;
                    break;
            }
            if (!isMove_local)
            {
                playerTank.IsMove = false;
                isMove_local = true;
            }
            // bắn đạn
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                //playerTank.IsFire = true;
                //Console.WriteLine("Bắn");
                playerTank.CreatBullet(@"\Images\triangleBullet1.png", @"\Images\rocketBullet1.png");
            }
        }
        #endregion sự kiện phím

        #region các hàm xử lí chính
        // game over
        private void GameOver(int scores, int killed)
        {
            // dừng các timer
            tmrGameLoop.Stop();

            // hiển thị điểm và số lượng địch đã tiêu diệt
            lblGameOverScores.Text = scores.ToString();
            lblGameOverTotal.Text = killed.ToString();
            // hiển thị panel GameOver
            pnGameOver.Top = 3;
            pnGameOver.Left = 3;
            pnGameOver.Enabled = true;
            // cập nhật thông tin lên panel
            picGameOverRank.Image = Image.FromFile(String.Format("{0}{1:00}.png",
                Common.path + @"\Images\rank", PlayerInfor.rank));
            lblGameOverLevel.Text = "LEVEL " + level;
        }

        // game next
        private void GameNext(int scores, int killed)
        {
            // dừng các timer
            tmrGameLoop.Stop();

            // hiển thị level hiện tại lên panel thông tin
            lblNextLevelLevel.Text = "LEVEL " + level;
            // tăng level
            this.level++;
            // nếu level đang chơi phá vỡ kỉ lục trước
            if (level > PlayerInfor.level)
            {
                PlayerInfor.rank = PlayerInfor.level;
                PlayerInfor.level++;
            }
            // hiển thị điểm và số lượng địch đã tiêu diệt
            lblNextLevelScores.Text = scores.ToString();
            lblNextLevelTotal.Text = killed.ToString();
            // hiển thị panel NextLevel
            pnNextLevel.Top = 3;
            pnNextLevel.Left = 3;
            pnNextLevel.Enabled = true;
            // hiển thị ảnh rank lên panel thông tin
            picNextLevelRank.Image = Image.FromFile(String.Format("{0}{1:00}.png",
              Common.path + @"\Images\rank", PlayerInfor.rank));

        }

        // game win
        private void GameWin(int scores, int killed)
        {
            // dừng các timer
            tmrGameLoop.Stop();

            // hiển thị panel GameWin
            pnGameWin.Top = 3;
            pnGameWin.Left = 3;
            pnGameWin.Enabled = true;
            // rank cuối cùng 
            PlayerInfor.rank = PlayerInfor.level;
            // hiển thị điểm và số lượng địch đã tiêu diệt
            lblGameWinScores.Text = scores.ToString();
            lblGameWinTotal.Text = killed.ToString();
            // hiển thị level hiện tại lên panel thông tin
            lblGameWinLevel.Text = "LEVEL " + level;
            picGameWinRank.Image = Image.FromFile(String.Format("{0}{1:00}.png",
            Common.path + @"\Images\rank", PlayerInfor.rank));
        }
        #endregion các hàm xử lí chính

        #region xử lí hiển thị lại vật phẩm

        // tìm tọa độ vị trí hiển thị vật phẩm
        private Point SearchLocationItem(int[,] map)
        {
            Random rand = new Random();
            for (int y = rand.Next(4, 37); y >= 2; y--)
                for (int x = rand.Next(4, 42); x >= 2; x--)
                    if (map[y, x] == 0 || map[y, x] == 4)
                    {
                        rand = null;
                        // nếu tìm được vị trí là đường đi hoặc bụi cây thì sẽ là vị trí item
                        return new Point(x * Common.STEP, y * Common.STEP);
                    }
            rand = null;
            return playerTank.Rect.Location;
        }

        // thời gian chờ và xuất hiện của vật phẩm

        // thời gian tác dụng của vật phẩm

        #endregion xử lí hiển thị lại vật phẩm

        #region các hàm sự kiện click_button trong game
        private void lblInfor_MouseEnter(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Color.FromArgb(180, 180, 180);
        }

        private void lblInfor_MouseLeave(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Color.Transparent;
        }

        // bật thuộc tính enable = true các nút level
        private void LabelEnableOn()
        {
            this.lblInforPandP.Enabled = true;
            this.lblInforMenu.Enabled = true;
            this.lblInforExit.Enabled = true;
            this.lblInforChat.Enabled = true;

        }

        // tắt thuộc tính enable = false các nút level
        private void LabelEnableOff()
        {
            this.lblInforPandP.Enabled = false;
            this.lblInforMenu.Enabled = false;
            this.lblInforExit.Enabled = false;
            this.lblInforChat.Enabled = false; 

        }

        // nhấn các nút label trên giao diện
        bool isOnOff = false;
        private void lblInfor_Click(object sender, EventArgs e)
        {
            Label label = ((Label)sender);
            switch (label.Tag.ToString())
            {
                case "pandp":
                    if (isOnOff == false)
                    {
                        tmrGameLoop.Stop();

                        label.Text = "Continue...";
                        label.ForeColor = Color.Orange;
                        isOnOff = true;

                    }
                    else
                    {
                        tmrGameLoop.Start();

                        label.Text = "Pause";
                        label.ForeColor = Color.White;
                        isOnOff = false;

                    }
                    break;
                case "menu":
                    // bật các button level của form menu
                    this.formMenu.ShowOpenedLevels(PlayerInfor.level);
                    // hiển thị form menu
                    this.formMenu.Show();
                    this.Close();
                    break;
                case "chat":
                    if (_chatRoom == null || _chatRoom.IsDisposed)
                    {
                        _chatRoom = new ChatRoom();
                        _chatRoom.FormClosing += ChatRoom_FormClosing;
                    }
                    _chatRoom.Show();
                    break;
                case "exit":
                    Application.Exit();
                    break;
            }
        }

        // sự kiện click button trên giao thông tin game
        private void btn_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Tag.ToString())
            {
                case "tag_menu":
                    // bật các button level của form menu
                    this.formMenu.ShowOpenedLevels(PlayerInfor.level);
                    // hiển thị form menu
                    this.formMenu.Show();
                    this.Close();
                    break;
                case "tag_gamestart":
                    // khởi động lại vòng lặp game
                    this.GameStart();
                    pnGameOver.Location = new Point(3, -900);
                    pnNextLevel.Location = new Point(3, -900);
                    pnGameWin.Location = new Point(3, -900);
                    pnGameOver.Enabled = false;
                    pnNextLevel.Enabled = false;
                    pnGameWin.Enabled = false;
                    break;
            }
        }
        #endregion các hàm sự kiện click_button trong game

        #region các hàm hiển thị thông tin
        // hiển thị số lượng địch phải tiêu diệt bên bản thông tin
        private void ShowNumberEnemyTankDestroy(int n)
        {
            for (int i = 0; i < n; i++)
            {
                picNumberEnemyTanks[i].Image = Image.FromFile(Common.path + @"\Images\icon_enemyTank.png");
            }
        }
        #endregion các hàm hiển thị thông tin

        #region các hàm sự kiện thanh tiêu đề
        private Point titleClickPoint;
        private bool isZoom = false;
        private int w, h;

        // chuot click tieu de
        private void pnTitle_MouseDown(object sender, MouseEventArgs e)
        {
            titleClickPoint.X = MousePosition.X;
            titleClickPoint.Y = MousePosition.Y;
            this.w = e.X;
            this.h = e.Y;
        }
        // di chuyen form
        private void pnTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtons == MouseButtons.Left)
                this.Location = new Point(MousePosition.X - w, MousePosition.Y - h);
        }

        // chuột vào nút thu nhỏ
        private void picMinus_MouseEnter(object sender, EventArgs e)
        {
            this.picMinus.BackColor = Color.Green;
        }
        // chuột rời khỏi nút thu nhỏ
        private void picMinus_MouseLeave(object sender, EventArgs e)
        {
            this.picMinus.BackColor = Color.Transparent;
        }
        // thu nhỏ cửa sổ
        private void picMinus_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        // chuột vào nút phóng to
        private void picPlus_MouseEnter(object sender, EventArgs e)
        {
            this.picPlus.BackColor = Color.Orange;
        }
        // chuột rời khỏi nút phóng to
        private void picPlus_MouseLeave(object sender, EventArgs e)
        {
            this.picPlus.BackColor = Color.Transparent;
        }
        // phóng to cửa sổ
        private void picPlus_Click(object sender, EventArgs e)
        {
            if (isZoom == false)
            {
                this.WindowState = FormWindowState.Maximized;
                isZoom = true;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                isZoom = false;
            }
        }
        // chuột vào nút thoát
        private void picMultiply_MouseEnter(object sender, EventArgs e)
        {
            this.picMultiply.BackColor = Color.Red;
        }
        // chuột rời khỏi nút thoát
        private void picMultiply_MouseLeave(object sender, EventArgs e)
        {
            this.picMultiply.BackColor = Color.Transparent;
        }

        // trước khi thoát game
        private void frmGameMulti_FormClosing(object sender, FormClosingEventArgs e)
        {
            // lưu thông tin level người chơi lại
            PlayerInfor.WritePlayerLevel(@"\PlayerLevel.txt");
        }

        private void pnMainGame_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnTitle_Paint(object sender, PaintEventArgs e)
        {

        }

        // thoát game
        private void picMultiply_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion các hàm sự kiện thanh tiêu đề

      
        private void HandlePlayerPositionUpdated(PlayerTank player)
        {
            // Cập nhật vị trí xe tăng của người chơi khác trên màn hình
            if (player.Name != SocketClient.localPlayer.Name)
            {
                // Tìm xe tăng tương ứng trong danh sách xe tăng của bạn
                // Giả sử bạn có danh sách enemyTankManager.EnemyTanks để quản lý xe tăng địch

            }
        }
        private void HandlePlayerShoot(PlayerTank player, string bulletType, string gunName)
        {

        }
        private void ChatRoom_FormClosing(object sender, FormClosingEventArgs e)
        {
            _chatRoom.Hide();
            e.Cancel = true;
        }

        private void HandleReceiveMessage(string message)
        {

        }

    }
}