using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

class Program{
    TcpListener listener;
    readonly string validToken;
    NetworkStream nStream;
    bool ready = true;
    public Program(){
        Console.WriteLine("Serveeeer...");
        validToken = File.ReadAllText("token").Trim();
        listener = new TcpListener(IPAddress.Any,3000);
        listener.Start();
        while(true){
            var tcpClient = listener.AcceptTcpClient();
            Task.Factory.StartNew(()=>{
                if(nStream == null || !nStream.Socket.Connected)
                    ready = false;
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
                    if(result.SequenceEqual(validToken)){
                        if(nStream != null)
                            nStream.Close();
                        buffer = Encoding.UTF8.GetBytes("authorized");
                        nStream = tcpClient.GetStream();
                        nStream.Write(buffer);
                        ready = true;
                    }else{
                        buffer = Encoding.UTF8.GetBytes("unauthorized");
                        ns.Write(buffer);
                    }
                }else if(ready && nStream != null){
                    nStream.Write(buffer);
                    buffer = new byte[4096];
                    int lenght0 = nStream.Read(buffer);
                    tcpClient.GetStream().Write(buffer,0,lenght0);
                    tcpClient.Close();
                    // Console.ForegroundColor = ConsoleColor.Red;
                    // Console.WriteLine(Encoding.UTF8.GetString(buffer));
                    // Console.ForegroundColor = ConsoleColor.Green;
                    // Console.WriteLine(Encoding.UTF8.GetString(buffer));
                }else{
                    string str = 
                    "HTTP/1.0 503 Service Unavailable\n"+
                        "Content-Type: application/json; charset=UTF-8\n" +
                        "Content-Length: 27\n\n" +
                        "{\"status\":\"Tunnel offline\"}";
                    buffer = Encoding.UTF8.GetBytes(str);
                    tcpClient.GetStream().Write(buffer,0,Encoding.UTF8.GetByteCount(str));
                    tcpClient.Close();
                }
            });
        }
    }
static void Main() => new Program();
    
    
}