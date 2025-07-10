using System;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.Customs.NZ.Business.MessageBuilders
{
	public static class HMAC256
	{
		public static string GenerateNewHMAC(string pin, string xmlMsg)
		{
			var hmc = new HMACSHA256(Encoding.ASCII.GetBytes(pin));
			byte[] hmres = hmc.ComputeHash(Encoding.ASCII.GetBytes(xmlMsg));
			return Convert.ToBase64String(hmres);
		}
	}
}
