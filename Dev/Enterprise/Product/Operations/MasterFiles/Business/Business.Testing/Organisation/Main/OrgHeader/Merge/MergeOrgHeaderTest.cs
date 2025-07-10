using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MergeOrgHeader))]
	public class MergeOrgHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			MergeOrgHeader testMergeOrg = NewTestMergeOrgHeader("DEMORG");

			AssertEquals("Code", "DEMORG", testMergeOrg.OldOrganisation.OH_Code);
			AssertEquals("Full Name", "DEMO ORGANISATION", testMergeOrg.OldOrganisation.OH_FullName.ToUpper());
			AssertEquals("New Organisation PK should be empty GUID", ZGuid.Empty, testMergeOrg.NewOrganisationPk);

			MergeOrgHeader test1 = new MergeOrgHeader(Factory, null);
			AssertNotNull(test1);
			AssertNull(test1.OldOrganisation);
			AssertEquals("New Organisation PK should be empty GUID", ZGuid.Empty, test1.NewOrganisationPk);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "NEW ORGANISATION 1";
			org1.OH_Code = "~NEWORG1~";
			MergeOrgHeader test2 = new MergeOrgHeader(Factory, null, org1);
			AssertNotNull(test2);
			AssertNull(test1.OldOrganisation);
			AssertEquals("New Organisation PK should not be empty GUID", org1.PK, test2.NewOrganisationPk);

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "NEW ORGANISATION 2";
			org2.OH_Code = "~NEWORG2~";
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "OLD ORGANISATION 1";
			org3.OH_Code = "~OLDORG1~";
			MergeOrgHeader test3 = new MergeOrgHeader(Factory, org3, org1);
			AssertNotNull(test3);
			AssertEquals("Code", "~OLDORG1~", test3.OldOrganisation.OH_Code);
			AssertEquals("Full Name", "OLD ORGANISATION 1", test3.OldOrganisation.OH_FullName);
			AssertEquals("New Organisation PK should not be empty GUID", org1.PK, test3.NewOrganisationPk);
		}

		public void TestValidateNewOrganisationPk()
		{
			MergeOrgHeader testMergeOrg = NewTestMergeOrgHeader("DEMORG");
			AssertNoErrors(testMergeOrg.NewOrganisationPkInfo);
			testMergeOrg.RunPreSaveValidation();

			testMergeOrg.NewOrganisationPk = ZGuid.Invalid;
			AssertHasErrors(testMergeOrg.NewOrganisationPkInfo);

			testMergeOrg.NewOrganisationPk = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			AssertHasError(testMergeOrg.NewOrganisationPkInfo, (NoResString)"You cannot merge this organization because it is a special, system-defined organization.");

			OrgHeader oldOrg = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			testMergeOrg.NewOrganisationPk = oldOrg.PK;
			AssertHasError(testMergeOrg.NewOrganisationPkInfo, (NoResString)"The \"Old\" organization is the same as the \"New\" organization You cannot merge an organization with itself.");

			testMergeOrg = NewTestMergeOrgHeader("UNMATCHED");
			testMergeOrg.NewOrganisationPk = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			AssertHasError(testMergeOrg.NewOrganisationPkInfo, (NoResString)"You cannot merge this organization because it is a special, system-defined organization.");
			AssertHasError(testMergeOrg.OldOrganisationCodeInfo, (NoResString)"You cannot merge this organization because it is a special, system-defined organization.");

			oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			oldOrg.OH_IsConsignor = ZBool.True;
			oldOrg.OH_IsForwarder = ZBool.True;
			testMergeOrg = new MergeOrgHeader(Factory, oldOrg);
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_IsDebtor = ZBool.False;
			testMergeOrg.NewOrganisationPk = newOrg.PK;
			AssertNoNotifications(testMergeOrg.NewOrganisationPkInfo);
		}

		public void TestFormValidationPreventsDeletedPropertyAccess()
		{
			var org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			OrgAddress adrObj1 = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress adrObj2 = Factory.NewWithValidTestData<OrgAddress>();
			adrObj1.OA_Address1 = "street magic";
			adrObj1.OA_City = "SYD";
			adrObj2.OA_Address1 = "no street magic";
			adrObj2.OA_City = "LON";
			MergeOrgHeaderForTest mergeOrgHeader = new MergeOrgHeaderForTest(Factory, org1, org2);

			MergeOrgAddress adr1 = new MergeOrgAddress(Factory, adrObj1, null);
			MergeOrgAddress adr2 = new MergeOrgAddress(Factory, adrObj2, null);

			OrgContact cntObj1 = Factory.NewWithValidTestData<OrgContact>();
			OrgContact cntObj2 = Factory.NewWithValidTestData<OrgContact>();
			cntObj1.OC_ContactName = "Isaac";
			cntObj2.OC_ContactName = "Felix";

			MergeOrgContact cnt1 = new MergeOrgContact(Factory, cntObj1, null);
			MergeOrgContact cnt2 = new MergeOrgContact(Factory, cntObj2, null);
			mergeOrgHeader.OldOrgContactCollectionForSimilarOrgsByName.Add(cnt1);
			mergeOrgHeader.OldOrgAddressCollectionForSimilarOrgsByName.Add(adr1);

			mergeOrgHeader.CurrentOrgAddressCollection[0].OldObject.Delete();
			AssertNoExceptionThrown(() => mergeOrgHeader.ValidateOldOrganisations());
		}

		public void TestValidateNewOrganisationPk_WithWarehousePartAttributes()
		{
			var testMergeOrg = NewTestMergeOrgHeader("DEMORG");
			AssertNoErrors(testMergeOrg.NewOrganisationPkInfo);
			testMergeOrg.RunPreSaveValidation();

			testMergeOrg.NewOrganisationPk = ZGuid.Invalid;
			AssertHasErrors(testMergeOrg.NewOrganisationPkInfo);

			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			oldOrg.MiscServ.OM_IMPartAttrib1Name = "SLIP";
			oldOrg.MiscServ.OM_IMPartAttrib1Type = "NON";

			testMergeOrg = new MergeOrgHeader(Factory, oldOrg);
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.MiscServ.OM_IMPartAttrib1Name = "SLIP";
			newOrg.MiscServ.OM_IMPartAttrib1Type = "BAT";

			testMergeOrg.NewOrganisationPk = newOrg.PK;
			AssertHasError(testMergeOrg.NewOrganisationPkInfo, "Cannot merge the two Organizations because the Warehouse Part Attributes do not match.");

			oldOrg.MiscServ.OM_IMPartAttrib2Name = "slop";
			oldOrg.MiscServ.OM_IMPartAttrib2Type = "BAT";
			newOrg.MiscServ.OM_IMPartAttrib1Type = "NON";
			newOrg.MiscServ.OM_IMPartAttrib2Name = "SLOP";
			newOrg.MiscServ.OM_IMPartAttrib2Type = "BAT";
			testMergeOrg = new MergeOrgHeader(Factory, oldOrg);
			testMergeOrg.NewOrganisationPk = newOrg.PK;
			AssertNoErrors(testMergeOrg.NewOrganisationPkInfo);

			oldOrg.MiscServ.OM_IMPartAttrib3Name = "SLAP";
			oldOrg.MiscServ.OM_IMPartAttrib3Type = "SER";
			newOrg.MiscServ.OM_IMPartAttrib3Name = "SLUP";
			newOrg.MiscServ.OM_IMPartAttrib3Type = "SER";
			testMergeOrg = new MergeOrgHeader(Factory, oldOrg);
			testMergeOrg.NewOrganisationPk = newOrg.PK;
			AssertHasError(testMergeOrg.NewOrganisationPkInfo, "Cannot merge the two Organizations because the Warehouse Part Attributes do not match.");

			newOrg.MiscServ.OM_IMPartAttrib3Name = "SLAP";
			oldOrg.MiscServ.OM_IMUseExpiryDate = true;
			testMergeOrg = new MergeOrgHeader(Factory, oldOrg);
			testMergeOrg.NewOrganisationPk = newOrg.PK;
			AssertHasError(testMergeOrg.NewOrganisationPkInfo, "Cannot merge the two Organizations because the Warehouse Part Attributes do not match.");

			oldOrg.MiscServ.OM_IMUseExpiryDate = false;
			oldOrg.MiscServ.OM_IMUsePackingDate = true;
			testMergeOrg = new MergeOrgHeader(Factory, oldOrg);
			testMergeOrg.NewOrganisationPk = newOrg.PK;
			AssertHasError(testMergeOrg.NewOrganisationPkInfo, "Cannot merge the two Organizations because the Warehouse Part Attributes do not match.");
		}

		public void TestValidateNewOrganisationPk_WithWarehousePartAttributes_SerialNumber()
		{
			var testMergeOrg = NewTestMergeOrgHeader("DEMORG");
			AssertNoErrors(testMergeOrg.NewOrganisationPkInfo);
			testMergeOrg.RunPreSaveValidation();

			testMergeOrg.NewOrganisationPk = ZGuid.Invalid;
			AssertHasErrors(testMergeOrg.NewOrganisationPkInfo);

			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			oldOrg.MiscServ.OM_IMUseSerialNumber = true;

			testMergeOrg = new MergeOrgHeader(Factory, oldOrg);
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.MiscServ.OM_IMUseSerialNumber = false;

			testMergeOrg.NewOrganisationPk = newOrg.PK;
			AssertHasError(testMergeOrg.NewOrganisationPkInfo, "Cannot merge the two Organizations because the Warehouse Part Attributes do not match.");

			newOrg.MiscServ.OM_IMUseSerialNumber = true;
			testMergeOrg = new MergeOrgHeader(Factory, oldOrg);
			testMergeOrg.NewOrganisationPk = newOrg.PK;
			AssertNoErrors(testMergeOrg.NewOrganisationPkInfo);
		}

		public void TestValidateNewOrganisationPk_WithOrgCusAccount()
		{
			var errorMessage = "The old organization has been setup with Account Numbers, please remove the Account Numbers from the old organization prior to merge.";
			var newOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var oldOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			var mergedOrgHeader = new MergeOrgHeader(Factory, oldOrgHeader)
			{
				NewOrganisationPk = newOrgHeader.PK
			};

			orgCusAccount.CZ_OH = oldOrgHeader.PK;
			mergedOrgHeader.RunPreSaveValidation();
			AssertHasError(mergedOrgHeader.OldOrganisationCodeInfo, errorMessage);

			orgCusAccount.CZ_OH = newOrgHeader.PK;
			mergedOrgHeader.RunPreSaveValidation();
			AssertNoError(mergedOrgHeader.OldOrganisationCodeInfo, errorMessage);
		}

		public void TestValidateNewOrganisationPK_ControllingBranchRequired()
		{
			OrganisationRegistry.Instance.AllowMergeIgnoringARAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OrgRequiredFields requiredFields = new OrgRequiredFields(false, true, false, false, false, false, false, false, false, false, false); //RequireBranch = true
			DataRegistry.Instance.SetOrgCreditorRequiredFields(requiredFields);
			MergeOrgHeader testMergeOrg = SetupForARAPMerge_NoControllingBranchOnNewCompany();
			AssertHasError(testMergeOrg.NewOrganisationPkInfo, "For company company 1, the new organization lacks a Controlling Branch set up, but the old organization has organization type(s) specified in the registry setting 'Organization Required Fields' to require a Controlling Branch. Please enter a Controlling Branch.");
			AssertHasError(testMergeOrg.NewOrganisationPkInfo, "For company company 2, the new organization lacks a Controlling Branch set up, but the old organization has organization type(s) specified in the registry setting 'Organization Required Fields' to require a Controlling Branch. Please enter a Controlling Branch.");
		}

		//This test the scenario of Incident CS00425206/WI00121342
		public void TestValidateNewOrganisationPK_OldOrgHasLessCompaniesThanNewOrg()
		{
			OrganisationRegistry.Instance.AllowMergeIgnoringARAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OrgRequiredFields requiredFields = new OrgRequiredFields(false, true, false, false, false, false, false, false, false, false, false); //RequireBranch = true
			DataRegistry.Instance.SetOrgCreditorRequiredFields(requiredFields);
			MergeOrgHeader testMergeOrg = SetupForARAPMerge_OldOrgHasLessCompaniesThanNewOrg();

			AssertNoErrors(testMergeOrg.NewOrganisationPkInfo);
		}

		public void TestValidateNewOrganisationPK_ARAP_RegistryFalse()
		{
			OrganisationRegistry.Instance.AllowMergeIgnoringARAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			MergeOrgHeader testMergeOrg = SetupForARAPMerge();
			AssertHasError(testMergeOrg.NewOrganisationPkInfo, "The old organization has been setup with the following Organization Types. It can only be merged into another organization that is also setup with these types below:" + System.Environment.NewLine + "Payable (company 1), Receivable (company 1), Payable (company 2)");
		}

		public void TestValidateNewOrganisationPK_ARAP_RegistryTrue()
		{
			OrganisationRegistry.Instance.AllowMergeIgnoringARAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			MergeOrgHeader testMergeOrg = SetupForARAPMerge();
			AssertNoErrors(testMergeOrg.NewOrganisationPkInfo);
		}

		public void TestValidateNewOrganisationPK_DuplicateVoyageAccounts()
		{
			var principal1 = Factory.New<OrgHeader>();
			principal1.OH_Code = "PRINCIPAL1";

			var principal2 = Factory.New<OrgHeader>();
			principal2.OH_Code = "PRINCIPAL2";

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "VESSEL1";

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "VESSEL2";

			var vessel3 = Factory.NewWithValidTestData<RefVessel>();
			vessel3.RV_Name = "VESSEL3";

			var vessel4 = Factory.NewWithValidTestData<RefVessel>();
			vessel4.RV_Name = "VESSEL4";

			var voyage1 = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			voyage1[JobVoyageSchema.JV_RV_NKVessel] = vessel1.RV_FK;
			voyage1[JobVoyageSchema.JV_VoyageFlight] = "VOYAGE1";

			var voyage2 = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			voyage2[JobVoyageSchema.JV_RV_NKVessel] = vessel2.RV_FK;
			voyage2[JobVoyageSchema.JV_VoyageFlight] = "VOYAGE2";

			var voyage3 = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			voyage3[JobVoyageSchema.JV_RV_NKVessel] = vessel3.RV_FK;
			voyage3[JobVoyageSchema.JV_VoyageFlight] = "VOYAGE3";

			var voyage4 = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			voyage4[JobVoyageSchema.JV_RV_NKVessel] = vessel4.RV_FK;
			voyage4[JobVoyageSchema.JV_VoyageFlight] = "VOYAGE4";

			var voyAccount1 = (BusinessObject)Factory.New<Freight.Integration.Agency.IVoyageAccount>();
			voyAccount1[JobVoyAccountSchema.NA_JV] = voyage1.PK;
			voyAccount1[JobVoyAccountSchema.NA_OH] = principal1.PK;
			voyAccount1[JobVoyAccountSchema.NA_GC] = GlbCompany.CurrentCompany.PK;

			var voyAccount2 = (BusinessObject)Factory.New<Freight.Integration.Agency.IVoyageAccount>();
			voyAccount2[JobVoyAccountSchema.NA_JV] = voyage1.PK;
			voyAccount2[JobVoyAccountSchema.NA_OH] = principal2.PK;
			voyAccount2[JobVoyAccountSchema.NA_GC] = GlbCompany.CurrentCompany.PK;

			var voyAccount3 = (BusinessObject)Factory.New<Freight.Integration.Agency.IVoyageAccount>();
			voyAccount3[JobVoyAccountSchema.NA_JV] = voyage2.PK;
			voyAccount3[JobVoyAccountSchema.NA_OH] = principal1.PK;
			voyAccount3[JobVoyAccountSchema.NA_GC] = GlbCompany.CurrentCompany.PK;

			var voyAccount4 = (BusinessObject)Factory.New<Freight.Integration.Agency.IVoyageAccount>();
			voyAccount4[JobVoyAccountSchema.NA_JV] = voyage3.PK;
			voyAccount4[JobVoyAccountSchema.NA_OH] = principal2.PK;
			voyAccount4[JobVoyAccountSchema.NA_GC] = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var merge = new MergeOrgHeader(Factory, principal1);
			merge.NewOrganisationPk = principal2.PK;

			AssertHasError(merge.NewOrganisationPkInfo, "Both organizations contain voyage account records for the same vessel/voyage and company. You must resolve this before merging.");

			voyAccount2[JobVoyAccountSchema.NA_JV] = voyage4.PK;

			Factory.Save();

			merge = new MergeOrgHeader(Factory, principal1);
			merge.NewOrganisationPk = principal2.PK;

			AssertNoError(merge.NewOrganisationPkInfo, "Both organizations contain voyage account records for the same vessel/voyage and company. You must resolve this before merging.");
		}

		public void TestValidateNewOrganisationPK_DuplicatePermitHeader()
		{
			var testOrg1 = Factory.New<OrgHeader>();
			testOrg1.OH_Code = "TESTORG1";
			var testOrg2 = Factory.New<OrgHeader>();
			testOrg2.OH_Code = "TESTORG2";

			var permitHeader1 = Factory.New<IBaseCusPermitHeader>();
			var permitHeader2 = Factory.New<IBaseCusPermitHeader>();
			permitHeader1.CPH_Number = "0123456789";
			permitHeader2.CPH_Number = "9876543210";
			permitHeader1.CPH_RN_NKCountryCode = permitHeader2.CPH_RN_NKCountryCode = "AU";
			permitHeader1.CPH_StartDate = permitHeader2.CPH_StartDate = new ZDate(2018, 6, 7);
			permitHeader1.CPH_Type = permitHeader2.CPH_Type = "ABC";
			permitHeader1.CPH_SubType = permitHeader2.CPH_SubType = "DEF";
			permitHeader1.CPH_QtyValIndicator = permitHeader2.CPH_QtyValIndicator = "BTH";
			permitHeader1.CPH_OH_PermitHolder = testOrg1.PK;
			permitHeader2.CPH_OH_PermitHolder = testOrg2.PK;

			var merge = new MergeOrgHeader(Factory, testOrg2);
			merge.NewOrganisationPk = testOrg1.PK;
			AssertNoError(merge.NewOrganisationPkInfo, "Permit Number <0123456789> already exists for organization TESTORG1, please amend Permit Number or organization on this permit prior to merge");

			permitHeader2.CPH_Number = permitHeader1.CPH_Number;
			merge = new MergeOrgHeader(Factory, testOrg2);
			merge.NewOrganisationPk = testOrg1.PK;
			AssertHasError(merge.NewOrganisationPkInfo, "Permit Number <0123456789> already exists for organization TESTORG1, please amend Permit Number or organization on this permit prior to merge");
		}

		public void TestCheckFinancialAccountNumberMappings_SingleOrganisation()
		{
			var testOrg1 = Factory.New<OrgHeader>();
			testOrg1.OH_Code = "TESTORG1";
			var testOrg2 = Factory.New<OrgHeader>();
			testOrg2.OH_Code = "TESTORG2";

			var merge = new MergeOrgHeader(Factory, testOrg1);
			merge.NewOrganisationPk = testOrg2.PK;
			merge.RunPreSaveValidation();
			AssertNoNotifications(merge.OldOrganisationCodeInfo);

			var mergeForTest = new MergeOrgHeaderForTest(Factory, testOrg1);
			mergeForTest.NewOrganisationPk = testOrg2.PK;

			var expectedErrorMessage = $"A FAN mapping 12345 exists for organization TESTORG1 in South Africa Company {GlbCompany.CurrentCompany.GC_Code}, please amend FAN list in Registry for this organization prior to merge";
			mergeForTest.RunPreSaveValidation();
			AssertNoError(mergeForTest.OldOrganisationCodeInfo, expectedErrorMessage);
			Assert(!mergeForTest.HasErrors);

			mergeForTest.SetFANMapping(GlbCompany.CurrentCompany.PK, testOrg1.PK, "12345");
			mergeForTest.RunPreSaveValidation();
			AssertHasError(mergeForTest.OldOrganisationCodeInfo, expectedErrorMessage);
			Assert(mergeForTest.HasErrors);
		}

		public void TestCheckFinancialAccountNumberMappings_MultipleOrganisations()
		{
			var testOrg1 = Factory.New<OrgHeader>();
			testOrg1.OH_Code = "TESTORG1";
			var testOrg2 = Factory.New<OrgHeader>();
			testOrg2.OH_Code = "TESTORG2";
			var testOrg3 = Factory.New<OrgHeader>();
			testOrg3.OH_Code = "TESTORG3";

			var mergeForTest = new MergeOrgHeaderForTest(Factory, null, testOrg1);
			mergeForTest.HasToLoadSimilarOrgs = true;
			mergeForTest.CurrentOrgHeaderCollection.Add(testOrg2);
			mergeForTest.CurrentOrgHeaderCollection.Add(testOrg3);

			var expectedErrorMessage = $"A FAN mapping 12345 exists for organization TESTORG2 in South Africa Company {GlbCompany.CurrentCompany.GC_Code}, please amend FAN list in Registry for this organization prior to merge";
			mergeForTest.RunPreSaveValidation();
			AssertNoRowError(testOrg2, expectedErrorMessage);
			Assert(!mergeForTest.HasErrors);
			AssertNoError(mergeForTest.OldOrganisationCodeInfo, expectedErrorMessage);

			mergeForTest.SetFANMapping(GlbCompany.CurrentCompany.PK, testOrg2.PK, "12345");
			mergeForTest.RunPreSaveValidation();
			AssertHasRowError(testOrg2, expectedErrorMessage);
			Assert(mergeForTest.HasErrors);
			AssertHasError(mergeForTest.OldOrganisationCodeInfo, expectedErrorMessage);
		}

		public void TestMergeWithDuplicateProducts_DifferentProducts()
		{
			AssertMergeWithDuplicateProducts(AssertNoErrors, (oldOrg, newOrg) =>
			{
				var part1 = Factory.New<OrgSupplierPart>();
				part1.OP_PartNum = "DUPTEST-11";
				part1.RelatedOrganisations.AddOwner(oldOrg);

				var part2 = Factory.New<OrgSupplierPart>();
				part2.OP_PartNum = "DUPTEST-12";
				part2.RelatedOrganisations.AddOwner(newOrg);
			});
		}

		public void TestMergeWithDuplicateProducts_DuplicateProducts()
		{
			AssertMergeWithDuplicateProducts(propertyInfo => AssertHasError("Duplicate Products", propertyInfo, "Merge will result in duplicate products associated with the new organization. Product codes are: DUPTEST-20"),
				(oldOrg, newOrg) =>
				{
					var part1 = Factory.New<OrgSupplierPart>();
					part1.OP_PartNum = "DUPTEST-20";
					part1.RelatedOrganisations.AddOwner(oldOrg);

					var part2 = Factory.New<OrgSupplierPart>();
					part2.OP_PartNum = "DUPTEST-20";
					part2.RelatedOrganisations.AddOwner(newOrg);
				});
		}

		public void TestMergeWithDuplicateProducts_DuplicateButInactive()
		{
			AssertMergeWithDuplicateProducts(AssertNoErrors, (oldOrg, newOrg) =>
			{
				var part1 = Factory.New<OrgSupplierPart>();
				part1.OP_PartNum = "DUPTEST-30";
				part1.RelatedOrganisations.AddOwner(oldOrg);

				var part2 = Factory.New<OrgSupplierPart>();
				part2.OP_PartNum = "DUPTEST-30";
				part2.RelatedOrganisations.AddOwner(newOrg);
				part2.OP_IsActive = false;
			});
		}

		public void TestMergeWithDuplicateProducts_DifferentRelationships()
		{
			AssertMergeWithDuplicateProducts(AssertNoErrors, (oldOrg, newOrg) =>
			{
				var part1 = Factory.New<OrgSupplierPart>();
				part1.OP_PartNum = "DUPTEST-40";
				part1.RelatedOrganisations.AddOwner(oldOrg);

				var part2 = Factory.New<OrgSupplierPart>();
				part2.OP_PartNum = "DUPTEST-40";
				part2.RelatedOrganisations.AddOrganisationIfNotExist(newOrg.PK, "WCN");
			});
		}

		public void TestMergeWithDuplicateProducts_NoRelatedProducts()
		{
			AssertMergeWithDuplicateProducts(AssertNoErrors, null);
		}

		public void TestMergeWithDuplicateProducts_SelfDuplicate()
		{
			AssertMergeWithDuplicateProducts(AssertNoErrors, (oldOrg, newOrg) =>
			{
				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = "DUPTEST-50";
				part.RelatedOrganisations.AddOwner(oldOrg);
				part.RelatedOrganisations.AddOwner(newOrg);
			});
		}

		void AssertMergeWithDuplicateProducts(Action<ZPropertyInfo> assertExpectation, Action<OrgHeader, OrgHeader> setupProducts)
		{
			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();

			if (setupProducts != null)
			{
				setupProducts(oldOrg, newOrg);
			}

			Factory.Save();

			var merge = new MergeOrgHeader(Factory, oldOrg, newOrg);
			merge.RunPreSaveValidation();

			assertExpectation(merge.OldOrganisationCodeInfo);
		}

		public void TestMergeWithDuplicateProductsMore()
		{
			var org21 = Factory.NewWithValidTestData<OrgHeader>();
			var org22 = Factory.NewWithValidTestData<OrgHeader>();

			for (int i = 0; i < 11; i++)
			{
				var part1 = Factory.New<OrgSupplierPart>();
				part1.OP_PartNum = "DUPTEST-2" + i;
				part1.RelatedOrganisations.AddOwner(org21);

				var part2 = Factory.New<OrgSupplierPart>();
				part2.OP_PartNum = "DUPTEST-2" + i;
				part2.RelatedOrganisations.AddOwner(org22);
			}

			Factory.Save();

			var merge2 = new MergeOrgHeader(Factory, org21, org22);

			merge2.RunPreSaveValidation();

			// when there are more duplicate products than maxProductsToDisplay, we should indicate that there more duplicates than shown
			AssertHasErrorContaining(merge2.OldOrganisationCodeInfo, ",...");
		}

		MergeOrgHeader SetupForARAPMerge()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();
			GlbCompany company2 = Factory.New<GlbCompany>();
			GlbCompany company3 = Factory.New<GlbCompany>();
			company1.GC_Name = "company 1";
			company2.GC_Name = "company 2";
			company3.GC_Name = "company 3";
			MergeOrgHeader testMergeOrg = NewTestMergeOrgHeader("DEMORG");
			AssertNoErrors(testMergeOrg.NewOrganisationPkInfo);
			testMergeOrg.RunPreSaveValidation();

			OrgHeader oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgCompanyData data1 = Factory.New<OrgCompanyData>();
			data1.OB_OH = oldOrg.PK;
			data1.OB_GC = company1.PK;
			data1.OB_IsCreditor = true;
			data1.OB_IsDebtor = true;
			OrgCompanyData data2 = Factory.New<OrgCompanyData>();
			data2.OB_OH = oldOrg.PK;
			data2.OB_GC = company2.PK;
			data2.OB_IsCreditor = true;
			data2.OB_IsDebtor = true;
			testMergeOrg = new MergeOrgHeader(Factory, oldOrg);

			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgCompanyData data3 = Factory.New<OrgCompanyData>();
			data3.OB_OH = newOrg.PK;
			data3.OB_GC = company3.PK;
			data3.OB_IsDebtor = true;
			data3.OB_IsCreditor = true;
			OrgCompanyData data4 = Factory.New<OrgCompanyData>();
			data4.OB_OH = newOrg.PK;
			data4.OB_GC = company2.PK;
			data4.OB_IsDebtor = true;
			testMergeOrg.NewOrganisationPk = newOrg.PK;
			return testMergeOrg;
		}

		MergeOrgHeader SetupForARAPMerge_NoControllingBranchOnNewCompany()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();
			GlbCompany company2 = Factory.New<GlbCompany>();
			GlbCompany company3 = Factory.New<GlbCompany>();
			company1.GC_Name = "company 1";
			company1.GC_Code = "C1";
			company2.GC_Name = "company 2";
			company2.GC_Code = "C2";
			company3.GC_Name = "company 3";
			company3.GC_Code = "C3";
			GlbBranch branch1 = Factory.New<GlbBranch>();
			branch1.GB_BranchName = "branch 1";
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "B1";
			GlbBranch branch2 = Factory.New<GlbBranch>();
			branch2.GB_BranchName = "branch 2";
			branch2.GB_GC = company2.PK;
			branch2.GB_Code = "B2";
			GlbBranch branch3 = Factory.New<GlbBranch>();
			branch3.GB_BranchName = "branch 3";
			branch3.GB_GC = company3.PK;
			branch3.GB_Code = "B3";
			MergeOrgHeader testMergeOrg = NewTestMergeOrgHeader("DEMORG");
			AssertNoErrors(testMergeOrg.NewOrganisationPkInfo);
			testMergeOrg.RunPreSaveValidation();

			OrgHeader oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgCompanyData data1 = Factory.New<OrgCompanyData>();
			data1.OB_GC = company1.PK;
			data1.OB_OH = oldOrg.PK;
			data1.OB_IsCreditor = true;
			data1.OB_IsDebtor = true;

			OrgCompanyData data2 = Factory.New<OrgCompanyData>();
			data2.OB_GC = company2.PK;
			data2.OB_OH = oldOrg.PK;
			data2.OB_IsCreditor = true;
			data2.OB_IsDebtor = true;

			OrgCompanyData data3 = Factory.New<OrgCompanyData>();
			data3.OB_GC = company3.PK;
			data3.OB_OH = oldOrg.PK;

			data1.OB_GB_ControllingBranch = branch1.PK;
			data2.OB_GB_ControllingBranch = branch2.PK;
			testMergeOrg = new MergeOrgHeader(Factory, oldOrg);

			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgCompanyData data4 = Factory.New<OrgCompanyData>();
			data4.OB_GC = company2.PK;
			data4.OB_OH = newOrg.PK;
			data4.OB_IsDebtor = true;
			data4.OB_IsCreditor = true;

			OrgCompanyData data5 = Factory.New<OrgCompanyData>();
			data5.OB_GC = company3.PK;
			data5.OB_OH = newOrg.PK;
			data5.OB_IsDebtor = true;

			data4.OB_GB_ControllingBranch = ZGuid.Empty;
			data5.OB_GB_ControllingBranch = ZGuid.Empty;
			testMergeOrg.NewOrganisationPk = newOrg.PK;
			return testMergeOrg;
		}

		MergeOrgHeader SetupForARAPMerge_OldOrgHasLessCompaniesThanNewOrg()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();
			GlbCompany company2 = Factory.New<GlbCompany>();
			GlbCompany company3 = Factory.New<GlbCompany>();
			company1.GC_Name = "company 1";
			company1.GC_Code = "C1";
			company2.GC_Name = "company 2";
			company2.GC_Code = "C2";
			company3.GC_Name = "company 3";
			company3.GC_Code = "C3";
			GlbBranch branch1 = Factory.New<GlbBranch>();
			branch1.GB_BranchName = "branch 1";
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "B1";
			GlbBranch branch2 = Factory.New<GlbBranch>();
			branch2.GB_BranchName = "branch 2";
			branch2.GB_GC = company2.PK;
			branch2.GB_Code = "B2";
			GlbBranch branch3 = Factory.New<GlbBranch>();
			branch3.GB_BranchName = "branch 3";
			branch3.GB_GC = company3.PK;
			branch3.GB_Code = "B3";
			MergeOrgHeader testMergeOrg = NewTestMergeOrgHeader("DEMORG");
			AssertNoErrors(testMergeOrg.NewOrganisationPkInfo);
			testMergeOrg.RunPreSaveValidation();

			OrgHeader oldOrg = Factory.NewWithValidTestData<OrgHeader>();

			OrgCompanyData data2 = Factory.New<OrgCompanyData>();
			data2.OB_GC = company2.PK;
			data2.OB_OH = oldOrg.PK;
			data2.OB_IsCreditor = true;
			data2.OB_IsDebtor = true;

			OrgCompanyData data3 = Factory.New<OrgCompanyData>();
			data3.OB_GC = company3.PK;
			data3.OB_OH = oldOrg.PK;
			data3.OB_IsCreditor = true;
			data3.OB_IsDebtor = true;

			data2.OB_GB_ControllingBranch = branch2.PK;
			data3.OB_GB_ControllingBranch = branch3.PK;

			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgCompanyData data4 = Factory.New<OrgCompanyData>();
			data4.OB_GC = company1.PK;
			data4.OB_OH = newOrg.PK;
			data4.OB_IsDebtor = true;
			data4.OB_IsCreditor = true;

			OrgCompanyData data5 = Factory.New<OrgCompanyData>();
			data5.OB_GC = company2.PK;
			data5.OB_OH = newOrg.PK;
			data5.OB_IsDebtor = true;
			data5.OB_IsCreditor = true;

			OrgCompanyData data6 = Factory.New<OrgCompanyData>();
			data6.OB_GC = company3.PK;
			data6.OB_OH = newOrg.PK;
			data6.OB_IsDebtor = true;
			data6.OB_IsCreditor = true;

			data4.OB_GB_ControllingBranch = branch1.PK;
			data5.OB_GB_ControllingBranch = branch2.PK;
			data6.OB_GB_ControllingBranch = branch3.PK;

			testMergeOrg = new MergeOrgHeader(Factory, oldOrg);
			testMergeOrg.NewOrganisationPk = newOrg.PK;
			return testMergeOrg;
		}

		public void TestProperties()
		{
			MergeOrgHeader test1 = new MergeOrgHeader(Factory, null);
			AssertNotNull(test1);
			AssertNull(test1.OldOrganisation);
			AssertNull(test1.NewOrganisation);
			AssertEquals("New Organisation PK should be empty GUID", ZGuid.Empty, test1.NewOrganisationPk);

			OrgHeader org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			test1.OldOrganisation = org1;
			AssertEquals(org1, test1.OldOrganisation);

			OrgHeader org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			test1.NewOrganisationPk = org2.PK;
			AssertEquals(org2, test1.NewOrganisation);
		}

		public void TestSettingOldOrNewOrgSetupsCollections()
		{
			OrgHeader org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			MergeOrgHeaderForTest test1 = new MergeOrgHeaderForTest(Factory, null);
			Assert(!test1.CollectionsAreSet);
			MergeOrgHeaderForTest test2 = new MergeOrgHeaderForTest(Factory, org1);
			Assert(!test2.CollectionsAreSet);
			MergeOrgHeaderForTest test3 = new MergeOrgHeaderForTest(Factory, null, org2);
			Assert(!test3.CollectionsAreSet);
			MergeOrgHeaderForTest test4 = new MergeOrgHeaderForTest(Factory, org1, org2);
			Assert(test4.CollectionsAreSet);
			MergeOrgHeaderForTest test5 = new MergeOrgHeaderForTest(Factory, null);
			test5.OldOrganisation = org1;
			Assert(!test5.CollectionsAreSet);
			MergeOrgHeaderForTest test6 = new MergeOrgHeaderForTest(Factory, null);
			test6.NewOrganisationPk = org1.PK;
			Assert(!test6.CollectionsAreSet);
			MergeOrgHeaderForTest test7 = new MergeOrgHeaderForTest(Factory, null);
			test7.OldOrganisation = org1;
			test7.NewOrganisationPk = org2.PK;
			Assert(test7.CollectionsAreSet);
		}

		[ExpectNoExceptions()]
		public void TestNoExceptionsIfOldOrgNotSet()
		{
			MergeOrgHeaderForTest test1 = new MergeOrgHeaderForTest(Factory, null);
			AssertNull(test1.OldOrgContactCollection);
			AssertNull(test1.OldOrgAddressesCollection);
			AssertEquals(ZString.Empty, test1.OldOrganisationCode);
			AssertEquals(ZString.Empty, test1.OldOrganisationName);
			test1.ValidateNewOrganisationPk();
			test1.ValidateOrgFlags();
		}

		public void TestCollectionsByNameAndCode()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "MATCHING ORGANISATION";
			org1.OH_Code = "MUHAHA";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "NOT MATCHING ORGANISATION";
			org2.OH_Code = "MUZZHA";
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "MATCHING ORGANISATION";
			org3.OH_Code = "MU22HA";
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_FullName = "MATCHING ORGANISATION";
			org4.OH_Code = "MUHA23";
			OrgHeader org5 = Factory.NewWithValidTestData<OrgHeader>();
			org5.OH_FullName = "CODE MATCHING ORGANISATION 1";
			org5.OH_Code = "MUHAHA11";
			OrgHeader org6 = Factory.NewWithValidTestData<OrgHeader>();
			org6.OH_FullName = "CODE MATCHING ORGANISATION 2";
			org6.OH_Code = "MUHAHA33";

			Factory.Save();

			MergeOrgHeader merge = new MergeOrgHeader(Factory, null, org1);
			merge.HasToLoadSimilarOrgs = true;
			AssertEquals("There should be 2 orgs by names", 2, merge.OldOrgsCollectionByName.Count);
			AssertCollectionContains("There should be 2 orgs by names", org3, merge.OldOrgsCollectionByName);
			AssertCollectionContains("There should be 2 orgs by names", org4, merge.OldOrgsCollectionByName);
			AssertEquals("There should be 2 orgs by codes", 2, merge.OldOrgsCollectionByCode.Count);
			AssertCollectionContains("There should be 2 orgs by codes", org5, merge.OldOrgsCollectionByCode);
			AssertCollectionContains("There should be 2 orgs by codes", org6, merge.OldOrgsCollectionByCode);
		}

		public void TestCollectionsByPattern()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "full name";
			org1.MainAddress.OA_Address1 = "aaa";
			org1.MainAddress.OA_PostCode = "123";
			org1.MainAddress.OA_City = "zzz";
			org1.MainAddress.OA_State = "xxx";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "full house";
			org2.MainAddress.OA_Address1 = "bbb";
			org2.MainAddress.OA_PostCode = "123";
			org2.MainAddress.OA_City = "zzz";
			org2.MainAddress.OA_State = "xxx";
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "full 30 litre";
			org3.MainAddress.OA_Address1 = "aaa";
			org3.MainAddress.OA_PostCode = "123";
			org3.MainAddress.OA_City = "zzz";
			org3.MainAddress.OA_State = "xxx";
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_FullName = "full name again";
			org4.MainAddress.OA_Address1 = "a1c2c4f5";
			org4.MainAddress.OA_PostCode = "123";
			org4.MainAddress.OA_City = "zzz";
			org4.MainAddress.OA_State = "xxx";

			Factory.Save();

			MergeOrgHeader merge = new MergeOrgHeader(Factory, null, org1);
			merge.CurrentMatchThresholdCode = OrgMatchThresholds.Descriptions.Medium;
			merge.HasToLoadSimilarOrgs = true;
			AssertEquals("There should be 1 org by pattern", 1, merge.OldOrgsCollectionByPattern.Count);
			AssertCollectionContains("There should be 1 org by pattern", org3, merge.OldOrgsCollectionByPattern);
		}

		public void TestGetOldOrganisationCodes()
		{
			OrgHeaderCollection col = new OrgHeaderCollection(Factory);
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "DAVID BLAINE LTD";
			org1.OH_Code = "DAVID";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "TEDDY BEAR & CO";
			org2.OH_Code = "TEDDY";
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "ORANGE SODA LLC";
			org3.OH_Code = "ORANGE";
			col.Add(org1);
			col.Add(org2);
			col.Add(org3);
			StringCollectionX assertCol = new StringCollectionX();
			assertCol.Add("DAVID");
			assertCol.Add("TEDDY");
			assertCol.Add("ORANGE");

			MergeOrgHeader merge = new MergeOrgHeader(Factory, null);
			AssertEquals(assertCol.Count, merge.GetOldOrganisationCodes(col).Count);
			Array.ForEach(assertCol.ToArray(), (string s) =>
			{
				AssertCollectionContains(s, merge.GetOldOrganisationCodes(col));
			});
		}

		public void TestCurrentMode()
		{
			MergeOrgHeader test1 = new MergeOrgHeader(Factory, null);
			AssertEquals("Precondition:", CurrentQueryMode.Name, test1.CurrentMode);

			OrgHeader org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			MergeOrgHeaderForTest test2 = new MergeOrgHeaderForTest(Factory, org1, org2);
			AssertEquals("Precondition:", CurrentQueryMode.Name, test2.CurrentMode);

			test2.OldOrgContactCollectionForSimilarOrgsByCode.RemoveAll();
			test2.OldOrgContactCollectionForSimilarOrgsByName.RemoveAll();
			test2.OldOrgAddressCollectionForSimilarOrgsByCode.RemoveAll();
			test2.OldOrgAddressCollectionForSimilarOrgsByName.RemoveAll();

			test2.OldOrgContactCollectionForSimilarOrgsByCode.Add(new MergeOrgContact(Factory, null, null));
			test2.OldOrgContactCollectionForSimilarOrgsByCode.Add(new MergeOrgContact(Factory, null, null));
			test2.OldOrgContactCollectionForSimilarOrgsByName.Add(new MergeOrgContact(Factory, null, null));
			test2.OldOrgAddressCollectionForSimilarOrgsByCode.Add(new MergeOrgAddress(Factory, null, null));
			test2.OldOrgAddressCollectionForSimilarOrgsByCode.Add(new MergeOrgAddress(Factory, null, null));
			test2.OldOrgAddressCollectionForSimilarOrgsByName.Add(new MergeOrgAddress(Factory, null, null));

			AssertEquals("there should be 1 record because current is a query by name collection", 1, test2.CurrentOrgAddressCollection.Count);
			AssertEquals("there should be 1 record because current is a query by name collection", 1, test2.CurrentOrgContactCollection.Count);

			test2.CurrentMode = CurrentQueryMode.Code;

			AssertEquals("there should be 2 records because current is a query by code collection", 2, test2.CurrentOrgAddressCollection.Count);
			AssertEquals("there should be 2 records because current is a query by code collection", 2, test2.CurrentOrgContactCollection.Count);

			test2.CurrentMode = CurrentQueryMode.Pattern;

			AssertEquals("there should be 0 records because current is a query by pattern collection", 0, test2.CurrentOrgAddressCollection.Count);
			AssertEquals("there should be 0 records because current is a query by pattern collection", 0, test2.CurrentOrgContactCollection.Count);
		}

		public void TestValidateOldOrganisations()
		{
			OrgHeader org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			MergeOrgHeaderForTest test1 = new MergeOrgHeaderForTest(Factory, org1, org2);
			AssertEquals("Precondition:", CurrentQueryMode.Name, test1.CurrentMode);

			test1.OldOrgContactCollectionForSimilarOrgsByCode.RemoveAll();
			test1.OldOrgContactCollectionForSimilarOrgsByName.RemoveAll();
			test1.OldOrgAddressCollectionForSimilarOrgsByCode.RemoveAll();
			test1.OldOrgAddressCollectionForSimilarOrgsByName.RemoveAll();

			OrgAddress adrObj1 = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress adrObj2 = Factory.NewWithValidTestData<OrgAddress>();
			adrObj1.OA_Address1 = "street magic";
			adrObj1.OA_City = "NY";
			adrObj2.OA_Address1 = "no street magic";
			adrObj2.OA_City = "LON";

			MergeOrgAddress adr1 = new MergeOrgAddress(Factory, adrObj1, null);
			MergeOrgAddress adr2 = new MergeOrgAddress(Factory, adrObj2, null);

			OrgContact cntObj1 = Factory.NewWithValidTestData<OrgContact>();
			OrgContact cntObj2 = Factory.NewWithValidTestData<OrgContact>();
			cntObj1.OC_ContactName = "abu";
			cntObj2.OC_ContactName = "jafar";

			MergeOrgContact cnt1 = new MergeOrgContact(Factory, cntObj1, null);
			MergeOrgContact cnt2 = new MergeOrgContact(Factory, cntObj2, null);
			test1.OldOrgContactCollectionForSimilarOrgsByName.Add(cnt1);
			test1.OldOrgContactCollectionForSimilarOrgsByName.Add(cnt2);
			test1.OldOrgAddressCollectionForSimilarOrgsByName.Add(adr1);
			test1.OldOrgAddressCollectionForSimilarOrgsByName.Add(adr2);

			test1.ValidateOldOrganisations();
			AssertNoErrors(cnt1);
			AssertNoErrors(cnt2);
			AssertNoErrors(adr1);
			AssertNoErrors(adr2);

			OrgAddress adrObj3 = Factory.NewWithValidTestData<OrgAddress>();
			adrObj3.OA_Address1 = "street magic";
			adrObj3.OA_City = "NY";
			MergeOrgAddress adr3 = new MergeOrgAddress(Factory, adrObj3, null);

			OrgContact cntObj3 = Factory.NewWithValidTestData<OrgContact>();
			cntObj3.OC_ContactName = "jafar";
			MergeOrgContact cnt3 = new MergeOrgContact(Factory, cntObj3, null);

			test1.OldOrgContactCollectionForSimilarOrgsByName.Add(cnt3);
			test1.OldOrgAddressCollectionForSimilarOrgsByName.Add(adr3);

			test1.ValidateOldOrganisations();

			AssertNoErrors(cnt1);
			AssertHasErrors(cnt2.ActionInfo);
			AssertHasErrors(cnt3.ActionInfo);
			AssertHasErrors(adr1.ActionInfo);
			AssertNoErrors(adr2);
			AssertHasErrors(adr3.ActionInfo);
		}

		public void TestOldOrgsCollection_CountChanged()
		{
			OrgHeader org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			MergeOrgHeaderForTest test1 = new MergeOrgHeaderForTest(Factory, org1, org2);
			test1.HasToLoadSimilarOrgs = true;
			test1.OldOrgsCollectionByName.Add(org1);
			int count = test1.OldOrgAddressCollectionForSimilarOrgsByName.Count;

			OrgHeader org3 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));
			test1.OldOrgsCollectionByName.Add(org3);

			AssertEquals(count + org3.Addresses.Count, test1.OldOrgAddressCollectionForSimilarOrgsByName.Count);
		}

		public void TestDeletingOrganizationRemovesAddressesAndContacts()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "My Test 1";
			org1.MainAddress.OA_Address1 = "aaa";
			org1.MainAddress.OA_PostCode = "123";
			org1.MainAddress.OA_City = "zzz";
			org1.MainAddress.OA_State = "xxx";

			OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Bob";
			contact1.OC_OH = org1.PK;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "My Test 2";
			org2.MainAddress.OA_Address1 = "bbb";
			org2.MainAddress.OA_PostCode = "123";
			org2.MainAddress.OA_City = "zzz";
			org2.MainAddress.OA_State = "xxx";

			OrgContact contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Bill";
			contact2.OC_OH = org2.PK;

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "My Test 3";
			org3.MainAddress.OA_Address1 = "bbb2";
			org3.MainAddress.OA_PostCode = "1234";
			org3.MainAddress.OA_City = "zzz2";
			org3.MainAddress.OA_State = "xxx2";
			OrgContact contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "Jim";
			contact3.OC_OH = org3.PK;

			org1.OH_Code = "~test1~";
			org2.OH_Code = "~test2~";
			org3.OH_Code = "~test3~";

			MergeOrgHeaderForTest test1 = new MergeOrgHeaderForTest(Factory, org1, org2);
			test1.HasToLoadSimilarOrgs = true;
			test1.CurrentOrgHeaderCollection.Add(org2);
			test1.CurrentOrgHeaderCollection.Add(org3);
			AssertEquals(org2.Addresses.Count + org3.Addresses.Count, test1.CurrentOrgAddressCollection.Count);
			AssertEquals(org2.Contacts.Count + org3.Contacts.Count, test1.CurrentOrgContactCollection.Count);
			org2.Delete();
			org3.Delete();
			AssertEquals(0, test1.CurrentOrgAddressCollection.Count);
			AssertEquals(0, test1.CurrentOrgContactCollection.Count);
		}

		public void TestSetOrgsCollectionByPatterns()
		{
			OrgHeader org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			MergeOrgHeaderForTest test1 = new MergeOrgHeaderForTest(Factory, org1, org2);
			test1.CurrentMode = CurrentQueryMode.Pattern;

			AssertEquals("shoulnt be called yet", 0, test1.SetPatternsCount);

			test1.HasToLoadSimilarOrgs = true;
			BusinessObjectCollection col = test1.OldOrgsCollectionByPattern;
			AssertEquals("called when OldOrgsCollectionByPattern is created", 1, test1.SetPatternsCount);

			col = test1.OldOrgsCollectionByPattern;
			AssertEquals("called only once", 1, test1.SetPatternsCount);

			test1.CurrentMatchThresholdCode = "XXX";
			AssertEquals("called only once", 1, test1.SetPatternsCount);

			test1.PostChanges(false);
			AssertEquals("called when CurrentMatchThresholdCode is changed", 2, test1.SetPatternsCount);

			test1.CurrentMatchThresholdCode = "XXX";
			test1.PostChanges(false);
			AssertEquals("called when CurrentMatchThresholdCode is changed", 2, test1.SetPatternsCount);

			test1.CurrentMatchThresholdCode = "YYY";
			test1.PostChanges(false);
			AssertEquals("called each time when CurrentMatchThresholdCode is changed", 3, test1.SetPatternsCount);

			test1.MaxResults = 123;
			AssertEquals("changes not posted", 3, test1.SetPatternsCount);

			test1.PostChanges(false);
			AssertEquals("called when MaxResults is changed", 4, test1.SetPatternsCount);

			test1.MaxResults = 123;
			test1.PostChanges(false);
			AssertEquals("called when MaxResults is changed", 4, test1.SetPatternsCount);

			test1.MaxResults = 555;
			test1.PostChanges(false);
			AssertEquals("called each time when MaxResults is changed", 5, test1.SetPatternsCount);

			test1.PostChanges(true);
			AssertEquals("called when collection is changed", 6, test1.SetPatternsCount);
		}

		public void TestMaxResults()
		{
			OrgHeader org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			MergeOrgHeaderForTest test1 = new MergeOrgHeaderForTest(Factory, org1, org2);

			AssertEquals("Precondition:", 100, test1.MaxResults.ToZInt());
		}

		public void TestMatchThresholdsList()
		{
			MergeOrgHeader test1 = new MergeOrgHeader(Factory, null);
			Assert(test1.MatchThresholdsList.ContainsCode(OrgMatchThresholds.Codes.Low));
			Assert(test1.MatchThresholdsList.ContainsCode(OrgMatchThresholds.Codes.Medium));
			Assert(test1.MatchThresholdsList.ContainsCode(OrgMatchThresholds.Codes.High));
			Assert(test1.MatchThresholdsList.ContainsCode(OrgMatchThresholds.Codes.Extreme));
			AssertEquals(4, test1.MatchThresholdsList.Count);
		}

		public void TestAdditionalMessages()
		{
			MergeOrgHeader test1 = new MergeOrgHeader(Factory, null);
			AssertEquals("Precondition: Additional Messages should be empty", "", test1.AdditionalMessages);

			test1.MergedOrders.Add(new string[] { "org1", "xxx", "zzz" });
			test1.MergedOrders.Add(new string[] { "org1", "www", "123" });
			test1.MergedOrders.Add(new string[] { "org2", "678", "er4" });

			AssertEquals("\n\rThe following order numbers have been changed as part of the merge:\n\rorg1's Order xxx changed to zzz by merge\n\rorg1's Order www changed to 123 by merge\n\rorg2's Order 678 changed to er4 by merge", test1.AdditionalMessages);
		}

		public void TestNewOrganisationCode()
		{
			var mergeOrgHeader = new MergeOrgHeader(Factory, null);
			AssertEquals("Precondition", ZString.Empty, mergeOrgHeader.NewOrganisationCode);

			mergeOrgHeader.NewOrganisationPk = ZGuid.Invalid;
			AssertEquals(ZString.Empty, mergeOrgHeader.NewOrganisationCode);

			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_Code = "TESTORG";
			mergeOrgHeader.NewOrganisationPk = newOrg.PK;
			AssertEquals("TESTORG", mergeOrgHeader.NewOrganisationCode);
		}

		[ExpectNoExceptions]
		public void TestValiddateOrgFlagsDoesNotThrowShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();

			MergeOrgHeader test1 = new MergeOrgHeader(Factory, org1, org2);

			org1.Delete();

			test1.ValidateOrgFlags();
		}

		[ExpectNoExceptions]
		public void TestCheckARAPInAllCompanies_MultipleOrgCompanyDatasWithSameCompany_NoExceptionThrown()
		{
			// Arrange
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var oldCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			oldCompanyData.OB_GC = company1.PK;
			oldCompanyData.OB_OH = orgHeader.PK;
			oldCompanyData.OB_IsCreditor = true;
			oldCompanyData.OB_IsDebtor = false;
			var newOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = Factory.New<OrgCompanyData>();
			companyData.OB_GC = company1.PK;
			companyData.OB_OH = newOrgHeader.PK;
			companyData.OB_IsCreditor = true;
			companyData.OB_IsDebtor = false;
			companyData.OB_SystemLastEditTimeUtc = ZDateTime.UtcToday;
			Factory.Save();
			var duplicateCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			duplicateCompanyData.OB_GC = company1.PK;
			duplicateCompanyData.OB_IsCreditor = false;
			duplicateCompanyData.OB_IsDebtor = true;
			duplicateCompanyData.OB_OH = newOrgHeader.PK;
			duplicateCompanyData.OB_SystemLastEditTimeUtc = ZDateTime.UtcToday.AddDays(-1);
			newOrgHeader.CompanyDataCollection.Add(companyData);
			newOrgHeader.CompanyDataCollection.Add(duplicateCompanyData);
			var originalAllowMergeIgnoringARAPValue = OrganisationRegistry.Instance.AllowMergeIgnoringARAP.Value;
			OrganisationRegistry.Instance.AllowMergeIgnoringARAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			// Act
			var mergeOrgHeader = new MergeOrgHeader(Factory, orgHeader, newOrgHeader);
			BusinessObjectFactory.SaveTogether(mergeOrgHeader.SaveFactories);
			// Assert
			AssertEquals(mergeOrgHeader.NewOrganisationPk, newOrgHeader.PK);
			AssertNoErrors(mergeOrgHeader.NewOrganisationPkInfo);

			OrganisationRegistry.Instance.AllowMergeIgnoringARAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalAllowMergeIgnoringARAPValue);
		}

		public void TestMergeValidationRule_DuplicateTargetField_BuyerIsOld()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = Factory.Load<OrgHeader>(oldClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = Factory.Load<OrgHeader>(newClientPK);
			Factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet2, "ANY", "PA1").InsertAndReturnObject(TestConnection);

			var merge = new MergeOrgHeaderForTest(new BusinessObjectFactory(), oldOrg, newOrg);
			merge.RunPreSaveValidation();
			AssertHasError(merge.NewOrganisationPkInfo, "The merging organizations have conflicting barcode validation rules. Please ensure these target fields are not used in more than one organization (rule set): PA1");
		}

		public void TestMergeValidationRule_DuplicateTargetField_SupplierIsOld()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = Factory.Load<OrgHeader>(oldClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = Factory.Load<OrgHeader>(newClientPK);
			Factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Supplier = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Supplier = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet2, "ANY", "PA1").InsertAndReturnObject(TestConnection);

			var merge = new MergeOrgHeaderForTest(new BusinessObjectFactory(), oldOrg, newOrg);
			merge.RunPreSaveValidation();
			AssertHasError(merge.NewOrganisationPkInfo, "The merging organizations have conflicting barcode validation rules. Please ensure these target fields are not used in more than one organization (rule set): PA1");
		}

		public void TestMergeValidationRule_DuplicateTargetField_MultipleFields()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = Factory.Load<OrgHeader>(oldClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = Factory.Load<OrgHeader>(newClientPK);
			Factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet1, "ANY", "PA2").InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeValidationRule(ruleSet1, "ANY", "PA3").InsertAndReturnObject(TestConnection);

			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule4 = new BarcodeValidationRule(ruleSet2, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var rule5 = new BarcodeValidationRule(ruleSet2, "ANY", "PA2").InsertAndReturnObject(TestConnection);
			var rule6 = new BarcodeValidationRule(ruleSet2, "ANY", "PA3").InsertAndReturnObject(TestConnection);

			var merge = new MergeOrgHeaderForTest(new BusinessObjectFactory(), oldOrg, newOrg);
			merge.RunPreSaveValidation();
			AssertHasError(merge.NewOrganisationPkInfo, "The merging organizations have conflicting barcode validation rules. Please ensure these target fields are not used in more than one organization (rule set): PA1, PA2, PA3");
		}

		public void TestMergeValidationRule_DuplicateTargetField_OnlyDuplicateFields()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = Factory.Load<OrgHeader>(oldClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = Factory.Load<OrgHeader>(newClientPK);
			Factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet1, "ANY", "PA2").InsertAndReturnObject(TestConnection);

			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule5 = new BarcodeValidationRule(ruleSet2, "ANY", "PA2").InsertAndReturnObject(TestConnection);
			var rule6 = new BarcodeValidationRule(ruleSet2, "ANY", "PA3").InsertAndReturnObject(TestConnection);

			var merge = new MergeOrgHeaderForTest(new BusinessObjectFactory(), oldOrg, newOrg);
			merge.RunPreSaveValidation();
			AssertHasError(merge.NewOrganisationPkInfo, "The merging organizations have conflicting barcode validation rules. Please ensure these target fields are not used in more than one organization (rule set): PA2");
		}

		public void TestMergeValidationRule_NoDuplication()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = Factory.Load<OrgHeader>(oldClientPK);
			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = Factory.Load<OrgHeader>(newClientPK);
			Factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet2, "ANY", "PA2").InsertAndReturnObject(TestConnection);

			var merge = new MergeOrgHeaderForTest(new BusinessObjectFactory(), oldOrg, newOrg);
			merge.RunPreSaveValidation();
			AssertNoErrors(merge.NewOrganisationPkInfo);
		}

		public void TestMergeValidationRule_MultipleOrganisations_DuplicateBetweenOldOrganisations()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var oldClientPK1 = helper.CreateClient("TESTORG1");
			var oldOrg1 = Factory.Load<OrgHeader>(oldClientPK1);

			var oldClientPK2 = helper.CreateClient("TESTORG2");
			var oldOrg2 = Factory.Load<OrgHeader>(oldClientPK2);

			var newClientPK = helper.CreateClient("TESTORG3");
			var newOrg = Factory.Load<OrgHeader>(newClientPK);
			Factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK1.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK2.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet2, "ANY", "PA1").InsertAndReturnObject(TestConnection);

			var merge = new MergeOrgHeaderForTest(new BusinessObjectFactory(), null, newOrg);
			merge.HasToLoadSimilarOrgs = true;
			merge.CurrentOrgHeaderCollection.Add(oldOrg1);
			merge.CurrentOrgHeaderCollection.Add(oldOrg2);
			merge.RunPreSaveValidation();

			AssertHasError(merge.NewOrganisationPkInfo, "The merging organizations have conflicting barcode validation rules. Please ensure these target fields are not used in more than one organization (rule set): PA1");
		}

		public void TestMergeValidationRule_MultipleOrganisations_DuplicateBetweenNewAndOld()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var oldClientPK1 = helper.CreateClient("TESTORG1");
			var oldOrg1 = Factory.Load<OrgHeader>(oldClientPK1);

			var oldClientPK2 = helper.CreateClient("TESTORG2");
			var oldOrg2 = Factory.Load<OrgHeader>(oldClientPK2);

			var newClientPK = helper.CreateClient("TESTORG3");
			var newOrg = Factory.Load<OrgHeader>(newClientPK);
			Factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK1.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet2, "ANY", "PA1").InsertAndReturnObject(TestConnection);

			var merge = new MergeOrgHeaderForTest(new BusinessObjectFactory(), null, newOrg);
			merge.HasToLoadSimilarOrgs = true;
			merge.CurrentOrgHeaderCollection.Add(oldOrg1);
			merge.CurrentOrgHeaderCollection.Add(oldOrg2);
			merge.RunPreSaveValidation();

			AssertHasError(merge.NewOrganisationPkInfo, "The merging organizations have conflicting barcode validation rules. Please ensure these target fields are not used in more than one organization (rule set): PA1");
		}

		public void TestMergeValidationRule_MultipleOrganisations_MultipleFields()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var oldClientPK1 = helper.CreateClient("TESTORG1");
			var oldOrg1 = Factory.Load<OrgHeader>(oldClientPK1);

			var oldClientPK2 = helper.CreateClient("TESTORG2");
			var oldOrg2 = Factory.Load<OrgHeader>(oldClientPK2);

			var newClientPK = helper.CreateClient("TESTORG3");
			var newOrg = Factory.Load<OrgHeader>(newClientPK);
			Factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK1.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet1, "ANY", "PA2").InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeValidationRule(ruleSet1, "ANY", "PA3").InsertAndReturnObject(TestConnection);

			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule4 = new BarcodeValidationRule(ruleSet2, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var rule5 = new BarcodeValidationRule(ruleSet2, "ANY", "PA2").InsertAndReturnObject(TestConnection);
			var rule6 = new BarcodeValidationRule(ruleSet2, "ANY", "PA3").InsertAndReturnObject(TestConnection);

			var merge = new MergeOrgHeaderForTest(new BusinessObjectFactory(), null, newOrg);
			merge.HasToLoadSimilarOrgs = true;
			merge.CurrentOrgHeaderCollection.Add(oldOrg1);
			merge.CurrentOrgHeaderCollection.Add(oldOrg2);
			merge.RunPreSaveValidation();

			AssertHasError(merge.NewOrganisationPkInfo, "The merging organizations have conflicting barcode validation rules. Please ensure these target fields are not used in more than one organization (rule set): PA1, PA2, PA3");
		}

		public void TestMergeValidationRule_MultipleOrganisations_OnlyDuplicateFields()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var oldClientPK1 = helper.CreateClient("TESTORG1");
			var oldOrg1 = Factory.Load<OrgHeader>(oldClientPK1);

			var oldClientPK2 = helper.CreateClient("TESTORG2");
			var oldOrg2 = Factory.Load<OrgHeader>(oldClientPK2);

			var newClientPK = helper.CreateClient("TESTORG3");
			var newOrg = Factory.Load<OrgHeader>(newClientPK);
			Factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK1.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet1, "ANY", "PA2").InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeValidationRule(ruleSet1, "ANY", "PA3").InsertAndReturnObject(TestConnection);

			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule4 = new BarcodeValidationRule(ruleSet2, "ANY", "PRC").InsertAndReturnObject(TestConnection);
			var rule5 = new BarcodeValidationRule(ruleSet2, "ANY", "PA2").InsertAndReturnObject(TestConnection);
			var rule6 = new BarcodeValidationRule(ruleSet2, "ANY", "PA3").InsertAndReturnObject(TestConnection);

			var merge = new MergeOrgHeaderForTest(new BusinessObjectFactory(), null, newOrg);
			merge.HasToLoadSimilarOrgs = true;
			merge.CurrentOrgHeaderCollection.Add(oldOrg1);
			merge.CurrentOrgHeaderCollection.Add(oldOrg2);
			merge.RunPreSaveValidation();

			AssertHasError(merge.NewOrganisationPkInfo, "The merging organizations have conflicting barcode validation rules. Please ensure these target fields are not used in more than one organization (rule set): PA2, PA3");
		}

		public void TestMergeValidationRule_MultipleOrganisations_NoDuplication()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var oldClientPK1 = helper.CreateClient("TESTORG1");
			var oldOrg1 = Factory.Load<OrgHeader>(oldClientPK1);

			var oldClientPK2 = helper.CreateClient("TESTORG2");
			var oldOrg2 = Factory.Load<OrgHeader>(oldClientPK2);

			var newClientPK = helper.CreateClient("TESTORG3");
			var newOrg = Factory.Load<OrgHeader>(newClientPK);
			Factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK1.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK2.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet2, "ANY", "PA2").InsertAndReturnObject(TestConnection);
			var ruleSet3 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeValidationRule(ruleSet3, "ANY", "PA3").InsertAndReturnObject(TestConnection);

			var merge = new MergeOrgHeaderForTest(new BusinessObjectFactory(), null, newOrg);
			merge.HasToLoadSimilarOrgs = true;
			merge.CurrentOrgHeaderCollection.Add(oldOrg1);
			merge.CurrentOrgHeaderCollection.Add(oldOrg2);
			merge.RunPreSaveValidation();

			AssertNoErrors(merge.NewOrganisationPkInfo);
		}

		protected override BusinessObject GetNewBusinessObject() => NewTestMergeOrgHeader("DEMORG");

		MergeOrgHeader NewTestMergeOrgHeader(ZString oldOrgCode)
		{
			var oldOrg = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, oldOrgCode);
			return new MergeOrgHeader(Factory, oldOrg);
		}

		public class MergeOrgHeaderForTest : MergeOrgHeader
		{
			public MergeOrgHeaderForTest(BusinessObjectFactory factory, OrgHeader oldOrg)
				: base(factory, oldOrg)
			{ }

			public MergeOrgHeaderForTest(BusinessObjectFactory factory, OrgHeader oldOrg, OrgHeader newOrg)
				: base(factory, oldOrg, newOrg)
			{
			}

			bool collectionsAreSet;

			public bool CollectionsAreSet
			{
				get { return collectionsAreSet; }
			}

			protected override void SetOldOrgAddressesAndContactsCollectionNewOrganisation()
			{
				base.SetOldOrgAddressesAndContactsCollectionNewOrganisation();
				collectionsAreSet = true;
			}

			public int SetPatternsCount;
			protected override void SetOrgsCollectionByPatternsCore()
			{
				base.SetOrgsCollectionByPatternsCore();
				SetPatternsCount++;
			}

			public void SetFANMapping(ZGuid companyPk, ZGuid orgPk, ZString fan)
			{
				CompanyPk = companyPk;
				OrgPk = orgPk;
				FAN = fan;
			}
			ZGuid CompanyPk;
			ZGuid OrgPk;
			ZString FAN;

			protected override (ZGuid companyPK, ZGuid orgPK, ZString fan)[] GetFANsForOrgs(ZGuid[] orgPks)
			{
				var companyOrgFans = new List<(ZGuid, ZGuid, ZString)>();

				if (orgPks.Contains(OrgPk))
				{
					var companyOrgFanTuple = (CompanyPk, OrgPk, FAN);
					companyOrgFans.Add(companyOrgFanTuple);
				}

				return companyOrgFans.ToArray();
			}

			public bool ThrowExceptionOnOrgDelete { get; set; }
			protected override OrganisationMerger GetOrganisationMerger()
			{
				return new OrganisationMergerForTest(this) { ThrowExceptionOnOrgDelete = ThrowExceptionOnOrgDelete };
			}
		}
	}
}
