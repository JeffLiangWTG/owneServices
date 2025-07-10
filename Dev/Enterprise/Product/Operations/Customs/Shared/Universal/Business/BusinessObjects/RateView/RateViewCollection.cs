using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RateViewCollection : TariffEffectiveDatesRelatedFilteredCollection<RateView>
	{
		public RateViewCollection(TariffView parentTariff)
			: this(parentTariff, false, false)
		{
		}

		protected RateViewCollection(TariffView parentTariff, bool enableEffectiveDataGrouping, bool enableEffectiveDateFilter)
			: base(parentTariff, RateViewSchema.ZZ2_ZZ1_ParentTariffOrNationalCode, enableEffectiveDataGrouping, enableEffectiveDateFilter)
		{
		}

		public IEnumerable<RateView> GetRatesFor(ZDateTime dateOfValuation)
		{
			return this.Where(x => x.ZZ2_StartDate <= dateOfValuation && x.ZZ2_EndDate >= dateOfValuation);
		}
	}
}
