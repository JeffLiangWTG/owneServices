using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	static class CusCodeDataTypeAndCodeListProvider
	{
		public static ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = GetTypeListForJobDeclaration();
					break;
				case CusEntryLineSchema.Constants.Prefix:
					result = GetTypeListForCusEntryLine();
					break;
				case CusEntryHeaderSchema.Constants.Prefix:
					result = GetTypeListForCusEntryHeader();
					break;
				case CusAddInfoSchema.Constants.Prefix:
					result = GetTypeListForCusAddInfo();
					break;
				case CusDecHouseBillSchema.Constants.Prefix:
					result = GetTypeListForBill();
					break;
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetTypeListForJobComInvoiceLine();
					break;
				case JobComInvoiceHeaderSchema.Constants.Prefix:
					result = GetTypeListForJobComInvoiceHeader();
					break;
				case CusClassPartPivotSchema.Constants.Prefix:
					result = GetTypeListForCusClassPartPivot();
					break;
			}
			return result;
		}

		public static ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = GetCodeListForJobDeclaration();
					break;
				case CusEntryHeaderSchema.Constants.Prefix:
					result = GetCodeListForCusEntryHeader();
					break;
				case CusAddInfoSchema.Constants.Prefix:
					result = GetCodeListForCusAddInfo();
					break;
				case CusDecHouseBillSchema.Constants.Prefix:
					result = GetCodeListForBill();
					break;
				case CusContainerSchema.Constants.Prefix:
					result = GetCodeListForContainer();
					break;
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetCodeListForJobComInvoiceLine();
					break;
				case JobComInvoiceHeaderSchema.Constants.Prefix:
					result = GetCodeListForJobComInvoiceHeader();
					break;
				case CusClassPartPivotSchema.Constants.Prefix:
					result = GetCodeListForCusClassPartPivot();
					break;
			}
			return result;
		}

		#region Implementation

		static CodeDescriptionPairList GetTypeListForJobDeclaration()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.ContractNumber, CusCodeDataTypeList.Descriptions.ContractNumber);
			return result;
		}

		static CodeDescriptionPairList GetTypeListForCusEntryLine()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.PSCReasonCodes, CusCodeDataTypeList.Descriptions.PSCReasonCodes);
			return result;
		}

		static CodeDescriptionPairList GetTypeListForCusEntryHeader()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.ReconEntryOriginalCharge, CusCodeDataTypeList.Descriptions.ReconEntryOriginalCharge);
			result.AddPair(CusCodeDataTypeList.Codes.PSCReasonCodes, CusCodeDataTypeList.Descriptions.PSCReasonCodes);
			return result;
		}

		static CodeDescriptionPairList GetTypeListForCusClassPartPivot()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.CensusWarningOverride, CusCodeDataTypeList.Descriptions.CensusWarningOverride);
			return result;
		}

		static CodeDescriptionPairList GetTypeListForCusAddInfo()
		{
			var result = new CodeDescriptionPairList();
			// Don't export DeliveryOrderHeader.Constants.CusCodeDataBill as it's internal printing data
			result.AddPair(CusCodeDataTypeList.Codes.AffirmationCode, CusCodeDataTypeList.Descriptions.AffirmationCode);
			result.AddPair(CusCodeDataTypeList.Codes.CensusWarningOverride, CusCodeDataTypeList.Descriptions.CensusWarningOverride);
			result.AddPair(CusCodeDataTypeList.Codes.RegoNumber, CusCodeDataTypeList.Descriptions.RegoNumber);
			result.AddPair(CusCodeDataTypeList.Codes.VNEAdditionalNumber, CusCodeDataTypeList.Descriptions.VNEAdditionalNumber);
			result.AddPair(CusCodeDataTypeList.Codes.AMSLotCode, CusCodeDataTypeList.Descriptions.AMSLotCode);
			return result;
		}

		static CodeDescriptionPairList GetTypeListForBill()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.HouseBillRefNo, CusCodeDataTypeList.Descriptions.HouseBillRefNo);
			return result;
		}

		static CodeDescriptionPairList GetTypeListForJobComInvoiceHeader()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.RelatedDocument, CusCodeDataTypeList.Descriptions.RelatedDocument);

			return result;
		}

		static CodeDescriptionPairList GetTypeListForJobComInvoiceLine()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.InvoiceLineNumberRange, CusCodeDataTypeList.Descriptions.InvoiceLineNumberRange);
			result.AddPair(CusCodeDataTypeList.Codes.Fee, CusCodeDataTypeList.Descriptions.Fee);
			result.AddPair(CusCodeDataTypeList.Codes.Misc, CusCodeDataTypeList.Descriptions.Misc);
			result.AddPair(CusCodeDataTypeList.Codes.ReconEntryOriginalCharge, CusCodeDataTypeList.Descriptions.ReconEntryOriginalCharge);
			result.AddPair(CusCodeDataTypeList.Codes.LicenceAndPermit, CusCodeDataTypeList.Descriptions.LicenceAndPermit);
			result.AddPair(CusCodeDataTypeList.Codes.CensusWarningOverride, CusCodeDataTypeList.Descriptions.CensusWarningOverride);
			result.AddPair(CusCodeDataTypeList.Codes.DrawbackAdditionalExportTariffNumber, CusCodeDataTypeList.Descriptions.DrawbackAdditionalExportTariffNumber);

			return result;
		}

		static ICodeDescriptionPairList GetCodeListForCusEntryHeader()
		{
			return CusFeeCodeConstants.GetAccountingClassFeeCodeList(new BusinessObjectFactory());
		}

		static ICodeDescriptionPairList GetCodeListForJobComInvoiceHeader()
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(new RelatedDocumentIdentifierList());
			return result;
		}

		static ICodeDescriptionPairList GetCodeListForContainer()
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(new MiscCusCodeDataCodeList());
			return result;
		}

		static ICodeDescriptionPairList GetCodeListForBill()
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(new ReferenceQualifierList());
			result.AddRange(new MiscCusCodeDataCodeList());
			return result;
		}

		static ICodeDescriptionPairList GetCodeListForJobDeclaration()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.ContractNumber, CusCodeDataTypeList.Descriptions.ContractNumber);
			return result;
		}

		static ICodeDescriptionPairList GetCodeListForCusClassPartPivot()
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(new CensusWarningCodeList());
			return result;
		}

		static ICodeDescriptionPairList GetCodeListForJobComInvoiceLine()
		{
			var factory = new BusinessObjectFactory();
			var result = new CodeDescriptionPairList();
			result.AddRange(new CensusWarningCodeList());
			result.AddRange(LicencePermitTypeList.GetLicencePermitTypeList(factory));
			result.AddRange(new MiscCusCodeDataCodeList());
			result.AddRange(CusFeeCodeConstants.GetAccountingClassFeeCodeList(factory));
			result.AddPair(CusCodeDataTypeList.Codes.DrawbackAdditionalExportTariffNumber, CusCodeDataTypeList.Descriptions.DrawbackAdditionalExportTariffNumber);
			result.AddPair(CusCodeDataTypeList.Codes.InvoiceLineNumberRange, CusCodeDataTypeList.Descriptions.InvoiceLineNumberRange);
			return result;
		}

		static ICodeDescriptionPairList GetCodeListForCusAddInfo()
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(new AffirmationCodeConstants());
			result.AddRange(new StandAlonePriorNotice());
			result.AddRange(new RegoNumberCodeList());
			result.AddRange(new Customs.Business.BillTypeList());
			return result;
		}

		#endregion
	}
}
