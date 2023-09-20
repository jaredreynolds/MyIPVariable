using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace MyIPVariable;

public partial class Configuration
{
    public const string SectionName = "config";

    [GeneratedRegex("[^A-Fa-f0-9]")]
    private static partial Regex NonHexRegex();

    private string _rawMacAddress = string.Empty;

    public string MacAddress
    {
        get => _rawMacAddress;

        set
        {
            _rawMacAddress = value;
            PhysicalAddress = GetPhysicalAddress(_rawMacAddress);
        }
    }

    public PhysicalAddress PhysicalAddress { get; private set; } = PhysicalAddress.None;

    public string EnvironmentVariableName { get; set; } = Constants.EnvironmentVariableName;

    private static PhysicalAddress GetPhysicalAddress(string rawMacAddress)
    {
        var cleanMacAddress = NonHexRegex().Replace(rawMacAddress, string.Empty);
        return new PhysicalAddress(Convert.FromHexString(cleanMacAddress));
    }
}
