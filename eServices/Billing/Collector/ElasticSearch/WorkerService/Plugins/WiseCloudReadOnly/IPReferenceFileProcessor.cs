using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly;

internal static class IPReferenceFileProcessor
{
    public static Dictionary<string, List<string>> ProcessReferenceFile(string path, ILogger log)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Reference file path does not exist.", path);
        }

        using var reader = new StreamReader(path);
        if (reader.EndOfStream)
        {
            return new Dictionary<string, List<string>>();
        }

        var header = reader.ReadLine()?.Split(',');
        if (header == null || header.Length != 2 || header[0] != "Customer IP Address" || header[1] != "Firewall Object Name")
        {
            throw new InvalidDataException("Reference file has incorrect header");
        }

        var mapping = new Dictionary<string, List<string>>();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            var values = line.Split(',');
            if (values.Length < 2)
            {
                continue;
            }

            var ipPart = values[0].Trim();
            var firewallObject = values[1].Trim();
            var parts = firewallObject.Split('-');
            if (parts.Length < 2)
            {
                continue;
            }
            var customerCode = parts[1];

            if (ipPart.Contains('/'))
            {
                log.LogError($"Please replace subnet in {ipPart} with IP range.");
                continue;
            }
            if (ipPart.Contains('-'))
            {
                var range = ipPart.Split('-');
                var startIp = Ip4ToLong(range[0].Trim());
                var endIp = Ip4ToLong(range[1].Trim());
                for (var current = startIp; current <= endIp; current++)
                {
                    var ipAddress = LongToIP4(current);
                    Add(mapping, ipAddress, customerCode);
                }
            }
            else
            {
                Add(mapping, ipPart, customerCode);
            }
        }

        // remove ambiguous records
        return mapping
            .Where(kvp => kvp.Value.Count == 1)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private static void Add(Dictionary<string, List<string>> map, string ip, string code)
    {
        if (map.TryGetValue(ip, out var list))
        {
            list.Add(code);
        }
        else
        {
            map[ip] = new List<string> { code };
        }
    }

    private static bool IsValidIP(string ip)
    {
        var parts = ip.Split('.');
        if (parts.Length != 4) return false;
        return parts.All(p => int.TryParse(p, out var n) && n >= 0 && n <= 255);
    }

    private static long Ip4ToLong(string ip)
    {
        if (!IsValidIP(ip))
        {
            throw new ArgumentException("Invalid IP : " + ip);
        }
        var parts = ip.Split('.').Select(long.Parse).ToArray();
        return (parts[0] << 24) | (parts[1] << 16) | (parts[2] << 8) | parts[3];
    }

    private static string LongToIP4(long longIP)
    {
        var bytes = new int[4];
        for (var i = 0; i < 4; i++)
        {
            var num = (int)(longIP / Math.Pow(256, 3 - i));
            longIP -= (long)(num * Math.Pow(256, 3 - i));
            bytes[i] = num;
        }
        var ip = string.Join('.', bytes);
        if (!IsValidIP(ip))
        {
            throw new ArgumentException("LongToIP4 returns invalid IP " + ip);
        }
        return ip;
    }
}
