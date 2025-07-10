using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Summary description for PackLocationNonDependentCollection.
	/// </summary>
	public class PackLocationNonDependentCollection : BusinessObjectCollection<PackLocation>
	{
		public PackLocationNonDependentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public PackLocationNonDependentCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
