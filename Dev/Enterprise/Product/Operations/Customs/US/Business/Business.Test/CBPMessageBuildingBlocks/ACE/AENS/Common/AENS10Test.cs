using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS10Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			var aens10Block = new AENS10()
			{
				DistrictPortOfEntry = "3901",
				EntryFilerCode = "SV9",
				EntryNumber = "71019383",
				EntryTypeCode = "01",
				ModeOfTransportationMOTCode = "41",
				PaymentTypeCode = "1"
			};

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var notifications = new NotificationBuffer();
			((IBIRDHeaderRecord)aens10Block).Update(declaration, notifications);
			AssertEquals("3901", declaration.US_SchDEntry);
			AssertEquals("SV9", declaration.US_EntryFilerCode);
			AssertEquals("71019383", declaration.ImportEntryNumber);
			AssertEquals("01", declaration.US_EntryType);
			AssertEquals(TransportTypeList.Codes.Air, declaration.JE_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
			AssertEquals("1", declaration.US_PaymentType);
		}
	}
}
