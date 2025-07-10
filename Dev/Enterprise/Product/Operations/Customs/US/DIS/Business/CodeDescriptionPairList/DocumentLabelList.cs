using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.DIS.Business
{
	[Flags]
	public enum OptionalDataTypes
	{
		None = 0,
		Invoice = 1,
		Commodity = 2,
		Certificate = 4,
		Permit = 8,
		ToxicSubstance = 16,
		BondData = 32,
		PackingList = 64
	}

	public static class OptionalDataTypeCodes
	{
		public const string None = "";
		public const string Invoice = "INV";
		public const string Commodity = "COM";
		public const string Certificate = "CER";
		public const string Permit = "PER";
		public const string ToxicSubstance = "TOX";
		public const string BondData = "BND";
		public const string PackingList = "PCK";
	}

	class DocumentLabelList
	{
		public static ZString TryToGetDefaultFormType(ZString docType)
		{
			switch (docType)
			{
				case Core.Constants.RefDocTypes.CommercialInvoice:
				case Core.Constants.RefDocTypes.Invoice:
					return "CBP02";
				case Core.Constants.RefDocTypes.PackingList:
					return "CBP01";
				case Core.Constants.RefDocTypes.MasterBill:
					return "CBP11";
				default:
					return ZString.Empty;
			}
		}

		public static OptionalDataTypes GetOptionalDataTypes(ZZRefCusCodeListCombined refCusCodeList)
		{
			var result = OptionalDataTypes.None;

			if (refCusCodeList != null)
			{
				var optionalDataValues = refCusCodeList.GetAttributesValues(RefCusCodeListAttributeTypes.Codes.OptionalData);

				foreach (var value in optionalDataValues)
				{
					switch (value)
					{
						case OptionalDataTypeCodes.BondData:
							result |= OptionalDataTypes.BondData;
							break;

						case OptionalDataTypeCodes.Certificate:
							result |= OptionalDataTypes.Certificate;
							break;

						case OptionalDataTypeCodes.Commodity:
							result |= OptionalDataTypes.Commodity;
							break;

						case OptionalDataTypeCodes.Invoice:
							result |= OptionalDataTypes.Invoice;
							break;

						case OptionalDataTypeCodes.PackingList:
							result |= OptionalDataTypes.PackingList;
							break;

						case OptionalDataTypeCodes.Permit:
							result |= OptionalDataTypes.Permit;
							break;

						case OptionalDataTypeCodes.ToxicSubstance:
							result |= OptionalDataTypes.ToxicSubstance;
							break;
					}
				}
			}

			return result;
		}

		public static bool IsOptionalDataVisible(OptionalDataTypes optionalDataTypes, OptionalDataTypes targetData)
		{
			return (optionalDataTypes & targetData) == targetData;
		}

		public static bool IsNMFSForms(string code)
		{
			return code.StartsWith("NMF", StringComparison.Ordinal);
		}
	}
}
