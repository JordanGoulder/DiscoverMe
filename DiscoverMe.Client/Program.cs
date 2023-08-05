using System.Net;
using System.Net.Sockets;
using System.Text;

// DiscoverMe service port number
const int discoverMePort = 33455;

// hello request datagram
byte[] helloRequestDatagram = "hello?"u8.ToArray();

// hello response message
const string helloResponseMessage = "hello!";

Console.WriteLine($"{DateTimeOffset.Now:s} DiscoverMe client started");

// UdpClient to send/receive datagrams
UdpClient client = new();

// Something to hold the address of the server
IPEndPoint? serverEndPoint = null;

// Keep screaming until someone responds
while (true)
{
    // hello? Is there anybody out there...
    Console.WriteLine($"{DateTimeOffset.Now:s} hello? Is there anybody out there");
    client.Send(helloRequestDatagram, helloRequestDatagram.Length, new(IPAddress.Broadcast, discoverMePort));

    // Used to stop waiting for a response after a bit
    CancellationTokenSource cts = new();
    cts.CancelAfter(1000);

    Console.WriteLine($"{DateTimeOffset.Now:s} Listening to see if someone responds...");
    try
    {
        // Wait for a response
        var response = await client.ReceiveAsync(cts.Token);

        // I hear something. Did they say 'hello!' ?
        string message = Encoding.UTF8.GetString(response.Buffer);
        if (message == helloResponseMessage)
        {
            // Let me just jot down their address
            serverEndPoint = response.RemoteEndPoint;

            // No more screaming necessary
            break;
        }

        // Got a response but it isn't what I expected it to be so I should probably ignore it
        Console.WriteLine(
            $"{DateTimeOffset.Now:s} Someone at {response.RemoteEndPoint} heard me, but I don't know what '{message}' means.");

        // Wait a bit before we try again
        await Task.Delay(1000);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine($"{DateTimeOffset.Now:s} Hmm, no one responded.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{DateTimeOffset.Now:s} Opps, something went wrong: {ex.Message}");

        // Don't know what do with this error so I'll just quit
        break;
    }

    Console.WriteLine($"{DateTimeOffset.Now:s} Let's scream again");
}

// Yay! Someone heard me!!!
Console.WriteLine(serverEndPoint is not null
    ? $"{DateTimeOffset.Now:s} Someone at {serverEndPoint} heard me and answered!"
    : $"{DateTimeOffset.Now:s} I am all alone");

Console.WriteLine($"{DateTimeOffset.Now:s} DiscoverMe client stopped.");