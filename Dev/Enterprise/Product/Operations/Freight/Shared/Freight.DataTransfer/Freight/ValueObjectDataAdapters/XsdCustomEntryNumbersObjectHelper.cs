using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public static class XsdCustomEntryNumbersObjectHelper
	{
		public static Xsd.CustomsEntryNumberCollection ExportFromCusEntryNumCollection(CusEntryNumCollection customEntryNumbers)
		{
			Xsd.CustomsEntryNumberCollection result = new Xsd.CustomsEntryNumberCollection();

			foreach (CusEntryNumber entryNo in customEntryNumbers)
			{
				Xsd.CustomsEntryNumber xsdEntryNo = result.AddNew();
				xsdEntryNo.IsSpecified = true;
				xsdEntryNo.Country = entryNo.CE_RN_NKCountryCode;
				xsdEntryNo.Number = entryNo.CE_EntryNum;
				xsdEntryNo.Type = entryNo.CE_EntryType;
			}

			return result;
		}

		public static void ImportFromXsdCustomsEntryNumberCollection(
			Xsd.CustomsEntryNumberCollection xsdCustomEntryNumbers,
			ZGuid parentID, ZString parentTable,
			Func<CusEntryNumCollection> customEntryNumbersFunc,
			IValueObjectImportContext context)
		{
			CusEntryNumCollection customsEntryNumbers = null;
			if (xsdCustomEntryNumbers.Count > 0)
			{
				customsEntryNumbers = customEntryNumbersFunc();
			}
			foreach (Xsd.CustomsEntryNumber xsdEntryNo in xsdCustomEntryNumbers)
			{
				RefCountry country = RefCountry.LoadFromCountryCode(context.Factory, xsdEntryNo.Country);
				if (country != null)
				{
					bool hasErrors = false;
					if (xsdEntryNo.Number.Length > CusEntryNumSchema.CE_EntryNum.MaxLength)
					{
						context.Notify(new WarningNotification(WarningType.MaxLengthExceeded, Res.GetString("7495e70e-7304-44a4-972b-9ab7003497a3", "Customs Entry Number; '{0}'", xsdEntryNo.Number)));
						hasErrors = true;
					}
					if (xsdEntryNo.Type.Length > CusEntryNumSchema.CE_EntryType.MaxLength)
					{
						context.Notify(new WarningNotification(WarningType.MaxLengthExceeded, Res.GetString("ab704dd5-2ca2-4de8-88b0-43bc2928c71a", "Customs Entry Num. Type; '{0}'", xsdEntryNo.Type)));
						hasErrors = true;
					}

					if (!hasErrors && !FindMatchingCustomsEntryNumber(country, xsdEntryNo.Type, parentID))
					{
						CusEntryNumber entryNo = customsEntryNumbers.AddNew();
						entryNo.CE_ParentID = parentID;
						entryNo.CE_RN_NKCountryCode = country.Code;
						entryNo.CE_EntryNum = xsdEntryNo.Number.Trim(' ');
						entryNo.CE_EntryType = xsdEntryNo.Type;
						entryNo.CE_ParentTable = parentTable;
						entryNo.CE_EntryIsSystemGenerated = false;
					}
				}
			}
		}

		static bool FindMatchingCustomsEntryNumber(RefCountry country, string entryType, ZGuid parentID)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_RN_NKCountryCode, country.Code);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, parentID);

			return country.Factory.LoadTop1<CusEntryNumber>(query) != null;
		}
	}
}
