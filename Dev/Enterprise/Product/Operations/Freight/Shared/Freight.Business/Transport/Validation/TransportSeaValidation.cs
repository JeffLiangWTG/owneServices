using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Extensions;

namespace Enterprise.Freight.Business
{
	public class TransportSeaValidation : TransportValidation
	{
		public TransportSeaValidation(Transport transport)
			: base(transport)
		{
		}

		protected override void CheckJW_Vessel_WithoutSailing()
		{
			base.CheckJW_Vessel_WithoutSailing();

			if (Parent.JW_IsLinked)
			{
				NotifyIfNotEntered(Parent.JW_VesselInfo);
				ListValidation.ErrorIfInvalidCode(Parent.JW_VesselInfo, Parent.RefVessels);
			}
			else
			{
				Parent.JW_VesselInfo.ValidateVesselIsValid(() => Parent.Vessel);
			}
		}

		protected override void CheckJW_VoyageFlight()
		{
			base.CheckJW_VoyageFlight();

			if (!Parent.JW_VoyageFlightInfo.HasErrors() && (Parent.JW_IsLinked || !Parent.JW_VoyageFlight.IsEmpty))
			{
				if (Parent.JW_VoyageFlight.IsEmpty)
				{
					Parent.JW_VoyageFlightInfo.AddNotification(notificationType, Res.GetString("3b5977a2-277f-4ba2-8115-3c3a14d8e398", "Please enter a voyage number."));
				}
				else if (Regex.IsMatch(Parent.JW_VoyageFlight, "^([Vv][0-9]*)$"))
				{
					Parent.JW_VoyageFlightInfo.AddWarning(Res.GetString("147e60c5-f3a1-4f38-962c-044233f3fe1b", "Voyage number should not start with a 'V' followed by numbers. The system will add this 'V' automatically."));
				}
			}
		}

		protected override void CheckCarrierPK()
		{
			base.CheckCarrierPK();

			if (!Parent.CarrierPKInfo.HasErrors() && Parent.JW_IsLinked)
			{
				if (Parent.CarrierPK.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.CarrierPKInfo);
				}
				else
				{
					ValidateRequiredSecurity(Parent.CarrierPKInfo);
				}
			}
		}
	}
}
