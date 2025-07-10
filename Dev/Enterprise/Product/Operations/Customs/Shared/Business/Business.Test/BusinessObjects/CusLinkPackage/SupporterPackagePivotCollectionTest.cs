using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class SupporterPackagePivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestEverything()
		{
			var supporter = GetSupporter();
			var collection = supporter.CusPackPivots;

			var declaration = supporter.Declaration;

			var bill = declaration.PrimaryMasterBill;
			if (bill.PackingGroups[0] == null)
			{
				var packingInformation = bill.Declaration.PackingInformationCollection.AddNew();
				packingInformation.HouseBillContainer = new HouseBillContainer(bill, null);
				Factory.Save();
			}
			var package = bill.PackingGroups[0].Packages.AddNew();

			var pivot = collection.AddPivotFor(package);
			AssertEquals(declaration.PK, pivot.DeclarationPK);
			AssertEquals(package.PK, pivot.PackagePK);
			AssertEquals(supporter.Identifier, pivot.ParentPK);

			pivot.NumberOfPacks = 1;
			package.CW_PackQty = 69;
			Factory.Save();

			collection.Reload(true);

			AssertEquals(1, collection.Count);

			var pivotReloaded = collection.Cast<BusinessObject>().First();
			AssertEquals(pivot.Identifier, pivotReloaded.PK);
		}

		public void TestAddNewSetDeclarationPKReference()
		{
			var supporter = GetSupporter();
			var declaration = supporter.Declaration;
			var bill = declaration.PrimaryMasterBill;
			if (bill.PackingGroups[0] == null)
			{
				var packingInformation = bill.Declaration.PackingInformationCollection.AddNew();
				packingInformation.HouseBillContainer = new HouseBillContainer(bill, null);
				Factory.Save();
			}
			var package = bill.PackingGroups[0].Packages.AddNew();
			package.CW_PackQty = 10;
			package.CW_PackType = "NO";

			var collection = supporter.CusPackPivots;
			var pivot = (ICusPackagePivot)collection.AddNew();
			AssertEquals(declaration.PK, pivot.DeclarationPK);
			pivot.NumberOfPacks = 1;
			pivot.PackagePK = package.PK;
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestDefaultNumberOfPacks()
		{
			var supporter = GetSupporter();
			var collection = supporter.CusPackPivots;

			var bill = supporter.Declaration.PrimaryMasterBill;
			if (bill.PackingGroups[0] == null)
			{
				var packingInformation = bill.Declaration.PackingInformationCollection.AddNew();
				packingInformation.HouseBillContainer = new HouseBillContainer(bill, null);
				Factory.Save();
			}
			var package = bill.PackingGroups[0].Packages.AddNew();
			package.CW_PackQty = 30;

			var pivot = collection.AddPivotFor(package);
			AssertEquals("NumberOfPacks", 30, pivot.NumberOfPacks);

			pivot.NumberOfPacks = 20;

			var pivot2 = GetNewPivotOnSameDeclaration(supporter.Declaration, package);
			AssertEquals("NumberOfPacks", 10, pivot2.NumberOfPacks);
		}

		protected abstract ICusLinkPackageSupporter GetSupporter();

		protected abstract ICusPackagePivot GetNewPivotOnSameDeclaration(BaseJobDeclaration declaration, BasePackage package);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return (BusinessObjectCollection)GetSupporter().CusPackPivots;
		}
	}
}
