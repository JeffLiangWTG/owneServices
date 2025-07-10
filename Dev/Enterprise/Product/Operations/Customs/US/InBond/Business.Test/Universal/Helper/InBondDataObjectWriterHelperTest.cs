using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	sealed class InBondDataObjectWriterHelperTest : DataTransfer.Universal.Testing.InBondDataObjectWriterHelperTest<CusInBondHeader>
	{
		public void TestPopulateDispositions()
		{
			var bill = InBondHeader.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			moveDetail.DispositionCodes.AddNewIfNotExist("XX", ZDateTime.BrettsBirthday);
			moveDetail.DispositionCodes.AddNewIfNotExist("1K", ZDateTime.BrettsBirthday);
			moveDetail.DispositionCodes.AddNewIfNotExist("62", ZDateTime.BrettsBirthday);
			var moveDetailData = new InBondMoveDetail(DefaultDataObjectWriterStrategy.TestInstance);
			Helper.PopulateDispositions(moveDetail, moveDetailData);
			AssertNotNull(moveDetailData.AddInfoGroupCollection);
			var dispositions = moveDetailData.AddInfoGroupCollection.Where(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USDisposition);
			AssertEquals(3, moveDetailData.AddInfoGroupCollection.Count);
		}

		public void TestWayBillTypeList()
		{
			AssertType<WayBillTypeList>(Helper.WayBillTypeList);
		}

		public void TestBindToLists()
		{
			AssertType<Freight.Common.Business.BindToLists>(Helper.BindToLists);
		}

		public void TestGetCusInBondCargoDescCustomLabelsProvider()
		{
			var provider = Helper.GetCusInBondCargoDescCustomLabelsProvider();
			AssertType<CusInBondCargoDescCustomLabelsProvider>(provider);
		}

		new InBondDataObjectWriterHelper Helper => (InBondDataObjectWriterHelper)base.Helper;

		protected override DataTransfer.Universal.InBondDataObjectWriterHelper CreateHelper(CusInBondHeader header) => new InBondDataObjectWriterHelper(header);
	}
}
