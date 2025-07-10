using CargoWise.EntityFramework;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbLinehaulManifestCollection : ActiveBusinessObjectCollection<DtbLinehaulManifest>
	{
		public DtbLinehaulManifestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
