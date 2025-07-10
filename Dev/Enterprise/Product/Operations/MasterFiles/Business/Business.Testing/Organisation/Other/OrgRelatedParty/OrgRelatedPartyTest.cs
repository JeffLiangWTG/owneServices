using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRelatedParty))]
	sealed class OrgRelatedPartyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsEnterpriseLevelOnly()
		{
			var partyRecord = Factory.New<OrgRelatedParty>();
			foreach (ICodeDescription item in new RelatedPartyTypeList())
			{
				partyRecord.PR_PartyType = item.Code;
				var isExpectedType = item.Code == RelatedPartyTypeList.Codes.ShipperBroker || item.Code == RelatedPartyTypeList.Codes.ExportConsolidationDepot || item.Code == RelatedPartyTypeList.Codes.ControllingAgent;
				AssertEquals(isExpectedType, partyRecord.IsEnterpriseLevelOnly);
			}
		}

		public void TestIsCompanySpecific()
		{
			var partyRecord = Factory.New<OrgRelatedParty>();
			foreach (ICodeDescription item in new RelatedPartyTypeList())
			{
				partyRecord.PR_PartyType = item.Code;
				var isExpectedType = item.Code == RelatedPartyTypeList.Codes.APSettlementGroup || item.Code == RelatedPartyTypeList.Codes.ARSettlementGroup || item.Code == RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
				AssertEquals(isExpectedType, partyRecord.IsCompanySpecific);
			}
		}

		public void TestPartyTypeDescriptionInItalian()
		{
			var staff = Factory.New<IGlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_LoginName = "alpha";
			staff.GS_FullName = "Alpha Albert Anaheim";
			staff.GS_WorkingLanguage = Core.Constants.Languages.Italian;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var englishDescription = RelatedPartyTypeList.Descriptions.APNettingGroup.GetUnresolvedString();
				var italianDescription = RelatedPartyTypeList.Descriptions.APNettingGroup.ToString();
				var code = RelatedPartyTypeList.Codes.APNettingGroup; // the code is a string and it won't be translated
				AssertEquals("Check the description is in italian", "Gr. calc. imp. netto FO", italianDescription);
				AssertNotEquals(italianDescription, englishDescription);

				var testParty = RelatedPartyRecord;

				testParty.PartyTypeDescription = englishDescription;
				AssertEquals(code, testParty.PR_PartyType);
				AssertEquals(italianDescription, testParty.PartyTypeDescription);

				testParty.PartyTypeDescription = italianDescription;
				AssertEquals(code, testParty.PR_PartyType);
				AssertEquals(italianDescription, testParty.PartyTypeDescription);
			}
		}

		public void TestCalculatedDirection_WhenPartyTypeChanges()
		{
			RelatedPartyRecord.PR_PartyType = ZString.Empty;
			RelatedPartyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			AssertEquals(ZString.Empty, RelatedPartyRecord.CalculatedDirection);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			AssertEquals(RelatedPartyDirectionList.Codes.Delivery, RelatedPartyRecord.CalculatedDirection);
		}

		public void TestDefaultPR_RN_NKImporterCountry()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AU!2#";

			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "NZ!2#";
			Factory.Save();

			var partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			AssertEquals(string.Empty, partyRecord.PR_RN_NKImporterCountry);

			partyRecord.PR_Location = unloco.RL_Code;
			AssertEquals("AU", partyRecord.PR_RN_NKImporterCountry);

			partyRecord.PR_Location = unloco2.RL_Code;
			AssertEquals("NZ", partyRecord.PR_RN_NKImporterCountry);
		}

		public void TestICSPartyType()
		{
			var relatedParty = RelatedPartyRecord;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			CombineAssertions("SelfFilerForICS2", delegate
			{
				AssertEquals("IsCompanySpecific", false, relatedParty.IsCompanySpecific);
				AssertEquals("IsEnterpriseLevelOnly", false, relatedParty.IsEnterpriseLevelOnly);
				AssertEquals("ShouldHaveMode", true, relatedParty.ShouldHaveMode);
				AssertEquals("ShouldHaveDirection", false, relatedParty.ShouldHaveDirection);
				AssertEquals("DirectionReadonly", true, relatedParty.DirectionReadonly);
				AssertEquals("ShouldCalculateDirection", false, relatedParty.ShouldCalculateDirection);
				AssertEquals("CanHaveLocation", true, relatedParty.CanHaveLocation);
				AssertEquals("CanHaveContainerMode", false, relatedParty.CanHaveContainerMode);
			});
		}

		#region TestReturnAgentPartyType

		public void TestReturnAgentPartyType()
		{
			var relatedParty = RelatedPartyRecord;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ReturnAgent;
			CombineAssertions("ReturnAgent", delegate
			{
				AssertEquals("IsCompanySpecific", false, relatedParty.IsCompanySpecific);
				AssertEquals("IsEnterpriseLevelOnly", false, relatedParty.IsEnterpriseLevelOnly);
				AssertEquals("ShouldHaveMode", false, relatedParty.ShouldHaveMode);
				AssertEquals("ShouldHaveDirection", false, relatedParty.ShouldHaveDirection);
				AssertEquals("DirectionReadonly", true, relatedParty.DirectionReadonly);
				AssertEquals("ShouldCalculateDirection", false, relatedParty.ShouldCalculateDirection);
				AssertEquals("CanHaveLocation", false, relatedParty.CanHaveLocation);
			});
		}

		#endregion

		#region TestETailRelatedPartyTypes

		public void TestETailRelatedPartyTypes()
		{
			var relatedParty = RelatedPartyRecord;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ShipperBroker;
			CombineAssertions("ShipperBroker", delegate
			{
				AssertEquals("IsCompanySpecific", false, relatedParty.IsCompanySpecific);
				AssertEquals("IsEnterpriseLevelOnly", true, relatedParty.IsEnterpriseLevelOnly);
				AssertEquals("ShouldHaveMode", false, relatedParty.ShouldHaveMode);
				AssertEquals("ShouldHaveDirection", false, relatedParty.ShouldHaveDirection);
				AssertEquals("DirectionReadonly", true, relatedParty.DirectionReadonly);
				AssertEquals("ShouldCalculateDirection", false, relatedParty.ShouldCalculateDirection);
				AssertEquals("CanHaveLocation", false, relatedParty.CanHaveLocation);
			});

			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ExportConsolidationDepot;
			CombineAssertions("ExportConsolidationDepot", delegate
			{
				AssertEquals("IsCompanySpecific", false, relatedParty.IsCompanySpecific);
				AssertEquals("IsEnterpriseLevelOnly", true, relatedParty.IsEnterpriseLevelOnly);
				AssertEquals("ShouldHaveMode", false, relatedParty.ShouldHaveMode);
				AssertEquals("ShouldHaveDirection", false, relatedParty.ShouldHaveDirection);
				AssertEquals("DirectionReadonly", true, relatedParty.DirectionReadonly);
				AssertEquals("ShouldCalculateDirection", false, relatedParty.ShouldCalculateDirection);
				AssertEquals("CanHaveLocation", false, relatedParty.CanHaveLocation);
			});
		}

		#endregion

		public void TestJPAFRRelatedPartyTypes()
		{
			var relatedParty = RelatedPartyRecord;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.JapanNotificationParty;
			CombineAssertions("ShipperBroker", delegate
			{
				AssertEquals("IsCompanySpecific", false, relatedParty.IsCompanySpecific);
				AssertEquals("IsEnterpriseLevelOnly", false, relatedParty.IsEnterpriseLevelOnly);
				AssertEquals("ShouldHaveMode", false, relatedParty.ShouldHaveMode);
				AssertEquals("ShouldHaveDirection", true, relatedParty.ShouldHaveDirection);
				AssertEquals("DirectionReadonly", false, relatedParty.DirectionReadonly);
				AssertEquals("ShouldCalculateDirection", false, relatedParty.ShouldCalculateDirection);
				AssertEquals("CanHaveLocation", false, relatedParty.CanHaveLocation);
			});
		}

		#region TestCSAApprovedVendorPartyTypes

		public void TestCSAApprovedVendorPartyTypes()
		{
			var relatedParty = RelatedPartyRecord;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedVendor;
			CombineAssertions("ShipperBroker", delegate
			{
				AssertEquals("IsCompanySpecific", false, relatedParty.IsCompanySpecific);
				AssertEquals("IsEnterpriseLevelOnly", false, relatedParty.IsEnterpriseLevelOnly);
				AssertEquals("ShouldHaveMode", false, relatedParty.ShouldHaveMode);
				AssertEquals("ShouldHaveDirection", false, relatedParty.ShouldHaveDirection);
				AssertEquals("DirectionReadonly", true, relatedParty.DirectionReadonly);
				AssertEquals("ShouldCalculateDirection", false, relatedParty.ShouldCalculateDirection);
				AssertEquals("CanHaveLocation", false, relatedParty.CanHaveLocation);
			});
		}

		#endregion

		#region TestCSAApprovedUltimateConsigneePartyTypes

		public void TestCSAApprovedUltimateConsigneePartyTypes()
		{
			var relatedParty = RelatedPartyRecord;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee;
			CombineAssertions("ShipperBroker", delegate
			{
				AssertEquals("IsCompanySpecific", false, relatedParty.IsCompanySpecific);
				AssertEquals("IsEnterpriseLevelOnly", false, relatedParty.IsEnterpriseLevelOnly);
				AssertEquals("ShouldHaveMode", false, relatedParty.ShouldHaveMode);
				AssertEquals("ShouldHaveDirection", false, relatedParty.ShouldHaveDirection);
				AssertEquals("DirectionReadonly", true, relatedParty.DirectionReadonly);
				AssertEquals("ShouldCalculateDirection", false, relatedParty.ShouldCalculateDirection);
				AssertEquals("CanHaveLocation", false, relatedParty.CanHaveLocation);
			});
		}

		#endregion

		#region TestACGPartyType

		public void TestACGPartyType()
		{
			var relatedParty = RelatedPartyRecord;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			CombineAssertions("ACG", delegate
			{
				AssertEquals("IsCompanySpecific", true, relatedParty.IsCompanySpecific);
				AssertEquals("IsEnterpriseLevelOnly", false, relatedParty.IsEnterpriseLevelOnly);
				AssertEquals("ShouldHaveMode", false, relatedParty.ShouldHaveMode);
				AssertEquals("ShouldHaveDirection", false, relatedParty.ShouldHaveDirection);
				AssertEquals("DirectionReadonly", true, relatedParty.DirectionReadonly);
				AssertEquals("ShouldCalculateDirection", false, relatedParty.ShouldCalculateDirection);
				AssertEquals("CanHaveLocation", false, relatedParty.CanHaveLocation);
			});
		}

		#endregion

		#region TestSPCPartyType

		public void TestSPCPartyType()
		{
			var relatedParty = RelatedPartyRecord;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;

			CombineAssertions("SPC", delegate
			{
				AssertEquals("IsCompanySpecific", false, relatedParty.IsCompanySpecific);
				AssertEquals("IsEnterpriseLevelOnly", false, relatedParty.IsEnterpriseLevelOnly);
				AssertEquals("ShouldHaveMode", true, relatedParty.ShouldHaveMode);
				AssertEquals("ShouldHaveDirection", true, relatedParty.ShouldHaveDirection);
				AssertEquals("DirectionReadonly", false, relatedParty.DirectionReadonly);
				AssertEquals("ShouldCalculateDirection", false, relatedParty.ShouldCalculateDirection);
				AssertEquals("CanHaveLocation", true, relatedParty.CanHaveLocation);
			});
		}

		#endregion

		#region TestMANPartyType

		public void TestMANPartyType()
		{
			var relatedParty = RelatedPartyRecord;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.Manufacturer;
			CombineAssertions("MAN", delegate
			{
				AssertEquals("IsCompanySpecific", false, relatedParty.IsCompanySpecific);
				AssertEquals("IsEnterpriseLevelOnly", false, relatedParty.IsEnterpriseLevelOnly);
				AssertEquals("ShouldHaveMode", false, relatedParty.ShouldHaveMode);
				AssertEquals("ShouldHaveDirection", false, relatedParty.ShouldHaveDirection);
				AssertEquals("DirectionReadonly", true, relatedParty.DirectionReadonly);
				AssertEquals("ShouldCalculateDirection", false, relatedParty.ShouldCalculateDirection);
				AssertEquals("CanHaveLocation", false, relatedParty.CanHaveLocation);
			});
		}

		#endregion

		#region IReadOnlySecurity

		public void TestReadOnlySecurity()
		{
			bool oldRelatedPartyValue = Env.Security.OrgDetailsModifyRelatedParties.IsAllowed;
			bool oldFinRelatedPartyValue = Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed;
			bool oldNonFinRelatedPartyValue = Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed;

			OrgHeader organisation = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");

			try
			{
				organisation.AllRelatedParties.SetRelatedParty(Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG"), RelatedPartyTypeList.Codes.APNettingGroup, "PIC");
				OrgRelatedParty testParty = organisation.AllRelatedParties[0];

				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testParty.CompanyLevelInfo.ReadOnly);

				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testParty.CompanyLevelInfo.ReadOnly);

				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = true;

				Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = false;
				Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = true;

				testParty.PartyTypeDescription = RelatedPartyTypeList.Descriptions.APNettingGroup;
				Factory.Save();

				Assert("Access NOT Allowed - ReadOnly", testParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testParty.CompanyLevelInfo.ReadOnly);

				testParty.PartyTypeDescription = RelatedPartyTypeList.Descriptions.AccountingVATGSTGroup;
				Factory.Save();

				Assert("Access NOT Allowed - ReadOnly", testParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testParty.CompanyLevelInfo.ReadOnly);

				testParty.PartyTypeDescription = RelatedPartyTypeList.Descriptions.LocalTransport;
				Factory.Save();

				Assert("Access Allowed - Not ReadOnly", !testParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testParty.CompanyLevelInfo.ReadOnly);

				testParty.PartyTypeDescription = RelatedPartyTypeList.Descriptions.InvoiceWarehouseJobsTo;
				Factory.Save();

				Assert("Access NOT Allowed - ReadOnly", testParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testParty.CompanyLevelInfo.ReadOnly);

				Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = true;
				Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = false;

				testParty.PartyTypeDescription = RelatedPartyTypeList.Descriptions.LocalTransport;
				Factory.Save();

				Assert("Access NOT Allowed - ReadOnly", testParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testParty.CompanyLevelInfo.ReadOnly);

				testParty.PartyTypeDescription = RelatedPartyTypeList.Descriptions.APNettingGroup;
				Factory.Save();

				Assert("Access Allowed - Not ReadOnly", !testParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testParty.CompanyLevelInfo.ReadOnly);

				testParty.PartyTypeDescription = RelatedPartyTypeList.Descriptions.AccountingVATGSTGroup;
				Factory.Save();

				Assert("Access Allowed - Not ReadOnly", !testParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testParty.CompanyLevelInfo.ReadOnly);

				testParty.PartyTypeDescription = RelatedPartyTypeList.Descriptions.InvoiceWarehouseJobsTo;
				Factory.Save();

				Assert("Access Allowed - Not ReadOnly", !testParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testParty.CompanyLevelInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = oldRelatedPartyValue;
				Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = oldFinRelatedPartyValue;
				Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = oldNonFinRelatedPartyValue;
			}
		}

		public void TestReadOnlySecurity_ControllingAgent_ToAnyOrg_NewOrg()
		{
			AssertReadOnlySecurity_ControllingAgent_ToAnyOrg(false, Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg);
		}

		public void TestReadOnlySecurity_ControllingAgent_ToAnyOrg_ExistingOrg()
		{
			AssertReadOnlySecurity_ControllingAgent_ToAnyOrg(true, Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg);
		}

		void AssertReadOnlySecurity_ControllingAgent_ToAnyOrg(bool saveOrgs, ISecurityCheckpoint securityForAnyOrg)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();

			if (saveOrgs)
			{
				Factory.Save();
			}

			org.AllRelatedParties.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.ControllingAgent, "PIC");
			OrgRelatedParty relatedParty = org.AllRelatedParties[0];

			//Test new relatedParty
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityForAnyOrg.IsAllowed = false;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			securityForAnyOrg.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityForAnyOrg.IsAllowed = false;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			securityForAnyOrg.IsAllowed = false;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			//Test existing relatedParty
			if (saveOrgs)
			{
				Factory.Save();

				//Test exisiting Controlling Agent Related Party
				OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				securityForAnyOrg.IsAllowed = false;
				Assert("Access Not Allowed - ReadOnly", relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Not Allowed - ReadOnly", relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Not Allowed - ReadOnly", relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				securityForAnyOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				//Test exisiting non-Controlling Agent Related Party
				org.AllRelatedParties.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.APNettingGroup, "PIC");
				relatedParty = org.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.APNettingGroup, "PIC");
				Factory.Save();

				OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				securityForAnyOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				securityForAnyOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);
			}
		}

		public void TestReadOnlySecurity_ControllingAgent_ToOrgProxy_NewOrg()
		{
			AssertReadOnlySecurity_ControllingAgent_ToOrgProxy(false, Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg, Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg, false);
		}

		public void TestReadOnlySecurity_ControllingAgent_ToOrgProxy_BranchProxy_NewOrg()
		{
			AssertReadOnlySecurity_ControllingAgent_ToOrgProxy(false, Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg, Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg, true);
		}

		public void TestReadOnlySecurity_ControllingAgent_ToOrgProxy_ExistingOrg()
		{
			AssertReadOnlySecurity_ControllingAgent_ToOrgProxy(true, Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg, Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg, false);
		}

		public void TestReadOnlySecurity_ControllingAgent_ToOrgProxy_BranchProxy_ExistingOrg()
		{
			AssertReadOnlySecurity_ControllingAgent_ToOrgProxy(true, Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg, Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg, true);
		}

		void AssertReadOnlySecurity_ControllingAgent_ToOrgProxy(bool saveOrgs, ISecurityCheckpoint securityForAnyOrg, ISecurityCheckpoint securityForOwnOrg, bool testBranchProxy)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var branch = GlbCompany.CurrentCompany.Branches.AddNew();
			branch.GB_OH_OrgProxy = branchOrgProxy.PK;

			var companyOrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

			var orgProxyForTest = testBranchProxy ? branchOrgProxy : companyOrgProxy;

			if (saveOrgs)
			{
				Factory.Save();
			}

			org.AllRelatedParties.SetRelatedParty(orgProxyForTest, RelatedPartyTypeList.Codes.ControllingAgent, "PIC");
			OrgRelatedParty relatedParty = org.AllRelatedParties[0];

			//Test new relatedParty
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityForAnyOrg.IsAllowed = false;
			securityForOwnOrg.IsAllowed = false;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			securityForAnyOrg.IsAllowed = false;
			securityForOwnOrg.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			securityForAnyOrg.IsAllowed = true;
			securityForOwnOrg.IsAllowed = false;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			securityForAnyOrg.IsAllowed = true;
			securityForOwnOrg.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityForAnyOrg.IsAllowed = false;
			securityForOwnOrg.IsAllowed = false;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			securityForAnyOrg.IsAllowed = false;
			securityForOwnOrg.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			securityForAnyOrg.IsAllowed = true;
			securityForOwnOrg.IsAllowed = false;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			securityForAnyOrg.IsAllowed = true;
			securityForOwnOrg.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

			//Test existing relatedParty
			if (saveOrgs)
			{
				Factory.Save();

				//Test exisiting Controlling Agent Related Party
				OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = false;
				Assert("Access Not Allowed - ReadOnly", relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Not Allowed - ReadOnly", relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Not Allowed - ReadOnly", relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = true;
				securityForOwnOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = true;
				securityForOwnOrg.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = true;
				securityForOwnOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = true;
				securityForOwnOrg.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				//Test exisiting non-Controlling Agent Related Party
				org.AllRelatedParties.SetRelatedParty(companyOrgProxy, RelatedPartyTypeList.Codes.APNettingGroup, "PIC");
				relatedParty = org.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.APNettingGroup, "PIC");
				Factory.Save();

				OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = true;
				securityForOwnOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = true;
				securityForOwnOrg.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = true;
				securityForOwnOrg.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);

				securityForAnyOrg.IsAllowed = true;
				securityForOwnOrg.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PartyTypeDescriptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.PR_OH_RelatedPartyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !relatedParty.CompanyLevelInfo.ReadOnly);
			}
		}

		#endregion

		#region TestCanBeDeleted_CanBeAdded

		public void TestCanBeDeleted_CanBeAdded()
		{
			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = false;
			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = true;

			RelatedPartyRecord.PartyTypeDescription = RelatedPartyTypeList.Descriptions.APNettingGroup;
			Assert(!RelatedPartyRecord.IsAlrightToAddOrDelete);
			RelatedPartyRecord.PartyTypeDescription = RelatedPartyTypeList.Descriptions.LocalTransport;
			Assert(RelatedPartyRecord.IsAlrightToAddOrDelete);

			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = true;
			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = false;

			Assert(!RelatedPartyRecord.IsAlrightToAddOrDelete);
			RelatedPartyRecord.PartyTypeDescription = RelatedPartyTypeList.Descriptions.ARNettingGroup;
			Assert(RelatedPartyRecord.IsAlrightToAddOrDelete);
		}

		public void TestCanBeDeleted_CanBeAdded_ControllingAgent_ToAnyOrg_NewOrg()
		{
			AssertCanBeDeleted_CanBeAdded_ControllingAgent(false, Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg, Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg, false, false);
		}

		public void TestCanBeDeleted_CanBeAdded_ControllingAgent_ToAnyOrg_ExistingOrg()
		{
			AssertCanBeDeleted_CanBeAdded_ControllingAgent(true, Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg, Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg, false, false);
		}

		public void TestCanBeDeleted_CanBeAdded_ControllingAgent_ToOrgProxy_NewOrg()
		{
			AssertCanBeDeleted_CanBeAdded_ControllingAgent(false, Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg, Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg, true, false);
		}

		public void TestCanBeDeleted_CanBeAdded_ControllingAgent_ToOrgProxy_BranchProxy_NewOrg()
		{
			AssertCanBeDeleted_CanBeAdded_ControllingAgent(false, Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg, Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg, true, true);
		}

		public void TestCanBeDeleted_CanBeAdded_ControllingAgent_ToOrgProxy_ExistingOrg()
		{
			AssertCanBeDeleted_CanBeAdded_ControllingAgent(true, Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg, Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg, true, false);
		}

		public void TestCanBeDeleted_CanBeAdded_ControllingAgent_ToOrgProxy_BranchProxy_ExistingOrg()
		{
			AssertCanBeDeleted_CanBeAdded_ControllingAgent(true, Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg, Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg, true, true);
		}

		void AssertCanBeDeleted_CanBeAdded_ControllingAgent(bool saveOrgs, ISecurityCheckpoint securityForAnyOrg, ISecurityCheckpoint securityForOwnOrg, bool toOwnOrg, bool testBranchProxy)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var companyOrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var branch = GlbCompany.CurrentCompany.Branches.AddNew();
			branch.GB_OH_OrgProxy = branchOrgProxy.PK;

			var relatedOrg = toOwnOrg ? (testBranchProxy ? branchOrgProxy : companyOrgProxy) : anotherOrg;

			if (saveOrgs)
			{
				Factory.Save();
			}

			org.AllRelatedParties.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.ControllingAgent, "PIC");
			OrgRelatedParty relatedParty = org.AllRelatedParties[0];

			//New related party
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityForAnyOrg.IsAllowed = false;
			securityForOwnOrg.IsAllowed = false;
			Assert(!relatedParty.IsAlrightToAddOrDelete);

			securityForAnyOrg.IsAllowed = false;
			securityForOwnOrg.IsAllowed = true;
			Assert(relatedParty.IsAlrightToAddOrDelete);

			securityForAnyOrg.IsAllowed = true;
			securityForOwnOrg.IsAllowed = false;
			Assert(relatedParty.IsAlrightToAddOrDelete);

			securityForAnyOrg.IsAllowed = true;
			securityForOwnOrg.IsAllowed = true;
			Assert(relatedParty.IsAlrightToAddOrDelete);

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityForAnyOrg.IsAllowed = false;
			securityForOwnOrg.IsAllowed = false;
			Assert(relatedParty.IsAlrightToAddOrDelete);

			securityForAnyOrg.IsAllowed = false;
			securityForOwnOrg.IsAllowed = true;
			Assert(relatedParty.IsAlrightToAddOrDelete);

			securityForAnyOrg.IsAllowed = true;
			securityForOwnOrg.IsAllowed = false;
			Assert(relatedParty.IsAlrightToAddOrDelete);

			securityForAnyOrg.IsAllowed = true;
			securityForOwnOrg.IsAllowed = true;
			Assert(relatedParty.IsAlrightToAddOrDelete);

			if (saveOrgs)
			{
				Factory.Save();

				//Existing related party
				OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = false;
				Assert(!relatedParty.IsAlrightToAddOrDelete);

				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = true;
				Assert(relatedParty.IsAlrightToAddOrDelete);

				securityForAnyOrg.IsAllowed = true;
				securityForOwnOrg.IsAllowed = false;
				Assert(relatedParty.IsAlrightToAddOrDelete);

				securityForAnyOrg.IsAllowed = true;
				Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
				Assert(relatedParty.IsAlrightToAddOrDelete);

				OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = false;
				Assert(relatedParty.IsAlrightToAddOrDelete);

				securityForAnyOrg.IsAllowed = false;
				securityForOwnOrg.IsAllowed = true;
				Assert(relatedParty.IsAlrightToAddOrDelete);

				securityForAnyOrg.IsAllowed = true;
				securityForOwnOrg.IsAllowed = false;
				Assert(relatedParty.IsAlrightToAddOrDelete);

				securityForAnyOrg.IsAllowed = true;
				securityForOwnOrg.IsAllowed = true;
				Assert(relatedParty.IsAlrightToAddOrDelete);
			}
		}

		#endregion

		#region TestSettingPartyTypeSetsModeReadOnlyState

		public void TestSettingPartyTypeSetsModeReadOnlyState()
		{
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			AssertEquals("Should not be readonly", false, relatedPartyRecord.PR_FreightTransportModeInfo.ReadOnly);
			AssertEquals("Should not be readonly", false, relatedPartyRecord.PR_FreightContainerModeInfo.ReadOnly);

			RelatedPartyRecord.PR_FreightTransportMode = Core.Constants.TransportModes.Air;
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			AssertEquals("Should be readonly", true, relatedPartyRecord.PR_FreightTransportModeInfo.ReadOnly);
			AssertEquals("Should be readonly", true, relatedPartyRecord.PR_FreightContainerModeInfo.ReadOnly);
			AssertEquals("Should be empty", ZString.Empty, relatedPartyRecord.PR_FreightTransportMode);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			AssertEquals("Should not be readonly", false, relatedPartyRecord.PR_FreightTransportModeInfo.ReadOnly);
			AssertEquals("Should not be readonly", false, relatedPartyRecord.PR_FreightContainerModeInfo.ReadOnly);
		}

		#endregion

		#region TestContainerMode_ReadOnly

		public void TestContainerMode_ReadOnly()
		{
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			AssertEquals("Should not be readonly", false, relatedPartyRecord.PR_FreightContainerModeInfo.ReadOnly);

			RelatedPartyRecord.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			RelatedPartyRecord.PR_FreightContainerMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("Should not be readonly", false, relatedPartyRecord.PR_FreightContainerModeInfo.ReadOnly);
			AssertEquals("Should be LSE", "LSE", relatedPartyRecord.PR_FreightContainerMode);

			RelatedPartyRecord.PR_FreightTransportMode = Core.Constants.TransportModes.All;
			AssertEquals("Should be readonly", true, relatedPartyRecord.PR_FreightContainerModeInfo.ReadOnly);
			AssertEquals("Should be empty", ZString.Empty, relatedPartyRecord.PR_FreightContainerMode);
		}

		#endregion

		#region TestSettingRelatedPartyCreatesOtherRelatedPartiesForParent

		public void TestSettingRelatedPartyCreatesOtherRelatedPartiesForParent()
		{
			OrgHeader oldRelatedOrg = Factory.New<OrgHeader>();
			OrgHeader relatedOrg = Factory.New<OrgHeader>();
			Organisation.SetRelatedParty(oldRelatedOrg, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery);
			Organisation.SetRelatedParty(oldRelatedOrg, RelatedPartyTypeList.Codes.ReportRevenueTo, RelatedPartyDirectionList.Codes.Delivery);

			Organisation.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery);
			AssertEquals(relatedOrg.PK, Organisation.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery).PK);
			AssertEquals(relatedOrg.PK, Organisation.GetRelatedParty(RelatedPartyTypeList.Codes.ReportRevenueTo, RelatedPartyDirectionList.Codes.Delivery).PK);
		}

		#endregion

		#region TestSettingPartyTypeSetsForAddressReadOnlyState

		public void TestSettingPartyTypeSetsForAddressReadOnlyState()
		{
			foreach (ICodeDescription item in new RelatedPartyTypeList())
			{
				RelatedPartyRecord.PR_PartyType = item.Code;
				switch (item.Code)
				{
					case RelatedPartyTypeList.Codes.NationalDistributionCentre:
					case RelatedPartyTypeList.Codes.LocalTransport:
					case RelatedPartyTypeList.Codes.Warehouse:
					case RelatedPartyTypeList.Codes.PickupFrom:
					case RelatedPartyTypeList.Codes.DeliveryTo:
					case RelatedPartyTypeList.Codes.NotifyParty:
					case RelatedPartyTypeList.Codes.AuthorizedCargoReporter:
					case RelatedPartyTypeList.Codes.ProductRelationship:
						AssertEquals(string.Format("{0} - {1} should be NOT readonly.", item.Code, item.Description), false, relatedPartyRecord.PR_OAInfo.ReadOnly);
						break;
					default:
						AssertEquals(string.Format("{0} - {1} should be readonly", item.Code, item.Description), true, relatedPartyRecord.PR_OAInfo.ReadOnly);
						break;
				}
			}
		}

		#endregion

		public void TestLogging()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			parent.OH_Code = "PAR";
			parent.OH_FullName = "Parent Org";

			var related = Factory.NewWithValidTestData<OrgHeader>();
			related.OH_Code = "REL";
			related.OH_FullName = "Related Org";

			var link1 = Factory.New<OrgRelatedParty>();
			link1.PR_OH_Parent = parent.PK;
			link1.PR_OH_RelatedParty = related.PK;

			link1.Delete();
			Factory.Save();
			AssertEquals("Attached log is not added to related because it is deleted before save", false, related.Logs.Find(log => log.SL_Reference == "Attached - (PAR) Parent Org").Any());
			AssertEquals("Attached log is not added to parent because it is deleted before save", false, parent.Logs.Find(log => log.SL_Reference == "Attached - (REL) Related Org").Any());
			AssertEquals("Detached log is not added to related because it is deleted before save", false, related.Logs.Find(log => log.SL_Reference == "Detached - (PAR) Parent Org").Any());
			AssertEquals("Detached log is not added to parent because it is deleted before save", false, parent.Logs.Find(log => log.SL_Reference == "Detached - (REL) Related Org").Any());

			var link2 = Factory.New<OrgRelatedParty>();
			link2.PR_OH_Parent = parent.PK;
			link2.PR_OH_RelatedParty = related.PK;

			Factory.Save();
			AssertEquals("Attached log is added to parent", true, related.Logs.Find(log => log.SL_Reference == "Attached - (PAR) Parent Org").Any());
			AssertEquals("Attached log is added to related", true, parent.Logs.Find(log => log.SL_Reference == "Attached - (REL) Related Org").Any());

			link2.Delete();
			Factory.Save();
			AssertEquals("Detached log is added to parent", true, related.Logs.Find(log => log.SL_Reference == "Detached - (PAR) Parent Org").Any());
			AssertEquals("Detached log is added to related", true, parent.Logs.Find(log => log.SL_Reference == "Detached - (REL) Related Org").Any());

			var link3 = Factory.New<OrgRelatedParty>();
			link3.PR_OH_Parent = parent.PK;
			link3.PR_OH_RelatedParty = related.PK;

			Factory.Save();
			AssertEquals("Attached log count", 2, related.Logs.Find(log => log.SL_Reference == "Attached - (PAR) Parent Org").Count());
			AssertEquals("Attached log count", 2, parent.Logs.Find(log => log.SL_Reference == "Attached - (REL) Related Org").Count());
			AssertEquals("Detached log count", 1, related.Logs.Find(log => log.SL_Reference == "Detached - (PAR) Parent Org").Count());
			AssertEquals("Detached log count", 1, parent.Logs.Find(log => log.SL_Reference == "Detached - (REL) Related Org").Count());

			link3.Delete();

			var link4 = Factory.New<OrgRelatedParty>();
			link4.PR_OH_Parent = parent.PK;
			link4.PR_OH_RelatedParty = related.PK;
			link4.Delete();
			var link5 = Factory.New<OrgRelatedParty>();
			link5.PR_OH_Parent = parent.PK;
			link5.PR_OH_RelatedParty = related.PK;

			Factory.Save();
			AssertEquals("Attached log count should be unchanged", 2, related.Logs.Find(log => log.SL_Reference == "Attached - (PAR) Parent Org").Count());
			AssertEquals("Attached log count should be unchanged", 2, parent.Logs.Find(log => log.SL_Reference == "Attached - (REL) Related Org").Count());
			AssertEquals("Detached log count should be unchanged", 1, related.Logs.Find(log => log.SL_Reference == "Detached - (PAR) Parent Org").Count());
			AssertEquals("Detached log count should be unchanged", 1, parent.Logs.Find(log => log.SL_Reference == "Detached - (REL) Related Org").Count());

			link4.Delete();
			link5.Delete();

			var parent2 = Factory.NewWithValidTestData<OrgHeader>();
			parent2.OH_Code = "PA2";
			parent2.OH_FullName = "Parent Org 2";

			var related2 = Factory.NewWithValidTestData<OrgHeader>();
			related2.OH_Code = "RE2";
			related2.OH_FullName = "Related Org 2";

			Factory.Save();

			parent2.OH_Language = "EN-US";
			var link6 = Factory.New<OrgRelatedParty>();
			link6.PR_OH_Parent = parent2.PK;
			link6.PR_OH_RelatedParty = related2.PK;

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedParent2 = anotherFactory.Load<OrgHeader>(parent2.PK);
			loadedParent2.OH_Language = "EN-UK";
			anotherFactory.Save();

			try
			{
				Factory.Save();
			}
			catch { }

			AssertEquals("Save should fail because of concurrency error and link is not in database", false, link6.IsInDatabase);
			AssertEquals("Attached log is not added to related if save fails", false, related2.Logs.Find(log => log.SL_Reference == "Attached - (PA2) Parent Org 2").Any());
			AssertEquals("Attached log is not added to parent if save fails", false, parent2.Logs.Find(log => log.SL_Reference == "Attached - (RE2) Related Org 2").Any());
			AssertEquals("Detached log is not added to related if save fails", false, related2.Logs.Find(log => log.SL_Reference == "Detached - (PA2) Parent Org 2").Any());
			AssertEquals("Detached log is not added to parent if save fails", false, parent2.Logs.Find(log => log.SL_Reference == "Detached - (RE2) Related Org 2").Any());

			parent2.Reload();
			Factory.Save();
			AssertEquals("Attached log is added to group", true, related2.Logs.Find(log => log.SL_Reference == "Attached - (PA2) Parent Org 2").Any());
			AssertEquals("Attached log is added to staff", true, parent2.Logs.Find(log => log.SL_Reference == "Attached - (RE2) Related Org 2").Any());
			AssertEquals("Detached log should not be added", false, related2.Logs.Find(log => log.SL_Reference == "Detached - (PA2) Parent Org 2").Any());
			AssertEquals("Detached log should not be added", false, parent2.Logs.Find(log => log.SL_Reference == "Detached - (RE2) Related Org 2").Any());
		}

		[TestDate(2015, 1, 1)]
		public void TestEditTimeAndUser()
		{
			var editingStaff = Factory.NewWithValidTestData<GlbStaff>();
			editingStaff.GS_Code = "ES1";

			var parent = Factory.NewWithValidTestData<OrgHeader>();
			parent.OH_Code = "PAR";
			parent.OH_FullName = "Parent Org";

			var related = Factory.NewWithValidTestData<OrgHeader>();
			related.OH_Code = "REL";
			related.OH_FullName = "Related Org";

			Factory.Save();
			TestDateAttribute.AddYears(1);
			AssertNotEquals("Precondition", parent.OH_SystemLastEditTimeUtc, ZDateTime.Now);

			OrgRelatedParty link1;
			OrgRelatedParty link2;

			using (Env.SetTemporaryUserContext(new UserContext(editingStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				link1 = Factory.New<OrgRelatedParty>();
				link1.PR_OH_Parent = parent.PK;
				link1.PR_OH_RelatedParty = related.PK;

				link1.Delete();
				Factory.Save();

				AssertNotEquals("Related Org should not have been edited as link was deleted before save", related.OH_SystemLastEditTimeUtc, ZDateTime.Now);
				AssertNotEquals("Related Org should not have been edited as link was deleted before save", related.OH_SystemLastEditUser, editingStaff.GS_Code);
				AssertNotEquals("Parent Org should not have been edited as link was deleted before save", parent.OH_SystemLastEditTimeUtc, ZDateTime.Now);
				AssertNotEquals("Parent Org should not have been edited as link was deleted before save", parent.OH_SystemLastEditUser, editingStaff.GS_Code);

				link2 = Factory.New<OrgRelatedParty>();
				link2.PR_OH_Parent = parent.PK;
				link2.PR_OH_RelatedParty = related.PK;

				Factory.Save();

				AssertEquals("Related Org should have been edited as link was saved successfully", related.OH_SystemLastEditTimeUtc, ZDateTime.Now);
				AssertEquals("Related Org should have been edited as link was saved successfully", related.OH_SystemLastEditUser, editingStaff.GS_Code);
				AssertEquals("Parent Org should have been edited as link was saved successfully", parent.OH_SystemLastEditTimeUtc, ZDateTime.Now);
				AssertEquals("Parent Org should have been edited as link was saved successfully", parent.OH_SystemLastEditUser, editingStaff.GS_Code);
			}

			editingStaff = Factory.NewWithValidTestData<GlbStaff>();
			editingStaff.GS_Code = "ES2";
			Factory.Save();
			TestDateAttribute.AddYears(1);

			using (Env.SetTemporaryUserContext(new UserContext(editingStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				link2.Delete();
				Factory.Save();

				AssertEquals("Related Org should have been edited as link was deleted successfully", related.OH_SystemLastEditTimeUtc, ZDateTime.Now);
				AssertEquals("Related Org should have been edited as link was deleted successfully", related.OH_SystemLastEditUser, editingStaff.GS_Code);
				AssertEquals("Parent Org should have been edited as link was deleted successfully", parent.OH_SystemLastEditTimeUtc, ZDateTime.Now);
				AssertEquals("Parent Org should have been edited as link was deleted successfully", parent.OH_SystemLastEditUser, editingStaff.GS_Code);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestCreateTimeIsMaintained()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			parent.OH_Code = "PAR";
			parent.OH_FullName = "Parent Org";

			var related = Factory.NewWithValidTestData<OrgHeader>();
			related.OH_Code = "REL";
			related.OH_FullName = "Related Org";

			var partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_OH_Parent = parent.PK;
			partyRecord.PR_OH_RelatedParty = related.PK;

			Factory.Save();
			var intialTime = ZDateTime.UtcNow;
			AssertEquals("Precondition: Create time should be set by saving the record", intialTime, partyRecord.PR_SystemCreateTimeUtc);

			TestDateAttribute.AddYears(1);
			AssertNotEquals("Precondition: Current time should no longer be the same as initial time", ZDateTime.UtcNow, partyRecord.PR_SystemCreateTimeUtc);

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.Warehouse;
			Factory.Save();
			AssertEquals("Create time should not be changed by editing and saving the record", intialTime, partyRecord.PR_SystemCreateTimeUtc);
		}

		#region Properties

		#region TestSettingPR_GCInvalidatesLightValidation

		public void TestSettingPR_GCInvalidatesLightValidation()
		{
			RelatedPartyRecord.RunPreSaveValidation();
			AssertEquals("Precondition", true, RelatedPartyRecord.LightValidationIsValid);

			RelatedPartyRecord.PR_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals(false, RelatedPartyRecord.LightValidationIsValid);
		}

		#endregion

		#region CanHaveImporterCountry

		public void TestCanHaveImportCountry()
		{
			foreach (ICodeDescription item in new RelatedPartyTypeList())
			{
				RelatedPartyRecord.PR_PartyType = item.Code;
				switch (item.Code)
				{
					case RelatedPartyTypeList.Codes.ForwarderCoLoadWith:
					case RelatedPartyTypeList.Codes.SelfFilerForICS2:
						AssertEquals(string.Format("{0} - {1} can have Importer Country", item.Code, item.Description), true, RelatedPartyRecord.CanHaveImporterCountry);
						break;
					default:
						AssertEquals(string.Format("{0} - {1} can't have Importer Country", item.Code, item.Description), false, RelatedPartyRecord.CanHaveImporterCountry);
						break;
				}
			}
		}

		#endregion

		#region TestCanHaveContainerMode

		public void TestCanHaveContainerMode()
		{
			foreach (ICodeDescription item in new RelatedPartyTypeList())
			{
				RelatedPartyRecord.PR_PartyType = item.Code;
				switch (item.Code)
				{
					case RelatedPartyTypeList.Codes.SelfFilerForICS2:
						AssertEquals(string.Format("{0} - {1} can't have Container Mode", item.Code, item.Description), false, RelatedPartyRecord.CanHaveContainerMode);
						break;
					default:
						AssertEquals(string.Format("{0} - {1} can have Container Mode", item.Code, item.Description), true, RelatedPartyRecord.CanHaveContainerMode);
						break;
				}
			}
		}

		#endregion

		#region TestShouldHaveMode

		public void TestShouldHaveMode()
		{
			foreach (ICodeDescription item in new RelatedPartyTypeList())
			{
				RelatedPartyRecord.PR_PartyType = item.Code;
				switch (item.Code)
				{
					case RelatedPartyTypeList.Codes.CustomsAgentBroker:
					case RelatedPartyTypeList.Codes.ForwarderCFS:
					case RelatedPartyTypeList.Codes.LocalTransport:
					case RelatedPartyTypeList.Codes.LocalTransportBillTo:
					case RelatedPartyTypeList.Codes.ClientCFS:
					case RelatedPartyTypeList.Codes.ForwarderLocalTransport:
					case RelatedPartyTypeList.Codes.SendingAgent:
					case RelatedPartyTypeList.Codes.ReceivingAgent:
					case RelatedPartyTypeList.Codes.DeliveryAgent:
					case RelatedPartyTypeList.Codes.DeliveryTo:
					case RelatedPartyTypeList.Codes.PickupAgent:
					case RelatedPartyTypeList.Codes.PickupFrom:
					case RelatedPartyTypeList.Codes.ServiceProviderCreditor:
					case RelatedPartyTypeList.Codes.ForwarderCoLoadWith:
					case RelatedPartyTypeList.Codes.NotifyParty:
					case RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo:
					case RelatedPartyTypeList.Codes.InvoiceFreightJobsTo:
					case RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo:
					case RelatedPartyTypeList.Codes.AuthorizedCargoReporter:
					case RelatedPartyTypeList.Codes.SelfFilerForICS2:
						AssertEquals(string.Format("{0} - {1} should have a mode", item.Code, item.Description), true, RelatedPartyRecord.ShouldHaveMode);
						break;
					default:
						AssertEquals(string.Format("{0} - {1} should NOT have a mode", item.Code, item.Description), false, RelatedPartyRecord.ShouldHaveMode);
						break;
				}
			}
		}

		#endregion

		#region TestShouldCalculateDirection

		public void TestShouldCalculateDirection()
		{
			foreach (ICodeDescription item in new RelatedPartyTypeList())
			{
				RelatedPartyRecord.PR_PartyType = item.Code;
				switch (item.Code)
				{
					case RelatedPartyTypeList.Codes.CustomsAgentBroker:
					case RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo:
					case RelatedPartyTypeList.Codes.LocalTransport:
					case RelatedPartyTypeList.Codes.LocalTransportBillTo:
					case RelatedPartyTypeList.Codes.InvoiceFreightJobsTo:
					case RelatedPartyTypeList.Codes.ReportRevenueTo:
					case RelatedPartyTypeList.Codes.ReceivingAgent:
					case RelatedPartyTypeList.Codes.SendingAgent:
					case RelatedPartyTypeList.Codes.ControllingCustomer:
					case RelatedPartyTypeList.Codes.ClientCFS:
					case RelatedPartyTypeList.Codes.ForwarderCoLoadWith:
					case RelatedPartyTypeList.Codes.ExportConsolidationDepot:
					case RelatedPartyTypeList.Codes.ShipperBroker:
					case RelatedPartyTypeList.Codes.Warehouse:
					case RelatedPartyTypeList.Codes.NationalDistributionCentre:
					case RelatedPartyTypeList.Codes.ReturnAgent:
					case RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee:
					case RelatedPartyTypeList.Codes.CSAApprovedVendor:
					case RelatedPartyTypeList.Codes.AccountingVATGSTGroup:
					case RelatedPartyTypeList.Codes.JapanNotificationParty:
					case RelatedPartyTypeList.Codes.Manufacturer:
					case RelatedPartyTypeList.Codes.ContainerYard:
					case RelatedPartyTypeList.Codes.LocalForwarder:
					case RelatedPartyTypeList.Codes.ServiceProviderCreditor:
					case RelatedPartyTypeList.Codes.NotifyParty:
					case RelatedPartyTypeList.Codes.AuthorizedCargoReporter:
					case RelatedPartyTypeList.Codes.ProductRelationship:
					case RelatedPartyTypeList.Codes.SelfFilerForICS2:
						AssertEquals(string.Format("{0} - {1} should NOT calculate Direction", item.Code, item.Description), false, RelatedPartyRecord.ShouldCalculateDirection);
						break;
					default:
						AssertEquals(string.Format("{0} - {1} should calculate Direction", item.Code, item.Description), true, RelatedPartyRecord.ShouldCalculateDirection);
						break;
				}
			}
		}

		#endregion

		#region TestCanHaveLocation

		public void TestCanHaveLocation()
		{
			foreach (ICodeDescription item in new RelatedPartyTypeList())
			{
				RelatedPartyRecord.PR_PartyType = item.Code;
				switch (item.Code)
				{
					case RelatedPartyTypeList.Codes.ClientCFS:
					case RelatedPartyTypeList.Codes.CustomsAgentBroker:
					case RelatedPartyTypeList.Codes.ForwarderCFS:
					case RelatedPartyTypeList.Codes.ForwarderCoLoadWith:
					case RelatedPartyTypeList.Codes.LocalTransport:
					case RelatedPartyTypeList.Codes.ReceivingAgent:
					case RelatedPartyTypeList.Codes.SendingAgent:
					case RelatedPartyTypeList.Codes.DeliveryAgent:
					case RelatedPartyTypeList.Codes.DeliveryTo:
					case RelatedPartyTypeList.Codes.PickupAgent:
					case RelatedPartyTypeList.Codes.PickupFrom:
					case RelatedPartyTypeList.Codes.ServiceProviderCreditor:
					case RelatedPartyTypeList.Codes.SelfFilerForICS2:
						AssertEquals(string.Format("{0} - {1} can have location", item.Code, item.Description), true, RelatedPartyRecord.CanHaveLocation);
						break;
					default:
						AssertEquals(string.Format("{0} - {1} can't have location", item.Code, item.Description), false, RelatedPartyRecord.CanHaveLocation);
						break;
				}
			}
		}

		#endregion

		#region TestPR_PartyType_ClearsAddressIfPartyTypeDoesNotSupportAddresses

		public void TestPR_PartyType_ClearsAddressIfPartyTypeDoesNotSupportAddresses()
		{
			var relatedPartyRecord = Organisation.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			AssertEquals(Organisation.MainAddress.PK, relatedPartyRecord.PR_OA);

			relatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertEquals(ZGuid.Empty, relatedPartyRecord.PR_OA);
		}

		#endregion

		#region TestPR_OH_RelatedParty_DefaultsTheAddressToRelatedPartyMainAddressIfPartyTypeIsWarehouse

		public void TestPR_OH_RelatedParty_DefaultsTheAddressToRelatedPartyMainAddressIfPartyTypeIsWarehouse()
		{
			var relatedPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyRecord = Organisation.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord.PR_OH_RelatedParty = relatedPartyOrg.PK;
			AssertEquals(ZGuid.Empty, relatedPartyRecord.PR_OA);

			relatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.Warehouse;
			relatedPartyRecord.PR_OH_RelatedParty = relatedPartyOrg.PK;
			AssertEquals(relatedPartyOrg.MainAddress.PK, relatedPartyRecord.PR_OA);

			var newAddress = relatedPartyOrg.Addresses.AddNew();
			relatedPartyRecord.PR_OA = newAddress.PK;
			relatedPartyRecord.PR_OH_RelatedParty = relatedPartyOrg.PK;
			AssertEquals(newAddress.PK, relatedPartyRecord.PR_OA);
		}

		#endregion

		#region TestPR_OH_RelatedParty_DefaultsTheAddressToRelatedPartyOfficeAddressIfPartyTypeIsNotifyParty

		public void TestPR_OH_RelatedParty_DefaultsTheAddressToRelatedPartyOfficeAddressIfPartyTypeIsNotifyParty()
		{
			var relatedPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyRecord = Organisation.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord.PR_OH_RelatedParty = relatedPartyOrg.PK;
			AssertEquals(ZGuid.Empty, relatedPartyRecord.PR_OA);

			relatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.NotifyParty;
			relatedPartyRecord.PR_OH_RelatedParty = relatedPartyOrg.PK;
			AssertEquals(relatedPartyOrg.MainAddress.PK, relatedPartyRecord.PR_OA);

			var newAddress = relatedPartyOrg.Addresses.AddNew();
			relatedPartyRecord.PR_OA = newAddress.PK;
			relatedPartyRecord.PR_OH_RelatedParty = relatedPartyOrg.PK;
			AssertEquals(newAddress.PK, relatedPartyRecord.PR_OA);
		}

		#endregion

		#region TestPR_OH_RelatedParty_DefaultsTheAddressToMainAddressIfPartyTypeIsNationalDistributionCentre

		public void TestPR_OH_RelatedParty_DefaultsTheAddressToMainAddressIfPartyTypeIsNationalDistributionCentre()
		{
			var relatedPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyRecord = Organisation.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord.PR_OH_RelatedParty = relatedPartyOrg.PK;
			AssertEquals(ZGuid.Empty, relatedPartyRecord.PR_OA);

			relatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.NationalDistributionCentre;
			AssertEquals(Organisation.MainAddress.PK, relatedPartyRecord.PR_OA);

			var newRelatedPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyRecord.PR_OH_RelatedParty = newRelatedPartyOrg.PK;
			AssertEquals(Organisation.MainAddress.PK, relatedPartyRecord.PR_OA);

			relatedPartyRecord.PR_OH_RelatedParty = ZGuid.Empty;
			AssertEquals(Organisation.MainAddress.PK, relatedPartyRecord.PR_OA);
		}

		#endregion

		#region TestSettingPartyTypeSetsCompanyLevel

		public void TestSettingPartyTypeSetsCompanyLevel()
		{
			OrgHeader relatedOrg = Factory.New<OrgHeader>();
			Organisation.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery);
			Organisation.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP);
			OrgRelatedParty apSettlementGroupParty = Organisation.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP);
			OrgRelatedParty billToParty = Organisation.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery);
			AssertEquals(GlbCompany.CurrentCompany.PK, apSettlementGroupParty.PR_GC);
			AssertEquals(ZGuid.Empty, billToParty.PR_GC);
		}

		#endregion

		#region TestSettingPartyTypeSetsRelatedParty

		public void TestSettingPartyTypeSetsRelatedParty()
		{
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			AssertEquals(GlbCompany.CurrentCompany.GC_OH_OrgProxy, RelatedPartyRecord.PR_OH_RelatedParty);
		}

		#endregion

		#region TestSettingPartyTypeDefaultsDirection

		public void TestSettingPartyTypeDefaultsDirection()
		{
			RelatedPartyRecord.PR_FreightDirection = "XYZ";
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			AssertEquals("XYZ", RelatedPartyRecord.PR_FreightDirection);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertEquals("DLV", RelatedPartyRecord.PR_FreightDirection);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedVendor;
			AssertEquals("", RelatedPartyRecord.PR_FreightDirection);
		}

		#endregion

		#region TestSettingPartyTypeDefaultsLocation

		public void TestSettingPartyTypeDefaultsLocation()
		{
			RelatedPartyRecord.PR_Location = "AUMEL";
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			AssertEquals("AUMEL", RelatedPartyRecord.PR_Location);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			AssertEquals(ZString.Empty, RelatedPartyRecord.PR_Location);
		}

		#endregion

		#region TestSettingPartyTypeMarksAsNeedingValidation

		public void TestSettingPartyTypeInvalidatesLightValidation()
		{
			RelatedPartyRecord.RunPreSaveValidation();
			AssertEquals("Precondition", true, RelatedPartyRecord.LightValidationIsValid);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.NationalDistributionCentre;
			AssertEquals(false, RelatedPartyRecord.LightValidationIsValid);
		}

		#endregion

		#region TestPartyTypeDescription

		public void TestPartyTypeDescription()
		{
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			AssertEquals(RelatedPartyTypeList.Descriptions.CustomsAgentBroker, relatedPartyRecord.PartyTypeDescription);
		}

		#endregion

		#region TestForwarderPartyTypeDirection

		public void TestForwarderPartyTypeDirection()
		{
			RelatedPartyRecord.PR_FreightDirection = ZString.Empty;
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			AssertEquals(RelatedPartyDirectionList.Codes.Forwarder, RelatedPartyRecord.PR_FreightDirection);

			RelatedPartyRecord.PR_FreightDirection = ZString.Empty;
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;

			AssertEquals(RelatedPartyDirectionList.Codes.Delivery, RelatedPartyRecord.PR_FreightDirection);
		}

		#endregion

		#region TestCalculatedDirection

		public void TestCalculatedDirection()
		{
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			AssertEquals("Calculated Direction", ZString.Empty, RelatedPartyRecord.CalculatedDirection);
		}

		#endregion

		#region TestMarkInvoiceTermsAsNeedValidationForAP

		public void TestMarkInvoiceTermsAsNeedValidationForAP()
		{
			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().RunPreSaveValidation();
			AssertEquals("Precondition: ", false, Organisation.CompanyData.LoadARTermForAllInvoiceTypes().ShouldValidateOnSave);

			Organisation.CompanyData.RunPreSaveValidation();
			AssertEquals("Precondition: ", false, Organisation.CompanyData.ShouldValidateOnSave);
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			AssertEquals(true, Organisation.CompanyData.ShouldValidateOnSave);
			AssertEquals(false, Organisation.CompanyData.LoadARTermForAllInvoiceTypes().ShouldValidateOnSave);

			Organisation.CompanyData.RunPreSaveValidation();
			AssertEquals("Precondition: ", false, Organisation.CompanyData.ShouldValidateOnSave);
			RelatedPartyRecord.PR_OH_Parent = ZGuid.Empty;
			AssertEquals(true, Organisation.CompanyData.ShouldValidateOnSave);
			AssertEquals(false, Organisation.CompanyData.LoadARTermForAllInvoiceTypes().ShouldValidateOnSave);

			Organisation.CompanyData.RunPreSaveValidation();
			AssertEquals("Precondition: ", false, Organisation.CompanyData.ShouldValidateOnSave);
			RelatedPartyRecord.PR_OH_Parent = Organisation.PK;
			AssertEquals(true, Organisation.CompanyData.ShouldValidateOnSave);
			AssertEquals(false, Organisation.CompanyData.LoadARTermForAllInvoiceTypes().ShouldValidateOnSave);

			Organisation.CompanyData.RunPreSaveValidation();
			AssertEquals("Precondition: ", false, Organisation.CompanyData.ShouldValidateOnSave);
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			AssertEquals(true, Organisation.CompanyData.ShouldValidateOnSave);
			AssertEquals(false, Organisation.CompanyData.LoadARTermForAllInvoiceTypes().ShouldValidateOnSave);

			Organisation.CompanyData.RunPreSaveValidation();
			AssertEquals("Precondition: ", false, Organisation.CompanyData.ShouldValidateOnSave);
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ClientCFS;
			AssertEquals(false, Organisation.CompanyData.ShouldValidateOnSave);
			AssertEquals(false, Organisation.CompanyData.LoadARTermForAllInvoiceTypes().ShouldValidateOnSave);

			Organisation.CompanyData.RunPreSaveValidation();
			AssertEquals("Precondition: ", false, Organisation.CompanyData.ShouldValidateOnSave);
			RelatedPartyRecord.PR_OH_RelatedParty = ZGuid.Empty;
			AssertEquals(false, Organisation.CompanyData.ShouldValidateOnSave);
			AssertEquals(false, Organisation.CompanyData.LoadARTermForAllInvoiceTypes().ShouldValidateOnSave);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Organisation.CompanyData.RunPreSaveValidation();
			AssertEquals("Precondition: ", false, Organisation.CompanyData.ShouldValidateOnSave);
			RelatedPartyRecord.PR_OH_RelatedParty = ZGuid.Empty;
			AssertEquals(true, Organisation.CompanyData.ShouldValidateOnSave);
			AssertEquals(false, Organisation.CompanyData.LoadARTermForAllInvoiceTypes().ShouldValidateOnSave);

			Organisation.CompanyData.RunPreSaveValidation();
			AssertEquals("Precondition: ", false, Organisation.CompanyData.ShouldValidateOnSave);
			RelatedPartyRecord.Delete();
			AssertEquals(true, Organisation.CompanyData.ShouldValidateOnSave);
			AssertEquals(false, Organisation.CompanyData.LoadARTermForAllInvoiceTypes().ShouldValidateOnSave);
		}

		#endregion

		#region TestMarkInvoiceTermsAsNeedValidationForAR

		public void TestMarkInvoiceTermsAsNeedValidationForAR()
		{
			Organisation.CompanyData.RunPreSaveValidation();
			AssertEquals("Precondition: ", false, Organisation.CompanyData.ShouldValidateOnSave);

			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().RunPreSaveValidation();
			AssertARTermsShouldValidateOnSave("Precondition: ", false, Organisation.CompanyData.ARTerms);
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			AssertARTermsShouldValidateOnSave(true, Organisation.CompanyData.ARTerms);
			AssertEquals(false, Organisation.CompanyData.ShouldValidateOnSave);

			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().RunPreSaveValidation();
			AssertARTermsShouldValidateOnSave("Precondition: ", false, Organisation.CompanyData.ARTerms);
			RelatedPartyRecord.PR_OH_Parent = ZGuid.Empty;
			AssertARTermsShouldValidateOnSave(true, Organisation.CompanyData.ARTerms);
			AssertEquals(false, Organisation.CompanyData.ShouldValidateOnSave);

			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().RunPreSaveValidation();
			AssertARTermsShouldValidateOnSave("Precondition: ", false, Organisation.CompanyData.ARTerms);
			RelatedPartyRecord.PR_OH_Parent = Organisation.PK;
			AssertARTermsShouldValidateOnSave(true, Organisation.CompanyData.ARTerms);
			AssertEquals(false, Organisation.CompanyData.ShouldValidateOnSave);

			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().RunPreSaveValidation();
			AssertARTermsShouldValidateOnSave("Precondition: ", false, Organisation.CompanyData.ARTerms);
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			AssertARTermsShouldValidateOnSave(true, Organisation.CompanyData.ARTerms);
			AssertEquals(false, Organisation.CompanyData.ShouldValidateOnSave);

			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().RunPreSaveValidation();
			AssertARTermsShouldValidateOnSave("Precondition: ", false, Organisation.CompanyData.ARTerms);
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ClientCFS;
			AssertARTermsShouldValidateOnSave(false, Organisation.CompanyData.ARTerms);
			AssertEquals(false, Organisation.CompanyData.ShouldValidateOnSave);

			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().RunPreSaveValidation();
			AssertARTermsShouldValidateOnSave("Precondition: ", false, Organisation.CompanyData.ARTerms);
			RelatedPartyRecord.PR_OH_RelatedParty = ZGuid.Empty;
			AssertARTermsShouldValidateOnSave(false, Organisation.CompanyData.ARTerms);
			AssertEquals(false, Organisation.CompanyData.ShouldValidateOnSave);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().RunPreSaveValidation();
			AssertARTermsShouldValidateOnSave("Precondition: ", false, Organisation.CompanyData.ARTerms);
			RelatedPartyRecord.PR_OH_RelatedParty = ZGuid.Empty;
			AssertARTermsShouldValidateOnSave(true, Organisation.CompanyData.ARTerms);
			AssertEquals(false, Organisation.CompanyData.ShouldValidateOnSave);

			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().RunPreSaveValidation();
			AssertARTermsShouldValidateOnSave("Precondition: ", false, Organisation.CompanyData.ARTerms);
			RelatedPartyRecord.Delete();
			AssertARTermsShouldValidateOnSave(true, Organisation.CompanyData.ARTerms);
			AssertEquals(false, Organisation.CompanyData.ShouldValidateOnSave);
		}

		void AssertARTermsShouldValidateOnSave(bool shouldValidateOnSave, OrgARTermsCollection arTermsCollection)
		{
			AssertARTermsShouldValidateOnSave("", shouldValidateOnSave, arTermsCollection);
		}

		void AssertARTermsShouldValidateOnSave(string message, bool shouldValidateOnSave, OrgARTermsCollection arTermsCollection)
		{
			foreach (BusinessObject bizo in arTermsCollection)
			{
				AssertEquals(message, shouldValidateOnSave, bizo.ShouldValidateOnSave);
			}
		}

		#endregion

		public void TestCanDelete()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyCollection = new OrgRelatedPartyCollection(Factory, new ZQuery());
			var orgRelatedParty = relatedPartyCollection.AddNew();
			orgRelatedParty.PR_OH_Parent = orgHeader.PK;
			orgRelatedParty.PR_OH_RelatedParty = orgHeader2.PK;

			orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.Added;
			Assert("Can Delete", orgRelatedParty.CanDelete);
			orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.AddRejected;
			Assert("Can Delete", orgRelatedParty.CanDelete);
			orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.DeletePendingSeeCustomsMessagingMenu;
			Assert("Can Not Delete", !orgRelatedParty.CanDelete);
			orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.DeleteRejected;
			Assert("Can Delete", orgRelatedParty.CanDelete);
			orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.RefreshPendingSeeCustomsMessagingMenu;
			Assert("Can Not Delete", !orgRelatedParty.CanDelete);
			orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.WaitingForAddResponse;
			Assert("Can Not Delete", !orgRelatedParty.CanDelete);
			orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.WaitingForDeleteResponse;
			Assert("Can Not Delete", !orgRelatedParty.CanDelete);
			orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.WaitingForRefreshResponse;
			Assert("Can Not Delete", !orgRelatedParty.CanDelete);
			orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.AddPendingSeeCustomsMessagingMenu;
			Assert("Can Delete", orgRelatedParty.CanDelete);
			Factory.Save();
			Assert("Can Not Delete", !orgRelatedParty.CanDelete);
		}

		public void TestCanDelete_ControllingAgent_ToAnyOrg_NewOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();

			org.AllRelatedParties.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.ControllingAgent, "PIC");
			OrgRelatedParty relatedParty = org.AllRelatedParties[0];

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Assert(relatedParty.CanDelete);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Assert(relatedParty.CanDelete);

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Assert(relatedParty.CanDelete);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Assert(relatedParty.CanDelete);
		}

		public void TestCanDelete_ControllingAgent_ToAnyOrg_ExistingOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			org.AllRelatedParties.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.ControllingAgent, "PIC");
			OrgRelatedParty relatedParty = org.AllRelatedParties[0];

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Assert(relatedParty.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Assert(relatedParty.CanDelete);

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Assert(relatedParty.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Assert(relatedParty.CanDelete);

			Factory.Save();

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Assert(!relatedParty.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Assert(relatedParty.CanDelete);

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Assert(relatedParty.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Assert(relatedParty.CanDelete);
		}

		public void TestCanDelete_ControllingAgent_ToOrgProxy_NewOrg()
		{
			AssertCanDelete_ControllingAgent_ToOrgProxy_NewOrg(false);
		}

		public void TestCanDelete_ControllingAgent_ToOrgProxy_BranchProxy_NewOrg()
		{
			AssertCanDelete_ControllingAgent_ToOrgProxy_NewOrg(true);
		}

		void AssertCanDelete_ControllingAgent_ToOrgProxy_NewOrg(bool testBranchProxy)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var companyOrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var branch = GlbCompany.CurrentCompany.Branches.AddNew();
			branch.GB_OH_OrgProxy = branchOrgProxy.PK;

			var orgProxyForTest = testBranchProxy ? branchOrgProxy : companyOrgProxy;

			Factory.Save();

			org.AllRelatedParties.SetRelatedParty(orgProxyForTest, RelatedPartyTypeList.Codes.ControllingAgent, "PIC");
			OrgRelatedParty relatedPartyOrgProxy = org.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.ControllingAgent, "PIC");

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);
		}

		public void TestCanDelete_ControllingAgent_ToOrgProxy_ExistingOrg()
		{
			AssertCanDelete_ControllingAgent_ToOrgProxy_ExistingOrg(false);
		}

		public void TestCanDelete_ControllingAgent_ToOrgProxy_BranchProxy_ExistingOrg()
		{
			AssertCanDelete_ControllingAgent_ToOrgProxy_ExistingOrg(true);
		}

		void AssertCanDelete_ControllingAgent_ToOrgProxy_ExistingOrg(bool testBranchProxy)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var companyOrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var branch = GlbCompany.CurrentCompany.Branches.AddNew();
			branch.GB_OH_OrgProxy = branchOrgProxy.PK;

			var orgProxyForTest = testBranchProxy ? branchOrgProxy : companyOrgProxy;

			Factory.Save();

			//New related party
			org.AllRelatedParties.SetRelatedParty(orgProxyForTest, RelatedPartyTypeList.Codes.ControllingAgent, "PIC");
			OrgRelatedParty relatedPartyOrgProxy = org.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.ControllingAgent, "PIC");

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);

			Factory.Save();

			//Existing related party
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(!relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Assert(relatedPartyOrgProxy.CanDelete);

			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Assert(relatedPartyOrgProxy.CanDelete);
		}

		public void TestIsCSARelatedPartyType()
		{
			var orgRelatedParty = Factory.NewWithValidTestData<OrgRelatedParty>();
			Assert("Not", !orgRelatedParty.IsCSARelatedPartyType);
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee;
			Assert("Yes", orgRelatedParty.IsCSARelatedPartyType);
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedVendor;
			Assert("Yes", orgRelatedParty.IsCSARelatedPartyType);
		}

		public void TestUpdateCSAStatusAsAddedIfApplicable()
		{
			var orgRelatedParty = Factory.NewWithValidTestData<OrgRelatedParty>();
			AssertEquals("CSA Status", ZString.Empty, orgRelatedParty.PR_CustomsStatus);
			orgRelatedParty.UpdateCSAStatusAsAddedIfApplicable();
			AssertEquals("CSA Status", ZString.Empty, orgRelatedParty.PR_CustomsStatus);
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee;
			orgRelatedParty.UpdateCSAStatusAsAddedIfApplicable();
			AssertEquals("CSA Status", CSARelatedPartyStatusList.Codes.Added, orgRelatedParty.PR_CustomsStatus);
		}

		public void TestUpdateCSAStatusAsRefreshPendingIfApplicable()
		{
			var orgRelatedParty = Factory.NewWithValidTestData<OrgRelatedParty>();
			AssertEquals("CSA Status", ZString.Empty, orgRelatedParty.PR_CustomsStatus);
			orgRelatedParty.UpdateCSAStatusAsRefreshPendingIfApplicable();
			AssertEquals("CSA Status", ZString.Empty, orgRelatedParty.PR_CustomsStatus);
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee;
			orgRelatedParty.UpdateCSAStatusAsRefreshPendingIfApplicable();
			AssertEquals("CSA Status", CSARelatedPartyStatusList.Codes.RefreshPendingSeeCustomsMessagingMenu, orgRelatedParty.PR_CustomsStatus);
		}

		public void TestUpdateCSAStatusAsAddPendingIfApplicable()
		{
			var orgRelatedParty = Factory.NewWithValidTestData<OrgRelatedParty>();
			AssertEquals("CSA Status", ZString.Empty, orgRelatedParty.PR_CustomsStatus);
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee;
			AssertEquals("CSA Status", CSARelatedPartyStatusList.Codes.AddPendingSeeCustomsMessagingMenu, orgRelatedParty.PR_CustomsStatus);
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			AssertEquals("CSA Status", ZString.Empty, orgRelatedParty.PR_CustomsStatus);
		}

		public void TestPR_RN_NKImporterCountry_ResetAndReadOnly()
		{
			var partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderCFS;
			Assert(partyRecord.PR_RN_NKImporterCountryInfo.ReadOnly);

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderCoLoadWith;
			Assert(!partyRecord.PR_RN_NKImporterCountryInfo.ReadOnly);

			partyRecord.PR_RN_NKImporterCountry = "SG";
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderCFS;
			AssertEquals(ZString.Empty, partyRecord.PR_RN_NKImporterCountry);
		}

		#endregion

		#region Implementation

		OrgRelatedParty RelatedPartyRecord
		{
			get
			{
				if (relatedPartyRecord == null)
				{
					Organisation.AllRelatedParties.SetRelatedParty(Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG"), RelatedPartyTypeList.Codes.APNettingGroup, "PIC");

					relatedPartyRecord = Organisation.AllRelatedParties[0];
				}
				return relatedPartyRecord;
			}
		}
		OrgRelatedParty relatedPartyRecord;

		OrgHeader Organisation
		{
			get { return organisation ?? (organisation = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG")); }
		}
		OrgHeader organisation;

		#endregion
	}
}
