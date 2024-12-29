using SuperTank.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperTank
{
    public partial class Lobby : Form
    {
        private CancellationTokenSource _cts;
        private bool IsHost; // Add this line to declare the IsHost variable
        private bool isFormCreated = false;

        public ChatRoom _chatRoom;

        public Lobby()
        {
            InitializeComponent();
            namePlayer1.AutoEllipsis = true;
            namePlayer2.AutoEllipsis = true;
            namePlayer3.AutoEllipsis = true;
            namePlayer4.AutoEllipsis = true;
            this.Load += Lobby_Load;



            // Start ChatRoom form in background
            _chatRoom = new ChatRoom();
            _chatRoom.FormClosing += ChatRoom_FormClosing;
            _chatRoom.Show();
            _chatRoom.Hide();
        }

        private async void Lobby_Load(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            await RunContinuouslyAsync(_cts.Token);
        }
        private void Lobby_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();
            SocketClient.Disconnect();
            SocketClient.ClearLobby();
            //Login login = new Login();
            //login.Show();
        }
        private async Task RunContinuouslyAsync(CancellationToken token)
        {
            SocketClient.SendData($"SEND_LOBBY;{SocketClient.joinedRoom}");
            await Task.Delay(100, token);

            while (!token.IsCancellationRequested)
            {
                InitLobby();

                if (SocketClient.isStartGame && !isFormCreated)
                {
                    isFormCreated = true;
                    this.Invoke((MethodInvoker)delegate
                    {
                        frmGameMulti multi = new frmGameMulti();
                        multi.Show();
                        this.Hide();
                    });
                    return; // Thoát khỏi hàm sau khi đã chuyển form
                }
                else
                {
                    // Chỉ gửi SEND_LOBBY nếu game chưa bắt đầu
                    if (!SocketClient.isStartGame)
                        SocketClient.SendData($"SEND_LOBBY;{SocketClient.joinedRoom}");
                }

                try
                {
                    await Task.Delay(500, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private void InitLobby()
        {
            try
            {
                    if (SocketClient.joinedLobby.Host != null &&
                        SocketClient.localPlayer != null && SocketClient.localPlayer.Name == SocketClient.joinedLobby.Host.Name)
                    {
                        btn_Start.Enabled = true;
                        btn_Start.Visible = true;
                    }
                    else
                    {
                        btn_Start.Enabled = false;
                        btn_Start.Visible = false;
                    }

                    //string[] playersName = new string[4];
                    if (SocketClient.joinedLobby.PlayersName != null)
                    {
                        lb_Total.Text = "Total: " + SocketClient.joinedLobby.PlayersName.Count.ToString();
                    }
                    else
                    {
                        // Xử lý trường hợp PlayersName == null
                        lb_Total.Text = "Total: 0"; // Hoặc thông báo lỗi
                    }

                    lb_roomID.Text = "Room ID: " + SocketClient.joinedLobby.RoomId;
                    int countPlayer = SocketClient.joinedLobby.PlayersName.Count;

                    for (int i = 0; i < countPlayer; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                namePlayer1.Text = SocketClient.joinedLobby.PlayersName[i];
                                //ptb_player1.Image = Properties.Resources.ares;
                                lbReady1.Visible = true;
                                if (SocketClient.CheckIsReady(namePlayer1.Text))
                                {
                                    lbReady1.Text = "Ready";
                                    lbReady1.ForeColor = Color.Lime;
                                }
                                break;

                            case 1:
                                namePlayer2.Text = SocketClient.joinedLobby.PlayersName[i];
                                // ptb_player2.Image = Properties.Resources.knight;
                                lbReady2.Visible = true;
                                if (SocketClient.CheckIsReady(namePlayer2.Text))
                                {
                                    lbReady2.Text = "Ready";
                                    lbReady2.ForeColor = Color.Lime;
                                }
                                break;

                            case 2:
                                namePlayer3.Text = SocketClient.joinedLobby.PlayersName[i];
                                //ptb_player3.Image = Properties.Resources.serial_killer;
                                lbReady3.Visible = true;
                                if (SocketClient.CheckIsReady(namePlayer3.Text))
                                {
                                    lbReady3.Text = "Ready";
                                    lbReady3.ForeColor = Color.Lime;
                                }
                                break;

                            case 3:
                                namePlayer4.Text = SocketClient.joinedLobby.PlayersName[i];
                                //ptb_player4.Image = Properties.Resources.player1;
                                lbReady4.Visible = true;
                                if (SocketClient.CheckIsReady(namePlayer4.Text))
                                {
                                    lbReady4.Text = "Ready";
                                    lbReady4.ForeColor = Color.Lime;
                                }
                                break;
                        }
                    }
                    for (int i = countPlayer; i < 4; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                namePlayer1.Text = "Player1";
                                //ptb_player1.Image = Properties.Resources.anonymous;
                                lbReady1.Visible = false;
                                lbReady1.Text = "Not ready";
                                lbReady1.ForeColor = Color.Red;
                                break;

                            case 1:
                                namePlayer2.Text = "Player2";
                                //ptb_player2.Image = Properties.Resources.anonymous;
                                lbReady2.Visible = false;
                                lbReady2.Text = "Not ready";
                                lbReady2.ForeColor = Color.Red;
                                break;

                            case 2:
                                namePlayer3.Text = "Player3";
                                //ptb_player3.Image = Properties.Resources.anonymous;
                                lbReady3.Visible = false;
                                lbReady3.Text = "Not ready";
                                lbReady3.ForeColor = Color.Red;
                                break;

                            case 3:
                                namePlayer4.Text = "Player4";
                                // ptb_player4.Image = Properties.Resources.anonymous;
                                lbReady4.Visible = false;
                                lbReady4.Text = "Not ready";
                                lbReady4.ForeColor = Color.Red;
                                break;
                        }
                    }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        private void ptb_player1_Click(object sender, EventArgs e)
        {

        }

        private void btn_Ready_Click(object sender, EventArgs e)
        {
            SocketClient.SendData("READY");
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            btn_Start.Enabled = false; // Vô hiệu hóa nút ngay khi nhấn

            if (SocketClient.CheckIsReadyForAll())
            {
                SocketClient.SendData("START");
                // Không cần làm gì thêm ở đây vì tất cả client sẽ nhận được thông báo START và xử lý trong RunContinuouslyAsync
            }
            else
            {
                MessageBox.Show("Các người chơi khác vẫn chưa sẵn sàng!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btn_Start.Enabled = true; // Kích hoạt lại nút nếu điều kiện không thỏa mãn
            }
        }
        private void btn_Chat_Click(object sender, EventArgs e)
        {
            if (_chatRoom == null || _chatRoom.IsDisposed)
            {
                _chatRoom = new ChatRoom();
                _chatRoom.FormClosing += ChatRoom_FormClosing;
            }
            _chatRoom.Show();
        }
        private void ChatRoom_FormClosing(object sender, FormClosingEventArgs e)
        {
            _chatRoom.Hide();
            //e.Cancel = true;
        }
    }
}
