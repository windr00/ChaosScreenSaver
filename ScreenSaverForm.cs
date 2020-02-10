using SharpDX.Windows;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace ChaosScreen
{
    public partial class ScreenSaverForm
    {


        public RenderForm form;

        private void createForm(System.Drawing.Rectangle bounds)
        {
            form = new RenderForm("Chaos Screen Saver");
            form.FormBorderStyle = FormBorderStyle.None;
            form.MouseMove += this.ScreenSaverForm_MouseMove;
            form.MouseClick += this.ScreenSaverForm_Click;
            form.KeyDown += this.ScreenSaverForm_KeyDown;
            form.KeyPress += this.ScreenSaverForm_KeyPress;
            form.Bounds = bounds;
            Cursor.Hide();
            form.TopMost = true;


        }




        public ScreenSaverForm(System.Drawing.Rectangle bounds)
        {
            this.createForm(bounds);

        }

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out System.Drawing.Rectangle lpRect);

        private System.Drawing.Point point = new System.Drawing.Point();

        private bool previewMode = false;
        public ScreenSaverForm(IntPtr PreviewWndHandle)
        {
            System.Drawing.Rectangle ParentRect;
            GetClientRect(PreviewWndHandle, out ParentRect);
            this.createForm(ParentRect);

            // Set the preview window as the parent of this window
            SetParent(form.Handle, PreviewWndHandle);

            // Make this a child window so it will close when the parent dialog closes
            // GWL_STYLE = -16, WS_CHILD = 0x40000000
            SetWindowLong(form.Handle, -16, new IntPtr(GetWindowLong(form.Handle, -16) | 0x40000000));

            // Place our window inside the parent
            form.Size = ParentRect.Size;
            form.Location = new System.Drawing.Point(0, 0);

            this.previewMode = true;
        }

        private void ScreenSaverForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (previewMode)
            {
                return;
            }
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
            if (previewMode)
            {
                return;
            }
            Application.Exit();
        }

        private void ScreenSaverForm_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void ScreenSaverForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (previewMode)
            {
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }
    }
}
