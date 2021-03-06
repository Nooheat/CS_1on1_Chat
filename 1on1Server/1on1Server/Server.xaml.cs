﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace _1on1Server
{
    /// <summary>
    /// Server.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    

    public partial class Server : UserControl
    {
        private Grid root;
        private Socket socket = null;
        private Socket workingSocket = null;
        private Thread receivingThread = null;
        private List<string> ipList = new List<string>();
        private IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
        private bool shiftPressed = false;
        public Server(Grid root)
        {
            InitializeComponent();
            this.root = root;
            ServerOn();
            sendButton.Click += new RoutedEventHandler(sendButton_Click);
        }
        private void ServerOn()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                
                
                socket.Bind(ipep);
                socket.Listen(5);
                
                receivingThread = new Thread(new ThreadStart(receiving));
                receivingThread.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show("Server haven't launched\n" + e.ToString());
            }
        }
        private void receiving()
        {
            byte[] bytes = null;
            workingSocket = socket.Accept();
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
             {
                 textField.AppendText("System : Connecting Succeed...\n");
             }));
            

            while (true)
            {
                bytes = new byte[1024];
                try
                {
                    workingSocket.Receive(bytes, bytes.Length, SocketFlags.None);
                }
                catch (NullReferenceException ne) { MessageBox.Show("NullReference"); workingSocket.Close(); socket.Close(); return; }
                catch (SocketException se) {MessageBox.Show("Socket Error Occured"); workingSocket.Close(); socket.Close(); return; }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); workingSocket.Close(); socket.Close(); return; }
                string message = Encoding.Default.GetString(bytes);
                message = message.TrimEnd('\0');

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    textField.AppendText(message + "\n");
                    textField.ScrollToEnd();
                }));
                bytes = null;
            }
        }
        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            sendMsg();
        }

        private void sendMsg()
        {
            string message = inputField.Text;
            if (message.Equals("")) return;
            message = "Server : " + message;
            textField.AppendText(message.Trim() + "\n");
            textField.ScrollToEnd();
            try
            {
                workingSocket.Send(Encoding.Default.GetBytes(message));
            }catch(Exception e)
            {
                MessageBox.Show("sending failed");
            }
            inputField.Text = "";
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftPressed = true;
            }
            if (e.Key == Key.Return)
            {
                if (shiftPressed == true)
                {
                    inputField.AppendText(Environment.NewLine);
                    inputField.SelectionStart = inputField.Text.Length;
                }
                else
                    sendMsg();
            }
            
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key== Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftPressed = false;
            }
        }
    }
}
