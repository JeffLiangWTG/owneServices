using CargoWise.ComponentModel;

namespace Enterprise.Freight.Business
{
	public class TransportRoadValidation : TransportValidation
	{
		public TransportRoadValidation(Transport transport)
			: base(transport)
		{
		}

		protected override void CheckJW_VoyageFlight()
		{
			base.CheckJW_VoyageFlight();

			if (TransportSupporterWithSchedule?.SupportETD ?? true)
			{
				if (!Parent.JW_VoyageFlightInfo.HasErrors() && Parent.JW_VoyageFlight.IsEmpty)
				{
					Parent.JW_VoyageFlightInfo.AddWarning(Res.GetString("e052e7c8-7378-4be1-8318-62e08fdbdf09", "It is recommended that you enter a specific truck reference."));
				}
			}
		}
	}
}
