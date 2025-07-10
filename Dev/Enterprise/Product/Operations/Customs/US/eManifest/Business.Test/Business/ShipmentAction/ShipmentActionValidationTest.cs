using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class ShipmentActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB0_ShipmentControlNumber()
		{
			const string warning = "This shipment has been deleted from Customs file and can be deleted from the system.";
			action.Validation.ValidateB0_ShipmentControlNumber();
			AssertNoWarning(action.B0_ShipmentControlNumberInfo, warning);
			shipment.B0_ReleaseStatus = EntryStatusList.Codes.Cancelled;
			action.Validation.ValidateB0_ShipmentControlNumber();
			AssertHasWarning(action.B0_ShipmentControlNumberInfo, warning);
			shipment.Trip.BH_ReleaseStatus = EntryStatusList.Codes.Cancelled;
			action.Validation.ValidateB0_ShipmentControlNumber();
			AssertNoWarning(action.B0_ShipmentControlNumberInfo, warning);
		}

		public void TestCheckB0_ActionCode()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(action.B0_ActionCodeInfo, MessageActionCodes.Codes.Change, MessageActionCodes.Codes.Original);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			ValidationTestHelper.AssertErrorIfInvalidCode(action.B0_ActionCodeInfo, MessageActionCodes.Codes.Original, MessageActionCodes.Codes.Cancellation);
		}

		public void TestCheckB0_AmendmentReason()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(action.B0_AmendmentReasonInfo);
			action.B0_ActionCode = MessageActionCodes.Codes.Change;
			ValidationTestHelper.AssertErrorIfNotEntered(action.B0_AmendmentReasonInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(action.B0_AmendmentReasonInfo, ShipmentAmendmentCodes.Codes.C16, ShipmentAmendmentCodes.Codes.C01);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			ValidationTestHelper.AssertErrorIfNotEntered(action.B0_AmendmentReasonInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(action.B0_AmendmentReasonInfo, ShipmentAmendmentCodes.Codes.C01, ShipmentAmendmentCodes.Codes.C16);
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<Trip>().Shipments.AddNew();
			action = shipment.Trip.ShipmentsActions[0];
		}

		Shipment shipment;
		ShipmentAction action;
	}
}
