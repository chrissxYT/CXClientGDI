using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CXClientGDI
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int l, t, r, b;
        }

        string api_dir = @"D:\Bibliotheken\Desktop\cxclient\jars\cxclient_api";
        string mods_dir = @"D:\Bibliotheken\Desktop\cxclient\jars\cxclient_api\mods";
        int fnt_size = 14;

        public Form1()
        {
            InitializeComponent();
            SetWindowLong(Handle, -20, GetWindowLong(Handle, -20) | 0x80000 | 0x20);
            IntPtr ip = FindWindow(null, "CXClient | Minecraft 1.8.8");
            RECT r;
            GetWindowRect(ip, out r);
            Size = new Size(r.r - r.l, r.b - r.t);
            Top = r.t;
            Left = r.l;
            PrivateFontCollection collection = new PrivateFontCollection();
            collection.AddFontFile("Minecraft.ttf");
            Font = new Font(new FontFamily("Minecraft", collection), fnt_size);
            GC.SuppressFinalize(collection);
            File.Create(api_dir + "\\disable_iaui").Close();
        }

        Dictionary<string, bool> parse_enabled()
        {
            Dictionary<string, bool> mods = new Dictionary<string, bool>();
            foreach (byte[] b in split(File.ReadAllBytes(mods_dir + "\\enabled"), 11))
                mods.Add(Encoding.ASCII.GetString(b, 0, b.Length - 1), b[b.Length - 1] == 1);
            return mods;
        }

        byte[][] split(byte[] bytes, byte separator)
        {
            List<byte[]> bs = new List<byte[]>();
            List<byte> b = new List<byte>();
            foreach (byte by in bytes)
                if (by != separator)
                    b.Add(by);
                else
                {
                    bs.Add(b.ToArray());
                    b = new List<byte>();
                }
            return bs.ToArray();
        }

        void Form1_Paint(object sdr, PaintEventArgs pea)
        {
            try
            {
                try
                {
                    Graphics g = pea.Graphics;
                    int y = 50;
                    g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                    foreach (KeyValuePair<string, bool> mod in parse_enabled())
                    {
                        if (mod.Value)
                        {
                            g.DrawString(mod.Key, Font, Brushes.White, pea.ClipRectangle.X + 10, pea.ClipRectangle.Y + y);
                            y += fnt_size;
                            File.AppendAllText("log", DateTime.Now.ToString() + $" Drew {mod.Key}\r\n");
                        }
                    }
                    File.AppendAllText("log", DateTime.Now.ToString() + " Drew screen\r\n");
                }
                catch (Exception e)
                {
                    try
                    {
                        File.AppendAllText("log", DateTime.Now.ToString() + " " + e.ToString() + "\r\n");
                    }
                    catch (Exception e1)
                    {
                        MessageBox.Show("Exception trying to write exception:\n" + e1.ToString());
                    }
                }
                Thread.Sleep(200);
                Invalidate();
            }
            catch (Exception e)
            {
                MessageBox.Show("Some error in the outest try-scope of the paint-func:\n" + e.ToString());
                try
                {
                    Invalidate();
                }
                catch (Exception e1)
                {
                    MessageBox.Show("Cannot invalidate:\n" + e1.ToString());
                }
            }
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            File.Delete(api_dir + "\\disable_iaui");
        }
    }
}
