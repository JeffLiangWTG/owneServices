using System;
using System.Collections;
using System.Data;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderValidationRealTest : BusinessObjectValidationTestCase
	{
		#region Implementation

		OrgHeader company;

		protected override void SetUp()
		{
			base.SetUp();
			company = Factory.New<OrgHeader>();
			company.OH_FullName = "Test Organisation Pty Limited";
			company.OH_RL_NKClosestPort = "AUSYD";
			company.OH_Code = "DUUUUH";
			company.MainAddress.OA_Address1 = "This should have been updated";
			company.MainAddress.OA_City = "A city";
			company.MainAddress.OA_CompanyNameOverride = "Test Organisation Different Name";
		}

		#region OrgTypes used in testing

		string[] OrgTypes
		{
			get
			{
				if (orgTypes == null)
				{
					orgTypes = new ArrayList();
					orgTypes.Add(OrgHeader.Schema.OH_IsBroker);
					orgTypes.Add(OrgHeader.Schema.OH_IsShippingProvider);
					orgTypes.Add(OrgHeader.Schema.OH_IsCompetitor);
					orgTypes.Add(OrgHeader.Schema.OH_IsConsignee);
					orgTypes.Add(OrgHeader.Schema.OH_IsConsignor);
					orgTypes.Add(OrgHeader.Schema.OH_IsContainerYard);
					orgTypes.Add(OrgHeader.Schema.OH_IsAirCTO);
					orgTypes.Add(OrgHeader.Schema.OH_IsSeaCTO);
					orgTypes.Add(OrgHeader.Schema.OH_IsForwarder);
					orgTypes.Add(OrgHeader.Schema.OH_IsPackDepot);
					orgTypes.Add(OrgHeader.Schema.OH_IsSalesLead);
					orgTypes.Add(OrgHeader.Schema.OH_IsTransportClient);
					orgTypes.Add(OrgHeader.Schema.OH_IsWarehouseClient);
					orgTypes.Add(OrgCompanyDataSchema.OB_IsDebtor.Name);
					orgTypes.Add(OrgCompanyDataSchema.OB_IsCreditor.Name);
				}

				return (string[])orgTypes.ToArray(typeof(string));
			}
		}

		ArrayList orgTypes;

		#endregion

		#region Helper method

		string AirLineAndShippingLineBothCheckedErrorMessage => "An Organization record cannot be both a Shipping Line and an Airline. These should be setup as separate Organizations.";

		void AssertValidateAirLineAndShippingLineMutuallyExclusive(bool shouldHaveError, OrgHeader org)
		{
			if (shouldHaveError)
			{
				AssertHasError(org.OH_IsAirLineInfo, AirLineAndShippingLineBothCheckedErrorMessage);
				AssertHasError(org.OH_IsShippingLineInfo, AirLineAndShippingLineBothCheckedErrorMessage);
			}
			else
			{
				AssertNoErrors(org.OH_IsAirLineInfo);
				AssertNoErrors(org.OH_IsShippingLineInfo);
			}
		}

		#endregion

		#endregion

		#region TestValidateOH_Code

		public void TestValidateOH_Code()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			Env.Registry.CanUserEditOrganisationCode = true;

			OrgHeader company1 = newFactory.NewWithValidTestData<OrgHeader>();
			company1.OH_Code = "ABCDEF1";
			newFactory.Save();

			OrgHeader company2 = newFactory.NewWithValidTestData<OrgHeader>();

			company2.OH_Code = string.Empty;
			AssertHasErrors(company2.OH_CodeInfo);

			company2.OH_Code = "ABCDEF2";
			AssertNoErrors(company2.OH_CodeInfo);

			company2.OH_Code = "ABCDEF1";
			AssertHasErrors(company2.OH_CodeInfo);

			company2.OH_Code = "ABCDEF2";
			AssertNoErrors(company2.OH_CodeInfo);

			company2.OH_Code = "ABCD-EF2";
			AssertNoErrors(company2.OH_CodeInfo);

			company2.OH_Code = "ABC_DEF2";
			AssertNoErrors(company2.OH_CodeInfo);

			company2.OH_Code = "ABC.DEF2";
			AssertNoErrors(company2.OH_CodeInfo);

			company2.OH_Code = " ABCDEF2";
			AssertHasError(company2.OH_CodeInfo, (NoResString)"Organization codes cannot start with a space character. Please change your organization code.");

			Env.Registry.CanUserEditOrganisationCode = false;
			company2.OH_Code = "ABCDEF1";
			AssertNoErrors("No error as User cannot edit org code so validation doesn't run", company2.OH_CodeInfo);

			company2.OH_FullName = "互聯網上視頻";
			company2.OH_RL_NKClosestPort = "UAIEV";
			AssertHasWarning(company2.OH_CodeInfo,
				"Some characters from Organization Name could not be transliterated and were stripped out for the purpose of Code Generation. Non English characters are not allowed in Code. To edit Organization Codes, please enable " + ((IRegistryItemInternals)Env.Registry.RawRegistry.CanUserEditOrganisationCode).Location);

			Env.Registry.CanUserEditOrganisationCode = true;
			company2.OH_FullName = "互聯網上頻";
			company2.OH_RL_NKClosestPort = "GBLON";
			AssertHasWarning(company2.OH_CodeInfo,
				"Some characters from Organization Name could not be transliterated and were stripped out for the purpose of Code Generation. Non English characters are not allowed in Code.");

			company2.OH_FullName = "boom";
			company2.OH_Code = "互聯網上頻";
			AssertHasError(company2.OH_CodeInfo, "Organization Code only accepts Western European languages characters.");
		}

		#endregion

		#region OH_FullName

		public void TestValidateOH_FullName()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.Validation.ValidateOH_FullName();

			AssertEquals("Pre-condition", ZString.Empty, orgHeader.OH_FullName);
			AssertHasErrors("Full name is mandatory", orgHeader.OH_FullNameInfo);

			orgHeader.OH_FullName = "WizeTech Glabal";

			AssertNoErrors(orgHeader.OH_FullNameInfo);
			AssertNoWarnings(orgHeader.OH_FullNameInfo);

			orgHeader.OH_FullName = "Pablo Diego José Francisco de Paula Juan Nepomuceno Maria de los Remedios";

			AssertNoErrors(orgHeader.OH_FullNameInfo);
			AssertHasWarning(orgHeader.OH_FullNameInfo, "Some legacy documents cannot display company names over 50 characters. The name will be stored but truncated when displayed on these documents.");
		}

		public void TestValidateOH_FullNameOnDuplicateDetected()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.Validation.ValidateOH_FullName();

			AssertEquals("Pre-condition", ZString.Empty, orgHeader.OH_FullName);
			AssertHasErrors("Full name is mandatory", orgHeader.OH_FullNameInfo);

			orgHeader.OH_FullName = "WizeTech Glabal";

			AssertNoErrors(orgHeader.OH_FullNameInfo);
			AssertNoWarnings(orgHeader.OH_FullNameInfo);

			orgHeader.ValidateDuplicationResult(true);
			AssertHasWarning(orgHeader.OH_FullNameInfo, "The name you have entered resulted in potential duplicates. Please confirm that they are actual duplicates.");
		}

		public void TestValidateOH_FullNameCannotBeChangedOnceATransactionIsPostedInAPTCompany()
		{
			var ptCompany = Factory.NewWithValidTestData<GlbCompany>();
			ptCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;
			ptCompany.GC_IsGSTRegistered = false;

			var ptBranch = Factory.NewWithValidTestData<GlbBranch>();
			ptBranch.GB_GC = ptCompany.PK;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WizeTech Glabal";
			orgHeader.OH_RL_NKClosestPort = "PTLIS";

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_FullName = "WizeTech Glabal Duplicate";
			orgHeader1.OH_RL_NKClosestPort = "PTLIS";

			var cc1 = Factory.NewWithValidTestData<AccChargeCode>();
			cc1.AC_Code = "CC1";
			cc1.AC_Desc = "CC1 Desc";
			cc1.AC_GC = ptBranch.GB_GC;
			cc1.AC_ChargeType = "MRG";

			Factory.Save();

			AssertEquals(Constants.CountryCodes.Portugal, orgHeader.CountryCode);
			AssertEquals(Constants.CountryCodes.Portugal, orgHeader1.CountryCode);

			orgHeader.OH_FullName = "WizeTech Glabal Modified";
			AssertNoErrors(orgHeader.OH_FullNameInfo);

			orgHeader1.OH_FullName = "WizeTech Glabal Duplicate Modified";
			AssertNoErrors(orgHeader1.OH_FullNameInfo);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ptBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var newFactory = new BusinessObjectFactory();
				var arInvoice = newFactory.NewWithValidTestData<AccTransactionHeader>();
				arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				arInvoice.AH_TransactionType = TransactionTypes.Invoice;
				arInvoice.AH_OH = orgHeader.PK;

				var arInvoiceLine = newFactory.NewWithValidTestData<AccTransactionLines>();
				arInvoiceLine.AL_AH = arInvoice.PK;
				arInvoiceLine.AL_AT = ZGuid.Empty;
				arInvoiceLine.AL_AC = cc1.PK;
				arInvoiceLine.AL_LineAmount = 44;

				newFactory.Save();
			}

			orgHeader.OH_FullName = "WizeTech Glabal Modified again";
			AssertNoErrors("Still no error, as validation process is pulling value from cache.", orgHeader.OH_FullNameInfo);

			orgHeader1.OH_FullName = "WizeTech Glabal Duplicate Modified again";
			AssertNoErrors("Still no error", orgHeader1.OH_FullNameInfo);

			orgHeader.RunPreSaveValidation();
			orgHeader1.RunPreSaveValidation();
			AssertHasError("There should be an error, as PreSaveValidation process does not use cache.", orgHeader.OH_FullNameInfo, "You cannot edit the full name.At least one transaction has been posted in a Portugal Login Company in this database using this Organization.");
			AssertNoErrors("Still no error", orgHeader1.OH_FullNameInfo);

			orgHeader.OH_RL_NKClosestPort = "MXMEX";
			orgHeader1.OH_RL_NKClosestPort = "MXMEX";

			AssertEquals(Constants.CountryCodes.Mexico, orgHeader.CountryCode);
			AssertEquals(Constants.CountryCodes.Mexico, orgHeader1.CountryCode);

			orgHeader.RunPreSaveValidation();
			orgHeader1.RunPreSaveValidation();
			AssertNoErrors("No error because orgHeader country is not Portugal", orgHeader.OH_FullNameInfo);
			AssertNoErrors("Still no error", orgHeader1.OH_FullNameInfo);
		}

		#endregion

		#region TestValidateOH_RL_NKClosestPort

		public void TestValidateRelatedPortCode()
		{
			company.OH_RL_NKClosestPort = "////";
			AssertHasErrors("Related port on MainAddress is linked to Closest port", company.MainAddress.OA_RL_NKRelatedPortCodeInfo);

			company.OH_RL_NKClosestPort = "AUSYD";
			AssertNoErrors("Related port on MainAddress is linked to Closest port", company.MainAddress.OA_RL_NKRelatedPortCodeInfo);

			Type type = typeof(BusinessObject);
			FieldInfo rowField = type.GetField("Row", BindingFlags.Instance | BindingFlags.NonPublic);
			DataRow oaRow = rowField.GetValue(company.MainAddress) as DataRow;

			oaRow["OA_RL_NKRelatedPortCode"] = "UAIEV";
			company.MainAddress.Validation.ValidateOA_RL_NKRelatedPortCode();
			AssertHasErrors("Related port on MainAddress is linked to Closest port", company.MainAddress.OA_RL_NKRelatedPortCodeInfo);

			oaRow["OA_RL_NKRelatedPortCode"] = "AUSYD";
			company.MainAddress.Validation.ValidateOA_RL_NKRelatedPortCode();
			AssertNoErrors("Related port on MainAddress is linked to Closest port", company.MainAddress.OA_RL_NKRelatedPortCodeInfo);
		}

		#endregion

		#region TestValidateOH_RSL_ShippingLine

		public void TestValidateOH_RSL_ShippingLine()
		{
			company.OH_RSL_ShippingLine = new Guid("13A10572-601F-4BF1-A1A1-D382B82DEE2D");
			company.Validation.ValidateOH_RSL_ShippingLine();
			var expectedMessage = "This C1C Code is already entered against Organization " + company.OH_Code + ". \r\nPlease remove it from that Organization in order to save it against this one";
			AssertNoError(company.OH_RSL_ShippingLineInfo, expectedMessage);

			var company2 = Factory.New<OrgHeader>();
			company2.OH_Code = "DUUUUH2";
			company2.OH_RSL_ShippingLine = company.OH_RSL_ShippingLine;
			company2.Validation.ValidateOH_RSL_ShippingLine();
			AssertHasError(company2.OH_RSL_ShippingLineInfo, expectedMessage);

			company.Validation.ValidateOH_RSL_ShippingLine();
			expectedMessage = "This C1C Code is already entered against Organization " + company2.OH_Code + ". \r\nPlease remove it from that Organization in order to save it against this one";
			AssertHasError(company.OH_RSL_ShippingLineInfo, expectedMessage);
		}

		#endregion

		#region TestValidateRequiredFieldsFromRegistry

		public void TestValidateOA_Address2()
		{
			OrgRequiredFields fields = new OrgRequiredFields(true, false, false, false, false, false, false, false, false, false, false);
			CheckFieldIsMandatoryForDifferentOrgTypes(company.MainAddress, OrgAddressSchema.OA_Address2.Name, fields);
		}

		public void TestValidateOH_Branch()
		{
			OrgRequiredFields fields = new OrgRequiredFields(false, true, false, false, false, false, false, false, false, false, false);
			CheckGuidFieldIsMandatoryForOrgType(company.CompanyData, OrgCompanyDataSchema.OB_GB_ControllingBranch.Name, fields);
		}

		public void TestValidateOA_City()
		{
			OrgRequiredFields fields = new OrgRequiredFields(false, false, true, false, false, false, false, false, false, false, false);
			CheckFieldIsMandatoryForDifferentOrgTypes(company.MainAddress, OrgAddressSchema.OA_City.Name, fields);
		}

		public void TestValidateOA_Phone()
		{
			OrgRequiredFields fields = new OrgRequiredFields(false, false, false, true, false, false, false, false, false, false, false);
			CheckFieldIsMandatoryForDifferentOrgTypes(company.MainAddress, "OA_Phone_Formatted", fields, "02 9665 4455");
		}

		public void TestValidateOA_Fax()
		{
			OrgRequiredFields fields = new OrgRequiredFields(false, false, false, false, false, false, true, false, false, false, false);
			CheckFieldIsMandatoryForDifferentOrgTypes(company.MainAddress, "OA_Fax_Formatted", fields, "02 9665 4455");
		}

		public void TestValidateOA_Email()
		{
			OrgRequiredFields fields = new OrgRequiredFields(false, false, false, false, false, false, false, true, false, false, false);
			CheckFieldIsMandatoryForDifferentOrgTypes(company.MainAddress, OrgAddressSchema.OA_Email.Name, fields);
		}

		public void CheckGuidFieldIsMandatoryForOrgType(BusinessObject bizO, ZString field, OrgRequiredFields fields)
		{
			SetRequiredFields(fields);
			company.OH_IsTempAccount = false;

			foreach (ZString orgType in OrgTypes)
			{
				CheckRequireGuidFieldForOrgType(bizO, field, orgType);
			}

			company.OH_IsTempAccount = true;
			CheckRequireGuidFieldForOrgType(bizO, field, OrgHeader.Schema.OH_IsConsignee);
			CheckRequireGuidFieldForOrgType(bizO, field, OrgHeader.Schema.OH_IsConsignor);
			CheckRequireGuidFieldForOrgType(bizO, field, OrgCompanyDataSchema.OB_IsCreditor.Name);
			CheckRequireGuidFieldForOrgType(bizO, field, OrgCompanyDataSchema.OB_IsDebtor.Name);
		}

		void CheckRequireGuidFieldForOrgType(BusinessObject bizO, ZString field, ZString orgType)
		{
			Assert(field + " already has an error", !bizO.ZPropertyInfoHash[field].HasErrors());
			SetOrgType(orgType, false);

			bizO[field] = ZGuid.Empty;
			company.RunPreSaveValidation();
			Assert(field + " should not be mandatory for " + orgType, !bizO.ZPropertyInfoHash[field].HasErrors());

			SetOrgType(orgType, true);
			bizO[field] = ZGuid.Empty;
			company.RunPreSaveValidation();
			Assert(field + " should be mandatory for " + orgType, bizO.ZPropertyInfoHash[field].HasErrors());

			bizO[field] = ZGuid.NewZGuid();
			company.RunPreSaveValidation();
			Assert(field + " should be entered for " + orgType, !bizO.ZPropertyInfoHash[field].HasErrors());

			SetOrgType(orgType, false); // Reset OrgType for following tests
		}

		public void TestValidateRequireFaxEmailOrWeb()
		{
			OrgRequiredFields fields = new OrgRequiredFields(false, false, false, false, false, false, false, false, false, true, false);
			SetRequiredFields(fields);
			company.OH_IsTempAccount = false;

			foreach (ZString orgType in OrgTypes)
			{
				CheckFaxEmailOrWebForOrgType(orgType);
			}

			company.OH_IsTempAccount = true;
			CheckFaxEmailOrWebForOrgType(OrgHeader.Schema.OH_IsConsignee);
			CheckFaxEmailOrWebForOrgType(OrgHeader.Schema.OH_IsConsignor);
			CheckFaxEmailOrWebForOrgType(OrgCompanyDataSchema.OB_IsCreditor.Name);
			CheckFaxEmailOrWebForOrgType(OrgCompanyDataSchema.OB_IsDebtor.Name);
		}

		#region Required Field Org Type Combinations

		public void TestRequiredFieldsForOrgTypeCombination()
		{
			OrgRequiredFields brokerFields = new OrgRequiredFields(false, false, true, false, false, false, false, false, false, false, false);    // City required
			OrgRequiredFields creditorFields = new OrgRequiredFields(false, false, false, false, true, false, true, false, false, false, false);   // GST number, Fax number
			OrgRequiredFields debtorFields = new OrgRequiredFields(false, false, false, false, false, false, false, false, false, true, false);    // GST number, FaxEmailOrWeb
			Env.Registry.SetOrgBrokerRequiredFields(brokerFields);
			Env.Registry.SetOrgCreditorRequiredFields(creditorFields);
			Env.Registry.SetOrgDebtorRequiredFields(debtorFields);

			company.RunPreSaveValidation();
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_Address2);
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_City);
			CheckFieldIsNotInError(company.MainAddress, "OA_Phone_Formatted");
			CheckFieldIsNotInError(company.MainAddress, "OA_Fax_Formatted");
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_Email);

			company.OH_IsBroker = true;
			company.OH_IsCreditor = true;
			company.OH_IsDebtor = true;
			company.MainAddress.OA_City = string.Empty;
			company.MainAddress.OA_Phone_Formatted = string.Empty;
			company.MainAddress.OA_Fax_Formatted = string.Empty;
			company.RunPreSaveValidation();
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_Address2);
			CheckFieldIsInError(company.MainAddress, OrgAddress.Schema.OA_City);
			CheckFieldIsNotInError(company.MainAddress, "OA_Phone_Formatted");
			CheckFieldIsInError(company.MainAddress, "OA_Fax_Formatted");
			CheckFieldIsInError(company.MainAddress, OrgAddress.Schema.OA_Email);
			CheckFieldIsInError(company.MainWebURL, OrgWebURL.Schema.PU_URL);

			company.MainAddress.OA_City = "New City";
			company.MainAddress.OA_Email = "email@example.com";
			company.MainAddress.OA_Phone_Formatted = string.Empty;
			company.MainAddress.OA_Fax_Formatted = string.Empty;
			company.RunPreSaveValidation();
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_Address2);
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_City);
			CheckFieldIsNotInError(company.MainAddress, "OA_Phone_Formatted");
			CheckFieldIsInError(company.MainAddress, "OA_Fax_Formatted");
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_Email);
			CheckFieldIsNotInError(company.MainWebURL, OrgWebURL.Schema.PU_URL);

			company.MainAddress.OA_Fax_Formatted = "02 9988 7766";
			company.RunPreSaveValidation();
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_Address2);
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_City);
			CheckFieldIsNotInError(company.MainAddress, "OA_Phone_Formatted");
			CheckFieldIsNotInError(company.MainAddress, "OA_Fax_Formatted");
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_Email);
			CheckFieldIsNotInError(company.MainWebURL, OrgWebURL.Schema.PU_URL);
		}

		public void TestCreditorRequiredFieldsCompanySpecific()
		{
			OrgRequiredFields creditorFieldsForSystem = new OrgRequiredFields(false, false, false, false, false, false, false, true, false, false, false); // Email address
			OrgRequiredFields creditorFieldsForCompany = new OrgRequiredFields(false, false, false, false, false, false, true, false, false, false, false); // Fax number

			Env.Registry.SetOrgCreditorRequiredFields(creditorFieldsForSystem);
			Env.Registry.SetOrgCreditorRequiredFieldsForSpecificCompany(Env.CurrentCompanyPK, creditorFieldsForCompany);

			company.OH_IsCreditor = true;
			company.RunPreSaveValidation();
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_Email);
			CheckFieldIsInError(company.MainAddress, "OA_Fax_Formatted");
		}

		public void TestDebtorRequiredFieldsCompanySpecific()
		{
			OrgRequiredFields debtorFieldsForSystem = new OrgRequiredFields(false, false, false, false, false, false, false, true, false, false, false); // Email address
			OrgRequiredFields debtorFieldsForCompany = new OrgRequiredFields(false, false, false, false, false, false, true, false, false, false, false); // Fax number

			Env.Registry.SetOrgDebtorRequiredFields(debtorFieldsForSystem);
			Env.Registry.SetOrgDebtorRequiredFieldsForSpecificCompany(Env.CurrentCompanyPK, debtorFieldsForCompany);

			company.OH_IsDebtor = true;
			company.RunPreSaveValidation();
			CheckFieldIsNotInError(company.MainAddress, OrgAddress.Schema.OA_Email);
			CheckFieldIsInError(company.MainAddress, "OA_Fax_Formatted");
		}

		public void CheckFieldIsInError(BusinessObject bizO, ZString field)
		{
			Assert(field + " should be in error", bizO.ZPropertyInfoHash[field].HasErrors());
		}

		public void CheckFieldIsNotInError(BusinessObject bizO, ZString field)
		{
			Assert(field + " should not be in error", !bizO.ZPropertyInfoHash[field].HasErrors());
		}

		#endregion

		void CheckFieldIsMandatoryForDifferentOrgTypes(BusinessObject bizO, ZString field, OrgRequiredFields fields)
		{
			ZString validValue = "www.test.com";
			if (field == OrgAddressSchema.OA_Email.Name)
			{
				validValue = "test@example.com";
			}

			CheckFieldIsMandatoryForDifferentOrgTypes(bizO, field, fields, validValue);
		}

		void CheckFieldIsMandatoryForDifferentOrgTypes(BusinessObject bizO, ZString field, OrgRequiredFields fields, ZString validValue)
		{
			SetRequiredFields(fields);
			company.OH_IsTempAccount = false;

			foreach (ZString orgType in OrgTypes)
			{
				CheckOrgFieldForOrgType(bizO, field, orgType, validValue);
			}

			company.OH_IsTempAccount = true;
			CheckOrgFieldForOrgType(bizO, field, OrgHeader.Schema.OH_IsConsignee, validValue);
			CheckOrgFieldForOrgType(bizO, field, OrgHeader.Schema.OH_IsConsignor, validValue);
			CheckOrgFieldForOrgType(bizO, field, OrgCompanyDataSchema.OB_IsCreditor.Name, validValue);
			CheckOrgFieldForOrgType(bizO, field, OrgCompanyDataSchema.OB_IsDebtor.Name, validValue);
		}

		void CheckOrgFieldForOrgType(BusinessObject bizO, ZString field, ZString orgType, ZString validValue)
		{
			Assert(field + " already has an error", !bizO.ZPropertyInfoHash[field].HasErrors());
			SetOrgType(orgType, false);

			bizO[field] = string.Empty;
			company.RunPreSaveValidation();
			Assert(field + " should not be mandatory for " + orgType, !bizO.ZPropertyInfoHash[field].HasErrors());

			SetOrgType(orgType, true);
			bizO[field] = string.Empty;
			company.RunPreSaveValidation();
			Assert(field + " should be mandatory for " + orgType, bizO.ZPropertyInfoHash[field].HasErrors());

			bizO[field] = validValue;
			company.RunPreSaveValidation();
			Assert(field + " should be entered for " + orgType, !bizO.ZPropertyInfoHash[field].HasErrors());

			SetOrgType(orgType, false); // Reset OrgType for following tests
		}

		void SetOrgType(ZString orgType, ZBool validationRequired)
		{
			if (orgType.Equals(OrgCompanyDataSchema.OB_IsCreditor.Name) || orgType.Equals(OrgCompanyDataSchema.OB_IsDebtor.Name))
			{
				company.CompanyData[orgType] = validationRequired;
			}
			else
			{ company[orgType] = validationRequired; }
		}

		void CheckFaxEmailOrWebForOrgType(ZString orgType)
		{
			Assert("MainAddress.OA_Fax_Formatted already has an error", !company.MainAddress.OA_Fax_FormattedInfo.HasErrors());
			Assert("MainAddress.OA_Email already has an error", !company.MainAddress.OA_EmailInfo.HasErrors());
			Assert("MainWebURL.PU_URL already has an error", !company.MainWebURL.PU_URLInfo.HasError(OrgWebURLValidation.RequiredFieldsMessage));

			SetOrgType(orgType, false);
			company.MainWebURL.PU_URL = string.Empty;
			company.MainAddress.OA_Email = string.Empty;
			company.MainAddress.OA_Fax_Formatted = string.Empty;
			company.RunPreSaveValidation();
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should not be entered for " + orgType, !company.MainAddress.OA_Fax_FormattedInfo.HasErrors());
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should not be entered for " + orgType, !company.MainAddress.OA_EmailInfo.HasErrors());
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should not be entered for " + orgType, !company.MainWebURL.PU_URLInfo.HasError(OrgWebURLValidation.RequiredFieldsMessage));

			SetOrgType(orgType, true);
			company.MainWebURL.PU_URL = string.Empty;
			company.MainAddress.OA_Email = string.Empty;
			company.MainAddress.OA_Fax_Formatted = string.Empty;
			company.RunPreSaveValidation();
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be mandatory for " + orgType, company.MainAddress.OA_Fax_FormattedInfo.HasErrors());
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be mandatory for " + orgType, company.MainAddress.OA_EmailInfo.HasErrors());
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be mandatory for " + orgType, company.MainWebURL.PU_URLInfo.HasError(OrgWebURLValidation.RequiredFieldsMessage));

			company.MainWebURL.PU_URL = string.Empty;
			company.MainAddress.OA_Email = string.Empty;
			company.MainAddress.OA_Fax_Formatted = "02 9966 8877";
			company.RunPreSaveValidation();
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be entered for " + orgType, !company.MainAddress.OA_Fax_FormattedInfo.HasErrors());
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be entered for " + orgType, !company.MainAddress.OA_EmailInfo.HasErrors());
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be entered for " + orgType, !company.MainWebURL.PU_URLInfo.HasError(OrgWebURLValidation.RequiredFieldsMessage));

			company.MainWebURL.PU_URL = string.Empty;
			company.MainAddress.OA_Email = "test@example.com";
			company.MainAddress.OA_Fax_Formatted = string.Empty;
			company.RunPreSaveValidation();
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be entered for " + orgType, !company.MainAddress.OA_Fax_FormattedInfo.HasErrors());
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be entered for " + orgType, !company.MainAddress.OA_EmailInfo.HasErrors());
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be entered for " + orgType, !company.MainWebURL.PU_URLInfo.HasError(OrgWebURLValidation.RequiredFieldsMessage));

			company.MainWebURL.PU_URL = "www.test.com";
			company.MainAddress.OA_Email = string.Empty;
			company.MainAddress.OA_Fax_Formatted = string.Empty;
			company.RunPreSaveValidation();
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be entered for " + orgType, !company.MainAddress.OA_Fax_FormattedInfo.HasErrors());
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be entered for " + orgType, !company.MainAddress.OA_EmailInfo.HasErrors());
			Assert("Either MainAddress.OA_Fax_Formatted, MainAddress.OA_Email or MainWebURL.PU_URL should be entered for " + orgType, !company.MainWebURL.PU_URLInfo.HasError(OrgWebURLValidation.RequiredFieldsMessage));

			SetOrgType(orgType, false); // Reset OrgType for following tests
		}

		void SetRequiredFields(OrgRequiredFields fields)
		{
			Env.Registry.SetTempOrgConsigneeRequiredFields(fields);
			Env.Registry.SetTempOrgConsignorRequiredFields(fields);
			Env.Registry.SetTempOrgCreditorRequiredFields(fields);
			Env.Registry.SetTempOrgDebtorRequiredFields(fields);
			Env.Registry.SetOrgBrokerRequiredFields(fields);
			Env.Registry.SetOrgCarrierRequiredFields(fields);
			Env.Registry.SetOrgCompetitorRequiredFields(fields);
			Env.Registry.SetOrgConsigneeRequiredFields(fields);
			Env.Registry.SetOrgConsignorRequiredFields(fields);
			Env.Registry.SetOrgContainerYardRequiredFields(fields);
			Env.Registry.SetOrgCreditorRequiredFields(fields);
			Env.Registry.SetOrgCTORequiredFields(fields);
			Env.Registry.SetOrgDebtorRequiredFields(fields);
			Env.Registry.SetOrgForwarderRequiredFields(fields);
			Env.Registry.SetOrgPackDepotRequiredFields(fields);
			Env.Registry.SetOrgSalesLeadRequiredFields(fields);
			Env.Registry.SetOrgTransportClientRequiredFields(fields);
			Env.Registry.SetOrgWarehouseRequiredFields(fields);
		}

		#endregion

		#region TestValidateCustomLabelFieldName

		public void TestValidateCustomLabelFieldName()
		{
			OrgCustomLabels label1 = Factory.New<OrgCustomLabels>();
			company.CustomLabels.Add(label1);
			label1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomDate1;
			Assert("CustomLabels.Count == 1", company.CustomLabels.Count == 1);
			Assert("First label, no notifications", !label1.OT_FieldNameInfo.HasNotifications());

			OrgCustomLabels label2 = Factory.New<OrgCustomLabels>();
			company.CustomLabels.Add(label2);
			label2.OT_FieldName = Constants.CustomLabels.OrderLine.CustomDate2;
			Assert("CustomLabels.Count == 2", company.CustomLabels.Count == 2);
			Assert("First label, no notifications", !label1.OT_FieldNameInfo.HasNotifications());
			Assert("Second label, different text, no notifications", !label2.OT_FieldNameInfo.HasNotifications());

			OrgCustomLabels label3 = Factory.New<OrgCustomLabels>();
			company.CustomLabels.Add(label3);
			label3.OT_FieldName = Constants.CustomLabels.OrderLine.CustomDate1;
			Assert("CustomLabels.Count == 3", company.CustomLabels.Count == 3);
			Assert("First label, no notifications", !label1.OT_FieldNameInfo.HasNotifications());
			Assert("Second label, different text, no notifications", !label2.OT_FieldNameInfo.HasNotifications());
			Assert("Third label, test same as first label, error expected", label3.OT_FieldNameInfo.HasErrors());

			label3.OT_FieldName = Constants.CustomLabels.OrderLine.CustomDate3;
			Assert("CustomLabels.Count == 3", company.CustomLabels.Count == 3);
			Assert("First label, no notifications", !label1.OT_FieldNameInfo.HasNotifications());
			Assert("Second label, different text, no notifications", !label2.OT_FieldNameInfo.HasNotifications());
			Assert("Third label, different text, no notifications", !label3.OT_FieldNameInfo.HasNotifications());
		}

		#endregion

		#region TestValidateOH_IsActive

		public void TestValidateOH_IsActive()
		{
			StmNote note = company.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.InactiveRecordDetails.Description;
			note.ST_IsCustomDescription = false;

			company.OH_IsActive = false;
			Assert("No errors - note is valid for inactive org", !company.OH_IsActiveInfo.HasErrors());

			company.OH_IsActive = true;
			Assert("Error - note is not valid for active org", company.OH_IsActiveInfo.HasErrors());
		}

		#endregion

		#region TestValidateOH_Language

		public void TestValidateOH_Language()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			org.OH_Language = string.Empty;
			Assert("Error - no language entered", org.OH_LanguageInfo.HasErrors());

			org.OH_Language = "ZZZ";
			Assert("Error - invalid language entered", org.OH_LanguageInfo.HasErrors());

			org.OH_Language = Core.Constants.Languages.English;
			Assert("No error - valid language entered", !org.OH_LanguageInfo.HasErrors());
		}

		#endregion

		#region TestValidateOH_Category

		public void TestValidateOH_Category()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			org.OH_Category = string.Empty;
			Assert("Error - no category entered", org.OH_CategoryInfo.HasErrors());

			org.OH_Category = "ZZZ";
			Assert("Error - invalid category entered", org.OH_CategoryInfo.HasErrors());

			org.OH_Category = OrgConstants.Category.Government;
			Assert("No error - valid category entered", !org.OH_CategoryInfo.HasErrors());
		}

		#endregion

		#region TestValidate_ScreeningStatus

		public void TestValidate_ScreeningStatus_AllowedValues()
		{
			var org = Factory.New<OrgHeader>();
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Canceled;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Block;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NeedsScreening;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Release;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.RequiresReview;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = string.Empty;
			AssertHasError(org.OH_ScreeningStatusInfo, "Please enter a Screening Status.");

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(org.OH_ScreeningStatusInfo);

			org.OH_ScreeningStatus = "ZZZ";
			AssertHasError(org.OH_ScreeningStatusInfo, "Enter a valid Screening Status.");
		}

		#endregion

		#region TestValidateAPSettlementGroupPK

		public void TestValidateAPSettlementGroupPK()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgHeader relatedOrg = Factory.New<OrgHeader>();
			relatedOrg.OH_IsActive = false;
			org.APSettlementGroupPK = relatedOrg.PK;
			AssertHasWarning(org.APSettlementGroupPKInfo, "The Settlement Group is Inactive.");
		}

		#endregion

		#region TestValidateARSettlementGroupPK

		public void TestValidateARSettlementGroupPK()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgHeader relatedOrg = Factory.New<OrgHeader>();
			relatedOrg.OH_IsActive = false;
			org.ARSettlementGroupPK = relatedOrg.PK;
			AssertHasWarning(org.ARSettlementGroupPKInfo, "The Settlement Group is Inactive.");
		}

		#endregion

		#region TestValidateAirLineAndShippingLineMutuallyExclusive

		public void TestValidateAirLineAndShippingLineMutuallyExclusive()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderValidationReal = new OrgHeaderValidationReal(org);
			org.OH_IsAirLine = true;
			org.OH_IsShippingLine = false;
			AssertValidateAirLineAndShippingLineMutuallyExclusive(false, org);

			org.OH_IsShippingLine = true;
			AssertValidateAirLineAndShippingLineMutuallyExclusive(true, org);

			orgHeaderValidationReal.ValidateAll();
			AssertValidateAirLineAndShippingLineMutuallyExclusive(true, org);

			org.OH_IsAirLine = false;
			AssertValidateAirLineAndShippingLineMutuallyExclusive(false, org);

			org.OH_IsAirLine = true;
			AssertValidateAirLineAndShippingLineMutuallyExclusive(true, org);

			org.OH_IsShippingLine = false;
			AssertValidateAirLineAndShippingLineMutuallyExclusive(false, org);
		}

		#endregion

		public void TestValidateControllingCustomerMandatory()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.SetOrgTypeAsExpectedAndValidate(orgHeader.OH_IsControllingCustomerInfo);
			AssertHasError(orgHeader.OH_IsControllingCustomerInfo, "An Organization selected from here must have an Organization Type of Controlling Customer selected.");

			orgHeader.OH_IsControllingCustomer = true;
			orgHeader.SetOrgTypeAsExpectedAndValidate(orgHeader.OH_IsControllingCustomerInfo);
			AssertNoError(orgHeader.OH_IsControllingCustomerInfo, "An Organization selected from here must have an Organization Type of Controlling Customer selected.");
		}

		public void TestValidateControllingAgentMandatory()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.SetOrgTypeAsExpectedAndValidate(orgHeader.OH_IsControllingAgentInfo);
			AssertHasError(orgHeader.OH_IsControllingAgentInfo, "An Organization selected from here must have an Organization Type of Controlling Agent selected.");

			orgHeader.OH_IsControllingAgent = true;
			orgHeader.SetOrgTypeAsExpectedAndValidate(orgHeader.OH_IsControllingAgentInfo);
			AssertNoError(orgHeader.OH_IsControllingAgentInfo, "An Organization selected from here must have an Organization Type of Controlling Agent selected.");
		}
	}
}
