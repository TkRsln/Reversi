using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class Load : MonoBehaviour
{
    public bool connected {
        get {
            return client == null ? false : client.Connected;
        }
    }

    public const string IP_ADRES = "";
    public const int PORT = 11742;

    private TcpClient client;
    

    private void startTcp() 
    {
        try { 
            client = new TcpClient(IP_ADRES,PORT);
            
        }catch(Exception ex) { }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
