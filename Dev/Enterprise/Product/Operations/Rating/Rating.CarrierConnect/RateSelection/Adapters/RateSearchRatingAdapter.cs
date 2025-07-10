using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.CarrierConnect
{
	internal class RateSearchRatingAdapter(RateQueryBusinessObject parent, BusinessObjectFactory factory)
		: RatingAdapter<RateQueryBusinessObject>(parent)
	{
		public override ILocation Origin => LocationHelper.GetCachedLocationFromString(Parent.Origin, factory);

		public override ILocation Destination => LocationHelper.GetCachedLocationFromString(Parent.Destination, factory);

		public override ZString ContainerMode => Parent.ContainerMode;

		public override RateType RateTypeToUse => Parent.RateType;

		public override IJobDatesProvider JobDatesProvider => new RateSelectorDatesProvider(Parent);

		public override FreightMode FreightMode => Parent.FreightMode;

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				if (measureSet == null)
				{
					var converter = new RateSelectorMeasuresAdapter(Parent, factory);
					measureSet = converter.Convert(AdapterType);
				}

				return measureSet;
			}
		}

		IRateableMeasureSet measureSet;

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var result = new ChargeCodeGroupCollection();
				result.AddRange(Env.Registry.Rating.FreightRatedCodes);
				return result;
			}
		}
	}
}
