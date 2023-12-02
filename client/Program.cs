using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

class Program{
    TcpClient client, requestClient;
    NetworkStream nstream;
    public Program(){
        client = new TcpClient();
        client.Connect(IPAddress.Parse("127.0.0.1"),3000);
        
        while(!client.Connected);
        nstream = client.GetStream();
        nstream.Write(byteit("password69420"));
        string msg = getMsg().Trim();
        Console.WriteLine(msg);
        if(!msg.SequenceEqual("authorized")){
            Console.WriteLine("Auth error");
            return;
        }
        while(true){
            byte[] buffer = new byte[4096];
            int lenght0 = nstream.Read(buffer);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Encoding.UTF8.GetString(buffer));
            requestClient = new TcpClient();
            IPAddress[] addresses = Dns.GetHostAddresses("example.com");
            requestClient.Connect(/*IPAddress.Parse("127.0.0.1"),80*/addresses[0],80);
            requestClient.GetStream().Write(buffer,0,lenght0);
            buffer = new byte[4096];
            int lenght1 =  requestClient.GetStream().Read(buffer);
            int lenght2 =  requestClient.GetStream().Read(buffer,lenght1,4096-lenght1);
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine(Encoding.UTF8.GetString(buffer));
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine(Encoding.UTF8.GetString(buffer,lenght1,4096-lenght1));

            //Console.ResetColor();
            nstream.Write(buffer,0,Math.Min(lenght1 + lenght2,4096));
            requestClient.Close();
        }
    }
    public string getMsg(){
        byte[] buffer = new byte[4096];
        int lenght0 = nstream.Read(buffer);
        return Encoding.UTF8.GetString(buffer,0,lenght0).Trim();
    }
    public byte[] byteit(String s){
        byte[] buffer = new byte[s.Length];
        return Encoding.UTF8.GetBytes(s);
    }
    public static void Main()=>new Program();
}