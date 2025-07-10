using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BasePackageCollectionTest<T> : BusinessObjectCollectionViewTestCase<T> where T : BasePackageCollection
	{
		public void TestSetDefaultValues()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BasePackingGroup packGroup = declaration.PackingGroups.AddNew();
			BasePackage package = packGroup.Packages.AddNew();
			AssertEquals(packGroup.PK, package.CW_CR_HouseContainer);
		}

		public virtual void TestGetPackageWithPackTypeAndCount()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BasePackingGroup packGroup = declaration.PackingGroups.AddNew();
			BasePackage package1 = packGroup.Packages.AddNew();

			BasePackage package2 = packGroup.Packages.AddNew();
			package2.CW_PackQty = 5;
			package2.CW_PackType = "";

			AssertEquals(package2, packGroup.Packages.GetPackageWithPackTypeAndCount("", 5, true));
			AssertEquals(package1, packGroup.Packages.GetPackageWithPackTypeAndCount("YY", 5, true));

			package2.CW_PackType = "YY";
			AssertEquals(package2, packGroup.Packages.GetPackageWithPackTypeAndCount("YY", 5, true));
		}

		public void TestForeignKeyIsSet()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BasePackingGroup packGroup = declaration.PackingGroups.AddNew();
			BasePackage package = packGroup.Packages.AddNew();
			AssertEquals(packGroup.PK, package.CW_CR_HouseContainer);
		}

		public void TestPackingGroup()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill houseBill = declaration.Bills.AddNew();
			BasePackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			BasePackageCollection packages = new BasePackageCollection(packingGroup);
			AssertEquals(packingGroup, packages.PackingGroup);
		}

		public void TestDeclarationIsSet()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			Bill houseBill = testDec.Bills.AddNew();
			BasePackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			BasePackage package = packingGroup.Packages.AddNew();
			AssertEquals("As this is a dependent collection, removing from the collection will remove reference to the parent, which leads to null declaration. It needs to be manually set", testDec, package.Declaration);
		}

		public void TestTypedIndexer()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill houseBill = declaration.Bills.AddNew();
			BasePackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			BasePackageCollection packages = new BasePackageCollection(packingGroup);
			BasePackage package = packages.AddNew();
			AssertEquals(package, packages[0]);
		}

		protected BasePackageCollection Packages
		{
			get
			{
				if (fPackages == null)
				{
					BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
					Bill houseBill = declaration.Bills.AddNew();
					BasePackingGroup packingGroup = houseBill.PackingGroups.AddNew();
					fPackages = packingGroup.Packages;
				}
				return fPackages;
			}
		}
		BasePackageCollection fPackages;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BasePackage result = Factory.New<BasePackage>();
			result.CW_CR_HouseContainer = Packages.PackingGroup.PK;
			return result;
		}

		protected override T GetCollectionToTest()
		{
			return (T)Packages;
		}
	}
}
