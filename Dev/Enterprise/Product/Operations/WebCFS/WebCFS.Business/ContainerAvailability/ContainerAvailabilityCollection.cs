
using CargoWise.EntityFramework;

namespace Enterprise.WebCFS.Business
{
	/// <summary>
	/// Summary description for ContainerAvailabilityCollection.
	/// </summary>
	public class ContainerAvailabilityCollection : BusinessObjectCollection<ContainerAvailability>
	{
		public ContainerAvailabilityCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
