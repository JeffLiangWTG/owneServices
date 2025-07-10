using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OrgHeaderWrapper))]
	public class OrgHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetCustomsStateCode()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "COL";
			refCountryStates1.RW_RN_NKCountryCode = "CA";
			refCountryStates1.RW_Description = "COLIMA";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.IMPSTA, "OUT", "Import State Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.IMPSTA, "MXCMX", "DIF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = "CA";
			org.MainAddress.OA_State = "COLIMA";
			org.MainAddress.OA_RL_NKRelatedPortCode = "CA2NB";
			var wrapper = OrgHeaderWrapper.New(org.MainAddress) as IPGAContactDetails;
			var companyAddress = wrapper.CompanyAddress;
			AssertEquals("COL", companyAddress.State);

			org = Factory.New<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = "MX";
			org.MainAddress.OA_RL_NKRelatedPortCode = "MX2NB";
			org.MainAddress.OA_State = "CMX";
			wrapper = OrgHeaderWrapper.New(org.MainAddress);
			companyAddress = wrapper.CompanyAddress;
			AssertEquals("DIF", companyAddress.State);

			org = Factory.New<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Myanmar;
			wrapper = OrgHeaderWrapper.New(org.MainAddress);
			companyAddress = wrapper.CompanyAddress;
			AssertEquals(Core.Constants.CountryCodes.Myanmar, companyAddress.Country);
		}

		public void TestNofityParty()
		{
			var org = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(org);

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "SSN", Core.Constants.CountryCodes.UnitedStates);

			wrapper.ZO_NPID = "1234";
			AssertEquals("1234", wrapper.ZO_NPID);

			wrapper.ZO_OH_NP = notifyParty.PK;
			AssertEquals("SSN", wrapper.ZO_NPID);
			Assert(wrapper.ZO_NPIDInfo.ReadOnly);

			notifyParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "CBN", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("CBN", wrapper.ZO_NPID);

			notifyParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "EIN", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("EIN", wrapper.ZO_NPID);

			wrapper.ZO_OH_NP = ZGuid.Empty;
			AssertEquals(ZString.Empty, wrapper.ZO_NPID);
		}

		public void TestIControllerIDProviderMembers()
		{
			var org = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(org);

			IControllerIDProvider provider = OrgHeaderWrapper.New(org);
			AssertEquals("ControllerID", ControllerIDs.Organisation, provider.ControllerID);
			AssertEquals("BusinessObjectPK", org.PK.ToGuid(), provider.BusinessObjectPK);
		}

		//CS00136107 - there are two plugins into org which each create a new wrapper. they should use the same wrapper and the same addinfo
		public void TestWhenMultipleCallsToNewWithinTheSameFactoryOnlyOneAddInfo()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(org);
			wrapper.ZO_ImporterType = ImporterTypeList.Codes.Corporation;

			OrgHeaderWrapper secondWrapperSameOrg = OrgHeaderWrapper.New(org);
			AssertEquals(ImporterTypeList.Codes.Corporation, secondWrapperSameOrg.ZO_ImporterType);

			secondWrapperSameOrg.ZO_ImporterType = "";
			AssertEquals("", wrapper.ZO_ImporterType);
		}

		public void TestValuesArePersistedOK()
		{
			OrgHeaderWrapper wrapper = GetNewWrapper();
			wrapper.ZO_ImporterType = "Q";
			AssertEquals("Q", wrapper.ZO_ImporterType);
			wrapper.ZO_AccountNo = "123008";
			AssertEquals("123008", wrapper.ZO_AccountNo);
			wrapper.ZO_IsEINNumberVerifiedIndicator = "N";
			AssertEquals("N", wrapper.ZO_IsEINNumberVerifiedIndicator);
			wrapper.ZO_MFRRegExempt = "A";
			AssertEquals("A", wrapper.ZO_MFRRegExempt);
			wrapper.ZO_PaymentType = "8";
			AssertEquals("8", wrapper.ZO_PaymentType);
			wrapper.ZO_TaxDeferredInd = "2";
			AssertEquals("2", wrapper.ZO_TaxDeferredInd);
			wrapper.ZO_DoNotAutoGenerateSDCR = false;
			AssertEquals(false, wrapper.ZO_DoNotAutoGenerateSDCR);
			wrapper.ZO_NAFTAReconIndicator = true;
			AssertEquals(true, wrapper.ZO_NAFTAReconIndicator);
			wrapper.ZO_FileTheirOwnRecon = true;
			AssertEquals(true, wrapper.ZO_FileTheirOwnRecon);
			wrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.Class9802Recon;
			AssertEquals(ReconIssueCodeList.Codes.Class9802Recon, wrapper.ZO_OtherReconIndicator);
			wrapper.ZO_ENSPrintCustomAttrib1 = true;
			AssertEquals(true, wrapper.ZO_ENSPrintCustomAttrib1);
			wrapper.ZO_ENSPrintCustomAttrib2 = false;
			AssertEquals(false, wrapper.ZO_ENSPrintCustomAttrib2);
			wrapper.ZO_ENSPrintCustomAttrib3 = true;
			AssertEquals(true, wrapper.ZO_ENSPrintCustomAttrib3);
			wrapper.ZO_ENSPrintProduct = true;
			AssertEquals(true, wrapper.ZO_ENSPrintProduct);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgHeader organisationLoaded = factory2.Load<OrgHeader>(Organisation.PK);
			OrgHeaderWrapper wrapperLoaded = OrgHeaderWrapper.New(organisationLoaded);
			AssertEquals("Q", wrapperLoaded.ZO_ImporterType);
			AssertEquals("123008", wrapperLoaded.ZO_AccountNo);
			AssertEquals("N", wrapperLoaded.ZO_IsEINNumberVerifiedIndicator);
			AssertEquals("A", wrapperLoaded.ZO_MFRRegExempt);
			AssertEquals("8", wrapperLoaded.ZO_PaymentType);
			AssertEquals("2", wrapperLoaded.ZO_TaxDeferredInd);
			AssertEquals(false, wrapperLoaded.ZO_DoNotAutoGenerateSDCR);
			AssertEquals(true, wrapperLoaded.ZO_NAFTAReconIndicator);
			AssertEquals(true, wrapperLoaded.ZO_FileTheirOwnRecon);
			AssertEquals(ReconIssueCodeList.Codes.Class9802Recon, wrapperLoaded.ZO_OtherReconIndicator);
			AssertEquals(true, wrapper.ZO_ENSPrintProduct);
			AssertEquals(true, wrapper.ZO_ENSPrintCustomAttrib1);
			AssertEquals(false, wrapper.ZO_ENSPrintCustomAttrib2);
			AssertEquals(true, wrapper.ZO_ENSPrintCustomAttrib3);
		}

		public void TestFDAContactValues()
		{
			OrgHeaderWrapper wrapper = GetNewWrapper();
			Organisation.MainAddress.OA_State = "IL";
			Organisation.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";

			DeclarationTestHelper.AddPGAContact(Organisation.MainAddress, null, null, null, null, "");
			Organisation.MainAddress.OA_Fax = "13029938478";
			AssertEquals("3029938478", wrapper.ContactFaxForFDA);

			DeclarationTestHelper.AddPGAContact(Organisation.MainAddress, null, null, null, null, "+44 80019999");
			AssertEquals("4480019999", wrapper.ContactFaxForFDA);

			Organisation.MainAddress.OA_Fax = "";
			DeclarationTestHelper.AddPGAContact(Organisation.MainAddress, null, null, null, null, "");
			AssertEquals("0000000000", wrapper.ContactFaxForFDA);

			DeclarationTestHelper.AddPGAContact(Organisation.MainAddress, null, null, null, "", null);
			AssertEquals("NONE", wrapper.ContactEmailForFDA);

			Organisation.MainAddress.OA_State = "";
			Organisation.MainAddress.OA_RL_NKRelatedPortCode = "BSNAS";

			DeclarationTestHelper.AddPGAContact(Organisation.MainAddress, null, null, "", null, null);
			Organisation.MainAddress.OA_Phone = "+1 (242) 35006955";
			AssertEquals("+1 (242) 35006955", Organisation.MainAddress.OA_Phone);
			AssertEquals("Phone number for Non US Company, (Nasau Bahamas), should not strip out leading 1 from main address phone number", "124235006955", wrapper.ContactPhoneNoForFDA);

			DeclarationTestHelper.AddPGAContact(Organisation.MainAddress, null, null, null, null, "");
			Organisation.MainAddress.OA_Fax = "+1 (242) 35004222";
			AssertEquals("+1 (242) 35004222", Organisation.MainAddress.OA_Fax);
			AssertEquals("FAX number for Non US Company, (Nasau Bahamas), should not strip out leading 1 from main address fax number", "124235004222", wrapper.ContactFaxForFDA);
		}

		public void TestWrappedPropertyInfo()
		{
			OrgHeaderWrapper wrapper = GetNewWrapper();
			wrapper.ZO_ImporterType = "~";
			AssertHasMessageError(wrapper.ZO_ImporterTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestIsGovernmentImporter()
		{
			OrgHeaderWrapper wrapper = GetNewWrapper();
			wrapper.ZO_ImporterType = ImporterTypeList.Codes.ForeignGovernment;
			AssertEquals(true, wrapper.IsGovernmentImporter);

			wrapper.ZO_ImporterType = ImporterTypeList.Codes.Corporation;
			AssertEquals(false, wrapper.IsGovernmentImporter);

			wrapper.ZO_ImporterType = ImporterTypeList.Codes.StateGovernment;
			AssertEquals(true, wrapper.IsGovernmentImporter);

			wrapper.ZO_ImporterType = ImporterTypeList.Codes.SoleProprietor;
			AssertEquals(false, wrapper.IsGovernmentImporter);

			wrapper.ZO_ImporterType = ImporterTypeList.Codes.USGovernment;
			AssertEquals(true, wrapper.IsGovernmentImporter);
		}

		public void TestGetCustomsCode()
		{
			OrgHeaderWrapper wrapper = GetNewWrapper();
			OrgCusCode cusCode1 = wrapper.organisation.CustomsCodes.AddNew("AAA", "123");

			var cusCode2 = wrapper.organisation.CustomsCodes.AddNew("BBB", "235");

			AssertEquals("123", wrapper.GetCustomsCode(new ZString[] { "AAA", "BBB" }));
			AssertEquals("235", wrapper.GetCustomsCode(new ZString[] { "BBB", "AAA" }));

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("123", wrapper.GetCustomsCode(new ZString[] { "AAA", "BBB" }));
			AssertEquals("235", wrapper.GetCustomsCode(new ZString[] { "BBB", "AAA" }));
		}

		public void TestGetCustomsCodeWithDifferentTypes()
		{
			Organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EncryptedConsigneeNumber, "-23GGFRD234");
			AssertEquals("-23GGFRD234", OrgHeaderWrapper.GetCustomsCode(Organisation, OrgCusCode.USACodeTypes.EncryptedConsigneeNumber));

			Organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "GFGGFRD234");
			AssertEquals("GFGGFRD234", OrgHeaderWrapper.GetCustomsCode(Organisation, OrgCusCode.USACodeTypes.EmployerIdentificationNumber));
		}

		public void TestGetAddressCustomsRelatedCode()
		{
			Organisation.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "RT23GGFRD234");
			AssertEquals("RT23GGFRD234", OrgHeaderWrapper.GetAddressCustomsRelatedCode(Organisation.MainAddress, OrgMatchedCustomsRegNoType.MID));
		}

		public void TestGetCustomsCodeFromAddress()
		{
			Organisation.MainAddress.OA_OH = new Guid();
			AssertNoExceptionThrown(() => OrgHeaderWrapper.GetCustomsCodeFromAddress(Organisation.MainAddress, Core.Constants.CountryCodes.UnitedStates));
		}

		public void TestOrganisationProxiedProperties()
		{
			OrgHeaderWrapper wrapper = GetNewWrapper();
			wrapper.organisation.OH_FullName = "testing";
			AssertEquals("testing", wrapper.FullName);
			AssertEquals(wrapper.organisation.MainAddress, wrapper.MainAddress);

			OrgAddress mailingaddress = wrapper.organisation.Addresses.AddNew(OrgAddressType.Postal, true);
			AssertEquals(mailingaddress, wrapper.MailingAddress);
		}

		public void TestCollection()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = Organisation;

			OrgHeaderWrapper wrapper = GetNewWrapper();
			AssertNotNull(wrapper.Messages);
			AssertEquals(1, wrapper.Messages.Count);
			AssertEquals(true, wrapper.Messages.Contains(message.PK));
		}

		public void TestImportAddInfo()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			OrgHeader org = Factory.New<OrgHeader>();
			AssertEquals(org.CountryData.OV_RN_NKClientCountryRelation, Core.Constants.CountryCodes.Australia);
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(org);

			Assert("Does not throw exception when accessing ImportAddInfo", !wrapper.ImportAddInfo.HasWarnings);
		}

		public void TestBIRDDefaultBranch()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(org);
			AssertNull(wrapper.BIRDDefaultBranch);

			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			wrapper.ZO_GB = branch.PK;
			AssertEquals(branch, wrapper.BIRDDefaultBranch);
		}

		public void TestZO_BrokerToPayDefault()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(org);
			AssertEquals("Broker To Pay should be empty when Org created", ZString.Empty, wrapper.ZO_BrokerToPay);

			wrapper.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			AssertEquals("Broker To Pay should be NO, because Importer will pay", YesNoDefaultList.Codes.No, wrapper.ZO_BrokerToPay);

			wrapper.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals("Broker To Pay should be YES", YesNoDefaultList.Codes.Yes, wrapper.ZO_BrokerToPay);

			wrapper.ZO_BrokerToPay = ZString.Empty;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();

			OrgHeaderWrapper.New(creditor).ZO_AccountNo = "111111";
			creditor.CompanyData.OB_AB_APDefaultBankAccount = bankAccount.PK;
			Factory.Save();
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creditor.PK.ToGuid());

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "111111";
			account.BankAccount = bankAccount2.PK;
			USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			wrapper.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			wrapper.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHDebit;
			wrapper.ZO_AccountNo = "111111";
			AssertEquals("Broker To Pay should be YES, because Broker will pay using managed account", YesNoDefaultList.Codes.Yes, wrapper.ZO_BrokerToPay);

			wrapper.ZO_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Broker To Pay should be empty, because 'Individual Basis' and user should choose", ZString.Empty, wrapper.ZO_BrokerToPay);
		}

		public void TestDefaultReconBrokerToPay()
		{
			var org = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(org);
			AssertEquals("Recon Broker To Pay should be empty when Org created", ZString.Empty, wrapper.ZO_ReconBrokerToPay);

			wrapper.ZO_ReconPaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			AssertEquals("Recon Broker To Pay should be NO, because Importer will pay", YesNoDefaultList.Codes.No, wrapper.ZO_ReconBrokerToPay);

			wrapper.ZO_ReconPaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals("Recon Broker To Pay should be YES", YesNoDefaultList.Codes.Yes, wrapper.ZO_ReconBrokerToPay);

			wrapper.ZO_ReconPaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Recon Broker To Pay should be empty, because user should choose", ZString.Empty, wrapper.ZO_ReconBrokerToPay);
		}

		public void TestGetReconIssueCalculated()
		{
			var org = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(org);
			AssertEquals("Recon Issue - New organisation default", ReconIssues.NA, wrapper.GetReconIssueCalculated());

			wrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueClass9802Recon;
			AssertEquals("Recon Issue", ReconIssues._98 | ReconIssues.CL | ReconIssues.VL, wrapper.GetReconIssueCalculated());

			wrapper.ZO_OtherReconIndicator = ZString.Empty;
			AssertEquals("Recon Issue", ReconIssues.None, wrapper.GetReconIssueCalculated());
		}

		public void TestZO_OtherReconIndicator()
		{
			var org = Factory.New<OrgHeader>();
			var countryData = org.CountryData;
			var wrapper = OrgHeaderWrapper.New(org);
			AssertEquals("ZO_OtherReconIndicator by default on creation of new Organisation should be Not Applicable", ReconIssueCodeList.Codes.NotApplicable, wrapper.ZO_OtherReconIndicator);

			wrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes._9802Recon;
			AssertEquals("ZO_OtherReconIndicator", "98", wrapper.ZO_OtherReconIndicator);

			wrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			AssertEquals("ZO_OtherReconIndicator", "CL", wrapper.ZO_OtherReconIndicator);

			wrapper.ZO_OtherReconIndicator = ZString.Empty;
			AssertEquals("ZO_OtherReconIndicator - can be validly set to blank", "", wrapper.ZO_OtherReconIndicator);
		}

		public void TestCompanyAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "TEST NAME FROM HEADER";
			orgHeader.MainAddress.OA_Address1 = "ADDRESS1 FROM HEADER";
			orgHeader.MainAddress.OA_Email = "EMAIL@HEADER";

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_CompanyNameOverride = "TEST NAME FROM ADDRESS";
			orgAddress.OA_Address1 = "ADDRESS1 FROM ADDRESS";
			orgAddress.OA_Email = "EMAIL@ADDRESS";

			var wrapperFromHeader = OrgHeaderWrapper.New(orgHeader);
			var companyAddressFromHeader = ((IPGAContactDetails)wrapperFromHeader).CompanyAddress;
			AssertEquals("TEST NAME FROM HEADER", companyAddressFromHeader.CompanyName);
			AssertEquals("ADDRESS1 FROM HEADER", companyAddressFromHeader.AddressLine1);
			AssertEquals("EMAIL@HEADER", companyAddressFromHeader.Email);

			var wrapperFromAddress = OrgHeaderWrapper.New(orgAddress);
			var companyAddressFromAddress = ((IPGAContactDetails)wrapperFromAddress).CompanyAddress;
			AssertEquals("TEST NAME FROM ADDRESS", companyAddressFromAddress.CompanyName);
			AssertEquals("ADDRESS1 FROM ADDRESS", companyAddressFromAddress.AddressLine1);
			AssertEquals("EMAIL@ADDRESS", companyAddressFromAddress.Email);
		}

		public void TestCompanyNameWithLongName()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "1111111111AAAAAAAAAABBBBBBBBBBEEEEEEEEEETTTTTTTTTT$$$$$$$$$$555555555ooooooooooMMMMMMMMMM";
			orgHeader.MainAddress.OA_Address1 = "ADDRESS1 FROM HEADER";
			orgHeader.MainAddress.OA_Email = "EMAIL@HEADER";

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_CompanyNameOverride = "xxxxxxxxxxZZZZZZZZZZvvvvvvvvvvNNNNNNNNNNooooooooooEEEEEEEEEEqqqqqqqqqqTTTTTTTTTT";
			orgAddress.OA_Address1 = "ADDRESSOverride";
			orgAddress.OA_Email = "EMAIL@ADDRESS";

			var wrapperFromHeader = OrgHeaderWrapper.New(orgHeader);
			var companyAddressFromHeader = ((IPGAContactDetails)wrapperFromHeader).CompanyAddress;
			AssertEquals("1111111111AAAAAAAAAABBBBBBBBBBEEEEEEEEEETTTTTTTTTT$$$$$$$$$$555555555ooooooooooMMMMMMMMMM", companyAddressFromHeader.CompanyName);
			AssertEquals("ADDRESS1 FROM HEADER", companyAddressFromHeader.AddressLine1);
			AssertEquals("EMAIL@HEADER", companyAddressFromHeader.Email);

			var wrapperFromAddress = OrgHeaderWrapper.New(orgAddress);
			var companyAddressFromAddress = ((IPGAContactDetails)wrapperFromAddress).CompanyAddress;
			AssertEquals("xxxxxxxxxxZZZZZZZZZZvvvvvvvvvvNNNNNNNNNNooooooooooEEEEEEEEEEqqqqqqqqqqTTTTTTTTTT", companyAddressFromAddress.CompanyName);
			AssertEquals("ADDRESSOverride", companyAddressFromAddress.AddressLine1);
			AssertEquals("EMAIL@ADDRESS", companyAddressFromAddress.Email);
		}
		public void TestState()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_Address1 = "ABC";
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "US2WC";
			orgHeader.MainAddress.OA_State = "IL";
			var wrapperFromHeader = OrgHeaderWrapper.New(orgHeader);
			var companyAddressFromAddress = (IPGAContactDetails)wrapperFromHeader;
			AssertEquals(companyAddressFromAddress.CompanyAddress.State, "IL");
			AssertEquals(companyAddressFromAddress.CompanyAddress.Country, "US");

			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUMLK";
			orgHeader.MainAddress.OA_State = "NSW";
			wrapperFromHeader = OrgHeaderWrapper.New(orgHeader);
			companyAddressFromAddress = wrapperFromHeader;
			AssertEquals(companyAddressFromAddress.CompanyAddress.State, ZString.Empty);

			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "CAVAN";
			orgHeader.MainAddress.OA_State = "XA";
			wrapperFromHeader = OrgHeaderWrapper.New(orgHeader);
			companyAddressFromAddress = wrapperFromHeader;
			AssertEquals(companyAddressFromAddress.CompanyAddress.State, "XA");

			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "PRCAR";
			orgHeader.MainAddress.OA_State = "PR";
			wrapperFromHeader = OrgHeaderWrapper.New(orgHeader);
			companyAddressFromAddress = wrapperFromHeader;
			AssertEquals(companyAddressFromAddress.CompanyAddress.State, "PR");
			AssertEquals(companyAddressFromAddress.CompanyAddress.Country, "US");

			orgHeader.MainAddress.OA_State = "SAN JUAN";
			wrapperFromHeader = OrgHeaderWrapper.New(orgHeader);
			companyAddressFromAddress = wrapperFromHeader;
			AssertEquals(companyAddressFromAddress.CompanyAddress.State, "PR");
			AssertEquals(companyAddressFromAddress.CompanyAddress.Country, "US");
		}

		public void TestGetAddressForOrganisation()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			AssertEquals(orgHeader.MainAddress.PK, OrgHeaderWrapper.GetAddressForOrganisation(orgHeader).PK);

			var newAddress = orgHeader.Addresses.AddNew();
			AssertEquals(orgHeader.MainAddress.PK, OrgHeaderWrapper.GetAddressForOrganisation(orgHeader).PK);

			newAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			AssertEquals(newAddress.PK, OrgHeaderWrapper.GetAddressForOrganisation(orgHeader).PK);
		}

		public void TestGetAddressForPSTCarrier()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			AssertEquals(orgHeader.MainAddress.PK, OrgHeaderWrapper.GetAddressForPSTCarrier(orgHeader).PK);

			orgHeader.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			var mxAddress = orgHeader.Addresses.AddNew();
			mxAddress.OA_RL_NKRelatedPortCode = "MX~";
			AssertEquals(mxAddress.PK, OrgHeaderWrapper.GetAddressForPSTCarrier(orgHeader).PK);

			var caAddress = orgHeader.Addresses.AddNew();
			caAddress.OA_RL_NKRelatedPortCode = "CA~";
			AssertEquals(caAddress.PK, OrgHeaderWrapper.GetAddressForPSTCarrier(orgHeader).PK);

			var usAddress = orgHeader.Addresses.AddNew();
			usAddress.OA_RL_NKRelatedPortCode = "US~";
			AssertEquals(usAddress.PK, OrgHeaderWrapper.GetAddressForPSTCarrier(orgHeader).PK);

			usAddress.OA_IsActive = false;
			AssertEquals(caAddress.PK, OrgHeaderWrapper.GetAddressForPSTCarrier(orgHeader).PK);
		}
		public void TestOrgHeaderImporterBondQueryDate()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "ORG1";
			org2.OH_Code = "ORG2";

			MQEDIMessage message1 = Factory.New<MQEDIMessage>();
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2016, 9, 21);
			message1.EM_IsActive = true;
			message1.EM_MessageType = "KI";
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_LinkedObject = org1;
			MQEDIMessage message2 = Factory.New<MQEDIMessage>();
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2016, 7, 21);
			message2.EM_IsActive = true;
			message2.EM_MessageType = "KI";
			message2.EM_ReceiveTransmit = "TRX";
			message2.EM_LinkedObject = org1;
			Factory.Save();

			AssertEquals(new ZDateTime(2016, 9, 21), org1.ImporterBondQueryDate);
			AssertEquals(ZDateTime.Empty, org2.ImporterBondQueryDate);
		}

		public void TestOrgHeaderConsigneeAndShipTo()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TESCNE";
			var cneAddress = consignee.Addresses.AddNew();
			cneAddress.OA_Address1 = "CONSIGNEE ADDRESS 1";
			var shipTo = Factory.New<OrgHeader>();
			shipTo.OH_Code = "TESSHIP";
			var shipToAddress = shipTo.Addresses.AddNew();
			shipToAddress.OA_Address1 = "SHIPTO ADDRESS 1";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMP";
			var importerWrapper = OrgHeaderWrapper.New(importer);
			importerWrapper.ZO_OA_ConsigneeAddress = cneAddress.PK;
			importerWrapper.ZO_OA_ShipToAddress = shipToAddress.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedImporter = newFactory.Load<OrgHeader>(importer.PK);
			var loadedWrapper = OrgHeaderWrapper.New(loadedImporter);
			AssertEquals(cneAddress.PK, loadedWrapper.ZO_OA_ConsigneeAddress);
			AssertEquals(shipToAddress.PK, loadedWrapper.ZO_OA_ShipToAddress);
		}

		public void TestWarehouseDocAddressLoadsFromCountryData()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseAddr = Factory.New<OrgAddress>();

			org.CountryData.OV_OA_WarehouseAddress = warehouseAddr.PK;
			var wrapperFromHeader = OrgHeaderWrapper.New(org);
			AssertEquals(warehouseAddr.PK, wrapperFromHeader.OV_OA_WarehouseAddress);
		}

		public void TestWarehouseDocAddressSetsCountryData()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			var wrapperFromHeader = OrgHeaderWrapper.New(org);

			var docAddress = wrapperFromHeader.OV_OA_WarehouseAddress;
			Assert(!docAddress.IsValid);

			var warehouseAddr = Factory.New<OrgAddress>();
			wrapperFromHeader.OV_OA_WarehouseAddress = warehouseAddr.PK;
			AssertEquals(warehouseAddr.PK, org.CountryData.OV_OA_WarehouseAddress);
		}

		OrgHeaderWrapper GetNewWrapper()
		{
			return (OrgHeaderWrapper)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return OrgHeaderWrapper.New(Organisation);
		}

		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<OrgHeader>();
					fOrganisation.FillWithValidTestData();
				}
				return fOrganisation;
			}
		}
		OrgHeader fOrganisation;
	}
}
