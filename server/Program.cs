using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

class Program{
    TcpListener listener;
    readonly string validToken;
    NetworkStream nStream;
    bool ready = true;
    void resetBuffer(byte[] buffer){
        for(int i =0; i < buffer.Length;i++)
            buffer[i] = 0;
    }
    void setBuffer(byte[] buffer, string source){
        resetBuffer(buffer);
        byte[] buffer2 = Encoding.UTF8.GetBytes(source);
        Array.Copy(buffer2,buffer,buffer2.Length);
    }
    public Program(){
        Console.WriteLine("Serveeeer...");
        validToken = File.ReadAllText("token").Trim();
        listener = new TcpListener(IPAddress.Any,3000);
        listener.Start();
        while(true){
            var tcpClient = listener.AcceptTcpClient();
            Task.Factory.StartNew(()=>{
                var ns = tcpClient.GetStream();
                byte[] buffer = new byte[4096];
                int lenght = ns.Read(buffer);
                string result = Encoding.UTF8.GetString(buffer,0,lenght).Trim();
                bool isHttp = 
                    result.StartsWith("GET") ||
                    result.StartsWith("POST")||
                    result.StartsWith("PUT") ||
                    result.StartsWith("DELETE");
                if(!isHttp){
                    for(int i =0; i < result.Length && i < validToken.Length;i++){
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(validToken[i]);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(result[i]);
                    }
                    //Console.WriteLine("Lenghts: " + (validToken.Length - result.Length).ToString());
                    if(result.SequenceEqual(validToken)){
                        buffer = Encoding.UTF8.GetBytes("authorized");
                        nStream = tcpClient.GetStream();
                        nStream.Write(buffer);
                        ready = true;
                        //"HTTP/1.1 200 OK\nContent-Type: text/plain\nContent-Lenght: 0\n\nHello, World!\n".Any();
                    }else{
                        buffer = Encoding.UTF8.GetBytes("unauthorized");
                        ns.Write(buffer);
                    }
                }else if(ready && nStream != null){
                    nStream.Write(buffer);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Encoding.UTF8.GetString(buffer));
                    buffer = new byte[4096];
                    int lenght0 = nStream.Read(buffer);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(Encoding.UTF8.GetString(buffer));
                    tcpClient.GetStream().Write(buffer,0,lenght0);
                }
            });
        }
        
    }
static void Main() => new Program();
    
    
}