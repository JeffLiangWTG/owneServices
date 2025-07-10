using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeGLPostingOverrideCollection))]
	sealed class AccChargeGLPostingOverrideCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNoAuditLogsWithAttibute()
		{
			var headerRev = Factory.NewWithValidTestData<AccGLHeader>();
			headerRev.AG_AccountNum = "REV0123456";
			var headerWip = Factory.NewWithValidTestData<AccGLHeader>();
			headerWip.AG_AccountNum = "WIP0123456";
			var headerCst = Factory.NewWithValidTestData<AccGLHeader>();
			headerCst.AG_AccountNum = "CST0123456";
			var headerAcr = Factory.NewWithValidTestData<AccGLHeader>();
			headerAcr.AG_AccountNum = "ACR0123456";

			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "ABC";
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "DEF";

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var glPostingOverride = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride.Y1_GE = department1.PK;
			glPostingOverride.Y1_AG_ACR = headerAcr.PK;
			glPostingOverride.Y1_AG_CST = headerCst.PK;
			glPostingOverride.Y1_AG_REV = headerRev.PK;
			glPostingOverride.Y1_AG_WIP = headerWip.PK;
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				glPostingOverride.Y1_GE = department2.PK;
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.GLPostingOverrides.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestFindForDepartment()
		{
			var collection = (AccChargeGLPostingOverrideCollection)GetCollectionToTest();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			var department3 = Factory.NewWithValidTestData<GlbDepartment>();

			var override1 = AddGLPostingOverride(collection, departmentPK: department1.PK);
			var override2 = AddGLPostingOverride(collection, departmentPK: department2.PK);

			Factory.Save();

			AssertEquals("Should return override2", override2, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.All, department2, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL"));
			AssertNull("Should return null", collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.All, department3, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL"));
			AssertNoExceptionThrown("Should be no exception", () => collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.All, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL"));
		}

		public void TestAddingNewIsNotAllowedForCommentCharges()
		{
			AccChargeGLPostingOverrideCollection collection = (AccChargeGLPostingOverrideCollection)GetCollectionToTest();
			collection.ChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			Assert(collection.AllowNew);
			collection.ChargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			Assert(!collection.AllowNew);
		}

		public void TestFindForDepartmentAndClass()
		{
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			var department3 = Factory.NewWithValidTestData<GlbDepartment>();
			var collection = (AccChargeGLPostingOverrideCollection)GetCollectionToTest();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			orgHeader.CompanyData.OB_IsDebtor = true;

			var glPostingOverride1 = AddGLPostingOverride(collection, departmentPK: department1.PK, categoryClass: ConsolidatedAccountingCategoryClassList.Codes.All);
			var glPostingOverride2 = AddGLPostingOverride(collection, departmentPK: department2.PK, categoryClass: ConsolidatedAccountingCategoryClassList.Codes.ThirdParty);
			var glPostingOverride3 = AddGLPostingOverride(collection, ZGuid.Empty, categoryClass: ConsolidatedAccountingCategoryClassList.Codes.Intercompany);
			var glPostingOverride4 = AddGLPostingOverride(collection, department2.PK, categoryClass: ConsolidatedAccountingCategoryClassList.Codes.Intercompany);

			Factory.Save();

			AssertEquals(glPostingOverride4, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, department2, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL"));
			AssertEquals(glPostingOverride3, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, department1, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL"));
			AssertEquals(glPostingOverride3, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, department3, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL"));
			AssertEquals(glPostingOverride3, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL"));

			AssertEquals(glPostingOverride2, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, department2, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL"));
			AssertEquals(glPostingOverride1, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, department1, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL"));
			AssertNull(collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, department3, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL"));
			AssertNull(collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL"));
		}

		public void TestGetGLPostingOverride()
		{
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			var department3 = Factory.NewWithValidTestData<GlbDepartment>();
			var department4 = Factory.NewWithValidTestData<GlbDepartment>();
			var collection = (AccChargeGLPostingOverrideCollection)GetCollectionToTest();

			var glPostingOverride1 = AddGLPostingOverride(collection, department1.PK, "TPY", "SHP", "AIR", masterPaymentType: "PPD", housePaymentType: "CCX");
			var glPostingOverride2 = AddGLPostingOverride(collection, department1.PK, "TPY", "SHP", direction: "EXP", masterPaymentType: "PPD", housePaymentType: "CCX");
			var glPostingOverride3 = AddGLPostingOverride(collection, department2.PK, jobType: "SHP", transportMode: "SEA", masterPaymentType: "PPD", housePaymentType: "CCX");
			var glPostingOverride4 = AddGLPostingOverride(collection, department1.PK, jobType: "SHP", direction: "EXP", masterPaymentType: "PPD", housePaymentType: "CCX");
			var glPostingOverride5 = AddGLPostingOverride(collection, department3.PK, "INT", "SHP", "AIR", masterPaymentType: "PPD", housePaymentType: "CCX");
			var glPostingOverride6 = AddGLPostingOverride(collection, ZGuid.Empty, "INT", "SHP", direction: "IMP", consolContainerMode: "LCL", housePaymentType: "CCX");
			var glPostingOverride7 = AddGLPostingOverride(collection, ZGuid.Empty, "INT", "SHP", direction: "IMP", masterPaymentType: "PPD");
			var glPostingOverride8 = AddGLPostingOverride(collection, ZGuid.Empty, "INT", "SHP", direction: "IMP", housePaymentType: "CCX");
			var glPostingOverride9 = AddGLPostingOverride(collection, department3.PK, "TPY");
			var glPostingOverride10 = AddGLPostingOverride(collection, department1.PK, "TPY", "BRK", "AIR", masterPaymentType: "PPD", housePaymentType: "CCX");
			var glPostingOverride11 = AddGLPostingOverride(collection, department1.PK, "TPY", "BRK", direction: "EXP", masterPaymentType: "PPD", housePaymentType: "CCX");
			var glPostingOverride12 = AddGLPostingOverride(collection, department2.PK, jobType: "BRK", transportMode: "SEA", masterPaymentType: "PPD", housePaymentType: "CCX");
			var glPostingOverride13 = AddGLPostingOverride(collection, ZGuid.Empty, "INT", "BRK", direction: "IMP", housePaymentType: "CCX");
			var glPostingOverride14 = AddGLPostingOverride(collection, ZGuid.Empty, "ALL", "BRK", direction: "DOM", transportMode: "FIX", housePaymentType: "CCX");
			var glPostingOverride15 = AddGLPostingOverride(collection, ZGuid.Empty, "ALL", "BRK", direction: "DOM", transportMode: "IWT", housePaymentType: "CCX");
			var glPostingOverride16 = AddGLPostingOverride(collection, ZGuid.Empty, "ALL", "BRK", direction: "DOM", transportMode: "OWN", housePaymentType: "CCX");
			var glPostingOverride17 = AddGLPostingOverride(collection, ZGuid.Empty, "ALL", "BRK", direction: "DOM", transportMode: "MAI", housePaymentType: "CCX");

			Factory.Save();

			AssertEquals(glPostingOverride1, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, department1, "SHP", "AIR", "EXP", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride3, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, department2, "SHP", "SEA", "EXP", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride2, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, department1, "SHP", "ALL", "EXP", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride9, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, department3, "ALL", "AIR", "IMP", "ALL", "ALL", "ALL"));
			AssertNull(collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, department4, "ALL", "SEA", "IMP", "ALL", "ALL", "ALL"));
			AssertEquals(glPostingOverride5, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, department3, "SHP", "AIR", "IMP", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride6, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, null, "SHP", "SEA", "IMP", "LCL", "PPD", "CCX"));
			AssertEquals(glPostingOverride7, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, department3, "SHP", "SEA", "IMP", "FCL", "PPD", "CCX"));
			AssertEquals(glPostingOverride8, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, null, "SHP", "SEA", "IMP", "FCL", "CCX", "CCX"));
			AssertEquals(glPostingOverride3, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.All, department2, "SHP", "SEA", "EXP", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride4, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.All, department1, "SHP", "AIR", "EXP", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride10, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, department1, "BRK", "AIR", "EXP", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride12, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, department2, "BRK", "SEA", "EXP", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride11, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, department1, "BRK", "ALL", "EXP", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride13, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, null, "BRK", "AIR", "IMP", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride14, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, null, "BRK", "FIX", "DOM", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride15, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, null, "BRK", "IWT", "DOM", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride16, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, null, "BRK", "OWN", "DOM", "ALL", "PPD", "CCX"));
			AssertEquals(glPostingOverride17, collection.GetGLPostingOverride(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, null, "BRK", "MAI", "DOM", "ALL", "PPD", "CCX"));
		}

		AccChargeGLPostingOverride AddGLPostingOverride(AccChargeGLPostingOverrideCollection collection, ZGuid departmentPK, string categoryClass = "ALL", string jobType = "ALL", string transportMode = "ALL", string direction = "ALL", string consolContainerMode = "ALL", string masterPaymentType = "ALL", string housePaymentType = "ALL")
		{
			var glPostingOverride = collection.AddNew();
			glPostingOverride.Y1_ConsolidationAccountingCategoryClass = categoryClass;
			if (!departmentPK.IsEmpty)
			{
				glPostingOverride.Y1_GE = departmentPK;
			}
			glPostingOverride.Y1_JobType = jobType;
			glPostingOverride.Y1_TransportMode = transportMode;
			glPostingOverride.Y1_Direction = direction;
			glPostingOverride.Y1_ConsolContainerMode = consolContainerMode;
			glPostingOverride.Y1_MasterPaymentType = masterPaymentType;
			glPostingOverride.Y1_HousePaymentType = housePaymentType;
			Assert($"Expect no Errors for {categoryClass} {jobType} {transportMode} {direction} {consolContainerMode} {masterPaymentType} {housePaymentType}", !glPostingOverride.HasErrors);

			return glPostingOverride;
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccChargeGLPostingOverrideCollection(Factory.NewWithValidTestData<AccChargeCode>());
		}

		#endregion
	}
}
