using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShinKen
{
    public partial class Wiki : Form
    {
        public int X, Y;
        public bool active = false;
        private int PageNo = 0;
        string[] url = new string[2];

        public Wiki()
        {
            InitializeComponent();
            this.webBrowser1.ScriptErrorsSuppressed = false;
            url[0] = "http://wikiwiki.jp/sinken/";
            url[1] = "http://shinkenkr.com/";
            webBrowser1.Navigate(url[0]);
            this.Hide();
        }

        public void Wiki_Refresh()
        {
            webBrowser1.Navigate(url[PageNo]);
        }

        private void Wiki_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Hide();
            this.active = false;
        }

        private void Wiki_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
            //this.Hide();
        }

        private void Wiki_Click(object sender, EventArgs e)
        {
            if (PageNo == 1)
            {
                webBrowser1.Navigate(url[0]);
                PageNo = 0;
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (PageNo == 0)
            {
                webBrowser1.Navigate(url[1]);
                PageNo = 1;
            }
        }

        private void Wiki_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                webBrowser1.GoBack();
            }
        }
    }
}
