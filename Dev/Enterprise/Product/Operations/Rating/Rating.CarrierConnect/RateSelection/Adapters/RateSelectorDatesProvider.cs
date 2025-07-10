using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;

namespace Enterprise.Rating.CarrierConnect
{
	internal class RateSelectorDatesProvider(RateQueryBusinessObject parent) : JobDatesProvider<RateQueryBusinessObject>(parent)
	{
		public string TransitTimeOverride { get; set; }

		public override string TransitTime => TransitTimeOverride;

		protected override ZDateTime GetCostingAutoratingDateOverrideCore() => Parent.EffectiveDate ?? ZDateTime.Empty;
	}
}
