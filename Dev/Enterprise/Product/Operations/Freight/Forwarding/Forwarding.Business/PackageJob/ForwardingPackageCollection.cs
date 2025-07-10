using Enterprise.Packing.Business;

namespace Enterprise.Freight.Forwarding.Business.PackageJob
{
	public class ForwardingPackageCollection : PkgPackageCollection
	{
		public ForwardingPackageCollection(ForwardingPackageJob master)
			: base(master)
		{
		}

		public ForwardingPackageCollection(ForwardingPackage master)
			: base(master)
		{
		}

		public new ForwardingPackage AddNew()
		{
			return (ForwardingPackage)base.AddNew();
		}

		public new ForwardingPackage this[int index]
		{
			get { return (ForwardingPackage)base[index]; }
		}
	}
}
