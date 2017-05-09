using multithreadservTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace tmp
{
    public partial class Form1 : Form
    {
        private Thread myThread=null;
        private SocketCom sc = null;
        private string host;
        private int port;
        private Image im;
        private MemoryStream ms;
        private static string zynqipstr;
        private bool zynqipflag;
        private MotorCtrl mc;
        public Form1()
        {
            InitializeComponent();
            mc = new MotorCtrl();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
//            comboBox1.Items.Add("192.168.1.109");
           // comboBox1.Items.Add("192.168.1.2");
           // comboBox1.Items.Add("127.0.0.1");
            IPHostEntry ipe = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipa = ipe.AddressList[2];
            comboBox1.Items.Add(ipa.ToString());
            comboBox1.Text = comboBox1.Items[0].ToString();
            comboBox2.Items.Add("10001");
            comboBox2.Text = comboBox2.Items[0].ToString();
            timer1.Interval = 100;
            timer1.Start();
            zynqipflag = false;

            try
            {
                host = comboBox1.Text;
                port = Int32.Parse(comboBox2.Text);
            }
            catch (Exception ex)
            {
                richTextBox1.Text = ex.Message + ex.StackTrace;
                return;
            }
            button1.Text = "停止监听";
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            myThread = new Thread(new ThreadStart(WorkerThread));
            myThread.Start(); 
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text.Equals("开始监听"))
            {
                try
                {
                    host = comboBox1.Text;
                    port = Int32.Parse(comboBox2.Text);
                }
                catch (Exception ex)
                {
                    richTextBox1.Text = ex.Message + ex.StackTrace;
                    return;
                }
                button1.Text = "停止监听";
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                myThread = new Thread(new ThreadStart(WorkerThread));
                myThread.Start(); 
            }else if (button1.Text.Equals("停止监听"))
            {
                try
                {
                    myThread.Abort();
                    sc.Dispose();
                }catch
                {
                }
                button1.Text = "开始监听";
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
            }
        }
        private void WorkerThread()
        {
            try
            {
                sc = new SocketCom(host, port);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                return;
            }
            Imagedata imd;
            while (true)
            { 
                Thread.Sleep(100);
                imd=sc.getstrdata();
               
                if (imd != null)
                {   
                    ShowProgress(imd); 
                    imd.setflag(0);
                }
            }
        }
        
        public void ShowProgress(Imagedata imda)
        {
            System.EventArgs e = new MyProgressEvents(imda);
            object[] pList = { this, e };

            BeginInvoke(new MyProgressEventsHandler(UpdateUI), pList);
        }
       
        private delegate void MyProgressEventsHandler(object sender, MyProgressEvents e);
       
        private void UpdateUI(object sender, MyProgressEvents e)
        {
            if (null == e.im.getbytes()) return;
         //   label2.Text = e.im.getimagetype().ToString();
       //     if (e.im.getimagetype() == 1)//1是调试信息
            if(e.im.getimagesize()<1000)
            {
                string str="";
                try 
                { 
                    str=System.Text.Encoding.Default.GetString ( e.im.getbytes()); 
                }
                catch (Exception ex)
                {
                    return;
                }
                richTextBox1.Text = str;
                if (str.Substring(0,3).Equals("PID"))
                {
                    string [] pidstr=str.Substring(3).Split(',');
                    textBox1.Text = pidstr[0];
                    textBox2.Text = pidstr[1];
                    textBox3.Text = pidstr[2];
                }
                else if (str.Substring(0, 11).Equals("SitaIncDuty"))
                {
                    string[] res = str.Substring(11).Split(',');
                    label3.Text = res[0];
                    label6.Text = res[1];
                }
                else
                {
                    richTextBox1.Text = str;
                }
                return;
            }
            else //0是图像
            {
             //   byte[] tmpim = new byte[e.im.getimagesize()];
             //   Buffer.BlockCopy(e.im.getbytes(), 0, tmpim, 0, e.im.getimagesize());
             //   System.IO.File.WriteAllBytes(@"E:\recim1.jpg", tmpim);
                label11.Text = e.im.getimagesize().ToString();
                label12.Text = e.im.getimagenum().ToString();
                try
                {
                    BytesToImage(e.im.getbytes(), e.im.getimagesize());
                    if (null == im) return;

                    pictureBox1.Image = im; 
                    Thread.Sleep(10);
                  //  im = null;
                }
                catch (Exception ex)
                {
                    this.richTextBox1.Text = ex.Message + ex.StackTrace;
                    //  MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                }  
            }
         
        }
       
        public void BytesToImage(Byte[] buffer, int imagesize)
        {
            if ((buffer == null) || (imagesize <= 0)) return ;
            try
            {
                if (im != null)
                {
                    //im.Dispose();
                    im = null;
                }
                ms= new MemoryStream(buffer,0,imagesize);
                if (ms == null) return;
                im = System.Drawing.Image.FromStream(ms);
              
             //   ms.Dispose();
                ms = null;
            }
            catch(Exception ex)
            {
                this.richTextBox1.Text = ex.Message + ex.StackTrace;
            }
        }
        class Tmp
        {
            public Thread tt;
            Tmp(Thread tt)
            {
                this.tt = tt;
            }
        };
        public class MyProgressEvents : EventArgs
        {
            public Imagedata im;
            public MyProgressEvents(Imagedata imd)
            {
                im=imd;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sc != null)
            {
                sc.Dispose();
            }
            if (myThread != null)
            {
                myThread.Abort();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                double p = Double.Parse(textBox1.Text);
                double i = Double.Parse(textBox2.Text);
                double d = Double.Parse(textBox3.Text);
                sc.sendstring(textBox1.Text + '\n' + textBox2.Text + '\n' + textBox3.Text + '\n');
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            new Thread(getzynqipThread).Start();
            
        }
        private void getzynqipThread()
        {
            scoket_udp.UdpBroadCast udptmp=new scoket_udp.UdpBroadCast();
            udptmp.run();
          //  Thread.Sleep(3000);
            while (true)
            {
                if (udptmp.getbflag() == 1)
                {
                    zynqipstr = "zynq ip:" + udptmp.getzynqip();
                    break;
                }
                Thread.Sleep(10);
            }
            zynqipflag = true;
            //  comboBox1.Items.Add(udptmp.getzynqip());
            //  comboBox1.Text = udptmp.getzynqip();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label10.Text = System.DateTime.Now.ToString();
            if (zynqipflag == true)
            {
                richTextBox1.Text = zynqipstr;
                zynqipflag = false;
                button3.Enabled = true;
            }
        }

        private void button9_Click(object sender, EventArgs e) //捕获
        {
            if (button9.Text == "控制")
            {
                sc.sendstring(mc.setEnable(true));
                button9.Text = "释放";
            }
            else if (button9.Text == "释放")
            {
                sc.sendstring(mc.setEnable(false));
                button9.Text = "控制";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            sc.sendstring(mc.setStop(4000));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sc.sendstring(mc.setForeward(4000));
        }

        private void button7_Click(object sender, EventArgs e)
        {
            sc.sendstring(mc.setRight(4000));
        }

        private void button8_Click(object sender, EventArgs e)
        {
            sc.sendstring(mc.setLeft(4000));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            sc.sendstring(mc.setBackWard(4000));
        }

        private void button10_Click(object sender, EventArgs e)
        {
            sc.sendstring(mc.setTurn(4000));
        }
    }
}


namespace scoket_udp
{
    class UdpBroadCast
    {
        private  Socket sock;
        private  IPEndPoint iep1;
        private  EndPoint remote;
        private  byte[] senddata;
        private  byte[] receivedata;
        private Thread t;
        private Thread t2;
        private string zynqip;
        private int bflag = 0;
        public UdpBroadCast()
        {
            receivedata = new Byte[255];
            zynqip = "";
        }
        public void run( )
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,ProtocolType.Udp);
            iep1 = new IPEndPoint(IPAddress.Broadcast, 10001);            //255.255.255.255
            remote = new IPEndPoint(IPAddress.Any,10001);
          //  string hostname = Dns.GetHostName();
            IPHostEntry ipe = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipa = ipe.AddressList[2];
            
            string str1 = "zynqtest"+ipa.ToString();
            senddata = Encoding.ASCII.GetBytes(str1);
        //    string str = System.Text.Encoding.ASCII.GetString(senddata);
        //    MessageBox.Show(str);
            sock.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.Broadcast, 1);

            t = new Thread(BroadcastMessage);
            t.Start();
            t2 = new Thread(ReceiveMessage);
            t2.Start();
        }

        private void BroadcastMessage()
        {
            while (true)
            {
                sock.SendTo(senddata, iep1);
                Thread.Sleep(500);
            }
        }
        private void ReceiveMessage()
        {
            while (true)
            {
                sock.ReceiveFrom(receivedata, ref remote);
                string str = System.Text.Encoding.ASCII.GetString(receivedata);
              //  MessageBox.Show(str.Length.ToString());
             //  MessageBox.Show("K"+str+"K");
                bflag = 0;
                zynqip="";
                if (str.ElementAt(0)=='O'&&str.ElementAt(1)=='K')
                {
                    for (int i = 2; i < str.Length; i++)
                    {
                        if (str.ElementAt(i) != 0)
                            zynqip += str.ElementAt(i);
                        else 
                        {
                            break;
                        }
                      
                    }
                    bflag = 1;
                    Thread.Sleep(100);
                  //  MessageBox.Show("k"+zynqip+"k"+zynqip.Length);
                    //str.Substring(2, 13);
                    sock.Dispose();
                    t.Abort();
                    t2.Abort();
                   
                }
                Thread.Sleep(100);
            }
            
        }
        public string getzynqip()
        {
            return this.zynqip;
        }
        public int getbflag()
        {
            return this.bflag;
        }
    }
}