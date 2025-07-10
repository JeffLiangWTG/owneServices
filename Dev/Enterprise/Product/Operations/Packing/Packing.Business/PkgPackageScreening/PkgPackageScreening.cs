using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	public class PkgPackageScreening : AutoPkgPackageScreening
	{
		public PkgPackageScreening(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }
	}
}
