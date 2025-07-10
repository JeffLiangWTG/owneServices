using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using XmlWriter = CargoWise.RefDbRepo.Common.UniversalXmlWriter.XmlWriter;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public static class UniversalDataHelper
	{
		public static class Constants
		{
			public const string ILTariffs = "ILTariffs";
			public const string ILTradeGroups = "ILTradeGroups";
			public const string TariffType = "1";
			public const string Israel = "IL";
			public const string English = "EN";
			public const string VZR = "VZR";
			public const string VAT = "VAT";
			public const string CheckDigit = "CheckDigit";
			public const string CU1 = "CU1";
			public const string ValidState = "1";
		}

		public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);

		public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

		public static void ExportToXml<T>(XmlWriter xmlWriter, IEnumerable<T> collection, string filePath)
		{
			foreach (var data in collection)
			{
				xmlWriter.PopulateData(data);
			}

			xmlWriter.SaveXml(filePath);
		}

		public static void InitializeWriter(XmlWriter xmlWriter, DateTime publishTime, string dataSourceName, UpdateType updateType)
		{
			xmlWriter.SetDataSource(dataSourceName);
			xmlWriter.SetPublicationTime(publishTime);
			xmlWriter.SetUpdateType(updateType);
		}

		public static string HashPassword(string password)
		{
			var key = Encoding.UTF8.GetBytes("HMACKeyForLog");
			using(var hmac = new HMACSHA256(key))
			{
				var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
				return Convert.ToBase64String(hashBytes);
			}
		}

	}
}
