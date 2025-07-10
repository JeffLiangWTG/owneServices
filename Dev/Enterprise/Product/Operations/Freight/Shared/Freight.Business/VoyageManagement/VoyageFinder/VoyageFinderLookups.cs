using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class VoyageFinderLookups : ZLookups
	{
		public VoyageFinderLookups(VoyageFinder parent)
			: base(parent) { }

		public RefVesselCollection Vessels
		{
			get { return new RefVesselCollection(Factory); }
		}

		public SeaShippingProviderCollection Carriers
		{
			get { return new SeaShippingProviderCollection(Factory); }
		}
	}
}
