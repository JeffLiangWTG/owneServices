using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Portal.Models.Extensions
{
	public class UtilExtensions
	{
		public static string FormatAsseblyName(string text)
		{
			string[] parts = text.Split(',');
			if (parts.Length > 0)
			{
				return parts[0].Replace(".", " ");
			}

			return text;
		}
	}
}