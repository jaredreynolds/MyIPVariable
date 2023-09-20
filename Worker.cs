using Microsoft.Extensions.Options;
using System.Net;
using System.Net.NetworkInformation;
using System.Security;
using static MyIPVariable.Constants;

namespace MyIPVariable;

public class Worker : BackgroundService
{
    private readonly PhysicalAddress _physicalAddress;
    private readonly string _environmentVariableName;
    private readonly ILogger<Worker> _logger;

    public Worker(IOptions<Configuration> options, ILogger<Worker> logger)
    {
        _physicalAddress = options.Value.PhysicalAddress;
        _environmentVariableName = options.Value.EnvironmentVariableName;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(Shutdown);

        SetIPVariable();

        NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(CancellationCheckDelay, stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogDebug(ex, "Operation canceled; shutting down.");
                break;
            }
        }
    }

    private void SetIPVariable()
    {
        try
        {
            var ipAddress = GetIPAddress();
            _logger.LogDebug("IP address is {address}", ipAddress);

            Environment.SetEnvironmentVariable(_environmentVariableName, ipAddress.ToString(), EnvironmentVariableTarget.Machine);
        }
        catch (SecurityException ex)
        {
            _logger.LogError(ex, "Administrative access is required to set machine-level environment variable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching IP address or setting environment variable");
        }
    }

    private IPAddress GetIPAddress()
    {
        var adapters = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var adapter in adapters)
        {
            if (adapter.GetPhysicalAddress().Equals(_physicalAddress))
            {
                if (adapter.OperationalStatus != OperationalStatus.Up)
                {
                    _logger.LogError("Specified adapter {adapter} is {status}", _physicalAddress, adapter.OperationalStatus);
                    return IPAddress.None;
                }

                foreach (var ipaddress in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (ipaddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ipaddress.Address;
                    }
                }

                _logger.LogError("Could not find an IPv4 address on specified adapter {adapter}", _physicalAddress);
                return IPAddress.None;
            }
        }

        _logger.LogError("Could not find specified adapter {adapter}", _physicalAddress);
        return IPAddress.None;
    }

    private void OnNetworkAddressChanged(object? sender, EventArgs e)
    {
        SetIPVariable();
    }

    private void Shutdown()
    {
        NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;
    }
}
