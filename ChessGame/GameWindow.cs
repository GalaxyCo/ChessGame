using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChessGame
{
    public partial class GameWindow : Form
    {
        public GameWindow()
        {
            InitializeComponent();
        }

        private void statusScrollLeftBtn_Click(object sender, EventArgs e)
        {
            GameManager.scrollStatus(1);
        }

        private void statusScrollRightBtn_Click(object sender, EventArgs e)
        {
            GameManager.scrollStatus(-1);
        }

        private void shownEvent(object sender, EventArgs e)
        {
            GameManager.tryAIMove();
        }
    }
}
