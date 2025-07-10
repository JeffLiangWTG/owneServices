using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class PkgHandlingUnitCollection : ActiveBusinessObjectCollection<PkgHandlingUnit>
	{
		public PkgHandlingUnitCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
