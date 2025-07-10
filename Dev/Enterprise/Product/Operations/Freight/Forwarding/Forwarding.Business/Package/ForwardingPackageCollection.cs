using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingPackageCollection : PkgPackageCollection
	{
		public ForwardingPackageCollection(ForwardingPackLine master)
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
