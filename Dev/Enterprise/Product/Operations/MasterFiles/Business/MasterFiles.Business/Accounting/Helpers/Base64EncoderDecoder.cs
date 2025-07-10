using System;
using System.Text;

namespace Enterprise.MasterFiles.Business.Accounting
{
	public static class Base64EncoderDecoder
	{
		public static string Encode(string plainText)
		{
			var result = string.Empty;
			if (plainText != null)
			{
				var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
				result = Convert.ToBase64String(plainTextBytes);
			}
			return result;
		}

		public static string Decode(string base64EncodedData)
		{
			var result = string.Empty;
			if (base64EncodedData != null)
			{
				var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
				result = Encoding.UTF8.GetString(base64EncodedBytes);
			}
			return result;
		}
	}
}
