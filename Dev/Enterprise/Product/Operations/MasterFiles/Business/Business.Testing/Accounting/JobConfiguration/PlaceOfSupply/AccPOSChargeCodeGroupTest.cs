using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	[TestedType(typeof(AccPOSChargeCodeGroup))]
	sealed class AccPOSChargeCodeGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var group = Factory.New<AccPOSChargeCodeGroup>();

			AssertEquals("GRO_GroupType", "POS", group.GRO_GroupType);
			AssertEquals("GRO_GC", GlbCompany.CurrentCompany.PK, group.GRO_GC);
		}

		public void TestReadOnlyProperties()
		{
			var group = Factory.New<AccPOSChargeCodeGroup>();

			Assert("GRO_GroupType ReadOnly", group.GRO_GroupTypeInfo.ReadOnly);
			Assert("GRO_GC ReadOnly", group.GRO_GCInfo.ReadOnly);
		}

		public void TestCompany()
		{
			var group = Factory.New<AccPOSChargeCodeGroup>();

			AssertNotNull(group.Company);
			AssertEquals("Current Company", GlbCompany.CurrentCompany.PK, group.Company.PK);

			var otherCompany = Factory.New<GlbCompany>();
			group.GRO_GC = otherCompany.PK;
			AssertEquals("Assigned Company", otherCompany, group.Company);
		}

		public void TestChargeCodePivots()
		{
			var group = Factory.New<AccPOSChargeCodeGroup>();
			var pivot1 = Factory.New<AccPOSChargeCodeGroupPivot>();
			var pivot2 = Factory.New<AccPOSChargeCodeGroupPivot>();
			pivot2.GRP_GRO_Group = group.PK;

			AssertNotNull(group.ChargeCodePivots);
			AssertEquals("Only one pivot in collection", 1, group.ChargeCodePivots.Count);
			AssertEquals("It is pivot2", pivot2, group.ChargeCodePivots[0]);

			var pivot3 = group.ChargeCodePivots.AddNew();
			AssertEquals(2, group.ChargeCodePivots.Count);
			AssertEquals("GRP_GRO_Group is set to parent group", group.PK, pivot3.GRP_GRO_Group);
		}

		public void TestAccPlaceOfSupplyConfigurationssNotNullAndCorrectLevel()
		{
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();

			AssertNotNull(group.AccPlaceOfSupplyConfigurations);
			AssertEquals(AccPOSConfigurationLevel.ChargeCodeGroup, group.AccPlaceOfSupplyConfigurations.Level);
		}

		public void TestCanDelete()
		{
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();

			AssertEquals("Remove Charge Codes and Place of Supply Group level configurations before deleting Group.", group.ReasonForNotAbleToDelete);
			Assert(group.CanDelete);

			var companyConfig = group.AccPlaceOfSupplyConfigurations.AddNew();
			companyConfig.PSC_ParentTableCode = ZString.Empty;
			companyConfig.PSC_ParentId = ZGuid.Empty;
			Assert(group.CanDelete);

			var groupConfig = group.AccPlaceOfSupplyConfigurations.AddNew();
			AssertEquals(group.PK, groupConfig.PSC_ParentId);
			Assert(!group.CanDelete);

			group.AccPlaceOfSupplyConfigurations.DeleteAll();
			Assert(group.CanDelete);

			var chargePivot = group.ChargeCodePivots.AddNew();
			Assert(!group.CanDelete);

			chargePivot.Delete();
			Assert(group.CanDelete);
		}

		public void TestLogging()
		{
			var group = Factory.New<AccPOSChargeCodeGroup>();
			Assert("IAutoAdminLogTarget", group is IAutoAdminLogTarget);
			Assert("IsAutoLogged", group.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		public void TestLookups()
		{
			var group = Factory.New<AccPOSChargeCodeGroup>();
			Assert(group.Lookups is AccPOSChargeCodeGroupViewLookups);
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = (AccPOSChargeCodeGroup)base.GetNewBusinessObject();
			result.FillWithValidTestData();
			return result;
		}

		#endregion
	}
}
