using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCountryRequiredDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRD_RN_NKDestination()
		{
			RefCountry countryAT = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Austria);
			RefCountryRequiredDocument requiredDocument = countryAT.RequiredDocuments.AddNew();

			requiredDocument.Validation.ValidateRD_RN_NKDestination();
			AssertNoErrors(requiredDocument.RD_RN_NKDestinationInfo);

			requiredDocument.RD_RN_NKDestination = Constants.CountryCodes.Austria;
			requiredDocument.Validation.ValidateRD_RN_NKDestination();
			AssertNoErrors(requiredDocument.RD_RN_NKDestinationInfo);

			requiredDocument.RD_RN_NKDestination = "";
			requiredDocument.Validation.ValidateRD_RN_NKDestination();
			AssertNoErrors(requiredDocument.RD_RN_NKDestinationInfo);

			requiredDocument.RD_RN_NKOrigin = Constants.CountryCodes.Austria;
			requiredDocument.RD_RN_NKDestination = Constants.CountryCodes.Austria;
			requiredDocument.Validation.ValidateRD_RN_NKDestination();
			AssertNoErrors(requiredDocument.RD_RN_NKDestinationInfo);

			requiredDocument.RD_RN_NKOrigin = "AU";
			requiredDocument.RD_RN_NKDestination = "US";
			requiredDocument.Validation.ValidateRD_RN_NKDestination();
			AssertHasError(requiredDocument.RD_RN_NKDestinationInfo, "Origin and Destination cannot both be different countries/regions from the one selected.");
		}

		public void TestCheckRD_RN_NKOrigin()
		{
			RefCountry countryAT = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Austria);
			RefCountryRequiredDocument requiredDocument = countryAT.RequiredDocuments.AddNew();

			requiredDocument.Validation.ValidateRD_RN_NKOrigin();
			AssertNoErrors(requiredDocument.RD_RN_NKOriginInfo);

			requiredDocument.RD_RN_NKOrigin = Constants.CountryCodes.Austria;
			requiredDocument.Validation.ValidateRD_RN_NKOrigin();
			AssertNoErrors(requiredDocument.RD_RN_NKOriginInfo);

			requiredDocument.RD_RN_NKOrigin = "";
			requiredDocument.Validation.ValidateRD_RN_NKOrigin();
			AssertNoErrors(requiredDocument.RD_RN_NKOriginInfo);

			requiredDocument.RD_RN_NKOrigin = Constants.CountryCodes.Austria;
			requiredDocument.RD_RN_NKDestination = Constants.CountryCodes.Austria;
			requiredDocument.Validation.ValidateRD_RN_NKOrigin();
			AssertNoErrors(requiredDocument.RD_RN_NKOriginInfo);

			requiredDocument.RD_RN_NKOrigin = "AU";
			requiredDocument.RD_RN_NKDestination = "US";
			requiredDocument.Validation.ValidateRD_RN_NKOrigin();
			AssertHasError(requiredDocument.RD_RN_NKOriginInfo, "Origin and Destination cannot both be different countries/regions from the one selected.");
		}

		public void TestCheckRD_DocType()
		{
			RefCountryRequiredDocument requiredDocument = Factory.New<RefCountryRequiredDocument>();
			requiredDocument.Validation.ValidateRD_DocType();
			AssertHasErrors(requiredDocument.RD_DocTypeInfo);

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_DocType = "XXX";
			requiredDocument.RD_DocType = docType.RT_DocType;
			requiredDocument.Validation.ValidateRD_DocType();
			AssertHasErrors(requiredDocument.RD_DocTypeInfo);

			docType.RT_ReferenceType = Constants.ReferenceTypes.SupplyChainLogistics;
			docType.RT_IsActive = true;
			requiredDocument.Validation.ValidateRD_DocType();
			AssertNoErrors(requiredDocument.RD_DocTypeInfo);
		}

		public void TestCheckDuplicateRow()
		{
			RefCountry country1 = Factory.New<RefCountry>();
			country1.Code = "UA";
			RefCountryRequiredDocument requiredDocument1 = GetNewRequiredDoc(country1.Code, "BB");
			RefCountryRequiredDocument requiredDocument2 = GetNewRequiredDoc(country1.Code, "CC");
			country1.RequiredDocuments.Add(requiredDocument1);
			country1.RequiredDocuments.Add(requiredDocument2);
			requiredDocument1.Validation.ValidateAll();
			requiredDocument2.Validation.ValidateAll();
			AssertNoRowError(requiredDocument1, "Duplicate: A Required Document with this Document Type, Origin, Destination and Transport Mode already exists in this list.");
			AssertNoRowError(requiredDocument2, "Duplicate: A Required Document with this Document Type, Origin, Destination and Transport Mode already exists in this list.");

			RefCountryRequiredDocument requiredDocument3 = GetNewRequiredDoc(country1.Code, "BB");
			country1.RequiredDocuments.Add(requiredDocument3);
			requiredDocument1.Validation.ValidateAll();
			requiredDocument2.Validation.ValidateAll();
			requiredDocument3.Validation.ValidateAll();
			AssertHasRowError(requiredDocument1, "Duplicate: A Required Document with this Document Type, Origin, Destination and Transport Mode already exists in this list.");
			AssertNoRowError(requiredDocument2, "Duplicate: A Required Document with this Document Type, Origin, Destination and Transport Mode already exists in this list.");
			AssertHasRowError(requiredDocument3, "Duplicate: A Required Document with this Document Type, Origin, Destination and Transport Mode already exists in this list.");
		}

		RefCountryRequiredDocument GetNewRequiredDoc(ZString orig, ZString dest)
		{
			RefCountryRequiredDocument result = Factory.New<RefCountryRequiredDocument>();
			result.RD_RN_NKDestination = orig;
			result.RD_RN_NKOrigin = dest;
			result.RD_DocType = "XXX";
			return result;
		}

		const string err1 = "Doc Usage should be either Import, Export or Both if Origin and Destination are different Countries/Regions.";
		const string err2 = "Doc Usage should be Domestic if Origin and Destination are the same Country/Region.";

		public void TestCheckRD_DocUsage()
		{
			RefCountryRequiredDocument requiredDocument = Factory.New<RefCountryRequiredDocument>();

			RefCountry countryAT = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Austria);
			countryAT.RequiredDocuments.Add(requiredDocument);
			ZString aTCode = Constants.CountryCodes.Austria;

			requiredDocument.Validation.ValidateRD_DocUsage();
			AssertHasErrors(requiredDocument.RD_DocUsageInfo);

			requiredDocument.RD_DocUsage = "XXX";
			requiredDocument.Validation.ValidateRD_DocUsage();
			AssertHasErrors(requiredDocument.RD_DocUsageInfo);

			//DOM- NO ERROR
			ChangeRequiredDocument(requiredDocument, aTCode, aTCode, JobRequiredDocument.DocUsage.Domestic, false, false);
			ChangeRequiredDocument(requiredDocument, "", "", JobRequiredDocument.DocUsage.Domestic, false, false);
			//DOM- ERROR
			ChangeRequiredDocument(requiredDocument, aTCode, "", JobRequiredDocument.DocUsage.Domestic, true, false);
			ChangeRequiredDocument(requiredDocument, aTCode, Constants.CountryCodes.Argentina, JobRequiredDocument.DocUsage.Domestic, true, false);
			ChangeRequiredDocument(requiredDocument, "", aTCode, JobRequiredDocument.DocUsage.Domestic, true, false);
			//IMP- NO ERROR
			ChangeRequiredDocument(requiredDocument, aTCode, "", JobRequiredDocument.DocUsage.Import, false, false);
			ChangeRequiredDocument(requiredDocument, "", aTCode, JobRequiredDocument.DocUsage.Import, false, false);
			ChangeRequiredDocument(requiredDocument, "", "", JobRequiredDocument.DocUsage.Import, false, false);
			ChangeRequiredDocument(requiredDocument, aTCode, Constants.CountryCodes.Argentina, JobRequiredDocument.DocUsage.Import, false, false);
			//IMP- ERROR
			ChangeRequiredDocument(requiredDocument, aTCode, aTCode, JobRequiredDocument.DocUsage.Import, false, true);
			//EXP- NO ERROR
			ChangeRequiredDocument(requiredDocument, aTCode, "", JobRequiredDocument.DocUsage.Export, false, false);
			ChangeRequiredDocument(requiredDocument, "", aTCode, JobRequiredDocument.DocUsage.Export, false, false);
			ChangeRequiredDocument(requiredDocument, "", "", JobRequiredDocument.DocUsage.Export, false, false);
			ChangeRequiredDocument(requiredDocument, aTCode, Constants.CountryCodes.Argentina, JobRequiredDocument.DocUsage.Export, false, false);
			//EXP- ERROR
			ChangeRequiredDocument(requiredDocument, aTCode, aTCode, JobRequiredDocument.DocUsage.Import, false, true);
			//All- NO ERROR
			ChangeRequiredDocument(requiredDocument, aTCode, "", JobRequiredDocument.DocUsage.All, false, false);
			ChangeRequiredDocument(requiredDocument, "", aTCode, JobRequiredDocument.DocUsage.All, false, false);
			ChangeRequiredDocument(requiredDocument, "", "", JobRequiredDocument.DocUsage.All, false, false);
			//ALL- ERROR
			ChangeRequiredDocument(requiredDocument, aTCode, Constants.CountryCodes.Argentina, JobRequiredDocument.DocUsage.All, true, false);
			ChangeRequiredDocument(requiredDocument, aTCode, aTCode, JobRequiredDocument.DocUsage.All, false, true);
			//BOTH- NO ERROR
			ChangeRequiredDocument(requiredDocument, aTCode, "", JobRequiredDocument.DocUsage.Both, false, false);
			ChangeRequiredDocument(requiredDocument, "", aTCode, JobRequiredDocument.DocUsage.Both, false, false);
			ChangeRequiredDocument(requiredDocument, "", "", JobRequiredDocument.DocUsage.Both, false, false);
			ChangeRequiredDocument(requiredDocument, aTCode, Constants.CountryCodes.Argentina, JobRequiredDocument.DocUsage.Both, false, false);
			//BOTH- ERROR
			ChangeRequiredDocument(requiredDocument, aTCode, aTCode, JobRequiredDocument.DocUsage.Both, false, true);
		}

		void ChangeRequiredDocument(RefCountryRequiredDocument requiredDocument, string docDest, string docOrig, string docUse, bool hasErr1, bool hasErr2)
		{
			requiredDocument.RD_RN_NKDestination = docDest;
			requiredDocument.RD_RN_NKOrigin = docOrig;
			requiredDocument.RD_DocUsage = docUse;
			requiredDocument.Validation.ValidateRD_DocUsage();
			if (hasErr1)
			{
				AssertHasError(requiredDocument.RD_DocUsageInfo, err1);
			}
			else
			{
				AssertNoError(requiredDocument.RD_DocUsageInfo, err1);
			}

			if (hasErr2)
			{
				AssertHasError(requiredDocument.RD_DocUsageInfo, err2);
			}
			else
			{
				AssertNoError(requiredDocument.RD_DocUsageInfo, err2);
			}
		}

		public void TestCheckRD_TransportMode()
		{
			RefCountryRequiredDocument requiredDocument = Factory.New<RefCountryRequiredDocument>();
			requiredDocument.RD_TransportMode = "";
			requiredDocument.Validation.ValidateRD_TransportMode();
			AssertHasErrors(requiredDocument.RD_TransportModeInfo);

			requiredDocument.RD_TransportMode = "XXX";
			requiredDocument.Validation.ValidateRD_TransportMode();
			AssertHasErrors(requiredDocument.RD_TransportModeInfo);

			requiredDocument.RD_TransportMode = "ALL";
			requiredDocument.Validation.ValidateRD_TransportMode();
			AssertNoErrors(requiredDocument.RD_TransportModeInfo);
		}
	}
}
