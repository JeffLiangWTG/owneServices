using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public sealed class AirportToAirportDeliveryDueDateCalculatorTest : DeliveryDueDateCalculatorBaseTest
	{
		public void TestCalculationLog()
		{
			var expectedSuccesfulResult = "ETA of last transport leg has been selected as [Delivery Due Date].";

			AdditionalBasicSetUp();
			Factory.Save();
			var calculator = GetNewDeliveryDueDateCalculatorForTest();
			var result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Should successfully calculate DDD based on last transport leg's ETA", expectedSuccesfulResult, result.CalculationLog);

			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			calculator = GetNewDeliveryDueDateCalculatorForTest();
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Consolidation is not Direct, should show failure message", "Attached consolidation's type is not Direct.", result.CalculationLog);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			TransportLeg.JW_ETA = ZDateTime.Empty;
			calculator = GetNewDeliveryDueDateCalculatorForTest();
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("ETA is empty, should show failure message", "ETA of the last transport leg is blank.", result.CalculationLog);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			TransportLeg.JW_ETA = new ZDateTime(2023, 2, 12, 6, 0, 0);
			calculator = GetNewDeliveryDueDateCalculatorForTest();
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Should successfully calculate DDD based on last transport leg's ETA", expectedSuccesfulResult, result.CalculationLog);

			Shipment.Consols.AddNew();
			calculator = GetNewDeliveryDueDateCalculatorForTest();
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Has more than one consolidation, should show failure message", "Shipment has been attached to more than one consolidation.", result.CalculationLog);
		}

		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT;

		protected override ZString AssertMessage => "Expected Delivery Due Date: 6PM Sunday 12th Feb";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2023, 2, 12, 6, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
			Consol = Shipment.Consols.AddNew();
			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			TransportLeg = Consol.Transports[0];
			TransportLeg.JW_ETA = new ZDateTime(2023, 2, 12, 6, 0, 0);
			TransportLeg.JW_RL_NKDiscPort = "NZAKL";
		}

		ForwardingConsol Consol;
		Transport TransportLeg;

		protected override void AdditionalFallBackSetUp()
		{
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.Consols[0].Transports.RemoveAndDeleteAll();
		}
	}
}
