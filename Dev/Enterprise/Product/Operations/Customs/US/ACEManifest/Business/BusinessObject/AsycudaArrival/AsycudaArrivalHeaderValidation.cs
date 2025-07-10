using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaArrivalHeaderValidation : ASYCUDA.Business.AsycudaArrivalHeaderValidation
	{
		public AsycudaArrivalHeaderValidation(ASYCUDA.Business.AsycudaArrivalHeader parent) : base(parent)
		{
		}

		protected override void CheckATH_VoyageFlightNo()
		{
			var carrierCode = Parent.ManifestHeader?.AMA_CarrierCode ?? ZString.Empty;
			var errorString = AIMFlightHelper.CheckCombinedCarrierFlight(Parent.ATH_VoyageFlightNo, carrierCode);
			if (!errorString.IsEmpty)
			{
				Parent.ATH_VoyageFlightNoInfo.AddMessageError(errorString);
			}

			if (FlightDetailsAreDuplicated)
			{
				Parent.ATH_VoyageFlightNoInfo.AddError(ValidationConstants.FlightNumberIsDuplicated);
			}
		}

		protected override void CheckATH_ETAAtDischargePort()
		{
			base.CheckATH_ETAAtDischargePort();
			ValidateATH_VoyageFlightNo();
		}

		bool FlightDetailsAreDuplicated
		{
			get
			{
				var currentArrival = Parent;
				if (!currentArrival.IsInDatabase || currentArrival.ATH_VoyageFlightNoInfo.HasChanges || currentArrival.ATH_ETAAtDischargePortInfo.HasChanges)
				{
					var arrivalHeaders = currentArrival.ManifestHeader?.ArrivalHeaders;
					if (arrivalHeaders != null)
					{
						foreach (AsycudaArrivalHeader arrivalHeader in arrivalHeaders)
						{
							if (currentArrival != arrivalHeader)
							{
								if (currentArrival.ATH_VoyageFlightNo == arrivalHeader.ATH_VoyageFlightNo &&
									currentArrival.ATH_ETAAtDischargePort == arrivalHeader.ATH_ETAAtDischargePort)
								{
									return true;
								}
							}
						}
					}
				}

				return false;
			}
		}
	}
}
