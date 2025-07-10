using Enterprise.Packing.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingPackageHandlingUnitHandlingUnitDivotCollection : PkgPackageHandlingUnitHandlingUnitDivotCollection
	{
		public ForwardingPackageHandlingUnitHandlingUnitDivotCollection(ForwardingPackage master)
			: base(master)
		{
		}

		public new ForwardingPackageHandlingUnitDivot AddNew()
		{
			return (ForwardingPackageHandlingUnitDivot)base.AddNew();
		}

		public new ForwardingPackageHandlingUnitDivot this[int index]
		{
			get { return (ForwardingPackageHandlingUnitDivot)base[index]; }
		}
	}
}
