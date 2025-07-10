using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFLineRowCollection))]
	sealed class ISFLineRowCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ISFLineRowCollection>
	{
		public void TestLoadData()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFLine line1 = header.Lines.AddNew();
			CusISFLine line2 = header.Lines.AddNew();
			CusISFLine line3 = header.Lines.AddNew();
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			ISFLineRowCollection collection = new ISFLineRowCollection(billRow);
			AssertEquals(3, collection.Count);
			var list = collection.ToArray<ISFLineRow>();
			AssertEquals(1, list.Count(x => x.Line == line1));
			AssertEquals(1, list.Count(x => x.Line == line2));
			AssertEquals(1, list.Count(x => x.Line == line3));
		}

		public void TestAllow()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			ISFLineRowCollection collection = new ISFLineRowCollection(billRow);
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);
		}

		protected override ISFLineRowCollection GetCollectionToTest()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			var billRow = new ISFBillRow(headerRow);
			return new ISFLineRowCollection(billRow);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			return new ISFLineRow(header.Lines.AddNew(), headerRow);
		}
	}
}
