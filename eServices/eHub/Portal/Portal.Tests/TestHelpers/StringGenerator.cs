using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Portal.Tests
{
	public class StringGenerator
	{
		public static string GenerateString(int size)
		{
			string pattern = "This is a test string.";

			var sb = new StringBuilder();

			while (sb.Length < size)
			{
				sb.Append(pattern);
			}

			return sb.ToString().Substring(0, size);
		}
	}
}
