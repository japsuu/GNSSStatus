using System.Net.Sockets;
using GNSSStatus.Nmea;

namespace GNSSStatus.Networking;

/// <summary>
/// Implements a simple TCP client which connects to the specified server.
/// </summary>
public sealed class NmeaClient : IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly StreamReader _reader;
    private bool _shouldExit;
    
    public bool IsConnected { get; private set; }


    /// <summary>
    /// Constructs a new client with the given address and port.
    /// </summary>
    /// <param name="ipAddress">The IPV4 address of the server.</param>
    /// <param name="port">The port number to listen on.</param>
    public NmeaClient(string ipAddress, int port)
    {
        _tcpClient = new TcpClient(ipAddress, port);
        _reader = new StreamReader(_tcpClient.GetStream());
        IsConnected = true;
        
        Logger.LogInfo("Connected to server, listening for packets");
    }
    
    
    /// <summary>
    /// Reads the latest NMEA sentence from the server.
    /// </summary>
    public IEnumerable<Nmea0183Sentence> ReadSentence()
    {
        string? data = null;

        while (true)
        {
            if (_shouldExit)
                break;
            
            try
            {
                data = _reader.ReadLine();
            }
            catch (Exception ex)
            {
                Logger.LogException($"A socket error has occurred with the client socket {_tcpClient}", ex);
            }
            
            if (data == null)
                continue;
            
            if (!data.StartsWith('$'))
                continue;
                
            yield return new Nmea0183Sentence(data);
        }
    }
    
    
    /// <summary>
    /// Disconnects from the server
    /// </summary>
    public void Dispose()
    {
        _shouldExit = true;
        _tcpClient.Dispose();
        _reader.Dispose();
        
        Logger.LogInfo("Disconnected from server");
    }
}