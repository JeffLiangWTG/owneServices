using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(NctsHeaderToAttachCollection))]
	sealed class NctsHeaderToAttachCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollection()
		{
			var collection = new NctsHeaderToAttachCollection(GetEntry());
			collection.Add(GetHeader());

			var systemFilter = collection.FilterBusinessObjectDefaults["Show only jobs with an MRN?:Property0"];

			AssertNotNull(systemFilter);
			AssertEquals("Y", systemFilter.Value.ToString());
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new NctsHeaderToAttachCollection(GetEntry());

		protected override BusinessObject GetNewElementToAddToTheCollection() => GetHeader();

		CusInBondHeader GetHeader()
		{
			var nctsHeader = Factory.New<Integration.Customs.EU.NCTS.ICusInBondHeader>();
			((BusinessObject)nctsHeader)[CusInBondHeaderSchema.BH_HeaderType] = "D";
			var bill = Factory.New<Integration.Customs.EU.NCTS.ICusInBondBill>();
			var nctsGoodsItem = (BusinessObject)Factory.New<Integration.Customs.EU.NCTS.IDepartureCargoDesc>();
			nctsGoodsItem[CusInBondCargoDescSchema.BY_ParentID] = bill.PK;
			nctsGoodsItem[CusInBondCargoDescSchema.BY_ParentTableCode] = CusInBondBillSchema.Constants.Prefix;
			return (CusInBondHeader)nctsHeader;
		}

		CusEntryHeader GetEntry()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.FillWithValidTestData();
			return entry;
		}
	}
}
