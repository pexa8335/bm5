using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SuperTank;

namespace SuperTank
{
    public partial class newRoom : Form
    {
        public newRoom()
        {
            InitializeComponent();
            IPAddress ipServer = IPAddress.Loopback;
            IPEndPoint serverEP = new IPEndPoint(ipServer, 8989); // Sử dụng port 8989 như server đã chỉ định
            SocketClient.ConnectToServer(serverEP); // Gọi phương thức static mà không cần tạo đối tượng
            SocketClient.localPlayer = new Objects.PlayerTank { };
        }

        Lobby lobby;

        bool checkRoomID(string idRoom)
        {
            if (!string.IsNullOrEmpty(roomID.Text))
            {
                return true;
            }
            else
            {
                MessageBox.Show("Room ID required!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        void TurnForm()
        {
            lobby = new Lobby();
            this.Hide();
            lobby.Show();
        }

        private async void btn_joinRoom_Click(object sender, EventArgs e)
        {
            if (!checkRoomID(roomID.Text)) return;

            SocketClient.SendData($"JOIN_ROOM;{roomID.Text}");

            await WaitFunction();

            if (SocketClient.isJoinRoom)
            {
                TurnForm();
            }
            else
            {
                MessageBox.Show($"Phòng {roomID.Text} chưa được tạo hoặc đã đủ người hoặc đã bắt đầu chơi!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async Task WaitFunction()
        {
            await Task.Delay(700);
        }

        private void NewRoom_FormClosed(object sender, FormClosedEventArgs e)
        {
            SocketClient.Disconnect();
            SocketClient.ClearLobby();
            
        }

        private void newRoom_Load(object sender, EventArgs e)
        {

        }

        private async void btn_createRoom_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tb_ingameName.Text))
            {
                MessageBox.Show("In-game name is required!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string message = $"CONNECT;{tb_ingameName.Text}"; // Sử dụng ký tự phân tách là ';'
            SocketClient.SendData(message); // Gọi phương thức static mà không cần tạo đối tượng
            SocketClient.localPlayer.Name = tb_ingameName.Text;
            SocketClient.SendData($"CREATE_ROOM;{roomID.Text}");

            await WaitFunction();

            if (SocketClient.isCreateRoom)
            {
                TurnForm();
            }
            else
            {
                MessageBox.Show($"Room {roomID.Text} has been created!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async  void cbb_listRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            roomID.Text = cbb_listRoom.SelectedItem.ToString();
        }

        private async void btn_findRoom_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tb_ingameName.Text))
            {
                MessageBox.Show("In-game name is required!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string message = $"CONNECT;{tb_ingameName.Text}"; // Sử dụng ký tự phân tách là ';'
            SocketClient.SendData(message); // Gọi phương thức static mà không cần tạo đối tượng
            SocketClient.localPlayer.Name = tb_ingameName.Text;
            SocketClient.SendData("SEND_ROOM_LIST");

            await WaitFunction();

            cbb_listRoom.Items.Clear();
            int count = SocketClient.lobbies.Count;
            for (int i = 0; i < count; i++)
            {
                cbb_listRoom.Items.Add(SocketClient.lobbies[i].RoomId);
            }
        }
    }
}
