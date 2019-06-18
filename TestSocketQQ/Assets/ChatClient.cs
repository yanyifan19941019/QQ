using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
using System.Net;
using System.Threading;
using UnityEngine.UI;
using System.Text;

public class ChatClient : MonoBehaviour
{
    public static Hashtable ALLClients = new Hashtable(); // 客户列表
    private TcpClient _client;  // 客户端实体
    public string _clientIP;   // 客户端IP
    private string _clientNick; // 客户端昵称
    private byte[] data;     // 消息数据
    private bool ReceiveNick = true;
    public ChatClient(TcpClient client)
    {
        this._client = client;
        this._clientIP = client.Client.RemoteEndPoint.ToString();
        // 把当前客户端实例添加到客户列表当中
        ALLClients.Add(this._clientIP, this);
        data = new byte[this._client.ReceiveBufferSize];
        // 从服务端获取消息
        client.GetStream().BeginRead(data, 0, System.Convert.ToInt32(this._client.ReceiveBufferSize), ReceiveMessage, null);
    }
    // 从客戶端获取消息
    public void ReceiveMessage(IAsyncResult ar)
    {
        int bytesRead;
        try
        {
            lock (this._client.GetStream())
            {
                bytesRead = this._client.GetStream().EndRead(ar);
            }
            if (bytesRead < 1)
            {
                ALLClients.Remove(this._clientIP);
                Broadcast(this._clientNick + " 已经离开服务器");//已经离开服务器
                return;
            }
            else
            {
                string messageReceived = Encoding.UTF8.GetString(data, 0, bytesRead);
                if (ReceiveNick)
                {
                    this._clientNick = messageReceived;
                    Broadcast(this._clientNick + " 已经进入服务器");//已经进入服务器
                                                             //this.sendMessage("hello");
                    ReceiveNick = false;
                }
                else
                {
                    Broadcast(this._clientNick + ">>>>" + messageReceived);
                }
            }
            lock (this._client.GetStream())
            {
                this._client.GetStream().BeginRead(data, 0, System.Convert.ToInt32(this._client.ReceiveBufferSize), ReceiveMessage, null);
            }
        }
        catch (Exception ex)
        {
            ALLClients.Remove(this._clientIP);
            Broadcast(this._clientNick + " 已经离开服务器");//已经离开服务器
        }
    }

    // 向客戶端发送消息
    public void sendMessage(string message)
    {
        try
        {
            System.Net.Sockets.NetworkStream ns;
            lock (this._client.GetStream())
            {
                ns = this._client.GetStream();
            }
            // 对信息进行编码
            byte[] bytesToSend = Encoding.UTF8.GetBytes(message);
            ns.Write(bytesToSend, 0, bytesToSend.Length);
            ns.Flush();
        }
        catch (Exception ex)
        {
            Debug.Log("Error:" + ex);
        }
    }

    // 向客户端广播消息
    public void Broadcast(string message)
    {
        // oldstr= message+"\n";
        print(message);//打印消息

        foreach (DictionaryEntry c in ALLClients)
        {
            ((ChatClient)(c.Value)).sendMessage(message + Environment.NewLine);
        }
    }
    void Update()
    {
    }
}
