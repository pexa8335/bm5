using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperTank
{
    public partial class ChatRoom : Form
    {
        public ChatRoom()
        {
            InitializeComponent();
            SocketClient.OnReceiveMessage += UpdateMessage;
            messageBox.KeyDown += new KeyEventHandler(messageBox_KeyDown); // Subscribe to KeyDown event
        }

        private void UpdateMessage(string message)
        {
            if (showMessage.InvokeRequired)
            {
                showMessage.Invoke(new Action(() => UpdateMessage(message)));
            }
            else
            {
                showMessage.Items.Add(message.ToString());
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(messageBox.Text)) return;

            string message = String.Format($"[{SocketClient.localPlayer.Name}]: {messageBox.Text}");
            messageBox.Clear();
            showMessage.Items.Add(message);

            SocketClient.SendData($"MESSAGE;{message}");
        }

        private void messageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sendButton_Click(this, new EventArgs()); // Call the send button click event handler
                e.SuppressKeyPress = true; // Prevent the beep sound on Enter key press
            }
        }
    }
}
