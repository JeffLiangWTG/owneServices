using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RateTransportZonesValidation : AutoRateTransportZonesValidation
	{
		public RateTransportZonesValidation(AutoRateTransportZones parent)
			: base(parent)
		{
		}

		public new RateTransportZone Parent
		{
			get { return (RateTransportZone)base.Parent; }
		}

		protected override void CheckTZ_ZoneName()
		{
			base.CheckTZ_ZoneName();
			MandatoryValidation.CheckEntered(Parent.TZ_ZoneNameInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.TZ_ZoneNameInfo, Parent.TransportProvider.Zones);
		}
	}
}

