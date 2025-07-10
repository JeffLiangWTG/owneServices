using System;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	internal class StringToGuidMapping
	{
		public static Guid GetGuidForString(string value)
		{
			var valueToConvert = (value ?? "").ToUpperInvariant();

			using var sha256 = SHA256.Create();
			var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(valueToConvert));
			var guidBytes = new byte[16];
			Buffer.BlockCopy(hashBytes, 0, guidBytes, 0, 16);   // Take the first 16 bytes of the hash to create a GUID
			return new Guid(guidBytes);
		}
	}
}
