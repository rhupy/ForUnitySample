using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace ForUnitySample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            WhenItStopsDoThis(null, null);
            Application.ApplicationExit += new EventHandler(this.WhenItStopsDoThis);
            UnityEXE(this.panel1, @$"{System.Windows.Forms.Application.StartupPath}\UnityResource\ZRun_LINE_QUE_URP\LINE_QUE.exe", true, true);
        }
        private void WhenItStopsDoThis(object sender, EventArgs e)
        {
            try
            {
                Process[] processlist = Process.GetProcessesByName("LINE_QUE");
                for (int i = 0; i < processlist.Length; i++)
                    processlist[i].Kill();
            }
            catch { }
        }
        #region for Unity / Handle
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern IntPtr FromHandle(IntPtr hWnd);

        [DllImport("User32.dll")]
        private static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);
        [DllImport("user32.dll")]
        internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private IntPtr unityHWND = IntPtr.Zero;

        private const int WM_ACTIVATE = 0x0006;
        private readonly IntPtr WA_ACTIVE = new IntPtr(1);
        private readonly IntPtr WA_INACTIVE = new IntPtr(0);

        // WinForm Send
        static int PORT_SOCKET_FORM_SEND = 0;
        static UdpClient form_udp_Client;

        // Unity Listen
        static int PORT_SOCKET_UNITY_LISTEN = 0;
        IPEndPoint unity_udp_Listen_IpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_SOCKET_UNITY_LISTEN);

        public async Task UnityEXE(Panel panel, string path, bool newclient, bool fullSize)
        {
            if (newclient)
                if (form_udp_Client == null)
                    form_udp_Client = new UdpClient(PORT_SOCKET_FORM_SEND);

            ActivateUnityWindow();

            Process process = new Process();
            process.StartInfo.FileName = path;
            process.StartInfo.Arguments = "-parentHWND " +
            panel.Handle.ToInt32() + " " + Environment.CommandLine;
            process.StartInfo.Arguments = "-parentHWND " + panel.Handle.ToInt32()
                + " -screen-width " + panel.Width.ToString()
                + " -screen-height " + panel.Height.ToString()
                + " -screen-fullscreen";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            process.WaitForInputIdle();
            unityHWND = process.Handle;

            EnumChildWindows(panel.Handle, WindowEnum, IntPtr.Zero);
            process.Exited += new EventHandler(this.ExitUnityProcess);
            if(fullSize) panel.Resize += new EventHandler(this.UnityContainer_Resize);
        }
        private void ActivateUnityWindow()
        {
            SendMessage(unityHWND, WM_ACTIVATE, WA_ACTIVE, IntPtr.Zero);
        }
        private void DeactivateUnityWindow()
        {
            SendMessage(unityHWND, WM_ACTIVATE, WA_INACTIVE, IntPtr.Zero);
        }
        private int WindowEnum(IntPtr hwnd, IntPtr lparam)
        {
            unityHWND = hwnd;
            ActivateUnityWindow();
            return 0;
        }
        private void ExitUnityProcess(object sender, EventArgs e)
        {
            //ClosePort();
        }
        private void UnityContainer_Resize(object sender, EventArgs e)
        {
            MoveWindow(unityHWND, 0, 0, this.panel1.Width, this.panel1.Height, true);
            ActivateUnityWindow();
        }
        #endregion
    }
}