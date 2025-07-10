using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS30Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			var notifications = new NotificationBuffer();
			var declaration = Factory.New<JobDeclaration>();
			var aens30 = new AENS30
			{
				AssociatedWarehouseEntryDistrictPortCode = "123",
				AssociatedWarehouseEntryNumber = "ENT32432",
				AssociatedWarehouseEntryFilerCode = "SV9",
				FinalWarehouseWithdrawalIndicator = "Y"
			};

			((IBIRDHeaderRecord)aens30).Update(declaration, notifications);
			AssertEquals("123", declaration.US_WHSDistrictPortCode);
			AssertEquals("ENT32432", declaration.US_WHSEntryNumber);
			AssertEquals("SV9", declaration.US_WHSEntryFilerCode);
			AssertEquals(true, declaration.US_IsFinalWHS);
		}
	}
}
