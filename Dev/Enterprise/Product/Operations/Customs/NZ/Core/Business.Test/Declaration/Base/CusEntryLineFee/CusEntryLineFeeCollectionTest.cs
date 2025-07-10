using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using NUnit.Framework;

	[TestedType(typeof(CusEntryLineFeeCollection))]
	public class CusEntryLineFeeCollectionTest : Customs.Business.Testing.CusEntryLineFeeCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			return new CusEntryLineFeeCollection(entryLine, Factory);
		}

		[ExpectNoExceptions]
		public void TestIndexer()
		{
			testCollection.AddNew();
			CusEntryLineFee entryLineFee = testCollection[0];
		}

		public void TestAddNewSpecificType()
		{
			BusinessObject bO = testCollection.AddNew();
			Assert("BO is typeof CusEntryLineFee", bO is CusEntryLineFee);
		}

		public void TestAddNewCusEntryLineFeeWithFeeType()
		{
			CusEntryLineFee fee = testCollection.AddNew("FFF");
			AssertEquals("FFF", fee.CF_ChargeType);
		}

		protected CusEntryLineFeeCollection testCollection;
		protected override void SetUp()
		{
			base.SetUp();

			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			testCollection = new CusEntryLineFeeCollection(entryLine, Factory);
		}
	}
}
