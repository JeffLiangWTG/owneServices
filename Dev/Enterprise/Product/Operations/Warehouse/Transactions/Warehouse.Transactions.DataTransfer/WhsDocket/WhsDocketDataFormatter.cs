using CargoWise.Common;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsDocketDataFormatter
	{
		public virtual void FormatCSVData(OCsvLine line)
		{
#if DEBUG
			TESTWasFormatCSVDataCalled = true;
#endif
			for (int i = 0; i < line.FieldValues.Length; i++)
			{
				line.FieldValues[i] = GetFormattedStringValue(line.FieldValues[i]);
			}
		}

		public virtual void FormatXsdDocketData(Xsd.WhsDocket xsdDocket)
		{
#if DEBUG
			TESTWasFormatXsdDocketDataCalled = true;
#endif
			xsdDocket.Identifier.Reference = GetFormattedStringValue(xsdDocket.Identifier.Reference);

			xsdDocket.DocketDetail.WarehouseCode = GetFormattedStringValue(xsdDocket.DocketDetail.WarehouseCode);
			xsdDocket.DocketDetail.CustomerReference = GetFormattedStringValue(xsdDocket.DocketDetail.CustomerReference);
			xsdDocket.DocketDetail.TransportReference = GetFormattedStringValue(xsdDocket.DocketDetail.TransportReference);
			xsdDocket.DocketDetail.TransportServiceLevel = GetFormattedStringValue(xsdDocket.DocketDetail.TransportServiceLevel);
			xsdDocket.DocketDetail.ServiceLevel = GetFormattedStringValue(xsdDocket.DocketDetail.ServiceLevel);
		}

		public virtual void FormatXsdDocketLineData(Xsd.WhsDocketLine xsdLine)
		{
#if DEBUG
			TESTWasFormatXsdDocketLineDataCalled = true;
#endif
			xsdLine.Product = GetFormattedStringValue(xsdLine.Product);
			xsdLine.ProductUQ = GetFormattedStringValue(xsdLine.ProductUQ);
			xsdLine.Description = GetFormattedStringValue(xsdLine.Description);
			xsdLine.LineAttributes.BondedEntryKey = GetFormattedStringValue(xsdLine.LineAttributes.BondedEntryKey);
			xsdLine.LineAttributes.PartAttribute1 = GetFormattedStringValue(xsdLine.LineAttributes.PartAttribute1);
			xsdLine.LineAttributes.PartAttribute2 = GetFormattedStringValue(xsdLine.LineAttributes.PartAttribute2);
			xsdLine.LineAttributes.PartAttribute3 = GetFormattedStringValue(xsdLine.LineAttributes.PartAttribute3);
			xsdLine.LineAttributes.CustomAttribute1 = GetFormattedStringValue(xsdLine.LineAttributes.CustomAttribute1);
			xsdLine.LineAttributes.CustomAttribute2 = GetFormattedStringValue(xsdLine.LineAttributes.CustomAttribute2);
			xsdLine.LineAttributes.CustomAttribute3 = GetFormattedStringValue(xsdLine.LineAttributes.CustomAttribute3);
			xsdLine.LineAttributes.CustomAttribute4 = GetFormattedStringValue(xsdLine.LineAttributes.CustomAttribute4);
			xsdLine.LineAttributes.CustomAttribute5 = GetFormattedStringValue(xsdLine.LineAttributes.CustomAttribute5);
			xsdLine.LineAttributes.CustomAttribute6 = GetFormattedStringValue(xsdLine.LineAttributes.CustomAttribute6);

			xsdLine.LineComments = GetFormattedStringValue(xsdLine.LineComments);

			xsdLine.CustomsData.EntryKey = GetFormattedStringValue(xsdLine.CustomsData.EntryKey);
			xsdLine.CustomsData.DeclarationReference = GetFormattedStringValue(xsdLine.CustomsData.DeclarationReference);
			xsdLine.CustomsData.CountryOfOrigin = GetFormattedStringValue(xsdLine.CustomsData.CountryOfOrigin);
			xsdLine.CustomsData.CustomsQuantityUnit = GetFormattedStringValue(xsdLine.CustomsData.CustomsQuantityUnit);
			xsdLine.CustomsData.BondedWhsQuantityUnit = GetFormattedStringValue(xsdLine.CustomsData.BondedWhsQuantityUnit);
			xsdLine.CustomsData.TILVCurrency = GetFormattedStringValue(xsdLine.CustomsData.TILVCurrency);
			xsdLine.CustomsData.AddInfo = GetFormattedStringValue(xsdLine.CustomsData.AddInfo);
		}

		protected virtual ZString GetFormattedStringValue(ZString value)
		{
			return value.ToUpper();
		}

#if DEBUG

		public bool TESTWasFormatXsdDocketDataCalled;
		public bool TESTWasFormatXsdDocketLineDataCalled;
		public bool TESTWasFormatCSVDataCalled;

#endif
	}
}
