using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusLinkPackageCollection))]
	sealed class BaseCusLinkPackageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BaseCusLinkPackageCollection>
	{
		public void TestSyncLinkPackages()
		{
			var declaration = Factory.New<DeclarationForTest>();

			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			declaration.JE_MasterBill = "X";

			declaration.Packages.RemoveAndDeleteAll();

			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();

			var collection = new BaseCusLinkPackageCollection(invoice);

			var expectedPks = new[] { package1.PK, package2.PK, package3.PK };
			var actualPks = collection.Cast<BaseCusLinkPackage>().Select(c => c.PackagePk);

			AssertContainsExactElementsInAnyOrder("Should create 3 link packages.", expectedPks, actualPks);

			var package4 = declaration.Packages.AddNew();

			expectedPks = new[] { package1.PK, package2.PK, package3.PK, package4.PK };
			actualPks = collection.Cast<BaseCusLinkPackage>().Select(c => c.PackagePk);
			AssertContainsExactElementsInAnyOrder("Should create a new link package from package4.", expectedPks, actualPks);

			declaration.Packages.RemoveAndDelete(package2);

			expectedPks = new[] { package1.PK, package3.PK, package4.PK };
			actualPks = collection.Cast<BaseCusLinkPackage>().Select(c => c.PackagePk);
			AssertContainsExactElementsInAnyOrder("Should not contain package2 as it's deleted.", expectedPks, actualPks);
		}

		sealed class DeclarationForTest : BaseJobDeclaration
		{
			public DeclarationForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool SupportsChzPivotBetweenInvoiceHeaderAndPackingCore
			{
				get { return true; }
			}
		}

		protected override BaseCusLinkPackageCollection GetCollectionToTest()
		{
			return new BaseCusLinkPackageCollection(Supporter);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Supporter.Declaration;
			var bill = declaration.PrimaryMasterBill;
			var package = bill.PackingGroups[0].Packages.AddNew();

			var result = new BaseCusLinkPackage(Supporter);
			result.Package = package;

			return result;
		}

		ICusLinkPackageSupporter Supporter
		{
			get
			{
				if (supporter == null)
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					supporter = declaration.Invoices.AddNew();
					supporter.InvoiceLines.AddNew();

					declaration.JE_MasterBill = "X";
				}

				return supporter;
			}
		}
		BaseJobComInvoiceHeader supporter;
	}
}
