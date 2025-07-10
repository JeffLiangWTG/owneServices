using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class TransportRailValidation : TransportValidation
	{
		public TransportRailValidation(Transport transport)
			: base(transport)
		{
		}

		protected override void CheckJW_Vessel_WithoutSailing()
		{
			if (Parent.JW_IsLinked)
			{
				MandatoryValidation.CheckEntered(Parent.JW_VesselInfo);
			}
		}

		protected override void CheckJW_VoyageFlight()
		{
			base.CheckJW_VoyageFlight();

			if (!Parent.JW_VoyageFlightInfo.HasErrors() && Parent.JW_IsLinked && Parent.JW_VoyageFlight.IsEmpty)
			{
				Parent.JW_VoyageFlightInfo.AddError(Res.GetString("d0d51d5a-93da-4448-8242-32ec8d140f61", "Please enter a journey number."));
			}
		}
	}
}
