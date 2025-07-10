using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class DefaultPackagesTest : Customs.Business.Testing.DefaultPackagesTest
	{
		protected override BaseJobDeclaration GetDeclarationPackageRelevant()
		{
			return Factory.New<JobDeclaration>();
		}
	}

	[TestedType(typeof(Package))]
	public class PackageTest : Customs.Business.Testing.BasePackageTest
	{
		public void TestDoesNotDeleteIfPackageCountIsZeroAsEmptyContainersNeedZeroPackagesDeclared()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			PackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			Package package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 0;
			Factory.Save();
			AssertEquals("package.IsDeleted - not associated with a container", true, package.IsDeleted);

			package = packingGroup.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = "MSCU0394872";
			package.CW_PackQty = 0;
			Factory.Save();
			AssertEquals("package.IsDeleted - Empty Containers require zero packages", false, package.IsDeleted);
		}

		public override void TestIOneToOnePackingInformation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			Bill bill1 = declaration.Bills.AddNew();
			Bill bill2 = declaration.Bills.AddNew();

			CusContainer container1 = declaration.CusContainers.AddNew();
			CusContainer container2 = declaration.CusContainers.AddNew();

			Package package = declaration.Packages.AddNew();
			AssertNull(((IPackingInformation)package).HouseBillContainer.Container);
			AssertNull(((IPackingInformation)package).HouseBillContainer.HouseBill);

			((IPackingInformation)package).HouseBillContainer = new HouseBillContainer(bill1, null);
			AssertNull(((IPackingInformation)package).HouseBillContainer.Container);
			AssertEquals(bill1, ((IPackingInformation)package).HouseBillContainer.HouseBill);
			AssertEquals(bill1, package.PackingGroup.Bill);

			((IPackingInformation)package).HouseBillContainer = new HouseBillContainer(bill2, container1);
			AssertEquals(container1, ((IPackingInformation)package).HouseBillContainer.Container);
			AssertEquals(bill2, ((IPackingInformation)package).HouseBillContainer.HouseBill);
			AssertEquals(bill2, package.PackingGroup.Bill);
			AssertEquals(container1, package.PackingGroup.Container);

			((IPackingInformation)package).MarksAndNumbers = "MarksAndNumbers";
			AssertEquals("", package.CW_MarksAndNos);

			((IOneToOnePackingInformation)package).PackQty = 13;
			((IOneToOnePackingInformation)package).PackType = "QQ";
			AssertEquals(13, package.CW_PackQty);
			AssertEquals("QQ", package.CW_PackType);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = GetJobDeclaration();
			declaration.DisableDefaultPackingInformation = true;
			var houseBill = declaration.Bills.AddNew();
			var packingGroup = houseBill.PackingGroups.AddNew();
			return packingGroup.Packages.AddNew();
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
