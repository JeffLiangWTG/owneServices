using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.TransportCommon.Business
{
	public class PackageCollectionForTransport : ActiveBusinessObjectCollection<PkgPackage>
	{
		public PackageCollectionForTransport(DtbTransport transport)
			: base(transport.Factory, new TransportPackageRelationship(transport))
		{
		}

		#region AllowNew

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion
	}
}
