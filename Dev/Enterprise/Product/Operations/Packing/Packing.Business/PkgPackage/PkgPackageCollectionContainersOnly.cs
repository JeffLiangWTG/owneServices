using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public sealed class PkgPackageCollectionContainersOnly : PkgPackageCollectionCommon
	{
		public PkgPackageCollectionContainersOnly(PkgPackageJob master)
			: base(master, new ZQuery(PkgPackageSchema.KP_F3_NKPackType, Constants.PkgUnit.Container))
		{
		}

		protected override ZString GetPackTypeForNewElement(PkgPackage newElement)
		{
			return Constants.PkgUnit.Container;
		}

		protected override PkgPackageJob PackageJob
		{
			get { return (PkgPackageJob)Relationship.Master; }
		}
	}
}

