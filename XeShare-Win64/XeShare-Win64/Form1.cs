using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using DiscordRPC;
using DiscordRPC.Logging;
using JRPC_Client;
using XDevkit;

namespace XeShare_Win64
{
    public partial class Form1 : Form
    {
        IXboxConsole xbox;
        DiscordRpcClient client;
        bool isConnected = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            bunifuCustomLabel1.Text = DateTime.Now.ToShortTimeString();

            string clientIdPath0 = Application.StartupPath + "\\Tools\\discord_clientid.txt";
            client = new DiscordRpcClient(File.ReadAllText(clientIdPath0));
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
            client.Initialize();

            client.SetPresence(new RichPresence()
            {
                //Details = "XeShare",
                State = "Taking Screenshots",
                Timestamps = Timestamps.Now,
                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    LargeImageText = "XeShare",
                    //SmallImageKey = "image_small"
                }
            });

            /*try
            {
                MessageBox.Show(Application.StartupPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString()); 
            }*/
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            bunifuCustomLabel1.Text = DateTime.Now.ToShortTimeString();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            client.Dispose();
        }

        private void bunifuFlatButton3_Click(object sender, EventArgs e)
        {
            try
            {
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            if (isConnected) return;

            if (xbox.Connect(out xbox))
            {
                try
                {
                    xbox.XNotify("XeShare - Ready to Screenshot!");
                    isConnected = true;
                }
                catch (Exception ex)
                {
                    isConnected = false;
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void bunifuFlatButton2_Click(object sender, EventArgs e)
        {
            if (!isConnected) return;

            try
            {
                string subPath = "\\Screenshots\\" + DateTime.Now.ToString("dd-MM-yyyy");
                if (!Directory.Exists(Application.StartupPath + subPath))
                    Directory.CreateDirectory(Application.StartupPath + subPath);

                string fileName = Application.StartupPath + subPath + "\\Jtag_" + RandomString(10);
                string bmpPath = fileName + ".bmp";
                xbox.ScreenShot(bmpPath);

                string pngPath = fileName + ".png";
                Bitmap bmp = new Bitmap(bmpPath);
                ImageExtensions.SavePng(bmp, pngPath, 85L);

                using (var w = new WebClient())
                {
                    string clientIdPath1 = Application.StartupPath + "\\Tools\\imgur_clientid.txt";
                    string clientID = File.ReadAllText(clientIdPath1);
                    w.Headers.Add("Authorization", "Client-ID " + clientID);
                    var values = new NameValueCollection
                    {
                        {
                            "image", Convert.ToBase64String(File.ReadAllBytes(@pngPath))
                        }
                    };

                    byte[] response = w.UploadValues("https://api.imgur.com/3/upload.xml", values);

                    string res = XDocument.Load(new MemoryStream(response)).ToString();
                    var xdoc = XDocument.Load(new StringReader(res));
                    var imgur = xdoc.Descendants("data").Select(x => new {
                        Link = x.Element("link").Value,
                    }).FirstOrDefault();

                    MessageBox.Show(imgur.Link);
                    Clipboard.SetText(imgur.Link);

                    //MessageBox.Show(XDocument.Load(new MemoryStream(response)).ToString()); testing
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
