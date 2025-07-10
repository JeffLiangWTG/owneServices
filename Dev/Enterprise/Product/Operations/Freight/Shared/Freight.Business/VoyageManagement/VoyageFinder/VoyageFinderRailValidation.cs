using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	internal sealed class VoyageFinderRailValidation : VoyageFinderValidation
	{
		public VoyageFinderRailValidation(VoyageFinder finder)
			: base(finder) { }

		protected override void CheckJV_RV_NKVessel()
		{
			base.CheckJV_RV_NKVessel();
			MandatoryValidation.CheckEntered(Parent.JV_RV_NKVesselInfo);
		}
	}
}
