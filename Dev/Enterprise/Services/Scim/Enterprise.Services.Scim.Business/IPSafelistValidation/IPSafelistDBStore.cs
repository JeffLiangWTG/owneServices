using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.Services.Scim.Business
{
	public class IPSafelistDBStore : IIPSafelistDBStore
	{
		readonly IIPSafelistHelper Helper;

		public IPSafelistDBStore(IIPSafelistHelper helper)
		{
			Helper = helper ?? throw new ArgumentNullException(nameof(helper));
		}

		public async void UpdateSafelistToDB()
		{
			var ips = await Helper.GetSafelistedIps();

			if (ips == null || ips.Length == 0)
			{
				throw new ArgumentNullException(nameof(ips));
			}

			foreach (var ip in ips)
			{
				if (string.IsNullOrWhiteSpace(ip) || !IsValidCidrNotation(ip))
				{
					throw new InvalidOperationException($"Invalid IP address format: {ip}");
				}
			}

			var factory = new BusinessObjectFactory();
			var ipJson = JsonConvert.SerializeObject(ips);

			var existingRecord = factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, Config.Constants.ScimIpSafelistConstant)).FirstOrDefault();

			StmData stmData;
			if (existingRecord != null)
			{
				stmData = existingRecord;
			}
			else
			{
				stmData = factory.New<StmData>();
				stmData.SD_Name = Config.Constants.ScimIpSafelistConstant;
				stmData.SD_Type = "STR";
			}

			stmData.SD_BinaryValue = System.Text.Encoding.UTF8.GetBytes(ipJson);

			factory.Save();
		}

		bool IsValidCidrNotation(string cidr)
		{
			if (string.IsNullOrWhiteSpace(cidr))
			{
				return false;
			}

			var parts = cidr.Split('/');
			if (parts.Length != 2)
			{
				return false;
			}

			if (!System.Net.IPAddress.TryParse(parts[0], out var ipAddress))
			{
				return false;
			}

			if (!int.TryParse(parts[1], out int prefixLength))
			{
				return false;
			}

			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
			{
				return prefixLength >= 0 && prefixLength <= 32;
			}
			else if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
			{
				return prefixLength >= 0 && prefixLength <= 128;
			}

			return false;
		}
	}
}
