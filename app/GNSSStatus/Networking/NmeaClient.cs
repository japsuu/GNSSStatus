using System.Net.Sockets;
using GNSSStatus.Utils;

namespace GNSSStatus.Networking;

/// <summary>
/// Implements a simple TCP client which connects to the specified server.
/// </summary>
public sealed class NmeaClient : IDisposable
{
    private readonly string _ipAddress;
    private readonly int _port;
    private TcpClient? _tcpClient;
    private StreamReader? _reader;
    private int _nullDataCounter = 0;
    
    public bool IsConnected { get; private set; }


    /// <summary>
    /// Constructs a new client with the given address and port.
    /// </summary>
    /// <param name="ipAddress">The IPV4 address of the server.</param>
    /// <param name="port">The port number to listen on.</param>
    public NmeaClient(string ipAddress, int port)
    {
        _ipAddress = ipAddress;
        _port = port;
    }


    public void Connect()
    {
        if (IsConnected)
            return;
        
        Logger.LogInfo("Connecting to NMEA server...");

        _tcpClient = new TcpClient(_ipAddress, _port);
        _tcpClient.ReceiveTimeout = 5000;
        _reader = new StreamReader(_tcpClient.GetStream());
        IsConnected = true;
        
        Logger.LogInfo("Connected to NMEA server.");
    }
    
    
    /// <summary>
    /// Reads the latest NMEA sentence from the server.
    /// </summary>
    public IEnumerable<Nmea0183Sentence> ReadSentences()
    {
        if (!IsConnected)
        {
            Logger.LogWarning("Cannot read sentence from server. Not connected.");
            yield break;
        }

        while (true)
        {
            string? data;
            try
            {
                data = _reader!.ReadLine();
            }
            catch (IOException ex) when (ex.InnerException is SocketException)
            {
                Logger.LogError("A socket error has occurred or the read operation timed out.");
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError($"A socket error has occurred with the client socket {_tcpClient}:");
                Logger.LogError($"{ex}");
                break;
            }
            
            if (data == null)
            {
                _nullDataCounter++;
                
                if (_nullDataCounter > 5)
                {
                    Logger.LogWarning("No data received from server. Disconnecting...");
                    Dispose();
                    break;
                }
                
                continue;
            }
            
            if (!data.StartsWith('$'))
                continue;
            
            _nullDataCounter = 0;
            yield return new Nmea0183Sentence(data);
        }
    }
    
    
    /// <summary>
    /// Disconnects from the server
    /// </summary>
    public void Dispose()
    {
        if (!IsConnected)
            return;
        
        _tcpClient?.Dispose();
        _reader?.Dispose();
        _tcpClient = null;
        _reader = null;
        
        IsConnected = false;
        
        Logger.LogInfo("Disconnected from NMEA server");
    }
}