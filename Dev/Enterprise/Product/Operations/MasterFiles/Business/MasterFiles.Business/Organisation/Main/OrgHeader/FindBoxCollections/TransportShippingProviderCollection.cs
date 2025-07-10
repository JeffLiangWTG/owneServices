
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class TransportShippingProviderCollection : ShippingProviderCollection
	{
		public TransportShippingProviderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.ShippingLine));
		}
	}
}
