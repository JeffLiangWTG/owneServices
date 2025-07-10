using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsLocationTypeLookups : AutoWhsLocationTypeLookups
	{
		public WhsLocationTypeLookups(AutoWhsLocationType parent)
			: base(parent)
		{
		}

		public LocationClasses LocationClasses => base.Factory.GetCachedValue("WhsLocationTypeLookups|LocationClasses", () => new LocationClasses());

		public CycleCountGranularities CycleCountGranularities => Factory.GetCachedValue("WhsLocationTypeLookups|CycleCountGranularities", () => new CycleCountGranularities());

		public CodeDescriptionPairList TemperatureUnits => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.TemperatureTypes);
	}
}
