using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TemporaryOrgRemover.Remover))]
	sealed class TemporaryOrgRemoverTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDependentFields()
		{
			var fields = "OA_OH, OHM_OH_Carrier, OB_OH, OC_OH, OV_OH_OrgHeader, OK_OH, CZ_OH, OT_OH, OM_OH, OS_OH, P7_OH, OL_OH_Buyer, OL_OH_Supplier, PM_OH, O5_OH, PD_OH_Carrier, PD_OH_Client, PR_OH_Parent, ORC_OH, PD_OH_Client, EK_OH_MessageVAN, AI_OH_Client, PU_OH, OS_OA, OO_OH, OO_LocalGuid, PZ_OA, OV_OA_ApprovedLocation, ORF_OH, OAN_OH_Carrier, ONA_OH_Carrier, ONA_OH_Organization, OWC_OH_Client, PMA_OH, PME_OH, PMD_OH, PMN_OH, PMP_OH, PMR_OH, NRM_OH_Client, JDC_OH, ACC_OH_Creditor, OAA_OH_Carrier, ACI_OH_Carrier, RCO_OH_Client, OFC_OH_Organization, GOK_OH_Org, REQ_OH_AssignedOrganization, REQ_OH_ReviewerOrganization";
			AssertEquals(fields, TemporaryOrgRemover.Remover.DependentFields);
		}

		#region DeleteTemporaryOrgs

		public void TestDeleteTemporaryOrgs()
		{
			Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed = true;

			var orgActive1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgActive2 = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = GetNewTempOrg("TEMPTESTORG1");
			var org2 = GetNewTempOrg("TEMPTESTORG2");
			var org3 = GetNewTempOrg("TEMPTESTORG3");
			var org4 = GetNewTempOrg("TEMPTESTORG4");
			var org5 = GetNewTempOrg("TEMPTESTORG5");
			var org6 = GetNewTempOrg("TEMPTESTORG6");
			var org7 = GetNewTempOrg("TEMPTESTORG7");
			var org8 = GetNewTempOrg("TEMPTESTORG8");
			var org9 = GetNewTempOrg("TEMPTESTORG9");
			var org10 = GetNewTempOrg("TEMPTESTORGA");
			var org11 = GetNewTempOrg("TEMPTESTORGB");
			var org12 = GetNewTempOrg("TEMPTESTORGC");
			var org13 = GetNewTempOrg("TEMPTESTORGD");
			var org14 = GetNewTempOrg("TEMPTESTORGE");
			var org15 = GetNewTempOrg("TEMPTESTORGF");
			var org99 = GetNewTempOrg("TEMPTESTORGZ");

			var orgPatternMatchOverrideOrgActive1ToOrgActive2 = orgActive1.CreatePatternMatchOverrideForTest();
			orgPatternMatchOverrideOrgActive1ToOrgActive2.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgPatternMatchOverrideOrgActive1ToOrgActive2.OO_ForeignCode = "ABC";
			orgPatternMatchOverrideOrgActive1ToOrgActive2.OO_OH = orgActive1.PK;
			orgPatternMatchOverrideOrgActive1ToOrgActive2.OO_LocalGuid = orgActive2.PK;

			var orgPatternMatchOverrideOrgActive1ToOrg3 = orgActive1.CreatePatternMatchOverrideForTest();
			orgPatternMatchOverrideOrgActive1ToOrg3.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgPatternMatchOverrideOrgActive1ToOrg3.OO_ForeignCode = "XYZ";
			orgPatternMatchOverrideOrgActive1ToOrg3.OO_OH = orgActive1.PK;
			orgPatternMatchOverrideOrgActive1ToOrg3.OO_LocalGuid = org3.PK;

			var orgPatternMatchOverrideOrg3ToOrgActive1 = org3.CreatePatternMatchOverrideForTest();
			orgPatternMatchOverrideOrg3ToOrgActive1.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgPatternMatchOverrideOrg3ToOrgActive1.OO_ForeignCode = "GHJ";
			orgPatternMatchOverrideOrg3ToOrgActive1.OO_OH = org3.PK;
			orgPatternMatchOverrideOrg3ToOrgActive1.OO_LocalGuid = orgActive1.PK;

			var link1 = Factory.New<OrgSupplierBuyerLink>();
			link1.OL_OH_Buyer = org2.PK;
			link1.OL_OH_Supplier = org4.PK;
			var link2 = Factory.New<OrgSupplierBuyerLink>();
			link2.OL_OH_Supplier = org2.PK;
			link2.OL_OH_Buyer = org4.PK;

			var code = Factory.NewWithValidTestData<OrgCusCode>();
			code.OK_OH = org6.PK;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org7.PK;

			var relatedParty = Factory.NewWithValidTestData<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = org8.PK;

			var patternMatchingAddress = Factory.NewWithValidTestData<PatternMatchingAddress>();
			patternMatchingAddress.PMA_OH = org9.PK;

			var patternMatchingEmail = Factory.NewWithValidTestData<PatternMatchingEmail>();
			patternMatchingEmail.PME_OH = org10.PK;

			var patternMatchingDomain = Factory.NewWithValidTestData<PatternMatchingDomain>();
			patternMatchingDomain.PMD_OH = org11.PK;

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = org12.PK;

			var patternMatchingPhone = Factory.NewWithValidTestData<PatternMatchingPhone>();
			patternMatchingPhone.PMP_OH = org13.PK;

			var patternMatchingRegCode = Factory.NewWithValidTestData<PatternMatchingRegCode>();
			patternMatchingRegCode.PMR_OH = org14.PK;

			var account = Factory.NewWithValidTestData<OrgCusAccount>();
			account.CZ_OH = org15.PK;

			Factory.Save();

			var remover = new TemporaryOrgRemover.Remover();
			AssertEquals(16, remover.Organisations.Count);
			remover.Organisations[0].IncludeInDelete = false;

			//We add these references after Remover has been initialized, so that corresponding organizations
			//are part of the delete process but deletion will fail with SQL reference exception
			var newPart = OrgSupplierPart.New(Factory);
			newPart.OP_PartNum = "PartNum";
			var relation = newPart.RelatedOrganisations.AddNew();
			relation.OU_OH = org4.PK;
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = org5.MainAddress.PK;

			Factory.Save();

			var percentCompleteValues = new List<int>();
			var updateHandler = new TemporaryOrgRemover.Remover.ProgressChangedEventHandler(percentCompleteValue =>
			{
				percentCompleteValues.Add(percentCompleteValue);
				// simulate cancel when we have all but one update
				return percentCompleteValues.Count < remover.Organisations.Count - 1;
			});
			using (new DisposableAction(
				() => remover.ProgressChanged += updateHandler,
				() => remover.ProgressChanged -= updateHandler))
			{
				remover.DeleteTemporaryOrgs();
			}

			var businessObjectFactory = new BusinessObjectFactory();
			CombineAssertions(() =>
			{
				AssertEquals("Percentage complete update count", 15, percentCompleteValues.Count);
				Assert("Always increasing percentage complete", percentCompleteValues.Zip(percentCompleteValues.Skip(1), (a, b) => new { a, b }).All(x => x.b > x.a));
				AssertNotNull("Org1 Not included in delete", businessObjectFactory.Load<OrgHeader>(org1.PK));
				AssertNull("Org2 IsDeleted", businessObjectFactory.Load<OrgHeader>(org2.PK));
				AssertNull("Org3 IsDeleted", businessObjectFactory.Load<OrgHeader>(org3.PK));
				AssertNotNull("Delete Org4 caused SQL reference exception", businessObjectFactory.Load<OrgHeader>(org4.PK));
				AssertNotNull("Delete Org5 caused SQL reference exception", businessObjectFactory.Load<OrgHeader>(org5.PK));
				AssertNull("Org6 IsDeleted", businessObjectFactory.Load<OrgHeader>(org6.PK));
				AssertNull("Org7 IsDeleted", businessObjectFactory.Load<OrgHeader>(org7.PK));
				AssertNull("Org8 IsDeleted", businessObjectFactory.Load<OrgHeader>(org8.PK));
				AssertNull("Org9 IsDeleted", businessObjectFactory.Load<OrgHeader>(org9.PK));
				AssertNull("Org10 IsDeleted", businessObjectFactory.Load<OrgHeader>(org10.PK));
				AssertNull("Org11 IsDeleted", businessObjectFactory.Load<OrgHeader>(org11.PK));
				AssertNull("Org12 IsDeleted", businessObjectFactory.Load<OrgHeader>(org12.PK));
				AssertNull("Org13 IsDeleted", businessObjectFactory.Load<OrgHeader>(org13.PK));
				AssertNull("Org14 IsDeleted", businessObjectFactory.Load<OrgHeader>(org14.PK));
				AssertNull("Org15 IsDeleted", businessObjectFactory.Load<OrgHeader>(org15.PK));
				AssertNotNull("Org99 cancelled delete", businessObjectFactory.Load<OrgHeader>(org99.PK));

				AssertEquals("2 organisations could not be deleted", 2, remover.FailedOrganisations.Count);
				AssertEquals("FailedToDelete Org was Org 4", org4.PK, remover.FailedOrganisations[0].OrgPK);
				AssertEquals("FailedToDelete Org was Org 5", org5.PK, remover.FailedOrganisations[1].OrgPK);
				AssertNotNull("orgPatternMatchOverrideOrgActive1ToOrgActive2 is not deleted", businessObjectFactory.Load<OrgPatternMatchOverride>(orgPatternMatchOverrideOrgActive1ToOrgActive2.PK));
				AssertNull("orgPatternMatchOverrideOrgActive1ToOrg3 is deleted", businessObjectFactory.Load<OrgPatternMatchOverride>(orgPatternMatchOverrideOrgActive1ToOrg3.PK));
				AssertNull("orgPatternMatchOverrideOrg3ToOrgActive1 is deleted", businessObjectFactory.Load<OrgPatternMatchOverride>(orgPatternMatchOverrideOrg3ToOrgActive1.PK));
			});
		}

		#endregion

		#region BulkDeleteTemporaryOrgs

		public void TestBulkDeleteTemporaryOrgs()
		{
			Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed = true;

			var orgActive1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgActive2 = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = GetNewTempOrg("TEMPTESTORG1");
			var org2 = GetNewTempOrg("TEMPTESTORG2");
			var org3 = GetNewTempOrg("TEMPTESTORG3");
			var org4 = GetNewTempOrg("TEMPTESTORG4");
			var org5 = GetNewTempOrg("TEMPTESTORG5");
			var org6 = GetNewTempOrg("TEMPTESTORG6");
			var org7 = GetNewTempOrg("TEMPTESTORG7");
			var org8 = GetNewTempOrg("TEMPTESTORG8");
			var org9 = GetNewTempOrg("TEMPTESTORG9");
			var org10 = GetNewTempOrg("TEMPTESTORGA");
			var org11 = GetNewTempOrg("TEMPTESTORGB");
			var org12 = GetNewTempOrg("TEMPTESTORGC");
			var org13 = GetNewTempOrg("TEMPTESTORGD");
			var org14 = GetNewTempOrg("TEMPTESTORGE");
			var org15 = GetNewTempOrg("TEMPTESTORGF");
			var org99 = GetNewTempOrg("TEMPTESTORGZ");

			var orgPatternMatchOverrideOrgActive1ToOrgActive2 = orgActive1.CreatePatternMatchOverrideForTest();
			orgPatternMatchOverrideOrgActive1ToOrgActive2.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgPatternMatchOverrideOrgActive1ToOrgActive2.OO_ForeignCode = "ABC";
			orgPatternMatchOverrideOrgActive1ToOrgActive2.OO_LocalGuid = orgActive2.PK;

			var orgPatternMatchOverrideOrgActive1ToOrg3 = orgActive1.CreatePatternMatchOverrideForTest();
			orgPatternMatchOverrideOrgActive1ToOrg3.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgPatternMatchOverrideOrgActive1ToOrg3.OO_ForeignCode = "XYZ";
			orgPatternMatchOverrideOrgActive1ToOrg3.OO_LocalGuid = org3.PK;

			var orgPatternMatchOverrideOrg3ToOrgActive1 = org3.CreatePatternMatchOverrideForTest();
			orgPatternMatchOverrideOrg3ToOrgActive1.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgPatternMatchOverrideOrg3ToOrgActive1.OO_ForeignCode = "GHJ";
			orgPatternMatchOverrideOrg3ToOrgActive1.OO_OH = org3.PK;
			orgPatternMatchOverrideOrg3ToOrgActive1.OO_LocalGuid = orgActive1.PK;

			var link1 = Factory.New<OrgSupplierBuyerLink>();
			link1.OL_OH_Buyer = org2.PK;
			link1.OL_OH_Supplier = org4.PK;
			var link2 = Factory.New<OrgSupplierBuyerLink>();
			link2.OL_OH_Supplier = org2.PK;
			link2.OL_OH_Buyer = org4.PK;

			var code = Factory.NewWithValidTestData<OrgCusCode>();
			code.OK_OH = org6.PK;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org7.PK;

			var relatedParty = Factory.NewWithValidTestData<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = org8.PK;

			var patternMatchingAddress = Factory.NewWithValidTestData<PatternMatchingAddress>();
			patternMatchingAddress.PMA_OH = org9.PK;

			var patternMatchingEmail = Factory.NewWithValidTestData<PatternMatchingEmail>();
			patternMatchingEmail.PME_OH = org10.PK;

			var patternMatchingDomain = Factory.NewWithValidTestData<PatternMatchingDomain>();
			patternMatchingDomain.PMD_OH = org11.PK;

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = org12.PK;

			var patternMatchingPhone = Factory.NewWithValidTestData<PatternMatchingPhone>();
			patternMatchingPhone.PMP_OH = org13.PK;

			var patternMatchingRegCode = Factory.NewWithValidTestData<PatternMatchingRegCode>();
			patternMatchingRegCode.PMR_OH = org14.PK;

			var account = Factory.NewWithValidTestData<OrgCusAccount>();
			account.CZ_OH = org15.PK;

			Factory.Save();

			var remover = new TemporaryOrgRemover.Remover();
			AssertEquals(16, remover.Organisations.Count);
			remover.Organisations[0].IncludeInDelete = false;

			//We add these references after Remover has been initialized, so that corresponding organizations
			//are part of the delete process but deletion will fail with SQL reference exception
			var newPart = OrgSupplierPart.New(Factory);
			newPart.OP_PartNum = "PartNum";
			var relation = newPart.RelatedOrganisations.AddNew();
			relation.OU_OH = org4.PK;
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = org5.MainAddress.PK;

			Factory.Save();

			var percentCompleteValues = new List<int>();
			var updateHandler = new TemporaryOrgRemover.Remover.ProgressChangedEventHandler(percentCompleteValue =>
			{
				percentCompleteValues.Add(percentCompleteValue);
				// simulate cancel when we have all but one update
				return percentCompleteValues.Count < remover.Organisations.Count - 1;
			});
			using (new DisposableAction(
				() => remover.ProgressChanged += updateHandler,
				() => remover.ProgressChanged -= updateHandler))
			{
				remover.BulkDeleteTemporaryOrgs();
			}

			var businessObjectFactory = new BusinessObjectFactory();

			AssertEquals("Percentage complete update count", 15, percentCompleteValues.Count);
			Assert("Always increasing percentage complete", percentCompleteValues.Take(percentCompleteValues.Count - 1).Zip(percentCompleteValues.Skip(1), (a, b) => new { a, b }).All(x => x.b > x.a));
			AssertNull("Org1 IsDeleted (we ignore IncludeInDelete flags for BulkDelete)", businessObjectFactory.Load<OrgHeader>(org1.PK));
			AssertNull("Org2 IsDeleted", businessObjectFactory.Load<OrgHeader>(org2.PK));
			AssertNull("Org3 IsDeleted", businessObjectFactory.Load<OrgHeader>(org3.PK));
			AssertNotNull("Delete Org4 caused SQL reference exception", businessObjectFactory.Load<OrgHeader>(org4.PK));
			AssertNotNull("Delete Org5 caused SQL reference exception", businessObjectFactory.Load<OrgHeader>(org5.PK));
			AssertNull("Org6 IsDeleted", businessObjectFactory.Load<OrgHeader>(org6.PK));
			AssertNull("Org7 IsDeleted", businessObjectFactory.Load<OrgHeader>(org7.PK));
			AssertNull("Org8 IsDeleted", businessObjectFactory.Load<OrgHeader>(org8.PK));
			AssertNull("Org9 IsDeleted", businessObjectFactory.Load<OrgHeader>(org9.PK));
			AssertNull("Org10 IsDeleted", businessObjectFactory.Load<OrgHeader>(org10.PK));
			AssertNull("Org11 IsDeleted", businessObjectFactory.Load<OrgHeader>(org11.PK));
			AssertNull("Org12 IsDeleted", businessObjectFactory.Load<OrgHeader>(org12.PK));
			AssertNull("Org13 IsDeleted", businessObjectFactory.Load<OrgHeader>(org13.PK));
			AssertNull("Org14 IsDeleted", businessObjectFactory.Load<OrgHeader>(org14.PK));
			AssertNull("Org15 IsDeleted", businessObjectFactory.Load<OrgHeader>(org15.PK));
			AssertNotNull("Org99 cancelled delete", businessObjectFactory.Load<OrgHeader>(org99.PK));
			AssertEquals("2 organisations could not be deleted", 2, remover.FailedOrganisations.Count);
			AssertEquals("FailedToDelete Org was Org 4", org4.PK, remover.FailedOrganisations[0].OrgPK);
			AssertEquals("FailedToDelete Org was Org 5", org5.PK, remover.FailedOrganisations[1].OrgPK);
			AssertNotNull("orgPatternMatchOverrideOrgActive1ToOrgActive2 is not deleted", businessObjectFactory.Load<OrgPatternMatchOverride>(orgPatternMatchOverrideOrgActive1ToOrgActive2.PK));
			AssertNull("orgPatternMatchOverrideOrgActive1ToOrg3 is deleted", businessObjectFactory.Load<OrgPatternMatchOverride>(orgPatternMatchOverrideOrgActive1ToOrg3.PK));
			AssertNull("orgPatternMatchOverrideOrg3ToOrgActive1 is deleted", businessObjectFactory.Load<OrgPatternMatchOverride>(orgPatternMatchOverrideOrg3ToOrgActive1.PK));
		}

		#endregion

		#region Implementation

		OrgHeader GetNewTempOrg(string code)
		{
			var org = Factory.New<OrgHeader>();
			org.SetDefaultValuesForTemporaryOrganisation();
			org.OH_FullName = "TEST " + code;
			org.OH_Code = code;
			return org;
		}
		#endregion
	}
}
