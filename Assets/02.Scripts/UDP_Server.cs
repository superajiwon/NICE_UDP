using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;

public class UDP_Server : MonoBehaviour {

    [SerializeField] private string ipAddress = "172.30.1.30";
    [SerializeField] private int ConnectPort = 50001;
    [SerializeField] private string recvStr;
    
    [SerializeField] PayTest pay;

    private Socket socket;
    private EndPoint clientEnd;
    private IPEndPoint ipEnd;
    private string sendStr;
    private byte[] recvData = new byte[1024];
    private byte[] sendData = new byte[1024];
    private int recvLen;
    private Thread connectThread;

    public string _clientMessage;

    // Use this for initialization
    void Start()
    {
        InitSocket(); //      server
    }

    void InitSocket()
    {
        IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress[] addr = ipEntry.AddressList;

        for (int i = 0; i < addr.Length; i++)
        {
            print(string.Format("IP Address {0} : {1}", i, addr[i].ToString()));
        }

        ipAddress = addr[1].ToString();

        //ipAddress = "172.30.1.30" / ConnectPort = 50001;
        ipEnd = new IPEndPoint(IPAddress.Parse(ipAddress), ConnectPort);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipEnd);
        //     
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        clientEnd = (EndPoint)sender;
        print("      ");
        //        
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }

    public void SocketSend(string sendStr)
    {
        sendData = new byte[1024];
        sendData = Encoding.UTF8.GetBytes(sendStr);
        socket.SendTo(sendData, sendData.Length, SocketFlags.None, clientEnd);
    }

    void SocketReceive()
    {
        while (true)
        {
            recvData = new byte[1024];
            try
            {
                recvLen = socket.ReceiveFrom(recvData, ref clientEnd);

                print("    : " + clientEnd.ToString());
                if (recvLen > 0)
                {
                    recvStr = Encoding.UTF8.GetString(recvData, 0, recvLen);
                    print("      " + recvStr);

                    if (recvStr.Equals("first"))
                    {
                        SocketSend("first");
                    }

                    string[] message = recvStr.Split(',');
                    string clientMessage = message[0];
                    string value = message[1];
                    string type = message[2]; // QR인지 아닌지 (0 == 카드, 2 == 카카오, 4 == 네이버?, 6 = 제로)

                    print(clientMessage + value + type);

                    if (string.IsNullOrEmpty(_clientMessage))
                    {
                        _clientMessage = clientMessage;

                        if (_clientMessage.Equals("Charge"))
                        {
                            _clientMessage = string.Empty;

                            pay.PayEvent(int.Parse(value), int.Parse(type)); // QR
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }

    void SocketQuit()
    {
        //    
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //    socket
        if (socket != null)
            socket.Close();
        Debug.LogWarning("    ");
    }

    void OnApplicationQuit()
    {
        SocketQuit();
    }
}
