using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrganisationsFindBoxCollection))]
	public class OrganisationsFindBoxCollectionTest : BusinessObjectCollectionTestCase
	{
		#region OrganisationsForTest

		public class OrganisationsForTest : OrganisationsFindBoxCollection
		{
			public OrganisationsForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public OrganisationsForTest(BusinessObjectFactory factory, bool regenerateCodeWhenOrgTypesChange)
				: base(factory)
			{
				ResetTheOrgCodeRegenFlag = regenerateCodeWhenOrgTypesChange;
			}

			readonly bool ResetTheOrgCodeRegenFlag;

			protected override bool ShouldRegenerateCodeWhenOrgTypesChange
			{
				get { return ResetTheOrgCodeRegenFlag; }
			}

			public new ZQuery AdditionalFilter
			{
				get { return base.CreateAdditionalFilter(); }
			}
		}

		#endregion

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new OrganisationsFindBoxCollection(Factory, orgDefaults);
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			AssertEquals(true, ((OrganisationsFindBoxCollection)GetCollectionToTest()).AllowNewTemporaryOrganisations);
		}

		[ExpectExceptionMessage(typeof(Exception), "Unknown prefix")]
		public void TestDefaultsForNewChildPropertyWithInvalidFieldNameForOrgHeader()
		{
			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add("InvalidField", "Test");
			Organisations.DefaultsForNewChild = defaults;
		}

		[ExpectExceptionMessage(typeof(Exception), "Invalid field name for defaults - OM_InvalidField")]
		public void TestDefaultsForNewChildPropertyWithInvalidFieldNameForOrgMiscServ()
		{
			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add("OM_InvalidField", "Test");
			Organisations.DefaultsForNewChild = defaults;
		}

		[ExpectExceptionMessage(typeof(Exception), "Invalid field name for defaults - OB_InvalidField")]
		public void TestDefaultsForNewChildPropertyWithInvalidFieldNameForOrgCompanyData()
		{
			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add("OB_InvalidField", "Test");
			Organisations.DefaultsForNewChild = defaults;
		}

		[ExpectExceptionMessage(typeof(Exception), "Invalid field name for defaults - OA_InvalidField")]
		public void TestDefaultsForNewChildPropertyWithInvalidFieldNameForOrgAddress()
		{
			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add("OA_InvalidField", "Test");
			Organisations.DefaultsForNewChild = defaults;
		}

		[ExpectNoExceptions]
		public void TestDefaultsForNewChildPropertyWithValidFieldNames()
		{
			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add(OrgHeader.Schema.OH_IsSalesLead, true);
			defaults.Add(OrgHeader.Schema.OH_IsConsignee, false);
			defaults.Add(OrgMiscServ.Schema.OM_IMSendImportDocsTo, "TST");
			defaults.Add(OrgCompanyData.Schema.OB_IsCreditor, true);
			defaults.Add(OrgAddress.Schema.OA_Address2, true);
			Organisations.DefaultsForNewChild = defaults;
		}

		public void TestRemoveDefaultsFromUnmatchedNote()
		{
			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add(new OrgFieldDefault { IsAddedFromUnMatchedNote = true });
			defaults.Add(new OrgFieldDefault { IsAddedFromUnMatchedNote = false });
			defaults.RemoveDefaultsFromUnmatchedNote();
			AssertEquals(1, defaults.Count);
			AssertNotEquals(0, defaults.Count);
		}

		public void TestSetDefaultsForNewChild()
		{
			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();

			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add(OrgHeader.Schema.OH_IsSalesLead, true);
			defaults.Add(OrgHeader.Schema.OH_RL_NKClosestPort, "AUSYD");
			defaults.Add(OrgMiscServ.Schema.OM_CMClientSize, "TST");
			defaults.Add(OrgCompanyData.Schema.OB_IsCreditor, true);
			defaults.Add(OrgAddress.Schema.OA_Address1, "Address 1");
			defaults.Add(new OrgFieldDefault() { FieldName = OrgAddress.Schema.OA_Address2, Value = new ZString("Address 2"), IsConditional = true });
			Organisations.DefaultsForNewChild = defaults;
			OrgHeader newOrg = Organisations.AddNew();

			((IOrganisationDefaultProvider)Organisations).ShouldSetValuesFromConditionalDefaults = false;

			Assert("SalesLead is selected", newOrg.OH_IsSalesLead);
			AssertEquals("UNLOCO", "AUSYD", newOrg.OH_RL_NKClosestPort);
			AssertEquals("MiscServ.OM_CM", "TST", newOrg.MiscServ.OM_CMClientSize);
			AssertEquals("CompanyData.OB_IsCreditor", true, newOrg.CompanyData.OB_IsCreditor);
			AssertEquals("OrgAddress.OA_AddressLine1", "Address 1", newOrg.MainAddress.OA_Address1);
			AssertNotEquals("OrgAddress.OA_AddressLine2 shouldn't be default as ShouldSetValuesFromConditionalDefaults is set to false", "Address 2", newOrg.MainAddress.OA_Address2);

			((IOrganisationDefaultProvider)Organisations).ShouldSetValuesFromConditionalDefaults = true;
			newOrg = Organisations.AddNew();
			AssertEquals("OrgAddress.OA_AddressLine2 ", "Address 2", newOrg.MainAddress.OA_Address2);
		}

		public void TestValidateEntityOnSaving()
		{
			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add(OrgHeader.Schema.OH_IsSalesLead, true);
			Organisations.DefaultsForNewChild = defaults;

			OrgHeader organisation = Organisations.AddNew();
			organisation.OH_IsSalesLead = false;
			Organisations.ValidateEntityOnSaving(organisation);
			Assert("Organision.OH_IsSalesLead has error", organisation.OH_IsSalesLeadInfo.HasErrors());

			organisation.OH_IsSalesLead = true;
			Organisations.ValidateEntityOnSaving(organisation);
			Assert("Organision.OH_IsSalesLead has no errors", !organisation.OH_IsSalesLeadInfo.HasErrors());
		}

		public void TestValidateEntityOnSavingWithMiscServ()
		{
			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add(OrgHeader.Schema.OH_IsForwarder, true);
			defaults.Add(OrgMiscServ.Schema.OM_FWAgentBelongsToGroup, true);
			Organisations.DefaultsForNewChild = defaults;

			OrgHeader organisation = Organisations.AddNew();
			organisation.OH_IsForwarder = false;
			organisation.MiscServ.OM_FWAgentBelongsToGroup = false;
			Organisations.ValidateEntityOnSaving(organisation);
			Assert("Organision.OH_IsForwarder has error", organisation.OH_IsForwarderInfo.HasErrors());
			Assert("AgentBelongsToGroup has error", organisation.MiscServ.OM_FWAgentBelongsToGroupInfo.HasErrors());
			organisation.OH_IsForwarder = true;
			organisation.MiscServ.OM_FWAgentBelongsToGroup = true;
			Organisations.ValidateEntityOnSaving(organisation);
			Assert("Organision.OH_IsForwarder has no errors", !organisation.OH_IsForwarderInfo.HasErrors());
			Assert("AgentBelongsToGroup has no error", !organisation.MiscServ.OM_FWAgentBelongsToGroupInfo.HasErrors());
		}

		public void TestValidateEntityOnSavingWithCompanyData()
		{
			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add(OrgHeader.Schema.OH_IsForwarder, true);
			defaults.Add(OrgCompanyData.Schema.OB_IsDebtor, true);
			Organisations.DefaultsForNewChild = defaults;

			OrgHeader organisation = Organisations.AddNew();
			using (organisation.SuspendValidationTesting())
			using (organisation.CompanyData.SuspendValidationTesting())
			{
				organisation.OH_IsForwarder = false;
				organisation.CompanyData.OB_IsDebtor = false;
				Organisations.ValidateEntityOnSaving(organisation);
				Assert("Organision.OH_IsForwarder has error", organisation.OH_IsForwarderInfo.HasErrors());
				Assert("Organision.OrgCompanyData.OB_IsDebtor has error", organisation.CompanyData.OB_IsDebtorInfo.HasErrors());

				organisation.OH_IsForwarder = true;
				organisation.CompanyData.OB_IsDebtor = true;
				Organisations.ValidateEntityOnSaving(organisation);
				Assert("Organision.OH_IsForwarder has no errors", !organisation.OH_IsForwarderInfo.HasErrors());
				Assert("Organision.OrgCompanyData.OB_IsDebtor has no errors", !organisation.CompanyData.OB_IsDebtorInfo.HasErrors());
			}
		}

		public void TestValidateEntityOnSavingWhenNotIncludedInValidation()
		{
			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add(OrgHeader.Schema.OH_IsForwarder, true);
			defaults.Add(OrgHeader.Schema.OH_RL_NKClosestPort, "AUSYD", false);
			defaults.Add(OrgMiscServ.Schema.OM_FWAgentBelongsToGroup, true);
			defaults.Add(OrgMiscServ.Schema.OM_FWRequestForCreditAllowed, true, false);

			Organisations.DefaultsForNewChild = defaults;

			OrgHeader organisation = Organisations.AddNew();
			organisation.OH_IsForwarder = false;
			organisation.OH_RL_NKClosestPort = "USLAX";
			organisation.MiscServ.OM_FWAgentBelongsToGroup = false;
			organisation.MiscServ.OM_FWRequestForCreditAllowed = false;

			Organisations.ValidateEntityOnSaving(organisation);
			Assert("Organision.OH_IsForwarder has error", organisation.OH_IsForwarderInfo.HasErrors());
			Assert("Organision.OH_RL_NKClosestPort has no errors", !organisation.OH_RL_NKClosestPortInfo.HasErrors());
			Assert("Organisation.MiscServ.OM_FWAgentBelongsToGroup has error", organisation.MiscServ.OM_FWAgentBelongsToGroupInfo.HasErrors());
			Assert("Organisation.MiscServ.OM_FWRequestForCreditAllowed has  no errors", !organisation.MiscServ.OM_FWRequestForCreditAllowedInfo.HasErrors());

			organisation.OH_IsForwarder = true;
			organisation.MiscServ.OM_FWAgentBelongsToGroup = true;
			Organisations.ValidateEntityOnSaving(organisation);
			Assert("Organision.OH_IsForwarder has no errors", !organisation.OH_IsForwarderInfo.HasErrors());
			Assert("Organision.OH_RL_NKClosestPort has no errors", !organisation.OH_RL_NKClosestPortInfo.HasErrors());
			Assert("Organisation.MiscServ.OM_FWAgentBelongsToGroup has no errors", !organisation.MiscServ.OM_FWAgentBelongsToGroupInfo.HasErrors());
			Assert("Organisation.MiscServ.OM_FWRequestForCreditAllowed has  no errors", !organisation.MiscServ.OM_FWRequestForCreditAllowedInfo.HasErrors());
		}

		public void TestRegenerateCodeWhenOrgTypesChange()
		{
			SetupOrgCodeOverrideRegistries();

			OrganisationDefaults defaults = new OrganisationDefaults();
			defaults.Add(OrgHeader.Schema.OH_RL_NKClosestPort, "AUSYD", false);
			defaults.Add(OrgHeader.Schema.OH_FullName, "LALALA");
			defaults.Add(OrgHeader.Schema.OH_IsConsignee, true);
			Organisations.DefaultsForNewChild = defaults;

			OrgHeader organisation = Organisations.AddNew();
			AssertEquals("Precondition: RegenerateCodeWhenOrgTypesChange is false", false, organisation.RegenerateCodeWhenOrgTypesChange);
			AssertEquals("organisation is a consignee", true, organisation.OH_IsConsignee);

			AssertEquals("OH_Code", "1", organisation.OH_Code);

			Organisations = new OrganisationsForTest(Factory, true);
			Organisations.DefaultsForNewChild = defaults;
			organisation = Organisations.AddNew();
			AssertEquals("Precondition: RegenerateCodeWhenOrgTypesChange is true", true, organisation.RegenerateCodeWhenOrgTypesChange);
			AssertEquals("organisation is a consignee", true, organisation.OH_IsConsignee);

			organisation.Factory.Save();
			AssertEquals("OH_Code is generated", true, !organisation.OH_Code.IsEmpty);
		}

		void SetupOrgCodeOverrideRegistries()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Override;
			algorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Consignee].Selected = true;

			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Length = 5;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 0;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Order = 0;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 0;
			algorithm.Elements[OrgCodeElementDescription.CountryCode].Order = 0;
			algorithm.Elements[OrgCodeElementDescription.UnlocoCode].Order = 0;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 0;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 1;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 0;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			Env.Registry.CanUserEditOrganisationCode = true;
		}

		public void TestInactiveOrg_HasAdditionalFilterError()
		{
			var organisation = Organisations.AddNew();
			organisation.OH_IsActive = false;

			string errorMsg = Organisations.GetAllNotificationsWhenAdditionalFilterNotMet(organisation);
			AssertEquals("An Organization selected from here must be active.", errorMsg);
		}

		public void TestApplyActiveFilterOrNot()
		{
			var inActiveOrgs = new OrganisationsForTest(Factory)
			{
				ShouldApplyActiveFilter = true
			};

			AssertEquals("OH_IsActive = 1", inActiveOrgs.AdditionalFilter.FilterString);

			inActiveOrgs = new OrganisationsForTest(Factory)
			{
				ShouldApplyActiveFilter = false
			};

			AssertEquals(String.Empty, inActiveOrgs.AdditionalFilter.FilterString);
		}

		#region Implementation

		OrganisationsForTest Organisations;

		protected override void SetUp()
		{
			base.SetUp();
			Organisations = new OrganisationsForTest(Factory);
		}

		#endregion
	}
}
