using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingPackageHandlingUnitDivot : PkgPackageHandlingUnitDivot
	{
		public ForwardingPackageHandlingUnitDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new ForwardingPackage Package => (ForwardingPackage)base.Package;

		protected override PkgPackage GetPackageCore() => Factory.Load<ForwardingPackage>(KPD_KP_Package);
	}
}
