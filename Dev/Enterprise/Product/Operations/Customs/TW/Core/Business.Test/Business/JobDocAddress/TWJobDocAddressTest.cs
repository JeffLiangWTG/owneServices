using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWJobDocAddress))]
	public sealed class TWJobDocAddressTest : JobDocAddressTest
	{
		public void TestIDCodeTypeCaption_SupplierDocumentaryAddress()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(DocAddress.IDCodeTypeInfo, TWJobDocAddress.SupplierDocumentaryAddressCaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "ID", captionResourceString.Caption);
				AssertEquals("FullDescription", "The supplier's VAT code or passport number or ID card number.", captionResourceString.FullDescription);
			});
		}

		public void TestAEOCodeTypeCaption_SupplierDocumentaryAddress()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(DocAddress.AEOCodeTypeInfo, TWJobDocAddress.SupplierDocumentaryAddressCaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "AEO", captionResourceString.Caption);
				AssertEquals("FullDescription", "The supplier's Authorized Economic Operator (AEO) number.", captionResourceString.FullDescription);
			});
		}

		public void TestTPCCodeTypeCaption_SupplierDocumentaryAddress()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(DocAddress.TPCCodeTypeInfo, TWJobDocAddress.SupplierDocumentaryAddressCaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "TPC", captionResourceString.Caption);
				AssertEquals("FullDescription", "The \"tax payment on account\" business identifier for the supplier.", captionResourceString.FullDescription);
			});
		}

		public void TestIDCodeTypeCaption_ImporterDocumentaryAddress()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(DocAddress.IDCodeTypeInfo, TWJobDocAddress.ImporterDocumentaryAddressCaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "ID", captionResourceString.Caption);
				AssertEquals("FullDescription", "The importer's VAT code or passport number or ID card number.", captionResourceString.FullDescription);
			});
		}

		public void TestAEOCodeTypeCaption_ImporterDocumentaryAddress()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(DocAddress.AEOCodeTypeInfo, TWJobDocAddress.ImporterDocumentaryAddressCaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "AEO", captionResourceString.Caption);
				AssertEquals("FullDescription", "The importer's Authorized Economic Operator (AEO) number.", captionResourceString.FullDescription);
			});
		}

		public void TestTPCCodeTypeCaption_ImporterDocumentaryAddress()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(DocAddress.TPCCodeTypeInfo, TWJobDocAddress.ImporterDocumentaryAddressCaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "TPC", captionResourceString.Caption);
				AssertEquals("FullDescription", "The \"tax payment on account\" business identifier for the importer.", captionResourceString.FullDescription);
			});
		}

		public void TestIsLocalProcessor()
		{
			AssertIsDocAddressType(DocAddressType.LocalProcessorAddress, () => DocAddress.IsLocalProcessor);
		}

		public void TestIsLocalProcessorTranslatedDocAddress()
		{
			AssertIsDocAddressType(DocAddressType.LocalProcessorTranslatedDocAddress, () => DocAddress.IsLocalProcessorTranslatedDocAddress);
		}

		public void TestIsManufacturerTranslatedDocumentaryAddress()
		{
			AssertIsDocAddressType(DocAddressType.ManufacturerTranslatedDocumentaryAddress, () => DocAddress.IsManufacturerTranslatedDocumentaryAddress);
		}

		public void TestIsSupplierTranslatedDocumentaryAddress()
		{
			AssertIsDocAddressType(DocAddressType.SupplierTranslatedDocumentaryAddress, () => DocAddress.IsSupplierTranslatedDocumentaryAddress);
		}

		public void TestIsImporterTranslatedDocumentaryAddress()
		{
			AssertIsDocAddressType(DocAddressType.ImporterTranslatedDocumentaryAddress, () => DocAddress.IsImporterTranslatedDocumentaryAddress);
		}

		public void TestIsImporterDocumentaryAddress()
		{
			AssertIsDocAddressType(DocAddressType.ImporterDocumentaryAddress, () => DocAddress.IsImporterDocumentaryAddress);
		}

		public void TestIsSupplierDocumentaryAddress()
		{
			AssertIsDocAddressType(DocAddressType.SupplierDocumentaryAddress, () => DocAddress.IsSupplierDocumentaryAddress);
		}

		void AssertIsDocAddressType(DocAddressType docAddressType, Func<bool> isDocAddressOfType)
		{
			DocAddress.DocAddressType = docAddressType;
			AssertEquals(true, isDocAddressOfType());

			DocAddress.DocAddressType = DocAddressType.None;
			AssertEquals(false, isDocAddressOfType());
		}

		public void TestManufactureIDTypes()
		{
			AssertContainsExactElementsInAnyOrder(new[]
			{
				OrgCusCode.CodeTypes.VATCode,
				OrgCusCode.CodeTypes.PassportID,
				OrgCusCode.TaiwanCodeTypes.PID,
				OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber,
			}, DocAddress.ManufactureIDTypes);
		}

		TWJobDocAddress DocAddress => fDocAddress ?? (fDocAddress = Factory.New<TWJobDocAddress>());
		TWJobDocAddress fDocAddress;

		public void TestPopulatedAEONumber()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "X3";
			org1.OH_RL_NKClosestPort = "SGSIN";
			var address1 = org1.MainAddress;
			address1.OA_RN_NKCountryCode = "SG";
			org1.CustomsCodes.AddNew("AEO", "XXX1", "SG");

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "X4";
			org2.OH_RL_NKClosestPort = "CNSHA";
			var address2 = org2.MainAddress;
			address2.OA_RN_NKCountryCode = "CN";
			org2.CustomsCodes.AddNew("AEO", "XXX2", "TW");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = org1.PK;
			AssertEquals("XXX1", declaration.ImporterDocumentaryAddress.AEOCode);

			declaration.JE_OH_Importer = org2.PK;
			AssertNullOrEmpty("AEO number should be empty because their country codes are different.", declaration.ImporterDocumentaryAddress.AEOCode);
		}

		public void TestAEONumberWhenE2N_RN_NKCountryCodeChanged()
		{
			DocAddress.E2_AddressOverride = true;
			DocAddress.E2_RN_NKCountryCode = "TW";
			DocAddress.AEOCode = "1111";
			var aeoNumber = DocAddress.DocAddressNumbers.FindFirstByNumberType(OrgCusCode.TaiwanCodeTypes.AEO);
			CombineAssertions(() =>
			{
				AssertEquals("CountryCode must 'TW' when E2_RN_NKCountryCode change to 'TW'.", "TW", aeoNumber.E2N_RN_NKCountryCode);
				AssertEquals("E2N_Number must be '1111'.", "1111", aeoNumber.E2N_Number);
			});

			DocAddress.E2_RN_NKCountryCode = "US";
			AssertEquals("CountryCode must be 'US' when E2_RN_NKCountryCode change to 'US'", "US", aeoNumber.E2N_RN_NKCountryCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var jobDocAddress = declaration.JobDocAddress;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			jobDocAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.CBF;
			jobDocAddress.FRICodeType = OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
			var localAddress = jobDocAddress.LocalAddress;
			jobDocAddress.E2_CompanyName = "taiwang";
			jobDocAddress.E2_Address1 = "taiwang road 1";
			jobDocAddress.E2_Postcode = "001";
			localAddress.E2_CompanyName = "台湾";
			localAddress.E2_Address1 = "台湾街道1号";
			localAddress.E2_Postcode = "001";
			return jobDocAddress;
		}

		public void TestLocalAddressType()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			AssertEquals(DocAddressType.SupplierTranslatedDocumentaryAddress, jobDocAddress.LocalAddressType);

			jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			AssertEquals(DocAddressType.ImporterTranslatedDocumentaryAddress, jobDocAddress.LocalAddressType);

			jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			AssertEquals(DocAddressType.LocalProcessorTranslatedDocAddress, jobDocAddress.LocalAddressType);

			jobDocAddress.LocalAddressType = DocAddressType.NotifyParty;
			AssertEquals(DocAddressType.NotifyParty, jobDocAddress.LocalAddressType);

			jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.Applicant;
			AssertEquals(DocAddressType.ApplicantTranslatedDocumentaryAddress, jobDocAddress.LocalAddressType);

			jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
			AssertEquals(DocAddressType.ManufacturerTranslatedDocumentaryAddress, jobDocAddress.LocalAddressType);
		}

		public void TestTypeCode()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			AssertEquals(PartyIdentifierCodeList.Codes._58, jobDocAddress.TypeCode);
			jobDocAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
			AssertEquals(PartyIdentifierCodeList.Codes._53, jobDocAddress.TypeCode);
			jobDocAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.PID;
			AssertEquals(PartyIdentifierCodeList.Codes._174, jobDocAddress.TypeCode);
			jobDocAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
			AssertEquals(PartyIdentifierCodeList.Codes._160, jobDocAddress.TypeCode);
			jobDocAddress.IDCodeType = Constants.OrgCusCodeType.CustomCode;
			AssertEquals(Constants.OrgCusCodeType.CustomCode, jobDocAddress.TypeCode);

			jobDocAddress.E2_AddressOverride = false;
			jobDocAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			AssertEquals(ZString.Empty, jobDocAddress.TypeCode);
			jobDocAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
			AssertEquals(ZString.Empty, jobDocAddress.TypeCode);
			jobDocAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.PID;
			AssertEquals(ZString.Empty, jobDocAddress.TypeCode);
			jobDocAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
			AssertEquals(ZString.Empty, jobDocAddress.TypeCode);
			jobDocAddress.IDCodeType = Constants.OrgCusCodeType.CustomCode;
			AssertEquals(ZString.Empty, jobDocAddress.TypeCode);
		}

		public void TestIDCodeType()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var addresse = header.Addresses[2];
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();

			CombineAssertions("Supplier", () =>
			{
				var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
				supplierDocumentaryAddress.OrganisationPK = header.PK;
				supplierDocumentaryAddress.E2_OA_Address = addresse.PK;

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "PID001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("Supplier IDCode", "PID001", supplierDocumentaryAddress.IDCode);
				AssertEquals("Supplier IDCodeType", "PID", supplierDocumentaryAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("Supplier IDCode", "PAS001", supplierDocumentaryAddress.IDCode);
				AssertEquals("Supplier IDCodeType", "PAS", supplierDocumentaryAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("Supplier IDCode", "VAT001", supplierDocumentaryAddress.IDCode);
				AssertEquals("Supplier IDCodeType", "VAT", supplierDocumentaryAddress.IDCodeType);

				supplierDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
				supplierDocumentaryAddress.IDCode = "PAS002";
				AssertEquals("Supplier IDCode", "VAT001", supplierDocumentaryAddress.IDCode);
				AssertEquals("Supplier IDCodeType", "VAT", supplierDocumentaryAddress.IDCodeType);

				supplierDocumentaryAddress.E2_AddressOverride = true;
				supplierDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
				supplierDocumentaryAddress.IDCode = "PAS002";
				AssertEquals("Supplier IDCode", "PAS002", supplierDocumentaryAddress.IDCode);
				AssertEquals("Supplier IDCodeType", "PAS", supplierDocumentaryAddress.IDCodeType);
			});

			CombineAssertions("Importer", () =>
			{
				header.CustomsCodes.RemoveAll();
				var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
				importerDocumentaryAddress.OrganisationPK = header.PK;
				importerDocumentaryAddress.E2_OA_Address = addresse.PK;

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "PID001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("Importer IDCode", "PID001", importerDocumentaryAddress.IDCode);
				AssertEquals("Importer IDCodeType", "PID", importerDocumentaryAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("Importer IDCode", "PAS001", importerDocumentaryAddress.IDCode);
				AssertEquals("Importer IDCodeType", "PAS", importerDocumentaryAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("Importer IDCode", "VAT001", importerDocumentaryAddress.IDCode);
				AssertEquals("Importer IDCodeType", "VAT", importerDocumentaryAddress.IDCodeType);

				importerDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
				importerDocumentaryAddress.IDCode = "PAS002";
				AssertEquals("Importer IDCode", "VAT001", importerDocumentaryAddress.IDCode);
				AssertEquals("Importer IDCodeType", "VAT", importerDocumentaryAddress.IDCodeType);

				importerDocumentaryAddress.E2_AddressOverride = true;
				importerDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
				importerDocumentaryAddress.IDCode = "PAS002";
				AssertEquals("Importer IDCode", "PAS002", importerDocumentaryAddress.IDCode);
				AssertEquals("Importer IDCodeType", "PAS", importerDocumentaryAddress.IDCodeType);
			});

			CombineAssertions("Local Processor", () =>
			{
				header.CustomsCodes.RemoveAll();
				var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
				var localProcessorAddress = controllingMessageHeader.LocalProcessorAddress;
				localProcessorAddress.OrganisationPK = header.PK;
				localProcessorAddress.E2_OA_Address = addresse.PK;

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("Local Processor IDCode", "PAS001", localProcessorAddress.IDCode);
				AssertEquals("Local Processor IDCodeType", "PAS", localProcessorAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "PID001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("Local Processor IDCode", "PID001", localProcessorAddress.IDCode);
				AssertEquals("Local Processor IDCodeType", "PID", localProcessorAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("Local Processor IDCode", "VAT001", localProcessorAddress.IDCode);
				AssertEquals("Local Processor IDCodeType", "VAT", localProcessorAddress.IDCodeType);

				addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber, "FRI001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("Local Processor IDCode", "FRI001", localProcessorAddress.IDCode);
				AssertEquals("Local Processor IDCodeType", "FRI", localProcessorAddress.IDCodeType);

				localProcessorAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
				localProcessorAddress.IDCode = "PAS002";
				AssertEquals("Local Processor IDCode", "FRI001", localProcessorAddress.IDCode);
				AssertEquals("Local Processor IDCodeType", "FRI", localProcessorAddress.IDCodeType);

				localProcessorAddress.E2_AddressOverride = true;
				localProcessorAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
				localProcessorAddress.IDCode = "PAS002";
				AssertEquals("Local Processor IDCode", "PAS002", localProcessorAddress.IDCode);
				AssertEquals("Local Processor IDCodeType", "PAS", localProcessorAddress.IDCodeType);
			});
		}

		public void TestLocalAddress()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var jobDocAddress = declaration.JobDocAddress;
			jobDocAddress.LocalAddressType = Enterprise.MasterFiles.Integration.DocAddressType.None;
			AssertNull(jobDocAddress.LocalAddress);
			jobDocAddress.LocalAddressType = Enterprise.MasterFiles.Integration.DocAddressType.DefermentParty;
			AssertNull(jobDocAddress.LocalAddress);
			jobDocAddress.E2_AddressOverride = true;
			var localAddress = jobDocAddress.LocalAddress;
			AssertNotNull(localAddress);

			jobDocAddress.E2_CompanyName = "taiwang";
			jobDocAddress.E2_Address1 = "taiwang road 1";
			jobDocAddress.E2_Postcode = "001";

			localAddress.E2_CompanyName = "台湾";
			localAddress.E2_Address1 = "台湾街道1号";
			localAddress.E2_Postcode = "001";

			Factory.Save();
			var newlocalAddress = new BusinessObjectFactory().Load<TWJobDocAddress>(localAddress.PK);
			AssertNotNull(newlocalAddress);
			AssertEquals("DFP", newlocalAddress.E2_AddressType);
			AssertEquals("台湾", newlocalAddress.E2_CompanyName);

			jobDocAddress.E2_AddressOverride = false;
			Factory.Save();
			AssertNull(new BusinessObjectFactory().Load<TWJobDocAddress>(localAddress.PK));

			jobDocAddress = declaration.JobDocAddress;
			jobDocAddress.LocalAddressType = Enterprise.MasterFiles.Integration.DocAddressType.DefermentParty;
			jobDocAddress.E2_AddressOverride = true;
			localAddress = jobDocAddress.LocalAddress;
			AssertNotNull(localAddress);
			jobDocAddress.E2_CompanyName = "taiwang";
			jobDocAddress.E2_Address1 = "taiwang road 1";
			jobDocAddress.E2_Postcode = "001";
			localAddress.E2_CompanyName = "台湾";
			localAddress.E2_Address1 = "台湾街道1号";
			localAddress.E2_Postcode = "001";
			Factory.Save();
			AssertNotNull(new BusinessObjectFactory().Load<TWJobDocAddress>(localAddress.PK));
			jobDocAddress.Delete();
			Factory.Save();
			AssertNull(new BusinessObjectFactory().Load<TWJobDocAddress>(localAddress.PK));

			jobDocAddress = Factory.New<TWJobDocAddress>();
			AssertNull(jobDocAddress.LocalAddress);
			jobDocAddress.LocalAddressType = MasterFiles.Integration.DocAddressType.AdditionalConsignee;
			AssertNull(jobDocAddress.LocalAddress);
			jobDocAddress.E2_AddressOverride = true;
			AssertNull(jobDocAddress.LocalAddress);
		}

		public void TestLocalAddressShoudBeNullWhenChangeE2_AddressOverrideIsFalse()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var jobDocAddress = declaration.JobDocAddress;
			jobDocAddress.LocalAddressType = Enterprise.MasterFiles.Integration.DocAddressType.DefermentParty;
			jobDocAddress.E2_AddressOverride = true;
			AssertNotNull(jobDocAddress.LocalAddress);

			jobDocAddress.E2_AddressOverride = false;
			AssertNull(jobDocAddress.LocalAddress);
		}

		public void TestContactDetail_Mobile()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.OrganisationPK = header.PK;
			AssertEquals("Mobile is empty", @"Phone +1 (273) 5495200
Fax 001FAX
001@xx.com", jobDocAddress.ContactDetail);

			header.MainAddress.OA_Mobile = "+88621234567";
			Factory.InvalidateCachedProperties();
			AssertEquals("Mobile has value", @"Phone +1 (273) 5495200
Mobile +88621234567
Fax 001FAX
001@xx.com", jobDocAddress.ContactDetail);
		}

		public void TestIsDocAddressTypeProperties()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();

			CombineAssertions("DocAddressType is empty", () =>
			{
				AssertEquals("IsLocalProcessor", false, jobDocAddress.IsLocalProcessor);
				AssertEquals("IsLocalProcessorTranslatedDocAddress", false, jobDocAddress.IsLocalProcessorTranslatedDocAddress);
				AssertEquals("IsManufacturerTranslatedDocumentaryAddress", false, jobDocAddress.IsManufacturerTranslatedDocumentaryAddress);
			});

			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			AssertEquals("IsLocalProcessor", true, jobDocAddress.IsLocalProcessor);

			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorTranslatedDocAddress;
			AssertEquals("IsLocalProcessorTranslatedDocAddress", true, jobDocAddress.IsLocalProcessorTranslatedDocAddress);

			jobDocAddress.DocAddressType = DocAddressType.ManufacturerTranslatedDocumentaryAddress;
			AssertEquals("IsManufacturerTranslatedDocumentaryAddress", true, jobDocAddress.IsManufacturerTranslatedDocumentaryAddress);
		}

		public void TestProperties()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var jobDocAddress = Factory.New<TWJobDocAddress>();

			CombineAssertions(() =>
			{
				AssertEquals("AddressDetail when empty", "", jobDocAddress.AddressDetail);
				AssertEquals("LocalAddressDetail when empty", "", jobDocAddress.LocalAddressDetail);
				AssertEquals("ContactDetail when empty", "", jobDocAddress.ContactDetail);

				jobDocAddress.OrganisationPK = header.PK;
				AssertEquals("AddressDetail when OrganisationPK has value", @"HAPPY CO., LTD.
1500 HAPPY RD
ORANGE DISTRICT
ADDINFO ADDRESS
APPLE CITY  TAIPEI  12345", jobDocAddress.AddressDetail);
				AssertEquals("LocalAddressDetail when OrganisationPK has value", @"綠晃科技股份有限公司
臺北加工出口區園東街6號
附加信息2
臺北巿  TAIPEI  台灣  90093", jobDocAddress.LocalAddressDetail);
				AssertEquals("ContactDetail when OrganisationPK has value", @"Phone +1 (273) 5495200
Fax 001FAX
001@xx.com", jobDocAddress.ContactDetail);

				jobDocAddress.E2_AddressOverride = true;
				AssertEquals("AddressDetail when E2_AddressOverride is true", "", jobDocAddress.AddressDetail);
				AssertEquals("LocalAddressDetail when E2_AddressOverride is true", "", jobDocAddress.LocalAddressDetail);
				AssertEquals("ContactDetail when E2_AddressOverride is true", "", jobDocAddress.ContactDetail);
				jobDocAddress.E2_AddressOverride = false;

				jobDocAddress.E2_OA_Address = header.Addresses[1].PK;
				AssertEquals("AddressDetail when E2_OA_Address change to Addresses1", @"HAPPY CO., LTD.1
004 HAPPY RD
ORANGE DISTRICT1
ADDINFO ADDRESS1
APPLE CITY1  TAIPEI  001", jobDocAddress.AddressDetail);
				AssertEquals("LocalAddressDetail when E2_OA_Address changed to Addresses1", "", jobDocAddress.LocalAddressDetail);
				AssertEquals("ContactDetail when E2_OA_Address changed to Addresses1", @"Phone +2 (273) 5495200
Fax 002FAX
002@xx.com", jobDocAddress.ContactDetail);

				var addresse = header.Addresses[2];
				jobDocAddress.E2_OA_Address = addresse.PK;
				AssertEquals("AddressDetail when E2_OA_Address changed to Addresses2", @"台湾分公司
地址1
地址2
附加信息
台北  TAIPEI  002", jobDocAddress.AddressDetail);
				AssertEquals("LocalAddressDetail when E2_OA_Address changed to Addresses2", "", jobDocAddress.LocalAddressDetail);
				AssertEquals("ContactDetail when E2_OA_Address changed to Addresses2", @"Phone +3 (273) 5495200
Fax 003FAX
003@xx.com", jobDocAddress.ContactDetail);

				AssertEquals("IDCode", "", jobDocAddress.IDCode);
				AssertEquals("IDCodeType", "", jobDocAddress.IDCodeType);
				AssertEquals("IDCode.MaxLength", 35, jobDocAddress.IDCodeInfo.MaxLength);
				AssertEquals("IDCodeType.MaxLength", 3, jobDocAddress.IDCodeTypeInfo.MaxLength);
				AssertEquals("IDCodeTypeList", "Lookups.IDCodeTypeList", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ListAttribute>(jobDocAddress.IDCodeTypeInfo).ListDataSourceMember);
				AssertEquals("AEOCode", "123465789", jobDocAddress.AEOCode);
				AssertEquals("AEOCodeType", "AEO", jobDocAddress.AEOCodeType);
				AssertEquals("AEOCode.MaxLength", 35, jobDocAddress.AEOCodeInfo.MaxLength);
				AssertEquals("AEOCodeTypeList", "Lookups.AEOCodeTypeList", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ListAttribute>(jobDocAddress.AEOCodeTypeInfo).ListDataSourceMember);

				AssertEquals("TPCCode", "", jobDocAddress.TPCCode);
				AssertEquals("TPCCodeType", "TPC", jobDocAddress.TPCCodeType);
				AssertEquals("TPCCode.MaxLength", 35, jobDocAddress.TPCCodeInfo.MaxLength);
				AssertEquals("TPCCodeTypeList", "Lookups.TPCCodeTypeList", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ListAttribute>(jobDocAddress.TPCCodeTypeInfo).ListDataSourceMember);

				AssertEquals("CBPCode", "", jobDocAddress.CBPCode);
				AssertEquals("CBPCodeType", "", jobDocAddress.CBPCodeType);
				AssertEquals("CBPCode.MaxLength", 35, jobDocAddress.CBPCodeInfo.MaxLength);
				AssertEquals("CBPCodeType.MaxLength", 3, jobDocAddress.CBPCodeTypeInfo.MaxLength);
				AssertEquals("CBPCodeTypeList", "Lookups.CBPCodeTypeList", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ListAttribute>(jobDocAddress.CBPCodeTypeInfo).ListDataSourceMember);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "PID001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("IDCode", "PID001", jobDocAddress.IDCode);
				AssertEquals("IDCodeType", "PID", jobDocAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("IDCode", "PAS001", jobDocAddress.IDCode);
				AssertEquals("IDCodeType", "PAS", jobDocAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("IDCode", "VAT001", jobDocAddress.IDCode);
				AssertEquals("IDCodeType", "VAT", jobDocAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.TPC, "TPC001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("TPCCode", "TPC001", jobDocAddress.TPCCode);

				addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("CBPCode", "FTZ001", jobDocAddress.CBPCode);
				AssertEquals("CBPCodeType", "FTZ", jobDocAddress.CBPCodeType);

				addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.CBF, "CBF001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("CBPCode", "CBF001", jobDocAddress.CBPCode);
				AssertEquals("CBPCodeType", "CBF", jobDocAddress.CBPCodeType);

				addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.EPZ, "EPZ001", Core.Constants.CountryCodes.Taiwan);
				AssertEquals("CBPCode", "EPZ001", jobDocAddress.CBPCode);
				AssertEquals("CBPCodeType", "EPZ", jobDocAddress.CBPCodeType);

				jobDocAddress.E2_OA_Address = header.Addresses[1].PK;
				AssertEquals("CBPCode", "", jobDocAddress.CBPCode);
				AssertEquals("CBPCodeType", "", jobDocAddress.CBPCodeType);
			});
		}

		public void TestSetLocalAddressDefaultValues()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var jobDocAddress = declaration.JobDocAddress;

			CombineAssertions(() =>
			{
				jobDocAddress.OrganisationPK = header.PK;
				AssertEquals("AddressDetail when OrganisationPK has value", @"HAPPY CO., LTD.
1500 HAPPY RD
ORANGE DISTRICT
ADDINFO ADDRESS
APPLE CITY  TAIPEI  12345", jobDocAddress.AddressDetail);
				AssertEquals("LocalAddressDetail when OrganisationPK has value", @"綠晃科技股份有限公司
臺北加工出口區園東街6號
附加信息2
臺北巿  TAIPEI  台灣  90093", jobDocAddress.LocalAddressDetail);
				AssertEquals("ContactDetail when OrganisationPK has value", @"Phone +1 (273) 5495200
Fax 001FAX
001@xx.com", jobDocAddress.ContactDetail);

				jobDocAddress.E2_AddressOverride = true;
				AssertEquals("E2_CompanyName", "HAPPY CO., LTD.", jobDocAddress.E2_CompanyName);
				AssertEquals("E2_Address1", "1500 HAPPY RD", jobDocAddress.E2_Address1);
				AssertEquals("E2_Address2", "ORANGE DISTRICT", jobDocAddress.E2_Address2);
				AssertEquals("E2_AdditionalAddressInformation", "addinfo address", jobDocAddress.E2_AdditionalAddressInformation);
				AssertEquals("E2_RN_NKCountryCode", "TW", jobDocAddress.E2_RN_NKCountryCode);
				AssertEquals("E2_City", "APPLE CITY", jobDocAddress.E2_City);
				AssertEquals("E2_Postcode", "12345", jobDocAddress.E2_Postcode);
				AssertEquals("E2_State", "TPE", jobDocAddress.E2_State);

				AssertEquals("E2_Contact", "", jobDocAddress.E2_Contact);
				AssertEquals("E2_Email", "001@xx.com", jobDocAddress.E2_Email);
				AssertEquals("E2_Phone", "+1 (273) 5495200", jobDocAddress.E2_Phone);
				AssertEquals("E2_Mobile", "", jobDocAddress.E2_Mobile);
				AssertEquals("E2_Fax", "001FAX", jobDocAddress.E2_Fax);

				var localAddress = jobDocAddress.LocalAddress;
				AssertEquals("LocalAddress.E2_CompanyName", "綠晃科技股份有限公司", localAddress.E2_CompanyName);
				AssertEquals("LocalAddress.E2_Address1", "臺北加工出口區園東街6號", localAddress.E2_Address1);
				AssertEquals("LocalAddress.E2_Address2", "", localAddress.E2_Address2);
				AssertEquals("LocalAddress.E2_AdditionalAddressInformation", "附加信息2", localAddress.E2_AdditionalAddressInformation);
				AssertEquals("LocalAddress.E2_RN_NKCountryCode", "TW", localAddress.E2_RN_NKCountryCode);
				AssertEquals("LocalAddress.E2_City", "臺北巿", localAddress.E2_City);
				AssertEquals("LocalAddress.E2_Postcode", "90093", localAddress.E2_Postcode);
				AssertEquals("LocalAddress.E2_State", "TPE", localAddress.E2_State);

				AssertEquals("IDCode", "", jobDocAddress.IDCode);
				AssertEquals("IDCodeType", "VAT", jobDocAddress.IDCodeType);
				AssertEquals("AEOCode", "123465789", jobDocAddress.AEOCode);
				AssertEquals("AEOCodeType", "AEO", jobDocAddress.AEOCodeType);

				AssertEquals("TPCCode", "", jobDocAddress.TPCCode);
				AssertEquals("TPCCodeType", "TPC", jobDocAddress.TPCCodeType);
				AssertEquals("CBPCode", "", jobDocAddress.CBPCode);
				AssertEquals("CBPCodeType", "EPZ", jobDocAddress.CBPCodeType);

				header.MainAddress.TranslatedAddresses[0].OTA_Address2 = "左转饭店正对面";
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertEquals("LocalAddress.E2_Address2", "左转饭店正对面", jobDocAddress.LocalAddress.E2_Address2);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "PID001", Core.Constants.CountryCodes.Taiwan);
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertEquals("IDCode", "PID001", jobDocAddress.IDCode);
				AssertEquals("IDCodeType", "PID", jobDocAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertEquals("IDCode", "PAS001", jobDocAddress.IDCode);
				AssertEquals("IDCodeType", "PAS", jobDocAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertEquals("IDCode", "VAT001", jobDocAddress.IDCode);
				AssertEquals("IDCodeType", "VAT", jobDocAddress.IDCodeType);

				header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.TPC, "TPC001", Core.Constants.CountryCodes.Taiwan);
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertNullOrEmpty("TPCCode", jobDocAddress.TPCCode);

				header.MainAddress.OA_RN_NKCountryCode = "CA";
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertEquals("LocalAddress.E2_RN_NKCountryCode", "CA", jobDocAddress.LocalAddress.E2_RN_NKCountryCode);
				header.MainAddress.OA_RN_NKCountryCode = "TW";

				var addresse = header.Addresses[2];
				jobDocAddress.E2_OA_Address = addresse.PK;

				addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "CCP001", Core.Constants.CountryCodes.Taiwan);
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertNullOrEmpty("CBPCode", jobDocAddress.CBPCode);
				AssertEquals("CBPCodeType", "EPZ", jobDocAddress.CBPCodeType);

				addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPW001", Core.Constants.CountryCodes.Taiwan);
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertNullOrEmpty("CBPCode", jobDocAddress.CBPCode);
				AssertEquals("CBPCodeType", "EPZ", jobDocAddress.CBPCodeType);

				addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertEquals("CBPCode", "FTZ001", jobDocAddress.CBPCode);
				AssertEquals("CBPCodeType", "FTZ", jobDocAddress.CBPCodeType);

				addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.CBF, "CBF001", Core.Constants.CountryCodes.Taiwan);
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertEquals("CBPCode", "CBF001", jobDocAddress.CBPCode);
				AssertEquals("CBPCodeType", "CBF", jobDocAddress.CBPCodeType);

				addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.EPZ, "EPZ001", Core.Constants.CountryCodes.Taiwan);
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_AddressOverride = true;
				AssertEquals("CBPCode", "EPZ001", jobDocAddress.CBPCode);
				AssertEquals("CBPCodeType", "EPZ", jobDocAddress.CBPCodeType);

				declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
				jobDocAddress = declaration.JobDocAddress;
				jobDocAddress.OrganisationPK = header.PK;
				jobDocAddress.ShouldClearAddressFieldsWhenOverride = true;
				jobDocAddress.E2_AddressOverride = true;

				AssertEquals("E2_Address1 when ShouldClearAddressFieldsWhenOverride is true", "", jobDocAddress.E2_Address1);
				AssertEquals("E2_Address2 when ShouldClearAddressFieldsWhenOverride is true", "", jobDocAddress.E2_Address2);
				AssertEquals("E2_AdditionalAddressInformation when ShouldClearAddressFieldsWhenOverride is true", "", jobDocAddress.E2_AdditionalAddressInformation);
				AssertEquals("E2_RN_NKCountryCode when ShouldClearAddressFieldsWhenOverride is true", "TW", jobDocAddress.E2_RN_NKCountryCode);
				AssertEquals("E2_City when ShouldClearAddressFieldsWhenOverride is true", "", jobDocAddress.E2_City);
				AssertEquals("E2_Postcode when ShouldClearAddressFieldsWhenOverride is true", "", jobDocAddress.E2_Postcode);
				AssertEquals("E2_State when ShouldClearAddressFieldsWhenOverride is true", "", jobDocAddress.E2_State);

				AssertEquals("E2_Contact when ShouldClearAddressFieldsWhenOverride is true", "", jobDocAddress.E2_Contact);
				AssertEquals("E2_Email when ShouldClearAddressFieldsWhenOverride is true", "001@xx.com", jobDocAddress.E2_Email);
				AssertEquals("E2_Phone when ShouldClearAddressFieldsWhenOverride is true", "+1 (273) 5495200", jobDocAddress.E2_Phone);
				AssertEquals("E2_Mobile when ShouldClearAddressFieldsWhenOverride is true", "", jobDocAddress.E2_Mobile);
				AssertEquals("E2_Fax when ShouldClearAddressFieldsWhenOverride is true", "001FAX", jobDocAddress.E2_Fax);

				localAddress = jobDocAddress.LocalAddress;
				AssertEquals("LocalAddress.E2_CompanyName when ShouldClearAddressFieldsWhenOverride is true", "", localAddress.E2_CompanyName);
				AssertEquals("LocalAddress.E2_Address1 when ShouldClearAddressFieldsWhenOverride is true", "", localAddress.E2_Address1);
				AssertEquals("LocalAddress.E2_Address2 when ShouldClearAddressFieldsWhenOverride is true", "", localAddress.E2_Address2);
				AssertEquals("LocalAddress.E2_AdditionalAddressInformation when ShouldClearAddressFieldsWhenOverride is true", "", localAddress.E2_AdditionalAddressInformation);
				AssertEquals("LocalAddress.E2_RN_NKCountryCode when ShouldClearAddressFieldsWhenOverride is true", "TW", localAddress.E2_RN_NKCountryCode);
				AssertEquals("LocalAddress.E2_City when ShouldClearAddressFieldsWhenOverride is true", "", localAddress.E2_City);
				AssertEquals("LocalAddress.E2_Postcode when ShouldClearAddressFieldsWhenOverride is true", "", localAddress.E2_Postcode);
				AssertEquals("LocalAddress.E2_State when ShouldClearAddressFieldsWhenOverride is true", "", localAddress.E2_State);
			});
		}

		public void TestLocalAddressDetailCountryWithNonTWOrganization()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			header.MainAddress.OA_RN_NKCountryCode = "AU";
			jobDocAddress.OrganisationPK = header.PK;

			AssertEquals(@"綠晃科技股份有限公司
臺北加工出口區園東街6號
附加信息2
臺北巿  TPE  澳洲  90093", jobDocAddress.LocalAddressDetail);
		}

		public void TestSetCodeDefaultValues()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
			header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AEO, "AEO001", Core.Constants.CountryCodes.Taiwan);
			header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.TPC, "TPC001", Core.Constants.CountryCodes.Taiwan);
			header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber, "FRI001", Core.Constants.CountryCodes.Taiwan);
			var addresse = header.Addresses[2];
			addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "CCP001", Core.Constants.CountryCodes.Taiwan);
			addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT002", Core.Constants.CountryCodes.Taiwan);
			addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AEO, "AEO002", Core.Constants.CountryCodes.Taiwan);
			addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.TPC, "TPC002", Core.Constants.CountryCodes.Taiwan);
			addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber, "FRI002", Core.Constants.CountryCodes.Taiwan);
			addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.EPZ, "EPZ002", Core.Constants.CountryCodes.Taiwan);

			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;

			supplierDocumentaryAddress.E2_OA_Address = addresse.PK;
			importerDocumentaryAddress.E2_OA_Address = addresse.PK;

			CombineAssertions("Export", () =>
			{
				declaration.JE_MessageType = "EXP";
				supplierDocumentaryAddress.OrganisationPK = header.PK;
				supplierDocumentaryAddress.E2_OA_Address = addresse.PK;
				supplierDocumentaryAddress.E2_AddressOverride = false;
				supplierDocumentaryAddress.E2_AddressOverride = true;

				AssertEquals("Supplier IDCode", "VAT001", supplierDocumentaryAddress.IDCode);
				AssertEquals("Supplier IDCodeType", "VAT", supplierDocumentaryAddress.IDCodeType);
				AssertEquals("Supplier TPCCode", "TPC001", supplierDocumentaryAddress.TPCCode);
				AssertEquals("Supplier AEOCode", "AEO001", supplierDocumentaryAddress.AEOCode);
				AssertEquals("Supplier CBPCode", "EPZ002", supplierDocumentaryAddress.CBPCode);
				AssertEquals("Supplier CBPCodeType", "EPZ", supplierDocumentaryAddress.CBPCodeType);

				importerDocumentaryAddress.OrganisationPK = header.PK;
				importerDocumentaryAddress.E2_OA_Address = addresse.PK;
				importerDocumentaryAddress.E2_AddressOverride = false;
				importerDocumentaryAddress.E2_AddressOverride = true;
				AssertEquals("Importer IDCode", "VAT001", importerDocumentaryAddress.IDCode);
				AssertEquals("Importer IDCodeType", "VAT", importerDocumentaryAddress.IDCodeType);
				AssertNullOrEmpty("Importer TPCCode", importerDocumentaryAddress.TPCCode);
				AssertEquals("Importer AEOCode", "AEO001", importerDocumentaryAddress.AEOCode);
				AssertEquals("Importer CBPCode", "EPZ002", importerDocumentaryAddress.CBPCode);
				AssertEquals("Importer CBPCodeType", "EPZ", importerDocumentaryAddress.CBPCodeType);
			});

			CombineAssertions("Import", () =>
			{
				declaration.JE_MessageType = "IMP";
				supplierDocumentaryAddress.E2_AddressOverride = false;
				supplierDocumentaryAddress.OrganisationPK = header.PK;
				supplierDocumentaryAddress.E2_OA_Address = addresse.PK;
				supplierDocumentaryAddress.E2_AddressOverride = true;

				AssertEquals("Supplier IDCode", "VAT001", supplierDocumentaryAddress.IDCode);
				AssertEquals("Supplier IDCodeType", "VAT", supplierDocumentaryAddress.IDCodeType);
				AssertNullOrEmpty("Supplier TPCCode", supplierDocumentaryAddress.TPCCode);
				AssertEquals("Supplier AEOCode", "AEO001", supplierDocumentaryAddress.AEOCode);
				AssertEquals("Supplier CBPCode", "EPZ002", supplierDocumentaryAddress.CBPCode);
				AssertEquals("Supplier CBPCodeType", "EPZ", supplierDocumentaryAddress.CBPCodeType);

				importerDocumentaryAddress.E2_AddressOverride = false;
				importerDocumentaryAddress.OrganisationPK = header.PK;
				importerDocumentaryAddress.E2_OA_Address = addresse.PK;
				importerDocumentaryAddress.E2_AddressOverride = true;
				AssertEquals("Importer IDCode", "VAT001", importerDocumentaryAddress.IDCode);
				AssertEquals("Importer IDCodeType", "VAT", importerDocumentaryAddress.IDCodeType);
				AssertEquals("Importer TPCCode", "TPC001", importerDocumentaryAddress.TPCCode);
				AssertEquals("Importer AEOCode", "AEO001", importerDocumentaryAddress.AEOCode);
				AssertEquals("Importer CBPCode", "EPZ002", importerDocumentaryAddress.CBPCode);
				AssertEquals("Importer CBPCodeType", "EPZ", importerDocumentaryAddress.CBPCodeType);

				declaration.CusEntryInstruction.CEI_Style = "L1";
				importerDocumentaryAddress.E2_AddressOverride = false;
				importerDocumentaryAddress.E2_AddressOverride = true;
				AssertEquals("Importer CBPCode", "CCP001", importerDocumentaryAddress.CBPCode);
				AssertEquals("Importer CBPCodeType", "CCP", importerDocumentaryAddress.CBPCodeType);
			});

			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			CombineAssertions("Local Processor", () =>
			{
				controllingMessageHeader.TW1_ControllingMessageType = "NX101";
				controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
				var localProcessorAddress = controllingMessageHeader.LocalProcessorAddress;
				localProcessorAddress.OrganisationPK = header.PK;
				localProcessorAddress.E2_OA_Address = addresse.PK;
				localProcessorAddress.E2_AddressOverride = true;
				AssertEquals("Local Processor IDCode", "123", localProcessorAddress.IDCode);
				AssertEquals("Local Processor IDCodeType", "ZZZ", localProcessorAddress.IDCodeType);
				AssertNullOrEmpty("Local Processor TPCCode", localProcessorAddress.TPCCode);
				AssertNullOrEmpty("Local Processor AEOCode", localProcessorAddress.AEOCode);
				AssertNullOrEmpty("Local Processor CBPCode", localProcessorAddress.CBPCode);
				AssertNullOrEmpty("Local Processor CBPCodeType", localProcessorAddress.CBPCodeType);

				controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code0;
				localProcessorAddress.E2_AddressOverride = false;
				localProcessorAddress.E2_AddressOverride = true;
				AssertEquals("Local Processor IDCode", "FRI002", localProcessorAddress.IDCode);
				AssertEquals("Local Processor IDCodeType", "FRI", localProcessorAddress.IDCodeType);
				AssertNullOrEmpty("Local Processor TPCCode", localProcessorAddress.TPCCode);
				AssertNullOrEmpty("Local Processor AEOCode", localProcessorAddress.AEOCode);
				AssertNullOrEmpty("Local Processor CBPCode", localProcessorAddress.CBPCode);
				AssertNullOrEmpty("Local Processor CBPCodeType", localProcessorAddress.CBPCodeType);
			});

			CombineAssertions("Manufacturer", () =>
			{
				controllingMessageHeader.TW1_ControllingMessageType = "NX101";
				controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code0;
				var invoiceLine = declaration.InvoiceLines.AddNew();
				var invoiceLineLinkControllingMsgHeaders = (InvoiceLineLinkControllingMsgHeader)invoiceLine.InvoiceLineLinkControllingMsgHeaders.First();
				invoiceLineLinkControllingMsgHeaders.IsLinkedCMHeader = true;
				var manufacturerAddress = invoiceLine.ManufacturerDocAddress;

				manufacturerAddress.OrganisationPK = header.PK;
				manufacturerAddress.E2_OA_Address = addresse.PK;
				manufacturerAddress.E2_AddressOverride = true;

				AssertEquals("Manufacturer IDCode", "VAT001", manufacturerAddress.IDCode);
				AssertEquals("Manufacturer IDCodeType", "VAT", manufacturerAddress.IDCodeType);
				AssertEquals("Manufacturer FRICode", "FRI002", manufacturerAddress.FRICode);
				AssertEquals("Manufacturer FRICodeType", "FRI", manufacturerAddress.FRICodeType);

				controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
				manufacturerAddress.E2_AddressOverride = false;
				manufacturerAddress.E2_AddressOverride = true;
				AssertEquals("Manufacturer IDCode", "VAT001", manufacturerAddress.IDCode);
				AssertEquals("Manufacturer IDCodeType", "VAT", manufacturerAddress.IDCodeType);
				AssertEquals("Manufacturer FRICode", "123", manufacturerAddress.FRICode);
				AssertEquals("Manufacturer FRICodeType", "ZZZ", manufacturerAddress.FRICodeType);

				invoiceLineLinkControllingMsgHeaders.IsLinkedCMHeader = false;
				manufacturerAddress.E2_AddressOverride = false;
				manufacturerAddress.E2_AddressOverride = true;
				AssertEquals("Manufacturer IDCode", "VAT001", manufacturerAddress.IDCode);
				AssertEquals("Manufacturer IDCodeType", "VAT", manufacturerAddress.IDCodeType);
				AssertNullOrEmpty("Manufacturer FRICode", manufacturerAddress.FRICode);
				AssertEquals("Manufacturer FRICodeType", "FRI", manufacturerAddress.FRICodeType);
			});
		}

		public void TestSetCBPCodeDefaultValuesForATPAndSPK()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var address1 = header.Addresses[1];
			address1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, "ATP01", Core.Constants.CountryCodes.Taiwan);
			var address2 = header.Addresses[2];
			address2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.SciencePark, "SPK01", Core.Constants.CountryCodes.Taiwan);

			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;

			declaration.JE_MessageType = "EXP";
			supplierDocumentaryAddress.OrganisationPK = header.PK;
			importerDocumentaryAddress.OrganisationPK = header.PK;

			CombineAssertions("Address not overrided", () =>
			{
				supplierDocumentaryAddress.E2_OA_Address = address1.PK;
				importerDocumentaryAddress.E2_OA_Address = address2.PK;
				supplierDocumentaryAddress.E2_AddressOverride = false;
				importerDocumentaryAddress.E2_AddressOverride = false;

				AssertEquals("Supplier CBPCode", "ATP01", supplierDocumentaryAddress.CBPCode);
				AssertEquals("Supplier CBPCodeType", "ATP", supplierDocumentaryAddress.CBPCodeType);
				AssertEquals("Importer CBPCode", "SPK01", importerDocumentaryAddress.CBPCode);
				AssertEquals("Importer CBPCodeType", "SPK", importerDocumentaryAddress.CBPCodeType);

				supplierDocumentaryAddress.E2_OA_Address = address2.PK;
				importerDocumentaryAddress.E2_OA_Address = address1.PK;
				supplierDocumentaryAddress.E2_AddressOverride = false;
				importerDocumentaryAddress.E2_AddressOverride = false;

				AssertEquals("Supplier CBPCode", "SPK01", supplierDocumentaryAddress.CBPCode);
				AssertEquals("Supplier CBPCodeType", "SPK", supplierDocumentaryAddress.CBPCodeType);
				AssertEquals("Importer CBPCode", "ATP01", importerDocumentaryAddress.CBPCode);
				AssertEquals("Importer CBPCodeType", "ATP", importerDocumentaryAddress.CBPCodeType);
			});

			CombineAssertions("Address overrided", () =>
			{
				supplierDocumentaryAddress.E2_OA_Address = address1.PK;
				importerDocumentaryAddress.E2_OA_Address = address2.PK;
				supplierDocumentaryAddress.E2_AddressOverride = true;
				importerDocumentaryAddress.E2_AddressOverride = true;

				AssertEquals("Supplier CBPCode", "ATP01", supplierDocumentaryAddress.CBPCode);
				AssertEquals("Supplier CBPCodeType", "ATP", supplierDocumentaryAddress.CBPCodeType);
				AssertEquals("Importer CBPCode", "SPK01", importerDocumentaryAddress.CBPCode);
				AssertEquals("Importer CBPCodeType", "SPK", importerDocumentaryAddress.CBPCodeType);

				supplierDocumentaryAddress.E2_OA_Address = address2.PK;
				importerDocumentaryAddress.E2_OA_Address = address1.PK;
				supplierDocumentaryAddress.E2_AddressOverride = true;
				importerDocumentaryAddress.E2_AddressOverride = true;

				AssertEquals("Supplier CBPCode", "SPK01", supplierDocumentaryAddress.CBPCode);
				AssertEquals("Supplier CBPCodeType", "SPK", supplierDocumentaryAddress.CBPCodeType);
				AssertEquals("Importer CBPCode", "ATP01", importerDocumentaryAddress.CBPCode);
				AssertEquals("Importer CBPCodeType", "ATP", importerDocumentaryAddress.CBPCodeType);
			});
		}

		public void TestCodeWithAddressOverride()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var jobDocAddress = declaration.JobDocAddress;
			jobDocAddress.E2_AddressOverride = false;
			CombineAssertions(() =>
			{
				Assert(jobDocAddress.IDCodeInfo.ReadOnly);
				Assert(jobDocAddress.IDCodeInfo.ReadOnly);
				Assert(jobDocAddress.AEOCodeInfo.ReadOnly);
				Assert(jobDocAddress.AEOCodeTypeInfo.ReadOnly);
				Assert(jobDocAddress.TPCCodeInfo.ReadOnly);
				Assert(jobDocAddress.TPCCodeTypeInfo.ReadOnly);
				Assert(jobDocAddress.CBPCodeInfo.ReadOnly);
				Assert(jobDocAddress.CBPCodeTypeInfo.ReadOnly);
				Assert(jobDocAddress.FRICodeInfo.ReadOnly);
				Assert(jobDocAddress.FRICodeTypeInfo.ReadOnly);
			});

			jobDocAddress.E2_AddressOverride = true;
			AssertEquals(OrgCusCode.CodeTypes.VATCode, jobDocAddress.IDCodeType);
			var localAddress = jobDocAddress.LocalAddress;
			jobDocAddress.E2_CompanyName = "taiwang";
			jobDocAddress.E2_Address1 = "taiwang road 1";
			jobDocAddress.E2_Postcode = "001";
			localAddress.E2_CompanyName = "台湾";
			localAddress.E2_Address1 = "台湾街道1号";
			localAddress.E2_Postcode = "001";

			jobDocAddress.IDCodeType = ZString.Empty;
			CombineAssertions(() =>
			{
				Assert(jobDocAddress.IDCodeInfo.ReadOnly);
				Assert(!jobDocAddress.IDCodeTypeInfo.ReadOnly);
				Assert(!jobDocAddress.AEOCodeInfo.ReadOnly);
				Assert(jobDocAddress.AEOCodeTypeInfo.ReadOnly);
				Assert(!jobDocAddress.TPCCodeInfo.ReadOnly);
				Assert(jobDocAddress.TPCCodeTypeInfo.ReadOnly);
				Assert(!jobDocAddress.CBPCodeInfo.ReadOnly);
				Assert(!jobDocAddress.CBPCodeTypeInfo.ReadOnly);
				Assert(jobDocAddress.FRICodeInfo.ReadOnly);
				Assert(!jobDocAddress.FRICodeTypeInfo.ReadOnly);
			});

			jobDocAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			Assert(!jobDocAddress.IDCodeInfo.ReadOnly);
			jobDocAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.CBF;
			Assert(!jobDocAddress.CBPCodeInfo.ReadOnly);
			jobDocAddress.FRICodeType = OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
			Assert(!jobDocAddress.FRICodeInfo.ReadOnly);

			jobDocAddress.IDCode = "12345675";
			jobDocAddress.AEOCode = "123456789";
			jobDocAddress.TPCCode = "11";
			jobDocAddress.CBPCode = "12345";
			jobDocAddress.FRICode = "87654321";

			Factory.Save();

			var pk = jobDocAddress.PK;
			var newJobDocAddress = new BusinessObjectFactory().Load<TWJobDocAddress>(pk);
			CombineAssertions(() =>
			{
				AssertEquals("12345675", newJobDocAddress.IDCode);
				AssertEquals("123456789", newJobDocAddress.AEOCode);
				AssertEquals("11", newJobDocAddress.TPCCode);
				AssertEquals("12345", newJobDocAddress.CBPCode);
				AssertEquals("87654321", newJobDocAddress.FRICode);
				AssertEquals("VAT", newJobDocAddress.IDCodeType);
				AssertEquals("AEO", newJobDocAddress.AEOCodeType);
				AssertEquals("TPC", newJobDocAddress.TPCCodeType);
				AssertEquals("CBF", newJobDocAddress.CBPCodeType);
				AssertEquals("FRI", newJobDocAddress.FRICodeType);
			});

			AssertEquals(5, new BusinessObjectFactory().Load<JobDocAddressNumber>(new ZQuery()).Length);
			var addressNumbers = new BusinessObjectFactory().Load<JobDocAddressNumber>(new ZQuery(JobDocAddressNumberSchema.E2N_E2, pk));
			AssertEquals(5, addressNumbers.Length);
			CombineAssertions(() =>
			{
				Assert(addressNumbers.Any(x => x.E2N_NumberType == "VAT" && x.E2N_Number == "12345675" && x.E2N_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan));
				Assert(addressNumbers.Any(x => x.E2N_NumberType == "AEO" && x.E2N_Number == "123456789" && x.E2N_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan));
				Assert(addressNumbers.Any(x => x.E2N_NumberType == "TPC" && x.E2N_Number == "11" && x.E2N_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan));
				Assert(addressNumbers.Any(x => x.E2N_NumberType == "CBF" && x.E2N_Number == "12345" && x.E2N_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan));
				Assert(addressNumbers.Any(x => x.E2N_NumberType == "FRI" && x.E2N_Number == "87654321" && x.E2N_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan));
			});

			jobDocAddress.IDCode = "";
			jobDocAddress.AEOCode = "";
			jobDocAddress.TPCCode = "";
			jobDocAddress.CBPCode = "";
			jobDocAddress.FRICode = "";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("", jobDocAddress.IDCodeType);
				AssertEquals("", jobDocAddress.CBPCodeType);
				AssertEquals("", jobDocAddress.FRICodeType);
				AssertEquals(0, new BusinessObjectFactory().Load<JobDocAddressNumber>(new ZQuery()).Length);
				AssertEquals(0, new BusinessObjectFactory().Load<JobDocAddressNumber>(new ZQuery(JobDocAddressNumberSchema.E2N_E2, pk)).Length);
			});

			jobDocAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			jobDocAddress.IDCode = "12345675";
			AssertEquals("12345675", jobDocAddress.IDCode);
			jobDocAddress.IDCodeType = "";
			AssertEquals("", jobDocAddress.IDCode);
			jobDocAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			jobDocAddress.IDCode = "12345675";
			jobDocAddress.IDCodeType = Constants.OrgCusCodeType.CustomCode;
			AssertEquals("12345675", jobDocAddress.IDCode);

			jobDocAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.CBF;
			jobDocAddress.CBPCode = "12345";
			AssertEquals("12345", jobDocAddress.CBPCode);
			jobDocAddress.CBPCodeType = "";
			AssertEquals("", jobDocAddress.CBPCode);

			Factory.Save();
			AssertEquals(1, new BusinessObjectFactory().Load<JobDocAddressNumber>(new ZQuery()).Length);
			addressNumbers = new BusinessObjectFactory().Load<JobDocAddressNumber>(new ZQuery(JobDocAddressNumberSchema.E2N_E2, pk));
			AssertEquals(1, addressNumbers.Length);
			Assert(addressNumbers.Any(x => x.E2N_NumberType == "ZZZ" && x.E2N_Number == "12345675" && x.E2N_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan));

			jobDocAddress.E2_AddressOverride = false;
			Factory.Save();
			AssertEquals(0, new BusinessObjectFactory().Load<JobDocAddressNumber>(new ZQuery()).Length);
		}

		public void TestIsExport()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var jobDocAddress = declaration.JobDocAddress;
			var manufacturerAddress = invoiceLine.ManufacturerDocAddress;
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var localProcessorAddress = controllingMessageHeader.LocalProcessorAddress;
			declaration.JE_MessageType = "EXP";
			CombineAssertions(() =>
			{
				Assert(jobDocAddress.IsExport);
				Assert(manufacturerAddress.IsExport);
				Assert(localProcessorAddress.IsExport);
			});

			declaration.JE_MessageType = "IMP";
			CombineAssertions(() =>
			{
				Assert(!jobDocAddress.IsExport);
				Assert(!manufacturerAddress.IsExport);
				Assert(!localProcessorAddress.IsExport);
			});

			var invoiceHeader = Factory.New<JobComInvoiceHeaderForJobDocAddressTest>();
			jobDocAddress = invoiceHeader.JobDocAddress;
			invoiceHeader.JZ_MessageType = "EXP";
			Assert(jobDocAddress.IsExport);
			invoiceHeader.JZ_MessageType = "IMP";
			Assert(!jobDocAddress.IsExport);
		}

		public void TestIsImport()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var jobDocAddress = declaration.JobDocAddress;
			var manufacturerAddress = invoiceLine.ManufacturerDocAddress;
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var localProcessorAddress = controllingMessageHeader.LocalProcessorAddress;
			declaration.JE_MessageType = "IMP";
			CombineAssertions(() =>
			{
				Assert(jobDocAddress.IsImport);
				Assert(manufacturerAddress.IsImport);
				Assert(localProcessorAddress.IsImport);
			});

			declaration.JE_MessageType = "EXP";
			CombineAssertions(() =>
			{
				Assert(!jobDocAddress.IsImport);
				Assert(!manufacturerAddress.IsImport);
				Assert(!localProcessorAddress.IsImport);
			});

			var invoiceHeader = Factory.New<JobComInvoiceHeaderForJobDocAddressTest>();
			jobDocAddress = invoiceHeader.JobDocAddress;
			invoiceHeader.JZ_MessageType = "IMP";
			Assert(jobDocAddress.IsImport);
			invoiceHeader.JZ_MessageType = "EXP";
			Assert(!jobDocAddress.IsImport);
		}

		public void TestImporterOrSupplierAddressDetailShouldExcludeUnrestrictedAdditionalAddressInformation()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.OrganisationPK = header.PK;

			CombineAssertions("Others", () =>
			{
				AssertEquals("Others AddressDetail", @"HAPPY CO., LTD.
1500 HAPPY RD
ORANGE DISTRICT
ADDINFO ADDRESS
APPLE CITY  TAIPEI  12345", jobDocAddress.AddressDetail);
				AssertEquals("Others LocalAddressDetail", @"綠晃科技股份有限公司
臺北加工出口區園東街6號
附加信息2
臺北巿  TAIPEI  台灣  90093", jobDocAddress.LocalAddressDetail);
			});

			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			CombineAssertions("Importer", () =>
			{
				AssertEquals("Importer AddressDetail", @"HAPPY CO., LTD.
1500 HAPPY RD
ORANGE DISTRICT
APPLE CITY  TAIPEI  12345", jobDocAddress.AddressDetail);
				AssertEquals("Importer LocalAddressDetail", @"綠晃科技股份有限公司
臺北加工出口區園東街6號
臺北巿  TAIPEI  台灣  90093", jobDocAddress.LocalAddressDetail);
			});

			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			CombineAssertions("Supplier", () =>
			{
				AssertEquals("Supplier AddressDetail", @"HAPPY CO., LTD.
1500 HAPPY RD
ORANGE DISTRICT
APPLE CITY  TAIPEI  12345", jobDocAddress.AddressDetail);
				AssertEquals("Supplier LocalAddressDetail", @"綠晃科技股份有限公司
臺北加工出口區園東街6號
臺北巿  TAIPEI  台灣  90093", jobDocAddress.LocalAddressDetail);
			});
		}

		public void TestCountryCodeWhenAddressOverrideIsTrue()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			header.MainAddress.OA_RN_NKCountryCode = "US";
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.OrganisationPK = header.PK;
			jobDocAddress.E2_AddressOverride = true;
			AssertEquals("US", jobDocAddress.E2_RN_NKCountryCode);
		}

		public void TestIsFreeTradeZone()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var address = header.MainAddress;

			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.OrganisationPK = header.PK;
			jobDocAddress.E2_OA_Address = address.PK;
			Assert(!jobDocAddress.IsFreeTradeZone);

			jobDocAddress.CBPCodeType = "FTZ";
			Assert(!jobDocAddress.IsFreeTradeZone);

			jobDocAddress.CBPCodeType = "AAA";
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ002", Core.Constants.CountryCodes.Australia);
			Assert(!jobDocAddress.IsFreeTradeZone);

			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
			Assert(jobDocAddress.IsFreeTradeZone);

			jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.OrganisationPK = header.PK;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.CBPCodeType = "AAA";
			Assert(!jobDocAddress.IsFreeTradeZone);

			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
			Assert(!jobDocAddress.IsFreeTradeZone);

			address.CustomsCodes.DeleteAll();
			jobDocAddress.CBPCodeType = "FTZ";
			Assert(jobDocAddress.IsFreeTradeZone);
		}

		public void TestIsIdentificationSameAsLocalProcessAddressForExport()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			declaration.JE_MessageType = "EXP";
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			var cMHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			var manufactureIDTypes = new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber };
			manufacturerDocAddress.E2_AddressOverride = true;
			localProcessorAddress.E2_AddressOverride = true;
			foreach (var codeTypes in manufactureIDTypes)
			{
				localProcessorAddress.IDCodeType = codeTypes;
				localProcessorAddress.IDCode = "123456789";
				manufacturerDocAddress.IDCodeType = codeTypes;
				manufacturerDocAddress.IDCode = "123456789";
				AssertEquals("IDCode and IDCodeType both Same LocalProcessAddress", true, manufacturerDocAddress.IsIdentificationSameAsLocalProcessAddressForExport(cMHeader));

				manufacturerDocAddress.IDCode = "99999999";
				AssertEquals("IDCode different LocalProcessAddress", false, manufacturerDocAddress.IsIdentificationSameAsLocalProcessAddressForExport(cMHeader));

				manufacturerDocAddress.IDCodeType = Constants.OrgCusCodeType.CustomCode;
				manufacturerDocAddress.IDCode = "123456789";
				AssertEquals("IDCodeType Type different LocalProcessAddress", false, manufacturerDocAddress.IsIdentificationSameAsLocalProcessAddressForExport(cMHeader));

				manufacturerDocAddress.FRICodeType = codeTypes;
				manufacturerDocAddress.FRICode = "123456789";
				AssertEquals("FRICode and FRICodeType both Same LocalProcessAddress", true, manufacturerDocAddress.IsIdentificationSameAsLocalProcessAddressForExport(cMHeader));

				manufacturerDocAddress.FRICode = "99999999";
				AssertEquals("FRICode different LocalProcessAddress", false, manufacturerDocAddress.IsIdentificationSameAsLocalProcessAddressForExport(cMHeader));

				manufacturerDocAddress.FRICodeType = Constants.OrgCusCodeType.CustomCode;
				manufacturerDocAddress.FRICode = "123456789";
				AssertEquals("FRICodeType different LocalProcessAddress", false, manufacturerDocAddress.IsIdentificationSameAsLocalProcessAddressForExport(cMHeader));
			}
		}

		public void TestCompanyChineseName()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var address = header.MainAddress;

			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var jobDocAddress = declaration.JobDocAddress;
			jobDocAddress.OrganisationPK = header.PK;
			jobDocAddress.E2_OA_Address = address.PK;
			Assert(!jobDocAddress.E2_AddressOverride);
			AssertEquals("綠晃科技股份有限公司", jobDocAddress.CompanyChineseName);

			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.LocalAddress.E2_CompanyName = "Override Name";
			AssertEquals("Override Name", jobDocAddress.CompanyChineseName);
		}

		public void TestCompanyEnglishName()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var jobDocAddress = declaration.JobDocAddress;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_CompanyName = "Override Name";
			AssertEquals("Override Name", jobDocAddress.CompanyEnglishName);

			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var address = header.MainAddress;
			jobDocAddress.E2_AddressOverride = false;
			jobDocAddress.OrganisationPK = header.PK;
			jobDocAddress.E2_OA_Address = address.PK;
			Assert(!jobDocAddress.E2_AddressOverride);
			AssertEquals("HAPPY CO., LTD.", jobDocAddress.CompanyEnglishName);

			address.OA_Language = Core.SharedConstants.Languages.French;
			AssertEquals(ZString.Empty, jobDocAddress.CompanyEnglishName);

			var translatedAddress = address.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			translatedAddress.OTA_CompanyName = "EN-US Name";
			AssertEquals("EN-US Name", jobDocAddress.CompanyEnglishName);
		}

		public void TestValidationType()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var jobDocAddress = declaration.JobDocAddress;
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();

			CombineAssertions(() =>
			{
				AssertType<TWJobDocAddressValidation>("LocalProcessorAddress Validation", controllingMessageHeader.LocalProcessorAddress.Validation);
				AssertType<TWJobDocAddressValidation>("SupplierDocumentaryAddress Validation", declaration.SupplierDocumentaryAddress.Validation);
				AssertType<TWJobDocAddressValidation>("ImporterDocumentaryAddress Validation", declaration.ImporterDocumentaryAddress.Validation);
				AssertType<TWJobDocAddressValidation>("ManufacturerAddress Validation", invoiceLine.ManufacturerDocAddress.Validation);
				AssertType<TWJobDocAddressValidation>("Ohter Validation", declaration.JobDocAddress.Validation);
			});
		}

		public void TestAddressCodeTypes()
		{
			var cMHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var jobDocAddress = Factory.New<TWJobDocAddress>();
				AssertType<BaseAddressCodeTypes>(jobDocAddress.AddressCodeTypes);

				jobDocAddress = Factory.New<TWJobDocAddress>();
				jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
				AssertType<ImporterAddressCodeTypes>(jobDocAddress.AddressCodeTypes);

				jobDocAddress = Factory.New<TWJobDocAddress>();
				jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
				AssertType<SupplierAddressCodeTypes>(jobDocAddress.AddressCodeTypes);

				jobDocAddress = Factory.New<TWJobDocAddress>();
				jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
				AssertType<ManufacturerAddressCodeTypes>(jobDocAddress.AddressCodeTypes);

				jobDocAddress = cMHeader.DocAddresses.AddNew() as TWJobDocAddress;
				jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
				AssertType<LocalProcessorAddressCodeTypes>(jobDocAddress.AddressCodeTypes);

				jobDocAddress = cMHeader.DocAddresses.AddNew() as TWJobDocAddress;
				jobDocAddress.DocAddressType = DocAddressType.Applicant;
				AssertType<CMApplicantAddressCodeTypes>(jobDocAddress.AddressCodeTypes);

				jobDocAddress = cMHeader.DocAddresses.AddNew() as TWJobDocAddress;
				jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
				AssertType<CMSupplierAddressCodeTypes>(jobDocAddress.AddressCodeTypes);

				jobDocAddress = cMHeader.DocAddresses.AddNew() as TWJobDocAddress;
				jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
				AssertType<CMImporterAddressCodeTypes>(jobDocAddress.AddressCodeTypes);
			});
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new TWJobDocAddressLightValidationTester(bizObjToTest);
		}

		class TWJobDocAddressLightValidationTester : LightValidationTester
		{
			public TWJobDocAddressLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != AutoJobDocAddressNumber.Schema.E2N_Number && propertyName != AutoJobDocAddressNumber.Schema.E2N_NumberType;
			}
		}

		public class JobDeclarationForJobDocAddressTest : JobDeclaration
		{
			public JobDeclarationForJobDocAddressTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public TWJobDocAddress JobDocAddress
			{
				get
				{
					if (fLocalProcessorAddress == null || fLocalProcessorAddress.IsDeleted)
					{
						fLocalProcessorAddress = TWJobDocAddresses.FindOrCreateWithDocAddressType(DocAddressType.LocalCartageAddress1);
						fLocalProcessorAddress.LocalAddressType = DocAddressType.AdditionalDeliveryAddress;
					}
					return fLocalProcessorAddress;
				}
			}
			TWJobDocAddress fLocalProcessorAddress;

			TWJobDocAddressDependentCollection TWJobDocAddresses
			{
				get
				{
					if (fDocAddresses == null)
					{
						fDocAddresses = new TWJobDocAddressDependentCollection(this);
						fDocAddresses.Load();
						RegisterEditableChildObject(fDocAddresses);
					}

					return fDocAddresses;
				}
			}
			TWJobDocAddressDependentCollection fDocAddresses;
		}

		public class JobComInvoiceHeaderForJobDocAddressTest : JobComInvoiceHeader
		{
			public JobComInvoiceHeaderForJobDocAddressTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public TWJobDocAddress JobDocAddress
			{
				get
				{
					if (fLocalProcessorAddress == null || fLocalProcessorAddress.IsDeleted)
					{
						fLocalProcessorAddress = TWJobDocAddresses.FindOrCreateWithDocAddressType(DocAddressType.LocalCartageAddress1);
						fLocalProcessorAddress.LocalAddressType = DocAddressType.AdditionalDeliveryAddress;
					}
					return fLocalProcessorAddress;
				}
			}
			TWJobDocAddress fLocalProcessorAddress;

			TWJobDocAddressDependentCollection TWJobDocAddresses
			{
				get
				{
					if (fDocAddresses == null)
					{
						fDocAddresses = new TWJobDocAddressDependentCollection(this);
						fDocAddresses.Load();
						RegisterEditableChildObject(fDocAddresses);
					}

					return fDocAddresses;
				}
			}
			TWJobDocAddressDependentCollection fDocAddresses;
		}
	}
}
