using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;

namespace ShinKen
{
    public partial class ShinKen : Form
    {
        #region Parameter
        private KeyboardHook k_hook = new KeyboardHook();
        //Wiki wiki;
        int WidthOfBar, HeightOfTitle;
        Bitmap btm;
        Timer time1 = new Timer();
        Timer time2 = new Timer();
        Timer time3 = new Timer();
        int top = 0;
        int left = 0;
        int ticking = 0;
        int timetick = 0;
        public int width = Screen.PrimaryScreen.Bounds.Width;
        public int height = Screen.PrimaryScreen.Bounds.Height;
        private int zoom = 0;
        public bool WikiActive = false;
        public bool MouseStart = false;
        int ticker = 0;

        #endregion
        
        #region Disable TAB in webbrowser
        // Structure contain information about low-level keyboard input event
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }
        //System level functions to be used for hook and unhook keyboard input
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);

        //Declaring Global objects
        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        private IntPtr captureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));
                if ((objKeyInfo.key == Keys.Tab)&& IsTopActive(this.Handle)) // Disabling Windows keys
                {
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }
        #endregion
        
        #region Capture ClientScreen Picture
        private void Snapshot(Rectangle range)
        {
            Bitmap b = new Bitmap(Screen.AllScreens[0].Bounds.Size.Width, Screen.AllScreens[0].Bounds.Size.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(0, 0, 0, 0, Screen.AllScreens[0].Bounds.Size);
                g.Dispose();
                //this.PIC.pictureBox1.Image = CutImage(btm,range);
                btm = CutImage(b, range);
                b.Dispose();
                //btm.Save("C:\\1.jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        Bitmap CutImage(Image img, Rectangle rect)
        {
            Bitmap b = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(b);
            g.DrawImage(img, 0, 0, rect, GraphicsUnit.Pixel);
            g.Dispose();
            return b;
        }
        #endregion

        #region IsWindowTopActive
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        public bool IsTopActive(IntPtr handle)
        {
            IntPtr activeHandle = GetForegroundWindow();
            return (activeHandle == handle);
        }
        #endregion

        #region Mouse Click
        [DllImport("user32.dll")]
        static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);
        [DllImport("user32.dll")]
        private static extern int SetCursorPos(int x, int y);
        [Flags]
        enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }
        private void Mouse_Click(int x, int y)
        {
            int mousex = Control.MousePosition.X;
            int mousey = Control.MousePosition.Y;
            SetCursorPos(this.Left + x, this.Top + y);
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
            SetCursorPos(mousex, mousey);

        }
        private void Mouse_Click(Point pos)
        {
            int mousex = Control.MousePosition.X;
            int mousey = Control.MousePosition.Y;
            SetCursorPos(this.Left + pos.X, this.Top + pos.Y);
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
            SetCursorPos(mousex, mousey);

        }
        #endregion

        #region Clear_Memory
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        /// <summary>
        /// 释放内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        #endregion

        private void StartTime1()
        {
            this.time1 = new Timer();
            this.time1.Interval = 300;
            this.time1.Tick += new System.EventHandler(this.time1_Tick);
            time1.Start();
        }

        public ShinKen()
        {          
                        
            InitializeComponent();
            this.webBrowser1.Navigate("http://www.dmm.com/netgame_s/shinken/");
            //this.webBrowser1.Navigate("http://www.dmm.com/netgame/social/-/gadgets/=/app_id=319803/");

            WidthOfBar = Convert.ToInt32((this.Size.Width - this.ClientRectangle.Width) / 2);
            HeightOfTitle = this.Height - this.ClientRectangle.Height - WidthOfBar;

            this.webBrowser1.ScrollBarsEnabled = false;
            this.webBrowser1.ScriptErrorsSuppressed = true;

            this.MaximizeBox = false;//使最大化窗口失效

            //wiki = new Wiki();

            this.MouseWheel += new MouseEventHandler(ShinKen_MouseWheel);

            k_hook.KeyDownEvent += new KeyEventHandler(hook_KeyDown);//Key Pressed Hook
            k_hook.Start();
            //this.richTextBox1.Hide();
            
            /*
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule; //Get Current Module
            objKeyboardProcess = new LowLevelKeyboardProc(captureKey); //Assign callback function each time keyboard process
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0); //Setting Hook of Keyboard Process for current module*/

            //this.webBrowser1.Location=new Point(-10000,1);
        }

        private void ShinKen_Refresh()
        {
            webBrowser1.Navigate("http://www.dmm.com/netgame/social/-/gadgets/=/app_id=319803/");
        }

        private void ShinKen_MouseEnter(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (zoom == 0)
            {
                this.Size = new Size(960 + 2 * WidthOfBar, 640 + WidthOfBar + HeightOfTitle);
            }
            if (zoom == 1)
            {
                this.Size = new Size(576 + 2 * WidthOfBar, 384 + WidthOfBar + HeightOfTitle);
            }
            if (zoom == 2)
            {
                this.Size = new Size(288 + 2 * WidthOfBar, 192 + WidthOfBar + HeightOfTitle);
            }
            StartTime1();

        }

        private void ShinKen_Load(object sender, EventArgs e)
        {
            
            this.time3.Interval = 10000;
            this.time3.Tick += new System.EventHandler(this.time3_Tick);
            time3.Start();
            if (MouseStart)
            {
                time2.Start();
                this.Text = ticking.ToString();
            }
        }

        private void time3_Tick(object sender, EventArgs e)
        {
            ticker++;
            ClearMemory();
            if (ticker == 15)
            {
                ticker = 0;
                this.time3.Dispose();
                this.time3 = new Timer();
                this.time3.Interval = 10000;
                this.time3.Tick += new System.EventHandler(this.time3_Tick);
                this.time3.Start();

            }
        }

        /// <summary>
        /// Detect the Rectangle Of Flash
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void time1_Tick(object sender, EventArgs e)
        {
            ticking++;
            //this.richTextBox1.Text = ticking.ToString();
            int i;
            int zoomrateLoca = 400;
            if (zoom == 1) zoomrateLoca = 240;
            if (zoom == 2) zoomrateLoca = 120;
            /*
            if ((top == 0) && (ticking > (500 / this.time1.Interval)))
            {
                Snapshot(new Rectangle(this.Left + WidthOfBar, this.Top + HeightOfTitle, this.ClientRectangle.Width, this.ClientRectangle.Height));
                if (this.WindowState == FormWindowState.Minimized) return;
                for (i = 2; i <= this.ClientRectangle.Height / 2; i++)
                {
                    if ((btm.GetPixel(zoomrateLoca, i).R - btm.GetPixel(zoomrateLoca, i + 1).R > 50)
                        && (btm.GetPixel(zoomrateLoca, i).R - btm.GetPixel(zoomrateLoca, i + 7).R > 50)
                        && (btm.GetPixel(zoomrateLoca, i).R - btm.GetPixel(zoomrateLoca, i + 17).R > 50)
                        && (btm.GetPixel(zoomrateLoca-40, i).R - btm.GetPixel(zoomrateLoca-40, i + 1).R > 50))
                    {
                        top = i + 1;
                        break;
                    }
                }
                if (top != 0)
                {
                    for (i = 2; i <= this.ClientRectangle.Width; i++)
                        if (btm.GetPixel(i, top+50).R - btm.GetPixel(i + 1, top+50).R > 50)
                        {
                            left = i + 1;
                            break;
                        }
                }
                //this.richTextBox1.Text = top.ToString() + "," + left.ToString();
                this.webBrowser1.Location = new Point(0 - left, 0 - top);
                //this.webBrowser1.Document.Window.ScrollTo(left, top);
                ticking = 0;
                time1.Stop();
            }*/
            if ((left == 0) && (ticking > (500 / this.time1.Interval)))
            {
                Snapshot(new Rectangle(this.Left + WidthOfBar, this.Top + HeightOfTitle, this.ClientRectangle.Width, this.ClientRectangle.Height));
                if (this.WindowState == FormWindowState.Minimized) return;
                if(zoom==0) top = 60;
                if (zoom == 1) top = 36;
                if (zoom == 2) top = 18;
                for (i = 12; i <= this.ClientRectangle.Width-2; i++)
                    if ((btm.GetPixel(i, top + 10).R - btm.GetPixel(i + 1, top + 10).R > 150)
                        &&(btm.GetPixel(i, top + 20).R - btm.GetPixel(i + 1, top + 20).R > 150)
                        &&(btm.GetPixel(i, top + 30).R - btm.GetPixel(i + 1, top + 30).R > 150)
                        && (btm.GetPixel(i, top + 10).R - btm.GetPixel(i + 15, top + 10).R > 150))

                    {
                        left = i + 1;
                        break;
                    }
                if (left == 0) top = 0;
                this.webBrowser1.Location = new Point(0 - left, 0 - top);
                ticking = 0;
                time1.Dispose();
            }
            //this.richTextBox1.Text = WidthOfBar.ToString() + "," + HeightOfTitle.ToString() + "," + this.Left.ToString() + "," + this.Top.ToString() ;
            #region test
            /*
            ticking++;
            //this.richTextBox1.Text = ticking.ToString();
            if (ticking == 5)
            {
                int height = this.webBrowser1.Document.Body.ScrollRectangle.Height;
                int width = this.webBrowser1.Document.Body.ScrollRectangle.Width;


                this.webBrowser1.Height = height;
                this.webBrowser1.Width = width;

                int count, i, j;
                if (top == 0)
                {
                    Bitmap bitmap = new Bitmap(width, height);  // 创建高度和宽度与网页相同的图片
                    Rectangle rectangle = new Rectangle(0, 0, width, height);  // 绘图区域
                    this.webBrowser1.DrawToBitmap(bitmap, rectangle);  // 截图 
                    bitmap.Save("c:/" + ticking.ToString() + ".jpg");

                    for (i = 2; i <= height - 2; i++)
                    {
                        count = 0;
                        for (j = 2; j <= width - 1; j++)
                            if (bitmap.GetPixel(j, i).R - bitmap.GetPixel(j, i + 1).R > 150)
                                count++;
                        if (count > 400)
                        {
                            top = i + 1;
                            break;
                        }
                    }
                    for (i = 2; i <= width - 2; i++)
                    {
                        count = 0;
                        for (j = 2; j <= height - 1; j++)
                            if (bitmap.GetPixel(i, j).R - bitmap.GetPixel(i + 1, j).R > 150)
                                count++;
                        if (count > 400)
                        {
                            left = i + 1;
                            break;
                        }
                    }
                    if (top > 600) top = 0;
                    //this.webBrowser1.Document.Window.ScrollTo(left, top);

                }

                this.richTextBox1.Text = height.ToString() + "," + top.ToString() + "," + left.ToString();
                //System.Windows.Forms.MessageBox.Show("研磨してください！");
            }
             * */
            #endregion
            return;
        }


        private int getValue(int x,double y)
        {
            int t;
            double p = x * y;
            t = (int)p;
            while (t + 1 < p) t++;
            return t;
        }
        private void hook_KeyDown(object sender, KeyEventArgs e)
        {
            #region Refresh the Page
            if (this.IsTopActive(this.Handle) && e.KeyValue == (int)Keys.F5)
            {
                this.webBrowser1.Document.Window.ScrollTo(left, top);
                this.ShinKen_Refresh();
            }            

            //if (this.IsTopActive(wiki.Handle) && e.KeyValue == (int)Keys.F5)
            //{
             //   wiki.Wiki_Refresh();
             //   top = 0;
           // }
            #endregion

            if (e.KeyValue == (int)Keys.F8)
            {
                if (this.IsTopActive(this.Handle))
                {
                    zoom++;
                    if (zoom == 3) zoom = 0;
                    if (zoom == 0)
                    {
                        webBrowser1.Document.Body.Style = "zoom:1.0";
                        //this.webBrowser1.Document.Window.ScrollTo(left, top);
                        this.webBrowser1.Location = new Point(0, 0);
                        top = 0;
                        left = 0;
                        StartTime1();
                        this.Height = (int)(640 * 1)+HeightOfTitle+WidthOfBar;
                        this.Width = (int)(960 * 1)+2*WidthOfBar;
                        this.Show();
                    }
                    else
                        if (zoom == 1)
                        {
                            webBrowser1.Document.Body.Style = "zoom:0.6";
                            //this.webBrowser1.Document.Window.ScrollTo(getValue(left,0.6), getValue(top,0.6));
                            this.webBrowser1.Location = new Point(0, 0);
                            top = 0;
                            left = 0;
                            StartTime1();
                            this.Height = (int)(640 * 0.6) + HeightOfTitle + WidthOfBar;
                            this.Width = (int)(960 * 0.6)+2 * WidthOfBar;
                            this.Show();
                            //wiki.Hide();
                        }
                        else
                        {
                            webBrowser1.Document.Body.Style = "zoom:0.3";
                            //this.webBrowser1.Document.Window.ScrollTo(left, top);
                            this.webBrowser1.Location = new Point(0, 0);
                            top = 0;
                            left = 0;
                            StartTime1();
                            this.Height = (int)(640 * 0.3) + HeightOfTitle + WidthOfBar;
                            this.Width = (int)(960 * 0.3) + 2 * WidthOfBar;
                            this.Show();
                           // wiki.Hide();
                        }
                }
                else
                {
                    this.Show();
                }
            }

            if (e.KeyValue == (int)Keys.Escape)
            {
                if (this.IsTopActive(this.Handle))
                {
                    //wiki.Hide();
                    this.Hide();                    
                }
                //if (this.IsTopActive(wiki.Handle))
               // {
               //     wiki.Hide();
               // }
            }

            #region continously mouse clicking
            if (e.KeyValue == (int)Keys.F9)
            {
                MouseStart = !MouseStart;
                if (MouseStart)
                {
                    this.time2 = new Timer();
                    this.time2.Interval = 1;
                    this.time2.Enabled = true;
                    this.time2.Tick += new System.EventHandler(this.time2_Tick);
                    //this.time2.Start();
                }
                else
                {
                    time2.Stop();
                    time2.Dispose();
                }

            }
            #endregion
            /*
            #region open wiki
            if ((e.KeyValue == (int)Keys.W) && this.IsTopActive(this.Handle))
            {
                if (WikiActive && wiki.Visible)
                {
                    wiki.Hide();
                    WikiActive = false;
                    return;
                }

                if (this.left + this.Right < width)
                {
                    wiki.Left = this.Right;
                    wiki.Top = this.Top;
                    wiki.Show();
                    this.Focus();
                    WikiActive = true;
                }
                else
                {
                    wiki.Left = this.Left - wiki.Width;
                    wiki.Top = this.Top;
                    wiki.Show();
                    this.Focus();
                    WikiActive = true;
                }

            }
            #endregion
            */
            #region ScreenShot
            if ((e.KeyValue == (int)Keys.P) && this.IsTopActive(this.Handle))
            {
                Snapshot(new Rectangle(this.Left + WidthOfBar, this.Top + HeightOfTitle, this.ClientRectangle.Width, this.ClientRectangle.Height));
                if (Directory.Exists(@"ScreenShot") == false)
                {
                    Directory.CreateDirectory(@"ScreenShot");
                }
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(@"ScreenShot");
                int fileNum= dir.GetFiles().Length;
                /*
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JPEG (*.jpg)|*.jpg|PNG (*.png)|*.png";
                saveFileDialog.ShowDialog();
                saveFileDialog.FileName = @"/pic/"+(fileNum+1).ToString();
                btm.Save(saveFileDialog.FileName);
                */
                btm.Save(@"ScreenShot/" + (fileNum + 1).ToString() + ".jpeg");
               // btm.Save(@"1.jpg");
            }
            #endregion

            if ((e.KeyValue == (int)Keys.T) && this.IsTopActive(this.Handle))
            {
                this.TopMost = !this.TopMost;
            }
        }

        private void time2_Tick(object sender, EventArgs e)
        {
            ticking++;
            if (!this.IsTopActive(this.Handle)) return;
            if (MouseStart)
            {
                //SetCursorPos(Control.MousePosition.X, Control.MousePosition.Y);
                for (int i = 0; i <= 0; i++)
                {
                    mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                    mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                }
            }
        }

        /// <summary>
        /// MouseWheel---->Scroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShinKen_MouseWheel(object sender, MouseEventArgs e)
        {
            int mousex = Control.MousePosition.X;
            int mousey = Control.MousePosition.Y;
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            if (e.Delta > 0)
            {
                Mouse_Wheel(45, 0);
            }
            else
            {
                Mouse_Wheel(-45, 0);
            }
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
            SetCursorPos(mousex, mousey);
        }
        private void Mouse_Wheel(int x, int y)
        {
            mouse_event(MouseEventFlag.Move, x, 1, 0, UIntPtr.Zero);
        }


        private void ShinKen_Move(object sender, EventArgs e)
        {
            if (this.left + this.Right < width)
            {
               // wiki.Left = this.Right;
               // wiki.Top = this.Top;
            }
            else
            {
               // wiki.Left = this.Left-wiki.Width;
               // wiki.Top = this.Top;
            }
        }


        private void ShinKen_FormClosing(object sender, FormClosingEventArgs e)
        {
            k_hook.Stop();
            
            //wiki.Dispose();
        }

        private void ShinKen_Click(object sender, MouseEventArgs e)
        {
        }

        
    }
}
