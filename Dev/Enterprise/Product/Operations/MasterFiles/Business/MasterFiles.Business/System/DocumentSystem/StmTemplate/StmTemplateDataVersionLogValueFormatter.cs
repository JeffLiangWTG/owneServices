using System;
using System.Globalization;
using System.IO;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmTemplateDataVersionLogValueFormatter : DataVersionLogValueFormatter
	{
		protected override string SerialiseBinaryValueToStringCore(string propertyName, byte[] binaryValue)
		{
			switch (propertyName)
			{
				case StmTemplateSchema.Constants.SO_Template:
					return binaryValue.Length == 0 ? string.Empty : Convert.ToBase64String(Compressor.Compress(binaryValue));
				default:
					throw new ArgumentException("Unsupported property: " + propertyName);
			}
		}

		protected override byte[] DeserialiseBinaryValueFromStringCore(string propertyName, string serialisedBinValue)
		{
			switch (propertyName)
			{
				case StmTemplateSchema.Constants.SO_Template:
					try
					{
						return Compressor.Uncompress(Convert.FromBase64String(serialisedBinValue));
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						throw new DataVersionLogValueFormatterException(Res.GetString("0DFC8681-3296-4A04-9AEB-E009CE68D20D", "This template is corrupted and unreadable."), ex);
					}
				default:
					throw new ArgumentException("Unsupported property: " + propertyName);
			}
		}

		protected override string GetFileExtensionFilterCore(string propertyName, string serialisedBinValue)
		{
			switch (propertyName)
			{
				case StmTemplateSchema.Constants.SO_Template:
					var binValue = DeserialiseBinaryValueFromString(propertyName, serialisedBinValue);
					return GetFileExtensionFilterCore(propertyName, binValue);
				default:
					throw new ArgumentException("Unsupported property: " + propertyName);
			}
		}

		protected override string GetFileExtensionFilterCore(string propertyName, byte[] binaryValue)
		{
			using (var excelInterface = ExcelInterfaceFactory.New())
			using (var stream = new MemoryStream(binaryValue))
			{
				try
				{
					excelInterface.LoadExcelFile(stream);
					return string.Format(CultureInfo.InvariantCulture, (NoResString)"Excel files (*.{0})|*.{0}", excelInterface.GetExtensionForExcelFromFile());
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					throw new DataVersionLogValueFormatterException(Res.GetString("91604847-3CC6-4C65-B55A-358FA6633DC1", "This template is not a compatible Excel file."), ex);
				}
			}
		}

		protected override bool CanSaveBinaryValueCore(string propertyName)
		{
			return string.Equals(propertyName, StmTemplateSchema.Constants.SO_Template, StringComparison.OrdinalIgnoreCase);
		}
	}
}
