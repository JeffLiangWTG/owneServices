using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingPackageLookups : PkgPackageLookups
	{
		public ForwardingPackageLookups(ForwardingPackage parent)
			: base(parent)
		{
		}

		new ForwardingPackage Parent => (ForwardingPackage)base.Parent;

		public override RefPackTypeCollection PackTypes
		{
			get { return GetPackTypes(Factory, Factory.Load<ForwardingPackageJob>(Parent.KP_KJ_ParentPackageJob) ); }
		}
	}
}
