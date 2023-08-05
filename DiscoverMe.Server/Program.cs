using System.Net;
using System.Net.Sockets;
using System.Text;

// DiscoverMe service port number
const int discoverMePort = 33455;

// hello response datagram
byte[] helloResponseDatagram = "hello!"u8.ToArray();

// hello request message
const string helloRequestMessage = "hello?";

Console.WriteLine($"{DateTimeOffset.Now:s} DiscoverMe service started.");

// UdpClient to send/receive datagrams
UdpClient server = new();

// Let me listen on the broadcast address even if someone else is already listening on it
server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

// Don't prevent anyone else from listening to the broadcast address
server.ExclusiveAddressUse = false;

// Only listen to broadcast messages
IPEndPoint broadcastEndPoint = new(IPAddress.Broadcast, discoverMePort);
server.Client.Bind(broadcastEndPoint);

// Keep listening to see if anyone screams
while (true)
{
    Console.WriteLine($"{DateTimeOffset.Now:s} Listening to see if anyone screams...");
    try
    {
        // Wait for a scream
        var request = await server.ReceiveAsync();

        // I heard something. Did they say 'hello?' ?
        string message = Encoding.UTF8.GetString(request.Buffer);
        if (message == helloRequestMessage)
        {
            // They said 'hello?'. Let me just holler back so they know where I am

            Console.WriteLine(
                $"{DateTimeOffset.Now:s} Someone at {request.RemoteEndPoint} was screaming.");

            Console.WriteLine(
                $"{DateTimeOffset.Now:s} Responding to {request.RemoteEndPoint} so they know where I am.");

            // Send a response
            await server.SendAsync(helloResponseDatagram, request.RemoteEndPoint);
        }
        else
        {
            // Got a request but it isn't what I expected it to be so I should probably ignore it
            Console.WriteLine(
                $"{DateTimeOffset.Now:s} Someone at {request.RemoteEndPoint} was screaming, but I don't know what '{message}' means.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{DateTimeOffset.Now:s} Opps, something went wrong: {ex.Message}");

        // Don't know what do with this error so I'll just quit
        break;
    }
}

Console.WriteLine($"{DateTimeOffset.Now:s} DiscoverMe service stopped.");