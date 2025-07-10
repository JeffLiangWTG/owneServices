using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS20Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			var notifications = new NotificationBuffer();
			var declaration = Factory.New<JobDeclaration>();
			var aens20 = new AENS20
			{
				CarrierCode = "A2",
				DistrictPortOfUnlading = "2809",
				LocationOfGoodsCode = "A001",
				ConveyanceName = "ABC VESSEL",
				DesignatedExamPortCode = "3901"
			};

			((IBIRDHeaderRecord)aens20).Update(declaration, notifications);
			AssertEquals("A2", declaration.US_UI_NKCarrierSCAC);
			AssertEquals("2809", declaration.US_SchDArrival);
			AssertEquals("A001", declaration.US_US_NKLocationOfGoods);
			AssertEquals("ABC VESSEL", declaration.JE_VesselName);
			AssertEquals("3901", declaration.US_SchDExam);
		}
	}
}
