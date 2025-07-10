using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Common
{
	public static class CommonHelper
	{
		public static int RandomInteger(int min = int.MinValue, int max = int.MaxValue)
		{
			var range = max - min + 1;
			var randomNumberBytes = new byte[4];

			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(randomNumberBytes);
			}

			return (Math.Abs(BitConverter.ToInt32(randomNumberBytes, 0)) % range) + min;
		}

		public static string RandomCharAndDigit(int length)
		{
			var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var sb = new StringBuilder(length);

			for (var i = 0; i < length; i++)
			{
				var randomIndex = RandomInteger(0, 1000) % chars.Length;
				sb.Append(chars[randomIndex]);
			}

			return sb.ToString();
		}
	}
}
