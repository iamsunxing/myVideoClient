using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

namespace multithreadservTest
{
    public class SocketCom
    {
        private Socket server;

        private createSocketThreads cst;

        private Thread tmpThread;

        public SocketCom(string host, int port)
        {
            //初始化IP地址  
            IPAddress local = IPAddress.Parse(host);
            IPEndPoint iep = new IPEndPoint(local, port);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            bool flag = false;
            try
            {
                server.Bind(iep);  //将套接字与本地终结点绑定
                flag = true;
            }
            catch
            {
              //  MessageBox.Show(ex.Message+ex.StackTrace);
            }
            if (flag)
            {
                server.Listen(20);
                server.ReceiveBufferSize = 640 * 480 * 3 + 54;
                cst = new createSocketThreads(server);
                tmpThread = new Thread(new ThreadStart(cst.createSocketThread));
                tmpThread.Start();
            }
        }
        public int sendstring(string s)
        {
           return cst.sendstring(s);
        }
        public Imagedata getstrdata()
        {
            if(cst!=null)
                return cst.getstrdata();
            else
                return null;
        }
        
        public void Dispose()
        { 
            if(cst != null)
                cst.Dispose();
            //Thread.Sleep(1);
            if (tmpThread != null)
                tmpThread.Abort();
            if (server != null)
                server.Close();

        }
    }

    public class createSocketThreads
    {
        private Socket server;

        private ClientThread newclient;

        private Thread thread;

        public createSocketThreads(Socket myserver)
        {
            this.server = myserver;
        }
        public int sendstring(string s)
        {
           return newclient.sendstring(s);
        }
        public void createSocketThread()
        {
         //   while (true)
            try
            {
                Socket client = server.Accept(); //得到包含客户端信息的套接字            
                newclient = new ClientThread(client);  //创建消息服务线程对象       
                thread = new Thread(new ThreadStart(newclient.ClientService));
                thread.Start();//把ClientThread类的ClientService方法委托给线程 
            }
            catch 
            {
                 //  MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }
       
        public Imagedata getstrdata()
        {
            if (newclient != null/* && newclient.getflag() == 1*/)
            {
         //       newclient.setflag(0);
                return newclient.getstrdata();
            }
            else
                return null;
        }

        public void Dispose()
        {
            if(thread != null)
                thread.Abort();
            if (newclient != null)
                newclient.Dispose();
            if (server != null)
                server.Close();
            
        }
    }

    public class ClientThread
    {
        private Socket service;
        private Imagedata imagedata;
        private Byte[] bytes;
        private int imagesize=-1;
        private int imagetype=1;
     //   private int flag = 0;
        public ClientThread(Socket clientsocket)
        {
            bytes = new byte[640 * 480 * 3 + 54];
            imagedata = new Imagedata(bytes, imagesize, imagetype);
            service = clientsocket;   //service对象接管对消息的控制  
        }
        public void Dispose()
        {
            if (imagedata != null)
                imagedata.Dispose();
            if (service != null)
                service.Close();
            bytes = null;
        }
        public Imagedata getstrdata()
        {
            if (imagedata.getflag() == 0) return null;
            return this.imagedata;
        }
        public int sendstring(string s)
        {
            try
            {
                if (service != null)
                    return service.Send(System.Text.Encoding.ASCII.GetBytes(s));
                else
                    return -1;
            }
            catch
            {
                return -1;
            }
        }
        public void ClientService()
        {
            while (/*(this.imagedata.getflag()==0) &&*/ (service.Receive(bytes, 1, SocketFlags.None) != 0))
            {
                // MessageBox.Show(bytes.ToString());
                if (bytes[0] == 'p')
                {
                    service.Receive(bytes, 3, SocketFlags.None);
                    if (bytes[0] == 'k' && bytes[1] == 'g' && bytes[2] == 'h')
                    {
                        //  service.Send(System.Text.Encoding.ASCII.GetBytes("start"));
                        service.Receive(bytes, 4, SocketFlags.None);//number of frame
                        imagedata.setimagenum(BitConverter.ToInt32(bytes, 0));
                        service.Receive(bytes, 4, SocketFlags.None);//number of frame
                        imagedata.setimagetype(BitConverter.ToInt32(bytes, 0)); 
                        service.Receive(bytes, 4, SocketFlags.None);//size
                        imagedata.setimagesize(BitConverter.ToInt32(bytes, 0));  // the size of frame
                        
                      //  Thread.Sleep(10);
                      //  service.Receive(bytes, imagedata.getimagesize(), SocketFlags.None);// the frame
                        byte[] tmpch = new byte[2];
                        int j = 0;
                        for (int i = 0; i < imagedata.getimagesize(); i++)
                        {
                            if (service.Receive(tmpch, 1, SocketFlags.None) != 0)
                            {
                                bytes[j++] = tmpch[0];
                            }
                        }
                       
                        if (imagedata.getimagetype() == 0)
                        {
                            byte[] tmpim = new byte[imagedata.getimagesize()];
                            Buffer.BlockCopy(bytes, 0, tmpim, 0, imagedata.getimagesize());
                            System.IO.File.WriteAllBytes(@"E:\recim.jpg", tmpim);
                        }
                        // MessageBox.Show(bytes[0].ToString()+" "+bytes[1].ToString()+" "+bytes[2].ToString());
                        byte[] bits = new byte[16];
                        service.Receive(bits, 16, SocketFlags.None);// the md5
                        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                        byte [] md5code = md5.ComputeHash(bytes);
                      //  string str11 =  System.Text.Encoding.ASCII.GetString(bits)+"\n" 
                      //              + System.Text.Encoding.ASCII.GetString(md5code);
                      //  MessageBox.Show(str11);
                        bool tmpflag=true;
                        for (int i = 0; i < 16; i++)
                        {
                            if (md5code[i] != bits[i])
                            {
                                tmpflag = false;
                                break;
                            }
                        }
                   //     if (tmpflag)
                        {
                            if (this.imagedata.getflag() == 0)
                            {
                                imagedata.setbytes(bytes);
                                this.imagedata.setflag(1);
                            }
                        }
                    }
                }
                Thread.Sleep(100);
            }
  
        }
    }
   
    public class Imagedata
    {
        private Byte[] bytes;
        private int imagesize;
        private int imagenum;
        private int imagetype;
        private Int32 flag;
        public Imagedata(Byte[] bytes, int imagesize, int imagetype)
        {
            this.bytes = new Byte[bytes.Length];
            this.imagesize = imagesize;
            this.imagetype = imagetype;
        }
        public int getflag()
        {
            return flag;
        }
        public void setflag(int flag)
        {
            this.flag = flag;
        }
        public int getimagetype()
        {
            return imagetype;
        }
        public void setimagetype(int imagetype)
        {
            this.imagetype=imagetype;
        }
        public int getimagesize()
        {
            return imagesize;
        }
        public void setimagesize(int imagesize)
        {
            this.imagesize = imagesize;
        }
        public int getimagenum()
        {
            return imagenum;
        }
        public void setimagenum(int imagenum)
        {
            this.imagenum = imagenum;
        }
        public byte[] getbytes()
        {
            return bytes;
        }
        public void setbytes(byte []src)
        {
            Array.Copy(src, bytes, src.Length);
         //   this.bytes = b;
        }
        public void Dispose()
        {
            bytes = null;
        }
    }
}