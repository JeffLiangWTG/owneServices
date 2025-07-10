using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	internal class CusInBondBillDataCalculatorTest : TestCaseWithFactory
	{
		public void TestGetHouseBillIssuerCode()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				var carr01 = Factory.New<USCarrierCombined>();
				carr01.UI_Code = "TESA";
				carr01.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;
				var forwarder = Factory.New<OrgHeader>();
				forwarder.OH_Code = "FORWARDER";
				var forwarderAddress = forwarder.MainAddress;
				forwarder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "TESA", Core.Constants.CountryCodes.UnitedStates);
				consol.JK_OA_SendingForwarderAddress = forwarderAddress.PK;
				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_HouseBill = "TESA123";
				Factory.Save();
				var calculator = new CusInBondBillDataCalculator(shipment);
				AssertEquals(carr01.UI_Code, calculator.GetHouseBillIssuerCode());
			}
		}
	}
}
