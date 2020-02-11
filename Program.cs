using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChaosScreen
{
    static class Program
    {

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();
        static void ShowScreenSaver()
        {
            var allScreens = Screen.AllScreens;
            ScreenSaverForm screensaver = null;
            for (int i = 0; i < allScreens.Length; i++)
            {
                if (i == 0)
                {
                    screensaver = new ScreenSaverForm(allScreens[i].Bounds);
                }
                else
                {
                    var black = new Blank(allScreens[i].Bounds);
                    black.Show();
                }
            }
            screensaver.Initialize();
            screensaver.Render();

        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0)
            {
                string firstArgument = args[0].ToLower().Trim();
                string secondArgument = null;

                // Handle cases where arguments are separated by colon.
                // Examples: /c:1234567 or /P:1234567
                if (firstArgument.Length > 2)
                {
                    secondArgument = firstArgument.Substring(3).Trim();
                    firstArgument = firstArgument.Substring(0, 2);
                }
                else if (args.Length > 1)
                    secondArgument = args[1];

                if (firstArgument == "/c")           // Configuration mode
                {
                    Application.Run(new About());
                }
                else if (firstArgument == "/p")      // Preview mode
                {
                    if (secondArgument == null)
                    {
                        MessageBox.Show("Sorry, but the expected window handle was not provided.",
                            "ScreenSaver", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    IntPtr previewWndHandle = new IntPtr(long.Parse(secondArgument));
                    var form = new ScreenSaverForm(previewWndHandle);
                }
                else if (firstArgument == "/s")      // Full-screen mode
                {
                    ShowScreenSaver();
                }
                else    // Undefined argument
                {
                    MessageBox.Show("Sorry, but the command line argument \"" + firstArgument +
                        "\" is not valid.", "ScreenSaver",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else    // No arguments - treat like /c
            {
                //var screen = new ScreenSaverForm(GetDesktopWindow());
                //screen.Initialize();
                //screen.Render();
                //Application.Run(new About());
                ShowScreenSaver();
            }

        }
    }
}
