//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRateOneOffCarrierValidation
//
//    This class should be used for overriding validation in AutoRateOneOffCarrierValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Rating.Business
{
	public class RateOneOffCarrierValidation : AutoRateOneOffCarrierValidation
	{
		public RateOneOffCarrierValidation(AutoRateOneOffCarrier parent) : base(parent)
		{
		}

		protected override void CheckTTC_OH_Carrier()
		{
			if (!Parent.IsInDatabase || Parent.TTC_OH_CarrierInfo.HasChanges)
			{
				foreach (RateOneOffCarrier carrier in ((RateOneOffCarrier)Parent).OneOffShipment.PossibleCarriers)
				{
					if (carrier != Parent && !carrier.TTC_OH_Carrier.IsEmpty && carrier.TTC_OH_Carrier == Parent.TTC_OH_Carrier)
					{
						Parent.TTC_OH_CarrierInfo.AddError(Res.GetString("8835cb26-f11c-4ac6-aa71-9ab8229a9b85", "Carrier has already been added."));
					}
				}
			}
		}

		protected override void CheckTTC_Frequency()
		{
			base.CheckTTC_Frequency();

			if (Parent.TTC_Frequency < 0)
			{
				Parent.TTC_FrequencyInfo.AddError(ErrorMessages.FrequencyGreaterThanZero);
			}
			else if (Parent.TTC_Frequency == 0 && !Parent.TTC_FrequencyUnit.IsEmpty)
			{
				Parent.TTC_FrequencyInfo.AddError(ErrorMessages.NoFrequency);
			}

			ValidateTTC_FrequencyUnit();
		}

		protected override void CheckTTC_FrequencyUnit()
		{
			base.CheckTTC_FrequencyUnit();
			if (!Parent.Lookups.FrequencyUnits.ContainsCode(Parent.TTC_FrequencyUnitInfo.Value))
			{
				Parent.TTC_FrequencyUnitInfo.AddError(ErrorMessages.InvalidFrequencyUnit);
			}
			else if (Parent.TTC_FrequencyUnit.IsEmpty && Parent.TTC_Frequency > 0)
			{
				Parent.TTC_FrequencyUnitInfo.AddError(ErrorMessages.NoFrequencyUnit);
			}

			ValidateTTC_Frequency();
		}

		protected override void CheckTTC_TransitTime()
		{
			base.CheckTTC_TransitTime();

			if (Parent.Lookups.TransitTimesList != null)
			{
				if (!Parent.TTC_TransitTime.IsEmpty && !Parent.Lookups.TransitTimesList.ContainsCode(Parent.TTC_TransitTimeInfo.Value))
				{
					Parent.TTC_TransitTimeInfo.AddError(ErrorMessages.InvalidTransitTime);
				}
			}
		}
	}
}
