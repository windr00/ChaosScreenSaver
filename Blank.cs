using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChaosScreen
{
    public partial class Blank : Form
    {
        public Blank(Rectangle bounds)
        {
            InitializeComponent();
            this.Bounds = bounds;
        }

        private System.Drawing.Point point = new Point();

        private void ScreenSaverForm_MouseMove(object sender, MouseEventArgs e)
        {

            if (!point.IsEmpty)
            {
                if (Math.Abs(e.X - point.X) > 50 || Math.Abs(e.Y - point.Y) > 50)
                {
                    Application.Exit();
                }
            }
            this.point = e.Location;
        }

        private void ScreenSaverForm_Click(object sender, EventArgs e)
        {

            Application.Exit();
        }

        private void ScreenSaverForm_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void ScreenSaverForm_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }

        private void Blank_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }
    }
}
