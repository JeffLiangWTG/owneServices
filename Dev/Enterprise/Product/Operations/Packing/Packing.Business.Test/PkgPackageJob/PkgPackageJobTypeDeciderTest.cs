using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageJobTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var pkgPackageJobTypeDecider = new PkgPackageJobTypeDecider();
			var decidedType = pkgPackageJobTypeDecider.GetTypeForLoad(((INeedRow)packageJob).Row, Factory);
			Assert(typeof(PkgPackageJob).IsAssignableFrom(decidedType));

			packageJob.KJ_ParentTableCode = CusPackingListSchema.Constants.Prefix;
			decidedType = pkgPackageJobTypeDecider.GetTypeForLoad(((INeedRow)packageJob).Row, Factory);
			Assert(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusPackageJob>().IsAssignableFrom(decidedType));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			decidedType = pkgPackageJobTypeDecider.GetTypeForLoad(((INeedRow)packageJob).Row, Factory);
			Assert(ObjectFactory.GetType<Enterprise.Integration.Customs.TW.ICusPackageJob>().IsAssignableFrom(decidedType));

			packageJob.KJ_ParentTableCode = DtbBookingConsolidationSchema.Constants.Prefix;
			decidedType = pkgPackageJobTypeDecider.GetTypeForLoad(((INeedRow)packageJob).Row, Factory);
			Assert(ObjectFactory.GetType<Enterprise.Integration.TransportBooking.IDtbBookingConsolidationPkgPackageJob>().IsAssignableFrom(decidedType));
		}

		public void TestGetTypeByParentTableCode()
		{
			var packageJob = Factory.New<PkgPackageJob>();

			packageJob.KJ_ParentTableCode = CusPackingListSchema.Constants.Prefix;
			var decidedType = PkgPackageJobTypeDecider.GetTypeByParentTableCode(CusPackingListSchema.Constants.Prefix);
			Assert(typeof(PkgPackageJob).IsAssignableFrom(decidedType));

			packageJob.KJ_ParentTableCode = DtbBookingConsolidationSchema.Constants.Prefix;
			decidedType = PkgPackageJobTypeDecider.GetTypeByParentTableCode(DtbBookingConsolidationSchema.Constants.Prefix);
			Assert(typeof(PkgPackageJob).IsAssignableFrom(decidedType));
		}

		public void TestGetTypeForBinding()
		{
			var decidedType = new PkgPackageJobTypeDecider().GetTypeForBinding();
			Assert(typeof(PkgPackageJob).IsAssignableFrom(decidedType));
		}

		public void TestGetTypeForNew()
		{
			var decidedType = new PkgPackageJobTypeDecider().GetTypeForNew();
			Assert(typeof(PkgPackageJob).IsAssignableFrom(decidedType));
		}
	}
}
