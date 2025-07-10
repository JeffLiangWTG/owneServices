using System;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using AWBConstants = Enterprise.Core.Constants.AWB;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class ConsolNatureAndQtyOfGoodsLithiumBatteryValidation : NatureAndQtyOfGoodsLithiumBatteryValidation
	{
		public ConsolNatureAndQtyOfGoodsLithiumBatteryValidation(ConsolNatureAndQtyOfGoodsLithiumBattery parent)
			: base(parent)
		{
		}

		ConsolExportAWBHeader ConsolExportAWBHeader => Parent.ParentRateLine.Master as ConsolExportAWBHeader;

		ForwardingConsol MAWBParentConsol => ConsolExportAWBHeader?.Consol;

		protected override void ValidateFlightIsCargoOnly()
		{
			base.ValidateFlightIsCargoOnly();

			ValidateFlightIsCargoOnly_LithiumBatteries();
			ValidateSpecialHandlingCodeSetToCAO();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-translatable string")]
		void ValidateFlightIsCargoOnly_LithiumBatteries()
		{
			if (Parent.LithiumBatteryType != AWBConstants.LithiumBatteryTypes.Codes.PI965 &&
				Parent.LithiumBatteryType != AWBConstants.LithiumBatteryTypes.Codes.PI968)
			{
				return;
			}

			var consolHasAnyPassengerFlights = MAWBParentConsol?
				.Transports.OfType<Transport>()
				.Any(transport => transport.TransportMode == Core.Constants.TransportModes.Air && !transport.JW_IsCargoOnly)
				?? false;

			var headerHasNoCargoOnlyRateLine = !ConsolExportAWBHeader.AWBRateLines.OfType<ExportAWBRateLine>().Any(rateLine =>
				string.Compare(rateLine.ER_NatureAndQtyOfGoodsType, AWBConstants.NatureAndQtyOfGoodsTypes.GoodsDescription, StringComparison.OrdinalIgnoreCase) == 0 &&
				string.Compare(rateLine.NatureAndQtyOfGoodsDescription, "Cargo Aircraft Only", StringComparison.OrdinalIgnoreCase) == 0);

			if (consolHasAnyPassengerFlights || headerHasNoCargoOnlyRateLine)
			{
				var message = Res.GetString("a2d84587-4dad-ea80-4903-bfc060552508",
					"Packing Instruction {0} for lithium batteries implies that cargo cannot be uplifted on a passenger flight. Ensure that all flights for this MAWB are cargo only and add G segment in the Nature and Quantity of Goods saying \"Cargo Aircraft Only\".",
					Parent.LithiumBatteryType);

				Parent.LithiumBatteryTypeInfo.AddWarning(message);
			}
		}

		void ValidateSpecialHandlingCodeSetToCAO()
		{
			if (Parent.LithiumBatteryType == AWBConstants.LithiumBatteryTypes.Codes.PI965 ||
				Parent.LithiumBatteryType == AWBConstants.LithiumBatteryTypes.Codes.PI968)
			{
				var rateLine = Parent.ParentRateLine;
				if (rateLine.Master != null)
				{
					if (!rateLine.Master.AWBSpecialHandlingItems.Cast<ExportAWBSpecialHandling>().Any(handling => handling.EP_SpecialHandling == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoAircraftOnly))
					{
						Parent.LithiumBatteryTypeInfo.AddWarning(Res.GetString("f3ad36c1-29f3-4e4f-bcc4-a8210c3c0e82", "There must be a special handling code set to CAO when {0} is chosen.", Parent.LithiumBatteryType));
					}
				}
			}
		}
	}
}
