using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVConsignmentUSCustomsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidate_WaybillNumber_ErrorIfLengthExceedsCharacterLimit_WhenWaybillNumberDoesNotHaveSCACCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);

			var consignment_WithWaybillLength12 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment_WithWaybillLength12.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment_WithWaybillLength12.HVC_WaybillNumber = "123456789012";

			var consignment_WithWaybillLength13 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment_WithWaybillLength13.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment_WithWaybillLength13.HVC_WaybillNumber = "1234567890123";

			var consignment_WithWaybillLength16 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment_WithWaybillLength16.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment_WithWaybillLength16.HVC_WaybillNumber = "1234567890123456";

			var consignment_WithWaybillLength17 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment_WithWaybillLength17.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment_WithWaybillLength17.HVC_WaybillNumber = "12345678901234567";

			CombineAssertions("Consignment with waybill length over 12 without SCAC code should have message errors", () =>
			{
				ValidateAndAssert(consignment_WithWaybillLength12);
				ValidateAndAssert(consignment_WithWaybillLength13, expectedMessageErrorMessageExceeding12Chars);
				ValidateAndAssert(consignment_WithWaybillLength16, expectedMessageErrorMessageExceeding12Chars);
				ValidateAndAssert(consignment_WithWaybillLength17, expectedMessageErrorMessageExceeding12Chars);
			});
		}

		public void TestValidate_WaybillNumber_ErrorIfLengthExceedsCharacterLimit_WhenWaybillNumberDoesHaveSCACCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);

			var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;

			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ABCD";
			carrier.UI_ModeOfTransportation = "41";

			var consignment_WithWaybillLength12 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment_WithWaybillLength12.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment_WithWaybillLength12.HVC_WaybillNumber = "ABCD56789012";

			var consignment_WithWaybillLength13 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment_WithWaybillLength13.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment_WithWaybillLength13.HVC_WaybillNumber = "ABCD567890123";

			var consignment_WithWaybillLength16 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment_WithWaybillLength16.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment_WithWaybillLength16.HVC_WaybillNumber = "ABCD567890123456";

			var consignment_WithWaybillLength17 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment_WithWaybillLength17.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment_WithWaybillLength17.HVC_WaybillNumber = "ABCD5678901234567";

			CombineAssertions("Consignment with waybill length over 16 without SCAC code should have warnings", () =>
			{
				ValidateAndAssert(consignment_WithWaybillLength12);
				ValidateAndAssert(consignment_WithWaybillLength13);
				ValidateAndAssert(consignment_WithWaybillLength16);
				ValidateAndAssert(consignment_WithWaybillLength17, expectedMessageErrorMessageExceeding16Chars);
			});
		}

		void ValidateAndAssert(HVLVConsignment consignment, string expectedMessageError = null)
		{
			consignment.Validation.ValidateHVC_WaybillNumberForUSCustomsJob();
			if (!string.IsNullOrEmpty(expectedMessageError))
			{
				AssertHasMessageError(consignment.HVC_WaybillNumberInfo, expectedMessageError);
			}
			else
			{
				AssertNoMessageErrors(consignment.HVC_WaybillNumberInfo);
			}
		}

		const string expectedMessageErrorMessageExceeding12Chars = "Consignment Waybill is greater than 12 characters and does not start with a valid SCAC code.";
		const string expectedMessageErrorMessageExceeding16Chars = "Consignment Waybill that starts with valid SCAC code is greater than 16 characters.";
	}
}
