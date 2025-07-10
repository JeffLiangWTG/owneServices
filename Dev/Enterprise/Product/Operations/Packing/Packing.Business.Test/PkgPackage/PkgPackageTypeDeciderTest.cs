using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var package = Factory.New<PkgPackage>();
			var pkgPackageTypeDecider = new PkgPackageTypeDecider();
			var decidedType = pkgPackageTypeDecider.GetTypeForLoad(((INeedRow)package).Row, Factory);
			Assert(typeof(PkgPackage).IsAssignableFrom(decidedType));

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentTableCode = CusPackingListSchema.Constants.Prefix;
			package = packageJob.Packages.AddNew();
			decidedType = pkgPackageTypeDecider.GetTypeForLoad(((INeedRow)package).Row, Factory);
			Assert(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusPackage>().IsAssignableFrom(decidedType));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			decidedType = pkgPackageTypeDecider.GetTypeForLoad(((INeedRow)package).Row, Factory);
			Assert(ObjectFactory.GetType<Enterprise.Integration.Customs.TW.ICusPackage>().IsAssignableFrom(decidedType));
		}

		public void TestGetTypeForBinding()
		{
			var decidedType = new PkgPackageTypeDecider().GetTypeForBinding();
			Assert(typeof(PkgPackage).IsAssignableFrom(decidedType));
		}

		public void TestGetTypeForNew()
		{
			var decidedType = new PkgPackageTypeDecider().GetTypeForNew();
			Assert(typeof(PkgPackage).IsAssignableFrom(decidedType));
		}
	}
}
