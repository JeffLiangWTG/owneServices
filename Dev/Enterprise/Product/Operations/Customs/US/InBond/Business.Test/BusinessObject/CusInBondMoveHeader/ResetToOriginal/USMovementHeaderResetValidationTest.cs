using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class USMovementHeaderResetValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRO_ResetToOriginal()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveheader = header.MovementHeaders.AddNew();
			moveheader.AllocateInBondNumber("1");
			var sendingObj = new InBondMessageSendingObject(moveheader, InBondMessageType.DepartureDelete);
			sendingObj.Send();
			var coll = new USMovementHeaderResetCollection(header);
			coll[0].RO_ResetToOriginal = true;
			coll[0].RO_ResetReason = "";
			AssertHasError(coll[0].RO_ResetReasonInfo, USMovementHeaderResetValidation.enterResettingReason);
			coll[0].RO_ResetReason = "reason";
			AssertNoErrors(USMovementHeaderResetValidation.selectMovementHeader, coll[0].RO_ResetToOriginalInfo);
			AssertNoError(coll[0].RO_ResetReasonInfo, USMovementHeaderResetValidation.enterResettingReason);
			coll[0].RO_ResetToOriginal = false;
			AssertHasErrors(USMovementHeaderResetValidation.selectMovementHeader, coll[0].RO_ResetToOriginalInfo);
		}
	}
}
