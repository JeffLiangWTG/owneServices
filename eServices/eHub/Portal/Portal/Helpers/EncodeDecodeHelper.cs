using System.Text;
using System.Security.Cryptography;

namespace CargoWise.eHub.Portal.Helpers
{
	public static class EncodeDecodeHelper
	{
		public static string EncodeBase64(string stringToEncode)
		{
			var stringBytes = System.Text.Encoding.UTF8.GetBytes(stringToEncode);
			return System.Convert.ToBase64String(stringBytes);
		}

		public static string DecodeBase64(string stringToDecode)
		{
			try
			{
				var base64EncodedBytes = System.Convert.FromBase64String(stringToDecode);
				return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
			}
			catch
			{
				return stringToDecode;
			}
		}

		public static string EncodeSHA256(string stringToEncode)
		{
			try
			{
				using (SHA256 sha256Hash = SHA256.Create())
				{
					var stringBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(stringToEncode));
					var builder = new StringBuilder();
					for (int i = 0; i < stringBytes.Length; i++)
					{
						builder.Append(stringBytes[i].ToString("x2"));
					}
					return builder.ToString();
				}
			}
			catch
			{
				return stringToEncode;
			}

		}
	}
}