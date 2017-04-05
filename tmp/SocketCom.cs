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

namespace multithreadservTest
{
    public class SocketCom
    {
        public Socket server;
        
        public createSocketThreads cst;

        Thread tmpThread;

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
                //server.
                flag = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message+ex.StackTrace);
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
 
        public Imagedata getstrdata()
        {
            if(cst!=null)
                return cst.getstrdata();
            else
                return null;
        }
        
        public void setgetflag(int flag)
        {
            if (cst != null)
                cst.setgetflag(flag);
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
        public Socket server;
        
        ClientThread newclient;

        Thread thread;

        public createSocketThreads(Socket myserver)
        {
            this.server = myserver;
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
            if (newclient != null && newclient.getflag == 1)
            {
               // newclient.getflag = 0;

                return newclient.imagedata;
            }
            else
                return null;
        }
        
        public void setgetflag(int flag)
        {
            if(newclient!=null)
            newclient.getflag = flag;
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
        public Socket service;
        public Imagedata imagedata;
        public Byte[] bytes;
        public int imagesize;
        public int getflag = 0;
        public ClientThread(Socket clientsocket)
        {
            bytes = new byte[640 * 480 * 3 + 54];
            imagedata = new Imagedata(bytes, imagesize);
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
        public void ClientService()
        {
            try
            {
                while ( (getflag == 0) && (service.Receive(bytes, 1, SocketFlags.None) != 0))
                {
                    if (bytes[0] == 'p')
                    {
                        service.Receive(bytes, 3, SocketFlags.None);
                        if (bytes[0] == 'k' && bytes[1] == 'g' && bytes[2] == 'h')
                        {
                            //  service.Send(System.Text.Encoding.ASCII.GetBytes("start"));
                            service.Receive(bytes, 4, SocketFlags.None);//number of frame
                            imagedata.imagenum = BitConverter.ToInt32(bytes, 0); 
                            service.Receive(bytes, 4, SocketFlags.None);//size
                            imagedata.imagesize = BitConverter.ToInt32(bytes, 0);  // the size of frame
                            Thread.Sleep(5);
                            service.Receive(bytes, imagesize, SocketFlags.None);// the frame
                            MessageBox.Show(bytes[0].ToString()+" "+bytes[1].ToString()+" "+bytes[2].ToString());

                            imagedata.bytes = bytes;
                            getflag = 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                service.Close();  //关闭套接字 
            }    
        }
    }
   
    public class Imagedata
    {
        public byte[] bytes;
        public int imagesize;
        public int imagenum;
        public Imagedata(byte[] bytes, int imagesize)
        {
            this.bytes = bytes;
            this.imagesize = imagesize;
        }
        public void Dispose()
        {
            bytes = null;
        }
    }
}