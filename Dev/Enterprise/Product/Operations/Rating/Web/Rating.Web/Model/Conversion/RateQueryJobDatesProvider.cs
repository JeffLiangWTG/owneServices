using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Web.Model.Conversion
{
	internal class RateQueryJobDatesProvider : JobDatesProvider<RateQuery>
	{
		public RateQueryJobDatesProvider(RateQuery parent) : base(parent)
		{
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return new ZDateTime(Parent.EffectiveDate.ToLocalTime().DateTime);
		}

		protected override ZDateTime GetArrivalDateCore()
		{
			return new ZDateTime(Parent.EffectiveDate.ToLocalTime().DateTime);
		}
	}
}
