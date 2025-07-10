using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public static class RefCusCodeListAttributeTypes
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Value in database, Value in database - Office Purpose, Value in DB")]
		public static class Codes
		{
			public const string MessageNum = "Message Number";
			public const string EMAIL = "EmailAddress";
			public const string AllowEmpty = "AllowEmpty";
			public const string Amount = "Amount";
			public const string AllowSpace = "AllowSpace";
			public const string PadWith = "PadWith";
			public const string Code = "Code";
			public const string DistrictOffices = "DistrictOffices";
			public const string ROOType = "ROOType";
			public const string Export = "Export";
			public const string Empty = "Empty";
			public const string Import = "Import";
			public const string CusApprovedExporter = "CusApprovedExporter";
			public const string IAllowCancel = "IAllowCancel";
			public const string OnlyForLine1 = "OnlyForLine1";
			public const string LicenceNumber = "LicenceNumber";
			public const string IUpdateEntryNumber = "IUpdateEntryNumber";
			public const string Schedule = "Schedule";
			public const string Pair = "Pair";
			public const string NotPair = "NotPair";
			public const string DepotType = "DepotType";
			public const string IUpdateReleaseDate = "IUpdateReleaseDate";
			public const string INotify = "INotify";
			public const string IUpdateCustomsStatus = "IUpdateCustomsStatus";
			public const string ISendEntryDocs = "ISendEntryDocs";
			public const string ICustomsCancelled = "ICustomsCancelled";
			public const string CustomsCleared = "CustomsCleared";
			public const string CustomsCommenced = "CustomsCommenced";
			public const string CustomsRejected = "CustomsRejected";
			public const string IAddEntryDocsToEDocs = "IAddEntryDocsToEDocs";
			public const string IUpdateProvisionalPaymentStatus = "IUpdateProvisionalPaymentStatus";
			public const string IsLiquidatedStatus = "IsLiquidatedStatus";
			public const string FreeTextRequired = "FreeTextRequired";
			public const string ICancelBondedWhs = "ICancelBondedWhs";
			public const string IUpdateBondedWhs = "IUpdateBondedWhs";
			public const string OptionalData = "OptionalData";
			public const string NVC = "NVC";
			public const string Port = "PORT";
			public const string VOC = "VOC";
			public const string ROLE = "ROLE";
			public const string MainCustomsOffice = "MainCustomsOffice";
			public const string Category = "Category";
			public const string USDISFormGroup = "USDISFormGroup";
			public const string USDISRequiredData = "USDISRequiredData";
			public const string USDISSupportedFileTypes = "USDISSupportedFileTypes";
			public const string Direction = "Direction";
			public const string Level = "Level";
			public const string ACTAV = "ACTAV";
			public const string System = "SYSTEM";
			public const string Company = "COMPANY";
			public const string Countable = "COUNTABLE";
			public const string USFSISEstablishmentNumberCompany = "USFSISEstablishmentNumberCompany";
			public const string USFSISEstablishmentNumberStreet = "USFSISEstablishmentNumberStreet";
			public const string USFSISEstablishmentNumberCity = "USFSISEstablishmentNumberCity";
			public const string USFSISEstablishmentNumberState = "USFSISEstablishmentNumberState";
			public const string USFSISEstablishmentNumberZip = "USFSISEstablishmentNumberZip";
			public const string USFSISEstablishmentNumberPhone = "USFSISEstablishmentNumberPhone";
			public const string IsLicense = "IsLicense";
			public const string DisplayCode = "DisplayCode";
			public const string CustomsOffice = "CUSTOMSOFFICE";
			public const string CodeSuffix = "CodeSuffix";
			public const string RoRoLocation = "RORO";
			public const string IUpdateCIQStatus = "IUpdateCIQStatus";
			public const string Permit = "Permit";
			public const string IPostCustomsAPInvoice = "IPostCustomsAPInvoice";
			public const string ControlAgency = "CONTROLAGENCY";
			public const string IsJointGuarantor = "IsJointGuarantor";
			public const string GVM = "GVM";
			public const string MEU = "MEU";
			public const string LicenseType = "LicenseType";
			public const string LicenseTypeCodes = "LicenseTypeCodes";
			public const string Department = "Dept";
			public const string USDISPackageCategory = "USDISPackageCategory";
			public const string USDISDocCode = "USDISDocCode";
			public const string USSPIException = "SPI_Exception";
			public const string IsExport = "IsExport";
			public const string IsImport = "IsImport";
			public const string IsExpressDelivery = "IsExpressDelivery";
			public const string IsDropDownValue = "IsDropDownValue";
			public const string IUpdateEntryPaymentInfo = "IUpdateEntryPaymentInfo";
			public const string IUpdateProvPayLiquidationDate = "IUpdateProvPayLiquidationDate";
			public const string AOSConformity = "AOSConformity";
			public const string AOSReplacement = "AOSReplacement";
			public const string AOSEvidence = "AOSEvidence";
			public const string AOSRetention = "AOSRetention";
			public const string IMarkEntryPayInfoAwaitingResp = "IMarkEntryPayInfoAwaitingResp";
			public const string INoFurtherActionRequired = "INoFurtherActionRequired";
			public const string IActionRequiredDespiteActionID = "IActionRequiredDespiteActionID";
			public const string IMakeBondCloseDisposition = "IMakeBondCloseDisposition";
			public const string INeutralInBondDisposition = "INeutralInBondDisposition";
			public const string IsExamDisposition = "IsExamDisposition";
			public const string IsHoldDisposition = "IsHoldDisposition";
			public const string IsHoldExamRemovedDisposition = "IsHoldExamRemovedDisposition";
			public const string HoldRemovedExamCompletedMapCode = "HoldRemovedExamCompletedMapCode";
			public const string IMakeBondCloseDisposition6263 = "IMakeBondCloseDisposition6263";
			public const string IExecuteAutoBilling = "IExecuteAutoBilling";
			public const string Type = "Type";
			public const string NoMerge = "NoMerge";
			public const string Usage = "Usage";
			public const string PortValidType = "PortValidType";
			public const string StatementText = "StatementText";
			public const string BankAccountNo = "BankAccountNo";
			public const string Purpose = "Purpose";
			public const string Preference = "Preference";
			public const string AuthCode = "AuthCode";
			public const string PGAIUCAgency = "PGAIUCAgency";
			public const string CustomsCode = "CustomsCode";
			public const string CADocumentTypePGAType = "PGA";
			public const string CADocumentTypeValuesAllowed = "ValuesAllowed";
			public const string CertificateType = "CertificateType";
			public const string Unlading = "Unlading";
			public const string State = "State";
			public const string Address1 = "Address1";
			public const string Address2 = "Address2";
			public const string Address3 = "Address3";
			public const string City = "City";
			public const string PostCode = "PostCode";
			public const string IsNational = "IsNational";
			public const string AllowECCNNLR = "AllowECCNNLR";
		}

		public static ZString[] GetAttributeValuesFor(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date, ZString code, ZString attributeName)
		{
			ZString[] result = null;
			if (factory != null && !dataGroupingCode.IsEmpty && !codeType.IsEmpty && date.IsValid && !code.IsEmpty && !attributeName.IsEmpty)
			{
				factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "RefCusCodeListAttributeValuesFor{0}_{1}_{2}_{3}", dataGroupingCode, codeType, code, date.Date.ToString("yyMMdd", CultureInfo.InvariantCulture)), () =>
				{
					var dictionaryList = new Dictionary<ZString, List<ZString>>();
					foreach (var loadedCode in ZZRefCusCodeListCombined.Loader.Load(factory, dataGroupingCode, codeType, date, new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, code), true))
					{
						foreach (var attribute in loadedCode.Attributes.Cast<ZZRefCusCodeListAttributeCombined>())
						{
							var name = attribute.ZZE_ZXE_NKName.ToUpper();
							List<ZString> list;
							if (!dictionaryList.TryGetValue(name, out list))
							{
								list = new List<ZString>();
								dictionaryList.Add(name, list);
							}
							list.Add(attribute.ZZE_Value);
						}
					}
					var dictionary = new Dictionary<ZString, ZString[]>();
					foreach (var pair in dictionaryList.OrderBy(x => x.Key))
					{
						var list = pair.Value;
						list.Sort();
						dictionary.Add(pair.Key, list.ToArray());
					}
					return dictionary;
				}).TryGetValue(attributeName.ToUpper(), out result);
			}
			return result ?? Array.Empty<ZString>();
		}
	}
}
