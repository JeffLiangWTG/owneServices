using CargoWise.EntityFramework;

namespace Enterprise.Freight.PortHubs.Business
{
	public class PortHubSelectionCollection : BusinessObjectCollection<PortHubSelection>
	{
		public PortHubSelectionCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public PortHubSelectionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
