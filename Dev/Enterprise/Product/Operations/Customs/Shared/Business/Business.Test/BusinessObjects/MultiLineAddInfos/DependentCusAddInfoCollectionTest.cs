using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MultiLineAddInfos.Testing
{
	[TestedType(typeof(DependentCusAddInfoCollection<CusAddInfo<AddInfoWithTypeCode>, BaseJobComInvoiceHeader>))]
	sealed class DependentCusAddInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAaddCloneFrom()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var collection = new DependentCusAddInfoCollection<CusAddInfo<AddInfoWithTypeCode>, BaseJobComInvoiceHeader>(invoice, "ABC");
			var addInfo = collection.AddNew();
			addInfo.B7_AddInfoData = "HELLO";

			var invoice2 = Factory.New<BaseJobComInvoiceHeader>();
			var collection2 = new DependentCusAddInfoCollection<CusAddInfo<AddInfoWithTypeCode>, BaseJobComInvoiceHeader>(invoice2, "ABC");
			AssertEquals(0, collection2.Count);
			collection2.AddCloneFrom(collection, new BusinessObjectCloneArgs());
			AssertEquals(1, collection2.Count);
			var clonedValue = collection2[0];
			AssertEquals(invoice2.PK, clonedValue.B7_ParentID);
			AssertEquals(invoice2.TablePrefix, clonedValue.B7_ParentTableCode);
			AssertEquals("ABC", clonedValue.B7_Type);
			AssertEquals("HELLO", clonedValue.B7_AddInfoData);
			AssertEquals(false, clonedValue.HasChanges);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DependentCusAddInfoCollection<CusAddInfo<AddInfoWithTypeCode>, BaseJobComInvoiceHeader>(Factory.New<BaseJobComInvoiceHeader>(), "ABC");
		}
	}
}
