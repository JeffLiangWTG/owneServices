using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class MessageSendingMovementValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMM_Send()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var mmSending = new MessageSendingMovement(moveHeader, ActionCode.ChangeEstDateOfArrival);

			header.BH_ETA = ZDateTime.Today.AddDays(-2);
			mmSending.MM_Send = true;
			AssertHasMessageErrorContaining(mmSending.MM_SendInfo, ValidationConstants.MessageSending.NewEstimatedDateOfArrival.ToString());

			header.BH_ETA = ZDateTime.Today.AddDays(-8);
			mmSending.MM_Send = false;
			mmSending.MM_Send = true;
			AssertNoMessageErrorContaining(mmSending.MM_SendInfo, ValidationConstants.MessageSending.NewEstimatedDateOfArrival.ToString());
		}

		public void TestCheckMM_Send_InBondNumberAllocation()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var moveHeaderInDifferentFactory = factory2.Load<CusInBondMoveHeader>(moveHeader.PK);
			var mmSendingInDifferentFactory = new MessageSendingMovement(moveHeaderInDifferentFactory, ActionCode.SubsequentInBondAmendment);
			mmSendingInDifferentFactory.MM_Send = true;
			var mmSending = new MessageSendingMovement(moveHeader, ActionCode.SubsequentInBondOriginal);
			mmSending.MM_Send = true;
			var messageError = ValidationConstants.MessageSending.InBondNumberAllocationIsInProgress(moveHeader.GetInBondNumberAllocationMutexLockInfo());
			AssertHasError(mmSending.MM_SendInfo, messageError);
			mmSending.MM_Send = false;
			AssertNoError(mmSending.MM_SendInfo, messageError);
			mmSendingInDifferentFactory.MM_Send = false;
			mmSending.MM_Send = true;
			AssertNoError(mmSending.MM_SendInfo, messageError);
			moveHeader.UnLockInBondNumberAllocationMutex();
			moveHeaderInDifferentFactory.UnLockInBondNumberAllocationMutex();
		}

		public void TestCheckMM_Date()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;

			var messageMovement = new MessageSendingMovement(moveHeader, ActionCode.VesselDeparture);
			messageMovement.MM_Send = true;
			messageMovement.MM_Date = ZDateTime.Empty;
			AssertHasMessageErrorContaining(messageMovement.MM_DateInfo, MandatoryValidation.YouHaveNotEntered);

			messageMovement.MM_Date = ZDateTime.Today;
			AssertNoMessageErrorContaining(messageMovement.MM_DateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(messageMovement.MM_DateInfo, ValidationConstants.MessageSending.ExportDateForVesselDeparture.ToString());

			messageMovement.MM_Date = ZDateTime.Today.AddDays(-1);
			AssertNoMessageErrorContaining(messageMovement.MM_DateInfo, ValidationConstants.MessageSending.ExportDateForVesselDeparture.ToString());

			messageMovement.MM_Date = ZDateTime.Today.AddDays(1);
			AssertHasMessageErrorContaining(messageMovement.MM_DateInfo, ValidationConstants.MessageSending.ExportDateForVesselDeparture.ToString());

			messageMovement = new MessageSendingMovement(moveHeader, ActionCode.VesselArrival);
			messageMovement.MM_Send = true;
			messageMovement.MM_Date = ZDateTime.Empty;
			AssertHasMessageErrorContaining(messageMovement.MM_DateInfo, MandatoryValidation.YouHaveNotEntered);

			messageMovement.MM_Date = ZDateTime.Today;
			AssertNoMessageErrorContaining(messageMovement.MM_DateInfo, MandatoryValidation.YouHaveNotEntered);

			messageMovement = new MessageSendingMovement(moveHeader, ActionCode.ChangeEstDateOfArrival);
			messageMovement.MM_Send = true;
			messageMovement.MM_Date = ZDateTime.Empty;
			AssertHasMessageErrorContaining(messageMovement.MM_DateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckMM_ForeignDeparturePort()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;

			var messageMovement = new MessageSendingMovement(moveHeader, ActionCode.VesselDeparture);
			messageMovement.MM_Send = true;
			messageMovement.MM_ForeignDeparturePort = ZString.Empty;
			AssertHasMessageErrorContaining(messageMovement.MM_ForeignDeparturePortInfo, MandatoryValidation.YouHaveNotEntered);

			messageMovement.MM_ForeignDeparturePort = "5000";
			AssertNoMessageErrorContaining(messageMovement.MM_ForeignDeparturePortInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
