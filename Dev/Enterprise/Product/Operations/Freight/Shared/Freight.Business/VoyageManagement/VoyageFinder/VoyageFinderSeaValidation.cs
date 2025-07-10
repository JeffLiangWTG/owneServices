using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	internal sealed class VoyageFinderSeaValidation : VoyageFinderValidation
	{
		public VoyageFinderSeaValidation(VoyageFinder finder)
			: base(finder) { }

		protected override void CheckJV_RV_NKVessel()
		{
			base.CheckJV_RV_NKVessel();
			MandatoryValidation.CheckEntered(Parent.JV_RV_NKVesselInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JV_RV_NKVesselInfo, Parent.Lookups.Vessels);
		}

		protected override void CheckJV_VoyageFlight()
		{
			base.CheckJV_VoyageFlight();
			MandatoryValidation.CheckEntered(Parent.JV_VoyageFlightInfo);

			if (!Parent.JV_VoyageFlight.IsEmpty && Parent.JV_VoyageFlight.StartsWith("V"))
			{
				Parent.JV_VoyageFlightInfo.AddWarning(Res.GetString("3b3b2582-0258-47bd-88de-6840ae5fe552", "Voyage Number should not start with a 'V'. The system will add this automatically."));
			}
		}

		protected override void CheckCarrierPK()
		{
			base.CheckCarrierPK();
			ListValidation.ErrorIfInvalidPK(Parent.CarrierPKInfo);
		}
	}
}
