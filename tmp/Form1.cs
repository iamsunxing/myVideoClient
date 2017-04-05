using multithreadservTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace tmp
{
    public partial class Form1 : Form
    {
        public Thread myThread=null;
        public SocketCom sc = null;
      //   public string host = "192.168.1.103";//寝室时
        public string host; //曾宪梓楼时
        //public string host = "127.0.0.1";   //
        public int port;
        public Image im;
        public MemoryStream ms;
        public Form1()
        {
            InitializeComponent();        
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("192.168.1.103");
            comboBox1.Items.Add("192.168.1.2");
            comboBox1.Items.Add("127.0.0.1");
            comboBox1.Text = comboBox1.Items[1].ToString();
            comboBox2.Items.Add("10001");
            comboBox2.Text = comboBox2.Items[0].ToString();

        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                host = comboBox1.Text;
                port = Int32.Parse(comboBox2.Text);
            }
            catch(Exception ex)
            {
                richTextBox1.Text = ex.Message + ex.StackTrace;
                return;
            }

            myThread = new Thread(new ThreadStart(WorkerThread));
            myThread.Start();
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
                Thread.Sleep(200);
              //  try
            //    {
                    imd=sc.getstrdata();
                    if (imd != null)
                    {
                        ShowProgress(imd); 
                    }
                    sc.setgetflag(0);
             //   }
              //  catch (Exception ex)
              //  {
              //      MessageBox.Show(ex.Message + ex.StackTrace);
              //  }  
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
            if (null == e.im.bytes) return;
            label2.Text = e.im.imagesize.ToString();
            label6.Text = e.im.imagenum.ToString();
            try
            {
                BytesToImage(e.im.bytes, e.im.imagesize);
                if (null == im) return;
                
                pictureBox1.BackgroundImage = im;
                im = null;
            }catch(Exception ex)
            {
                this.richTextBox1.Text = ex.Message + ex.StackTrace;
                //  MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }  
        }
       
        public void BytesToImage(Byte[] buffer, int imagesize)
        {
            if ((buffer == null) || (imagesize <= 0)) return ;
            try
            {
             //   imagesize = imagesize;
                FileStream fs = new FileStream("E:\\Desktop\\tmp\\tmp\\imagebuffer.bin", FileMode.Create);
                fs.Write(buffer, 0, imagesize);
                fs.Flush();
                fs.Close();
                if (im != null)
                {
                    //im.Dispose();
                    im = null;
                }
                ms= new MemoryStream(buffer,0,imagesize);
                if (ms == null) return;
                im = System.Drawing.Image.FromStream(ms,true,true);
              
             //   ms.Dispose();
                ms = null;
            }
            catch(Exception ex)
            {
                this.richTextBox1.Text = ex.Message + ex.StackTrace;
            }
        }
        
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
    }
}
