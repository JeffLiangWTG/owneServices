using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgStaffAssignmentsCollection))]
	sealed class OrgStaffAssignmentsCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultValues()
		{
			OrgHeader org = OrgHeader.New(Factory);
			AssertEquals("Company set on staff assignment", GlbCompany.CurrentCompany.PK, org.StaffAssignments.AddNew().O8_GC);
			OrganisationsDataRegistry.Instance.NewStaffAssignmentsAsGlobal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Company set on staff assignment", ZGuid.Empty, org.StaffAssignments.AddNew().O8_GC);
		}

		public void TestLoadSetsReadOnly()
		{
			GlbStaff testStaff1 = Factory.NewWithValidTestData<GlbStaff>();

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgStaffAssignments testAss1 = testOrg.StaffAssignments.AddNew();
			OrgStaffAssignments testAss2 = testOrg.StaffAssignments.AddNew();

			testAss1.O8_GS_NKPersonResponsible = testStaff1.GS_Code;
			testAss2.O8_GS_NKPersonResponsible = testStaff1.GS_Code;
			testAss1.O8_Role = "SAL";
			testAss2.O8_Role = "SAL";
			testAss1.O8_GC = ZGuid.Empty;

			Factory.Save();

			bool previousValue = Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed = true;
				testOrg.StaffAssignments.Load();
				Assert(!testAss1.ReadOnly);
				Assert(!testAss2.ReadOnly);

				Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed = false;
				testOrg.StaffAssignments.Load();
				Assert(testAss1.ReadOnly);
				Assert(!testAss2.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed = previousValue;
			}
		}

		public void TestRemoveAndDelete()
		{
			GlbStaff testStaff1 = Factory.NewWithValidTestData<GlbStaff>();

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgStaffAssignments testAss1 = testOrg.StaffAssignments.AddNew();
			OrgStaffAssignments testAss2 = testOrg.StaffAssignments.AddNew();
			OrgStaffAssignments testAss3 = testOrg.StaffAssignments.AddNew();

			testAss1.O8_GS_NKPersonResponsible = testStaff1.GS_Code;
			testAss2.O8_GS_NKPersonResponsible = testStaff1.GS_Code;
			testAss3.O8_GS_NKPersonResponsible = testStaff1.GS_Code;
			testAss1.O8_Role = "SAL";
			testAss2.O8_Role = "SAL";
			testAss3.O8_Role = "SAL";
			testAss1.O8_GC = ZGuid.Empty;
			testAss3.O8_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;

			Factory.Save();

			bool previousValue = Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed = false;

				testOrg.StaffAssignments.RemoveAndDelete(testAss2);
				Assert(!testOrg.StaffAssignments.Contains(testAss2));
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				testOrg.StaffAssignments.RemoveAndDelete(testAss1);
				AssertEquals(Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(testOrg.StaffAssignments.Contains(testAss1));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				testOrg.StaffAssignments.RemoveAndDelete(testAss3);
				AssertEquals(Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(testOrg.StaffAssignments.Contains(testAss3));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed = true;
				testOrg.StaffAssignments.RemoveAndDelete(testAss1);
				testOrg.StaffAssignments.RemoveAndDelete(testAss3);
				Assert(!testOrg.StaffAssignments.Contains(testAss1));
				Assert(!testOrg.StaffAssignments.Contains(testAss3));
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
			finally
			{
				Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed = previousValue;
			}
		}

		public void TestRelationshipFilter()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "ZUB";

			var org = Factory.NewWithValidTestData<OrgHeader>();

			OrgStaffAssignments assign1 = Factory.New<OrgStaffAssignments>();
			assign1.O8_OH = org.PK;
			assign1.O8_GC = GlbCompany.CurrentCompany.PK;
			assign1.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;

			OrgStaffAssignments assign2 = Factory.New<OrgStaffAssignments>();
			assign2.O8_OH = org.PK;
			assign2.O8_GC = otherCompany.PK;
			assign2.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			OrgStaffAssignmentsCollection assigns1 = new OrgStaffAssignmentsCollection(org);
			Assert(assigns1.CompanySpecific);
			assigns1.Load();
			AssertCollectionContains("Only company specific items appear", assign1, assigns1);
			AssertCollectionNotContains("Other company item does not appear", assign2, assigns1);

			OrgStaffAssignmentsCollection nonCompanyAssigns = new OrgStaffAssignmentsCollection(org);
			nonCompanyAssigns.CompanySpecific = false;
			AssertCollectionContains("All items appear - company item", assign1, nonCompanyAssigns);
			AssertCollectionContains("All items appear - other company item", assign2, nonCompanyAssigns);
		}

		public void TestSettingOverallStaffRoles()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			OrgHeader org = Factory.New<OrgHeader>();

			org.StaffAssignments.OverallAccountManager = staff.GS_Code;
			AssertEquals("Should be Overall Account Manager", staff, org.StaffAssignments.OverallAccountManagerStaff);

			org.StaffAssignments.OverallAccountManager = ZString.Empty;
			AssertNull("Overall Account Manager Staff should be null", org.StaffAssignments.OverallAccountManagerStaff);

			org.StaffAssignments.OverallController = staff.GS_Code;
			AssertEquals("Should be Overall Controller", staff, org.StaffAssignments.OverallControllerStaff);

			org.StaffAssignments.OverallController = ZString.Empty;
			AssertNull("Overall Controller Staff should be null", org.StaffAssignments.OverallControllerStaff);

			org.StaffAssignments.OverallCustomerServiceRep = staff.GS_Code;
			AssertEquals("Should be Overall Customer Service Rep", staff, org.StaffAssignments.OverallCustomerServiceRepStaff);

			org.StaffAssignments.OverallCustomerServiceRep = ZString.Empty;
			AssertNull("Overall Customer Service Rep should be null", org.StaffAssignments.OverallCustomerServiceRepStaff);

			org.StaffAssignments.OverallSalesRep = staff.GS_Code;
			AssertEquals("Should be Overall Sales Rep", staff, org.StaffAssignments.OverallSalesRepStaff);

			org.StaffAssignments.OverallSalesRep = ZString.Empty;
			AssertNull("Overall Sales Rep should be null", org.StaffAssignments.OverallSalesRepStaff);
		}

		public void TestOverallRepSecurity()
		{
			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "RA";
			staff2.GS_LoginName = "RAKHSH";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.StaffAssignments.OverallSalesRep = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			bool originalSecurity = Env.Security.OrgDetailsModifyStaffAssignmentsLookup["SAL"].IsAllowed;
			Env.Security.OrgDetailsModifyStaffAssignmentsLookup["SAL"].IsAllowed = false;

			try
			{
				org.StaffAssignments.RemoveAndDelete(org.StaffAssignments[0]);
				AssertEquals("Not deleted due to no security to modify overall rep", false, org.StaffAssignments[0].IsDeleted);
				AssertEquals("Still in collection due to no security to modify overall rep", true, org.StaffAssignments.Contains(org.StaffAssignments[0]));

				org.StaffAssignments.AllowDeleteOfOverallRepRegardlessOfSecurity = true;

				OrgStaffAssignments staffAssign = org.StaffAssignments[0];
				org.StaffAssignments.RemoveAndDelete(staffAssign);
				AssertEquals("Deleted due to overriding security to modify overall rep", true, staffAssign.IsDeleted);
				AssertEquals("Not in collection due to overriding security to modify overall rep", false, org.StaffAssignments.Contains(staffAssign));

				org.StaffAssignments.OverallSalesRep = GlbStaff.CurrentUser.GS_Code;

				Env.Security.OrgDetailsModifyStaffAssignmentsLookup["SAL"].IsAllowed = true;
				org.StaffAssignments.AllowDeleteOfOverallRepRegardlessOfSecurity = false;

				staffAssign = org.StaffAssignments[0];
				org.StaffAssignments.RemoveAndDelete(staffAssign);
				AssertEquals("Deleted due to having full security to modify overall rep", true, staffAssign.IsDeleted);
				AssertEquals("Not in collection due to full security to modify overall rep", false, org.StaffAssignments.Contains(staffAssign));
			}
			finally
			{
				Env.Security.OrgDetailsModifyStaffAssignmentsLookup["SAL"].IsAllowed = originalSecurity;
			}
		}

		public void TestGetStaffAssignment()
		{
			OrgHeader org = OrgHeader.New(Factory);

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff4 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments staffAssign1 = org.StaffAssignments.AddNew();
			staffAssign1.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			staffAssign1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			staffAssign1.O8_Role = "MAN";

			OrgStaffAssignments staffAssign2 = org.StaffAssignments.AddNew();
			staffAssign2.O8_Department = OrgStaffAssignmentsLookups.SeaFreightServices;
			staffAssign2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			staffAssign2.O8_Role = "MAN";

			OrgStaffAssignments staffAssign3 = org.StaffAssignments.AddNew();
			staffAssign3.O8_Department = "FIA";
			staffAssign3.O8_GS_NKPersonResponsible = staff3.GS_Code;
			staffAssign3.O8_Role = "MAN";
			staffAssign3.O8_GC = ZGuid.Empty;

			OrgStaffAssignments staffAssign4 = org.StaffAssignments.AddNew();
			staffAssign4.O8_Department = "FIA";
			staffAssign4.O8_GS_NKPersonResponsible = staff4.GS_Code;
			staffAssign4.O8_Role = "MAN";

			OrgStaffAssignments staffAssign5 = org.StaffAssignments.AddNew();
			staffAssign5.O8_Department = "FRT";
			staffAssign5.O8_GS_NKPersonResponsible = staff3.GS_Code;
			staffAssign5.O8_Role = "MAN";
			staffAssign5.O8_GC = ZGuid.Empty;

			ZString returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Air);
			AssertEquals("Staff 4 returned", staffAssign4.O8_GS_NKPersonResponsible, returnedStaff);

			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Sea);
			AssertEquals("Staff 2 returned", staffAssign2.O8_GS_NKPersonResponsible, returnedStaff);

			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Road);
			AssertEquals("Staff 1 returned", staffAssign1.O8_GS_NKPersonResponsible, returnedStaff);

			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", "FIA");
			AssertEquals("Staff 4 returned", staffAssign4.O8_GS_NKPersonResponsible, returnedStaff);

			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", "FRT");
			AssertEquals("Staff 3 returned", staffAssign3.O8_GS_NKPersonResponsible, returnedStaff);

			GlbStaff staff6 = Factory.NewWithValidTestData<GlbStaff>();

			var customDepartment = Factory.New<GlbDepartment>();
			customDepartment.GE_Export = true;
			customDepartment.GE_Post = true;
			customDepartment.GE_InternationalFreight = true;
			customDepartment.GE_Code = "FEP";

			OrgStaffAssignments staffAssign6 = org.StaffAssignments.AddNew();
			staffAssign6.O8_Department = customDepartment.GE_Code;
			staffAssign6.O8_GS_NKPersonResponsible = staff6.GS_Code;
			staffAssign6.O8_Role = "MAN";
			staffAssign6.O8_GC = ZGuid.Empty;

			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Post);
			AssertEquals("Staff 6 returned", staffAssign6.O8_GS_NKPersonResponsible, returnedStaff);
		}

		#region GetStaffAssignmentSale

		public void TestGetStaffAssignmentSeaSale()
		{
			OrgHeader org = OrgHeader.New(Factory);
			var customDepartment = Factory.New<GlbDepartment>();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();

			org = OrgHeader.New(Factory);
			customDepartment = Factory.New<GlbDepartment>();
			staff = Factory.NewWithValidTestData<GlbStaff>();
			customDepartment.GE_Sea = true;
			customDepartment.GE_Export = true;
			customDepartment.GE_Code = "FES";
			OrgStaffAssignments staffAssign = org.StaffAssignments.AddNew();
			staffAssign.O8_Department = customDepartment.GE_Code;
			staffAssign.O8_GS_NKPersonResponsible = staff.GS_Code;
			staffAssign.O8_Role = "SAL";
			staffAssign.O8_GC = ZGuid.Empty;
			ZString returnedStaff = org.StaffAssignments.GetStaffAssignment("SAL", OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Sea);
			AssertEquals("Staff correctly returned", staffAssign.O8_GS_NKPersonResponsible, returnedStaff);
		}

		public void TestGetStaffAssignmentRoadSale()
		{
			OrgHeader org = OrgHeader.New(Factory);
			var customDepartment = Factory.New<GlbDepartment>();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();

			org = OrgHeader.New(Factory);
			customDepartment = Factory.New<GlbDepartment>();
			staff = Factory.NewWithValidTestData<GlbStaff>();
			customDepartment.GE_Road = true;
			customDepartment.GE_Domestic = true;
			customDepartment.GE_Code = "FDR";
			OrgStaffAssignments staffAssign = org.StaffAssignments.AddNew();
			staffAssign.O8_Department = customDepartment.GE_Code;
			staffAssign.O8_GS_NKPersonResponsible = staff.GS_Code;
			staffAssign.O8_Role = "SAL";
			staffAssign.O8_GC = ZGuid.Empty;
			ZString returnedStaff = org.StaffAssignments.GetStaffAssignment("SAL", OrgStaffAssignmentsCollection.Direction.Domestic, OrgStaffAssignmentsCollection.AirSea.Road);
			AssertEquals("Staff correctly returned", staffAssign.O8_GS_NKPersonResponsible, returnedStaff);
		}

		public void TestGetStaffAssignmentRailSale()
		{
			OrgHeader org = OrgHeader.New(Factory);
			var customDepartment = Factory.New<GlbDepartment>();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();

			org = OrgHeader.New(Factory);
			customDepartment = Factory.New<GlbDepartment>();
			staff = Factory.NewWithValidTestData<GlbStaff>();
			customDepartment.GE_Rail = true;
			customDepartment.GE_Code = "RAI";
			OrgStaffAssignments staffAssign = org.StaffAssignments.AddNew();
			staffAssign.O8_Department = customDepartment.GE_Code;
			staffAssign.O8_GS_NKPersonResponsible = staff.GS_Code;
			staffAssign.O8_Role = "SAL";
			staffAssign.O8_GC = ZGuid.Empty;
			ZString returnedStaff = org.StaffAssignments.GetStaffAssignment("SAL", OrgStaffAssignmentsCollection.Direction.None, OrgStaffAssignmentsCollection.AirSea.Rail);
			AssertEquals("Staff correctly returned", staffAssign.O8_GS_NKPersonResponsible, returnedStaff);
		}

		#endregion

		public void TestGetStaffAssignmentActiveDepartment()
		{
			var org = OrgHeader.New(Factory);
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var importSeaDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS"));

			var customDepartment = Factory.New<GlbDepartment>();
			customDepartment.GE_GE = importSeaDepartment.PK;
			customDepartment.GE_Import = true;
			customDepartment.GE_Sea = true;
			customDepartment.GE_Code = "INS";

			var currentValue = importSeaDepartment.GE_IsActive;
			importSeaDepartment.GE_IsActive = false;

			var staffAssign1 = org.StaffAssignments.AddNew();
			staffAssign1.O8_Department = customDepartment.GE_Code;
			staffAssign1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			staffAssign1.O8_Role = "MAN";

			var returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Sea);
			AssertEquals("Staff 1 returned", staffAssign1.O8_GS_NKPersonResponsible, returnedStaff);

			importSeaDepartment.GE_IsActive = currentValue;
		}

		public void TestGetStaffAssignment_ShouldIgnoreProduct()
		{
			var org = Factory.New<OrgHeader>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var staffAssign1 = org.StaffAssignments.AddNew();
			staffAssign1.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			staffAssign1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			staffAssign1.O8_Role = "MAN";

			var staffAssign2 = org.StaffAssignments.AddNew();
			staffAssign2.O8_Department = OrgStaffAssignmentsLookups.SeaFreightServices;
			staffAssign2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			staffAssign2.O8_Role = "MAN";

			var returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Sea);
			AssertEquals("Staff 2 returned", staffAssign2.O8_GS_NKPersonResponsible, returnedStaff);

			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Road);
			AssertEquals("Staff 1 returned", staffAssign1.O8_GS_NKPersonResponsible, returnedStaff);

			staffAssign1.O8_Product = "ENT";
			staffAssign2.O8_Product = "ENT";

			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Sea);
			AssertEquals("Staff 2 is Product specific, should return null", ZString.Empty, returnedStaff);

			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Road);
			AssertEquals("Staff 1 is Product specific, should return null", ZString.Empty, returnedStaff);
		}

		public void TestGetStaffAssignment_Product()
		{
			var org = Factory.New<OrgHeader>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();

			var staffAssign1 = org.StaffAssignments.AddNew();
			staffAssign1.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			staffAssign1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			staffAssign1.O8_Role = "MAN";

			var staffAssign2 = org.StaffAssignments.AddNew();
			staffAssign2.O8_Department = OrgStaffAssignmentsLookups.SeaFreightServices;
			staffAssign2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			staffAssign2.O8_Role = "MAN";

			var returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsLookups.AllServices, "ENT");
			AssertEquals("Staff 2 returned", staffAssign1.O8_GS_NKPersonResponsible, returnedStaff);

			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsLookups.SeaFreightServices, "ENT");
			AssertEquals("Staff 1 returned", staffAssign2.O8_GS_NKPersonResponsible, returnedStaff);

			var staffAssign3 = org.StaffAssignments.AddNew();
			staffAssign3.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			staffAssign3.O8_GS_NKPersonResponsible = staff3.GS_Code;
			staffAssign3.O8_Role = "MAN";
			staffAssign3.O8_Product = "ENT";

			var staffAssign4 = org.StaffAssignments.AddNew();
			staffAssign4.O8_Department = OrgStaffAssignmentsLookups.SeaFreightServices;
			staffAssign4.O8_GS_NKPersonResponsible = staff4.GS_Code;
			staffAssign4.O8_Role = "MAN";
			staffAssign4.O8_Product = "ENT";

			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsLookups.AllServices, "ENT");
			AssertEquals("Should priorityze staffs with product", staff3.GS_Code, returnedStaff);

			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsLookups.SeaFreightServices, "ENT");
			AssertEquals("Should priorityze staffs with product", staff4.GS_Code, returnedStaff);
		}

		public void TestSetStaffAssignment()
		{
			OrgHeader org = OrgHeader.New(Factory);

			GlbStaff newStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff newStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff newStaff3 = Factory.NewWithValidTestData<GlbStaff>();

			org.StaffAssignments.SetStaffAssignment("MAN", newStaff3.GS_Code, OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Air);
			ZString returnedStaff = (org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Air));
			AssertEquals("New Staff 3 returned", newStaff3.GS_Code, returnedStaff);

			org.StaffAssignments.SetStaffAssignment("MAN", newStaff2.GS_Code, OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Sea);
			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Sea);
			AssertEquals("New Staff 2 returned", newStaff2.GS_Code, returnedStaff);

			org.StaffAssignments.SetStaffAssignment("MAN", newStaff1.GS_Code, OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Road);
			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Road);
			AssertEquals("New Staff 1 returned", newStaff1.GS_Code, returnedStaff);

			AssertEquals("3 staff assignments", 3, org.StaffAssignments.Count);

			org.StaffAssignments.SetStaffAssignment("MAN", ZString.Empty, OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Road);
			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Road);
			AssertEquals("Empty Staff returned", ZString.Empty, returnedStaff);

			AssertEquals("2 staff assignments as the last one was removed when set to empty", 2, org.StaffAssignments.Count);
		}

		public void TestSetStaffAssignmentWithDepartment()
		{
			OrgHeader org = OrgHeader.New(Factory);

			GlbStaff newStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff newStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			org.StaffAssignments.SetStaffAssignment("MAN", newStaff1.GS_Code, "FIA");
			ZString returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", "FIA");
			AssertEquals("New Staff 3 returned", newStaff1.GS_Code, returnedStaff);

			org.StaffAssignments.SetStaffAssignment("MAN", newStaff2.GS_Code, "WHS");
			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", "WHS");
			AssertEquals("New Staff 2 returned", newStaff2.GS_Code, returnedStaff);

			AssertEquals("2 staff assignments", 2, org.StaffAssignments.Count);

			org.StaffAssignments.SetStaffAssignment("MAN", ZString.Empty, "WHS");
			returnedStaff = org.StaffAssignments.GetStaffAssignment("MAN", "WHS");
			AssertEquals("Empty Staff returned", ZString.Empty, returnedStaff);

			AssertEquals("1 staff assignment as the last one was removed when set to empty", 1, org.StaffAssignments.Count);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader org = OrgHeader.New(Factory);
			return org.StaffAssignments;
		}

		#endregion
	}
}
