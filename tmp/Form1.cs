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
        public string host = "192.168.1.2"; //曾宪梓楼时
        //public string host = "127.0.0.1";   //
        public int port = 10001;
        public Image im;
        public MemoryStream ms;
        public Form1()
        {
            InitializeComponent();        
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myThread = new Thread(new ThreadStart(WorkerThread));
            myThread.Start(); 
        }

        private void WorkerThread()
        {
         //   int i = 0;
            sc = new SocketCom(host,port);
            if (sc == null)
            {
                MessageBox.Show("new SocketCom(host,port) failed.");
                return;
            }
            Imagedata imd;
            while (true)
            { 
                Thread.Sleep(200);
               
                try
                {
                    imd=sc.getstrdata();
                    if (imd != null)
                    ShowProgress(imd.bytes, imd.imagesize);
                    sc.setgetflag(0);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message+"\n"+ex.StackTrace);
                    this.richTextBox1.Text = ex.Message + ex.StackTrace;
                }
               
            }
        }
        
        public void ShowProgress(Byte[] msg, int imagesize)
        {
            System.EventArgs e = new MyProgressEvents(msg, imagesize);
            object[] pList = { this, e };

            BeginInvoke(new MyProgressEventsHandler(UpdateUI), pList);
        }
       
        private delegate void MyProgressEventsHandler(object sender, MyProgressEvents e);
       
        private void UpdateUI(object sender, MyProgressEvents e)
        {
            if (null == e.Msg) return;
            label2.Text = e.imagesize.ToString();
            try
            {
                BytesToImage(e.Msg,e.imagesize);
                if (null == im) return;
                
                pictureBox1.BackgroundImage = im;
               // im.Dispose();
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
            public Byte[] Msg=null;
            public int imagesize = 0;
            public MyProgressEvents(Byte[] msg, int per)
            {
                Msg = msg;
                imagesize = per;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(sc != null)
                sc.Dispose();
            if (myThread != null)
            {
                myThread.Abort();
            }
        }
    }
}
