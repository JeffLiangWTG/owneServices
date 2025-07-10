using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using NUnit.Framework;

	[TestedType(typeof(PackingGroup))]
	public class TestPackingGroup : Customs.Business.Testing.BasePackingGroupTest
	{
		protected override BaseJobDeclaration GetJobDeclaration()
		{
			JobDeclaration result = Factory.New<JobDeclaration>();
			result.DisableDefaultPackingInformation = true;
			return result;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			TestSaveAndDelete();
		}

		[ExpectNoExceptions]
		public void TestSaveAndDelete()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			PackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			Package package = packingGroup.Packages.AddNew();
			Factory.Save();

			package.Delete();
			packingGroup.Delete();
			houseBill.Delete();
		}

		public override void TestRemovesContainerReferenceOnSavingWhenContainerDoesNotExistInJobDeclaration()
		{
			var testDec = Factory.New<JobDeclaration>();
			var container = testDec.CusContainers.AddNew();
			var bill = testDec.Bills.AddNew();

			var packingGroup = testDec.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;
			packingGroup.CR_CU_HouseBill = bill.PK;

			AssertEquals("Container", container.PK, packingGroup.CR_CO_Container);

			testDec.CusContainers.RemoveAndDeleteAll();
			Factory.Save();

			AssertEquals(false, packingGroup.IsDeleted);
		}
	}
}
