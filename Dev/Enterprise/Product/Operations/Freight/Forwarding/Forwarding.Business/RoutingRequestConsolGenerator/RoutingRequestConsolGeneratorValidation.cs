using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class RoutingRequestConsolGeneratorValidation : AutoRoutingRequestConsolGeneratorValidation
	{
		public RoutingRequestConsolGeneratorValidation(AutoRoutingRequestConsolGenerator parent)
			: base(parent) { }

		protected override void CheckConsolsPerFlight()
		{
			base.CheckConsolsPerFlight();
			MandatoryValidation.CheckEntered(Parent.ConsolsPerFlightInfo);
			MandatoryValidation.CheckNotNegative(Parent.ConsolsPerFlightInfo);
		}

		protected override void CheckShipments()
		{
			base.CheckShipments();
			MandatoryValidation.CheckNotNegative(Parent.ShipmentsInfo);
		}

		protected override void CheckWeight()
		{
			base.CheckWeight();
			MandatoryValidation.CheckNotNegative(Parent.WeightInfo);
		}

		protected override void CheckWeightUnit()
		{
			base.CheckWeight();
			ListValidation.ErrorIfInvalidCode(Parent.WeightUnitInfo, Parent.Lookups.WeightUnitList);
		}

		protected override void CheckVolume()
		{
			base.CheckVolume();
			MandatoryValidation.CheckNotNegative(Parent.VolumeInfo);
		}

		protected override void CheckVolumeUnit()
		{
			base.CheckVolumeUnit();
			ListValidation.ErrorIfInvalidCode(Parent.VolumeUnitInfo, Parent.Lookups.VolumeUnitList);
		}

		protected override void CheckChargeable()
		{
			base.CheckChargeable();
			MandatoryValidation.CheckNotNegative(Parent.ChargeableInfo);
		}

		protected override void CheckAirlinePrefix()
		{
			base.CheckAirlinePrefix();

			if (!Parent.AirlinePrefix.IsEmpty)
			{
				if (!RefAirline.IsValidAirline2LetterCode(Parent.Factory, Parent.AirlinePrefix))
				{
					Parent.AirlinePrefixInfo.AddError(AirlinePrefixNotFoundErrorMessage);
				}
				else if (RefAirline.LoadFromAirline2LetterCode(Parent.Factory, Parent.AirlinePrefix) == null)
				{
					Parent.AirlinePrefixInfo.AddError(AirlinePrefixNotUniqueErrorMessage);
				}

				if (Parent.MultiDaysSelection.HasMultipleCarriers)
				{
					Parent.AirlinePrefixInfo.AddWarning(ResString.GetMultilingualString("eb3840ec-08eb-41b0-b833-348f8e822414", "There are multiple airline schedules selected."));
				}
			}
		}

		protected override void CheckServiceLevel()
		{
			base.CheckServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.ServiceLevelInfo, Parent.NeutralAirWaybillServiceLevelList);
		}

		#region Implementation

		public new RoutingRequestConsolGenerator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (RoutingRequestConsolGenerator)base.Parent; }
		}

		ResourceString AirlinePrefixNotFoundErrorMessage => ResString.GetMultilingualString("2E0BEC0C-0EA1-4CBE-A046-6450E25E8826",
			"The Airline Prefix does not match any airlines.");

		ResourceString AirlinePrefixNotUniqueErrorMessage => ResString.GetMultilingualString("4D344B54-E5D6-4652-B8EA-6BD2E6E2860B",
			"The Airline Prefix is not unique, so airline details cannot be imported into the created consol(s).");

		#endregion
	}
}
