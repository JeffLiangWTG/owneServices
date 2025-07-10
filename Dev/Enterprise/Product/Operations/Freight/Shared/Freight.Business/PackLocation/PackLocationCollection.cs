using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class PackLocationCollection : DependentBusinessObjectCollection<PackLocation, PackLine>
	{
		public PackLocationCollection(PackLine parent, BusinessObjectFactory factory) : base(parent, factory)
		{
			this.Parent = parent;
		}

		protected PackLine Parent;

		public ZInt TotalPackages
		{
			get
			{
				ZInt totalPackages = 0;
				foreach (PackLocation location in this)
				{
					totalPackages += location.JQ_NoPackages;
				}
				return totalPackages;
			}
		}
	}
}
