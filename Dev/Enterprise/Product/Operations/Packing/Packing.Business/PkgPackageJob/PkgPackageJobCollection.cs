using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class PkgPackageJobCollection : ActiveBusinessObjectCollection<PkgPackageJob>
	{
		/// <summary>
		/// This constructor is generally only used by the module grid.
		/// </summary>
		public PkgPackageJobCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
