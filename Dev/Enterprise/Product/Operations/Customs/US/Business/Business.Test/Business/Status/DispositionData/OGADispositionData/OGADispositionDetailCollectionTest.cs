using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OGADispositionDetailCollection))]
	sealed class OGADispositionDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNewIfNotExist()
		{
			var newOne = OGADispositionCode.OGADispositionDetails.AddNewIfNotExist("1", "1111", new ZDateTime(2015, 01, 01));
			AssertEquals("1", newOne.US_ReferenceIDQualifier);
			AssertEquals("1111", newOne.US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 01, 01), newOne.US_ReceiptDateTime);

			var newOne2 = OGADispositionCode.OGADispositionDetails.AddNewIfNotExist("1", "1111", new ZDateTime(2015, 01, 01));
			AssertEquals("No new element is not added", 1, OGADispositionCode.OGADispositionDetails.Count);
			AssertEquals("NewOne == newOne2", newOne, newOne2);

			var newOne3 = OGADispositionCode.OGADispositionDetails.AddNewIfNotExist("1", "2222", new ZDateTime(2015, 01, 01));
			AssertEquals("New element should be added", 2, OGADispositionCode.OGADispositionDetails.Count);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OGADispositionDetail>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return OGADispositionCode.OGADispositionDetails;
		}

		OGADispositionData OGADispositionCode
		{
			get { return fOGADispositionCode ?? (fOGADispositionCode = Factory.New<OGADispositionData>()); }
		}
		OGADispositionData fOGADispositionCode;
	}
}
