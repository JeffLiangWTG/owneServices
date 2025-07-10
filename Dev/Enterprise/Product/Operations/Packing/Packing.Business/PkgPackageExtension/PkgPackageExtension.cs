using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	public class PkgPackageExtension : AutoPkgPackageExtension
	{
		public PkgPackageExtension(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region Package

		public PkgPackage Package
		{
			get { return Factory.Load<PkgPackage>(KPN_KP_Package); }
		}

		#endregion

		#endregion
	}
}
