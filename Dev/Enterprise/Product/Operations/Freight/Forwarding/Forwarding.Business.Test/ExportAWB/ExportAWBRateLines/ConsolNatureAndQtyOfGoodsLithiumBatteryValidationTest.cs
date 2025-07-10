using Enterprise.Core;
using Enterprise.Freight.Forwarding.AWB.Business;
using NUnit.Framework;
using AWBConstants = Enterprise.Core.Constants.AWB;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ConsolNatureAndQtyOfGoodsLithiumBatteryValidation))]
	sealed class ConsolNatureAndQtyOfGoodsLithiumBatteryValidationTest : Forwarding.AWB.Business.Testing.NatureAndQtyOfGoodsLithiumBatteryValidationTest
	{
		public void TestValidateFlightIsCargo_HasLithiumBatteries()
		{
			TestValidateFlightIsCargoOnlyProperty("Precondition: No lithium batteries should not have warnings",
				expectedWarning: false,
				hasLithiumBatteries: false,
				hasPassengerFlight: true,
				hasNoCargoOnlyRateLine: true);

			TestValidateFlightIsCargoOnlyProperty("Passenger flights & No cargo only rate lines should have warning",
				expectedWarning: true,
				hasLithiumBatteries: true,
				hasPassengerFlight: true,
				hasNoCargoOnlyRateLine: true);

			TestValidateFlightIsCargoOnlyProperty("Passenger flights with lithium batteries should have warning",
				expectedWarning: true,
				hasLithiumBatteries: true,
				hasPassengerFlight: true,
				hasNoCargoOnlyRateLine: false);

			TestValidateFlightIsCargoOnlyProperty("Lithium batteries when no cargo only rate lines should have warning",
				expectedWarning: true,
				hasLithiumBatteries: true,
				hasPassengerFlight: false,
				hasNoCargoOnlyRateLine: true);

			TestValidateFlightIsCargoOnlyProperty("Lithium batteries with no passenger flights with cargo only rate lines should have no warning",
				expectedWarning: false,
				hasLithiumBatteries: true,
				hasPassengerFlight: false,
				hasNoCargoOnlyRateLine: false);
		}

		void TestValidateFlightIsCargoOnlyProperty(string message, bool expectedWarning, bool hasLithiumBatteries, bool hasPassengerFlight, bool hasNoCargoOnlyRateLine)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports[0].JW_IsCargoOnly = !hasPassengerFlight;

			var header = consol.AWBHeader;

			header.AWBRateLine5.ER_NatureAndQtyOfGoodsType = AWBConstants.NatureAndQtyOfGoodsTypes.LithiumBattery;
			header.AWBRateLine5.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = hasLithiumBatteries ? AWBConstants.LithiumBatteryTypes.Codes.PI968 : AWBConstants.LithiumBatteryTypes.Codes.PI967;
			header.AWBRateLine6.ER_NatureAndQtyOfGoodsType = AWBConstants.NatureAndQtyOfGoodsTypes.GoodsDescription;
			header.AWBRateLine6.NatureAndQtyOfGoodsDescription = hasNoCargoOnlyRateLine ? "NOT Cargo Aircraft Only" : "Cargo Aircraft Only";

			header.AWBRateLine5.NatureAndQtyOfGoodsLithiumBattery.Validation.ValidateLithiumBatteryType();

			var warningMessage = "Packing Instruction PI968 for lithium batteries implies that cargo cannot be uplifted on a passenger flight. Ensure that all flights for this MAWB are cargo only and add G segment in the Nature and Quantity of Goods saying \"Cargo Aircraft Only\".";
			if (expectedWarning)
			{
				AssertHasWarning(message, header.AWBRateLine5.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryTypeInfo, warningMessage);
			}
			else
			{
				AssertNoWarning(message, header.AWBRateLine5.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryTypeInfo, warningMessage);
			}
		}

		ForwardingConsol SetupConsolForIsCargoOnlyValidation(bool hasLithiumBatteries, bool hasPassengerFlight, bool hasNoCargoOnlyRateLine)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var header = consol.AWBHeader;

			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_IsCargoOnly = !hasPassengerFlight;

			header.AWBRateLine5.ER_NatureAndQtyOfGoodsType = AWBConstants.NatureAndQtyOfGoodsTypes.LithiumBattery;
			header.AWBRateLine5.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = hasLithiumBatteries ? AWBConstants.LithiumBatteryTypes.Codes.PI968 : AWBConstants.LithiumBatteryTypes.Codes.PI967;

			header.AWBRateLine6.ER_NatureAndQtyOfGoodsType = AWBConstants.NatureAndQtyOfGoodsTypes.GoodsDescription;
			header.AWBRateLine6.NatureAndQtyOfGoodsDescription = hasNoCargoOnlyRateLine ? "NOT Cargo Aircraft Only" : "Cargo Aircraft Only";

			return consol;
		}

		public void TestValidateFlightIsCargoOnly_VariableMessage()
		{
			var consol = SetupConsolForIsCargoOnlyValidation(true, true, false);
			var header = consol.AWBHeader;

			var warningMsgPi968 = "Packing Instruction PI968 for lithium batteries implies that cargo cannot be uplifted on a passenger flight. Ensure that all flights for this MAWB are cargo only and add G segment in the Nature and Quantity of Goods saying \"Cargo Aircraft Only\".";
			var warningMsgPi965 = "Packing Instruction PI965 for lithium batteries implies that cargo cannot be uplifted on a passenger flight. Ensure that all flights for this MAWB are cargo only and add G segment in the Nature and Quantity of Goods saying \"Cargo Aircraft Only\".";

			consol.AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = AWBConstants.LithiumBatteryTypes.Codes.PI965;
			AssertHasWarning(header.AWBRateLine5.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryTypeInfo, warningMsgPi965);
			consol.AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = AWBConstants.LithiumBatteryTypes.Codes.PI968;
			AssertHasWarning(header.AWBRateLine5.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryTypeInfo, warningMsgPi968);
		}

		public void TestValidateSpecialHandlingCodeSetToCAO()
		{
			var consol = SetupConsolForIsCargoOnlyValidation(true, true, false);
			var header = consol.AWBHeader;

			header.AWBRateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			header.AWBRateLine1.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Core.Constants.AWB.LithiumBatteryTypes.Codes.PI967;
			AssertNoErrors("Lithium Battery Type is not PI968.", header.AWBRateLine1.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryTypeInfo);

			header.AWBRateLine3.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Core.Constants.AWB.LithiumBatteryTypes.Codes.PI968;
			header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.Validation.ValidateAll();
			AssertHasWarning("Special handling code is not set to CAO for PI968.", header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryTypeInfo, "There must be a special handling code set to CAO when PI968 is chosen.");

			header.AWBRateLine3.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Core.Constants.AWB.LithiumBatteryTypes.Codes.PI965;
			header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.Validation.ValidateAll();
			AssertHasWarning("Special handling code is not set to CAO for PI965.", header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryTypeInfo, "There must be a special handling code set to CAO when PI965 is chosen.");

			var handling = header.AWBSpecialHandlingItems.AddNew();
			handling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoAircraftOnly;

			header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.Validation.ValidateAll();
			AssertNoErrors("Special handling code is set to CAO.", header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryTypeInfo);
		}

		protected override Forwarding.AWB.Business.NatureAndQtyOfGoodsValidation GetNewValidation(Forwarding.AWB.Business.NatureAndQtyOfGoods parent)
		{
			return new ConsolNatureAndQtyOfGoodsLithiumBatteryValidation((ConsolNatureAndQtyOfGoodsLithiumBattery)parent);
		}

		protected override Forwarding.AWB.Business.NatureAndQtyOfGoods GetNewParent()
		{
			return new ConsolNatureAndQtyOfGoodsLithiumBattery(Factory.New<ConsolExportAWBRateLine>());
		}
	}
}
