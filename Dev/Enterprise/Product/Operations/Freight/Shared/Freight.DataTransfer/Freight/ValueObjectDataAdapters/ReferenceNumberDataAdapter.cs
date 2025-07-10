using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public static class ReferenceNumberDataAdapter
	{
		public static void ImportReferenceNumbers(CusEntryNumAdditionalReferenceCollection numbers, Xsd.ReferenceNumberCollection value, IValueObjectImportContext context)
		{
			if (value.IsSpecified)
			{
				CodeDescriptionPairList entryNumberTypesList = null;

				IAdditionalReferenceNumberTypeProvider numberTypeProvider = numbers.Master as IAdditionalReferenceNumberTypeProvider;
				if (numberTypeProvider != null)
				{
					entryNumberTypesList = numberTypeProvider.GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.AdditionalReferenceNumber, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}

				if (entryNumberTypesList == null)
				{
					entryNumberTypesList = CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}

				string[] nonUniqueEntryNumberTypes = entryNumberTypesList.NonUnique();
				foreach (Xsd.ReferenceNumber valueNumber in value)
				{
					bool isUnique = !nonUniqueEntryNumberTypes.Contains<string>(valueNumber.Type);

					CusEntryNumber numberToBeUpdated = null;

					foreach (CusEntryNumber number in numbers)
					{
						if ((number.CE_RN_NKCountryCode == valueNumber.Country.Value) &&
							(isUnique && number.CE_EntryType == valueNumber.Type ||
							!isUnique && number.CE_EntryType == valueNumber.Type && number.CE_EntryNum == valueNumber.Number))
						{
							numberToBeUpdated = number;
							break;
						}
					}

					if (numberToBeUpdated == null)
					{
						numberToBeUpdated = numbers.AddNew();
					}

					context.SetPropertyInfoValueIfValueNotEmpty(numberToBeUpdated.CE_EntryTypeInfo, valueNumber.Type);
					context.SetPropertyInfoValueIfValueNotEmpty(numberToBeUpdated.CE_EntryNumInfo, valueNumber.Number);
					context.SetPropertyInfoValue(numberToBeUpdated.CE_RN_NKCountryCodeInfo, valueNumber.Country.Value);
				}
			}
		}

		public static void ExportReferenceNumbers(CusEntryNumAdditionalReferenceCollection numbers, Xsd.ReferenceNumberCollection value, IValueObjectExportContext context)
		{
			foreach (CusEntryNumber number in numbers)
			{
				Xsd.ReferenceNumber valueNumber = value.AddNew();
				valueNumber.Type = number.CE_EntryType;
				valueNumber.Number = number.CE_EntryNum;

				if (!number.CE_RN_NKCountryCode.IsEmpty)
				{
					valueNumber.Country.Value = number.CE_RN_NKCountryCode;

					RefCountry country = numbers.Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, number.CE_RN_NKCountryCode));

					if (country != null)
					{
						valueNumber.Country.Name = country.RN_DescMultilingual;
					}
				}
			}
		}
	}
}
