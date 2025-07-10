using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class AustraliaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Country.GetDefaultTaxCodeDescription(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber)); // Accounting consumption code

			list.OverridePair(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Res.GetString("OrgCusCode.CodeTypes.AUGSTCodeTemplate", "Australian Business Number (GST Registration Code)"));
			list.OverridePair(OrgCusCode.CodeTypes.CorporationCode, Res.GetString("OrgCusCode.CodeTypes.CorporationCodeAU", "Australian Corporation Number (Government Corporation Code)"));

			list.AddPair(OrgCusCode.CodeTypes.CustomsClientID, Res.GetString("OrgCusCode.CodeTypes.CustomsClientID", "CCID Customs Client Identifier"));
			list.AddPair(OrgCusCode.CodeTypes.CustomsCPPermitCode, Res.GetString("OrgCusCode.CodeTypes.CustomsCPPermitCode", "Customs Continuous Permit Code"));
			list.AddPair(OrgCusCode.CodeTypes.OneStopCode, Res.GetString("OrgCusCode.CodeTypes.OneStopCode", "1-Stop Trading Code"));
			list.AddPair(OrgCusCode.CodeTypes.MedicareID, Res.GetString("OrgCusCode.CodeTypes.MedicareID", "Medicare Card Number"));
			list.AddPair(OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, Res.GetString("OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser", "EXDOC EDI User"));
			list.AddPair(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, Res.GetString("OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber", "EXDOC Exporter Number"));
			list.AddPair(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber, Res.GetString("OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber", "NEXDOCS Exporter Number"));
			list.AddPair(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExternalID, Res.GetString("OrgCusCode.AustraliaCodeTypes.NexdocsExternalID", "NEXDOCS External ID"));
			list.AddPair(OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, Res.GetString("OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber", "EXDOC Establishment Number"));
			list.AddPair(OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber, Res.GetString("OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber", "EXDOC AMLC Performance Exporter Number"));
			list.AddPair(OrgCusCode.CodeTypes.eNettRegistrationNumber, Res.GetString("OrgCusCode.CodeTypes.eNettRegistrationNumber", "ComPay Registration Number"));
			list.AddPair(OrgCusCode.AustraliaCodeTypes.eParcelMerchantLocationID, Res.GetString("OrgCusCode.CodeTypes.eParcelMerchantLocationID", "eParcel Merchant Location ID"));
			list.AddPair(OrgCusCode.AustraliaCodeTypes.Diplomat, Res.GetString("OrgCusCode.AustraliaCodeTypes.Diplomat", "Diplomat/Embassy"));
			list.AddPair(OrgCusCode.CodeTypes.EUTracesID, Res.GetString("OrgCusCode.CodeTypes.EUTracesID", "EU TRACES Approval ID"));
			list.AddPair(OrgCusCode.AustraliaCodeTypes.ARN, Res.GetString("OrgCusCode.AustraliaCodeTypes.ARN", "ATO Reference Number (Australian Tax Office)"));
			list.AddPair(OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, Res.GetString("OrgCusCode.CodeTypes.TraderIdentificationNumber", "Trader Identification Number"));
			list.AddPair(OrgCusCode.AustraliaCodeTypes.AEO, Res.GetString("OrgCusCode.AustraliaCodeTypes.AEO", "Authorized Economic Operator"));
			list.AddPair(OrgCusCode.AustraliaCodeTypes.QuotaExporterNumber, Res.GetString("OrgCusCode.AustraliaCodeTypes.QuotaExporterNumber", "Quota Exporter Number"));
			list.AddPair(OrgCusCode.AustraliaCodeTypes.ApprovedArrangementNumber, Res.GetString("OrgCusCode.AustraliaCodeTypes.ApprovedArrangementNumber", "Approved Arrangement Number"));
			list.RemoveCode(OrgCusCode.CodeTypes.GovBusinessCode);
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokeragePrinter);
			list.RemoveCode(OrgCusCode.CodeTypes.ContainerChainCommunityCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Australia);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			result.Add(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Australia);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			return result;
		}
	}
}
