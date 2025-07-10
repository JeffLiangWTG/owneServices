using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(NonApplicableCusUSLVConsignmentCollection))]
	public class NonApplicableCusUSLVConsignmentCollectionTest : ActiveBusinessObjectCollectionTestCase<NonApplicableCusUSLVConsignmentCollection>
	{
		public void TestNonApplicableCriteria_HasNotBeenConverted()
		{
			var consignment = CreateNonApplicableConsignment();
			AssertCollectionContains("pre-condition", consignment, Clearance.NonApplicableConsignments);

			consignment.CE_EntryLineReference = "REF0010201";
			Clearance.NonApplicableConsignments.Refresh();

			AssertCollectionNotContains("consignment should not in collection", consignment, Clearance.NonApplicableConsignments);
		}

		public void TestNonApplicableCriteria_EntryNumIsEmpty()
		{
			var consignment = CreateNonApplicableConsignment();
			AssertCollectionContains("pre-condition", consignment, Clearance.NonApplicableConsignments);

			consignment.CE_EntryNum = "EntryNum";
			Clearance.NonApplicableConsignments.Refresh();

			AssertCollectionNotContains("consignment should not in collection", consignment, Clearance.NonApplicableConsignments);
		}

		public void TestSelectAllAndUnSelectAll()
		{
			CreateNonApplicableConsignment();
			CreateNonApplicableConsignment("7736251432");
			Assert("pre-condition", Clearance.NonApplicableConsignments.All(c => !c.ShouldConvertToStandaloneDeclaration));

			Clearance.NonApplicableConsignments.SelectedAll();
			Assert("Should all be selected ", Clearance.NonApplicableConsignments.All(c => c.ShouldConvertToStandaloneDeclaration));

			Clearance.NonApplicableConsignments.UnSelectedAll();
			Assert("Should all be unselected ", Clearance.NonApplicableConsignments.All(c => !c.ShouldConvertToStandaloneDeclaration));
		}

		CusUSLVConsignment CreateNonApplicableConsignment(string tariffNumber = null)
		{
			if (deminimus == null)
			{
				var grouping = Factory.NewWithValidTestData<RefDataGrouping>();
				grouping.ZZZ_DataGrouping = "US";
				grouping.ZZZ_Description = "United States";
				deminimus = Factory.New<RefCusTaxOrFee>();
				deminimus.ZZF_ZZZ_NKDataGrouping = "US";
				deminimus.ZZF_Code = "DEM";
				deminimus.ZZF_StartDate = new ZDateTime(1960, 1, 1);
				deminimus.ZZF_EndDate = ZDateTime.Today.AddYears(1);
				deminimus.ZZF_Value = 800;
				deminimus.ZZF_Description = "a value";
				Factory.Save();
			}

			var consignment = Clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();
			item.ULI_GoodsValue = 800.01;
			item.ULI_RX_NKCurrency = "USD";
			var tariff = Factory.New<USCTariff>();
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			item.ULI_Tariff = tariff.UE_Tariff = tariffNumber ?? "6864456123";

			Factory.Save();
			return consignment;
		}

		#region Implementation

		RefCusTaxOrFee deminimus;

		protected CusUSLVClearance Clearance => clearance ?? (clearance = Factory.New<CusUSLVClearance>());
		CusUSLVClearance clearance;
		protected override NonApplicableCusUSLVConsignmentCollection GetCollectionToTest() => Clearance.NonApplicableConsignments;
		protected override BusinessObject GetNewElementToAddToTheCollection() => CreateNonApplicableConsignment();

		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestAddNew()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert(true);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}

		#endregion
	}
}
