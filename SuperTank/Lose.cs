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
    public partial class Lose : Form
    {
        private frmGameMulti _gameForm;
        public Lose()
        {
            InitializeComponent();
        }
        public Lose(frmGameMulti gameForm)
        {
            InitializeComponent();
            _gameForm = gameForm;
        }



        private void Lose_Load(object sender, EventArgs e)
        {
            if (_gameForm != null)
            {
                _gameForm.Close();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
