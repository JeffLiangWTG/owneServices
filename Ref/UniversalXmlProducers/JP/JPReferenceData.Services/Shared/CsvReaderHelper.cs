using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class CsvReaderHelper
	{
		public static bool TryRead(string filePath, out IList<string> records, string encoding = "Shift-JIS")
		{
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				records = new List<string>();
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				using (var streamReader = new StreamReader(filePath, Encoding.GetEncoding(encoding)))
				{
					var config = new CsvHelper.Configuration.Configuration(CultureInfo.InvariantCulture);
					using (var csv = new CsvReader(streamReader, config))
					{
						while (csv.Read())
						{
							records.Add(csv.Context.RawRecord);
						}
					}
					records = records.Select(record => record.Replace("\r", "").Replace("\n", "")).ToList();
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorWriter.WriteException(ex);
				records = null;
				return false;
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}
	}
}
