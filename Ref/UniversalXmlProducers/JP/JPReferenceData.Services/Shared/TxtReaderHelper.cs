using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class TxtReaderHelper
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
					string line;
					while ((line = streamReader.ReadLine()) != null)
					{
						records.Add(line.Replace("\r", "").Replace("\n", ""));
					}
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
