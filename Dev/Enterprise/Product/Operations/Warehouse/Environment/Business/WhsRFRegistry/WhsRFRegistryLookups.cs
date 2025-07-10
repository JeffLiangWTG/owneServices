using Enterprise.Registry.Business;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsRFRegistryLookups : AutoWhsRFRegistryLookups
	{
		public WhsRFRegistryLookups(AutoWhsRFRegistry parent) : base(parent)
		{
		}

		public UOMPackTypesList UOMPackTypes => base.Factory.GetCachedValue("WhsRFRegistryLookups|UOMPackTypes", () =>
		{
			var uomPackTypes = new UOMPackTypesList();
			uomPackTypes.AddPairIfNotExist(WhsRFRegistry.DefaultUOMPackType, WhsRFRegistry.DefaultUOMPackType);
			return uomPackTypes;
		});
	}
}
