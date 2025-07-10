using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgCustomLabelsTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var orgCustomLabels = Factory.New<OrgCustomLabels>();
			var pkgPackageJobTypeDecider = new OrgCustomLabelsTypeDecider();
			var decidedType = pkgPackageJobTypeDecider.GetTypeForLoad(((INeedRow)orgCustomLabels).Row, Factory);
			AssertEquals(typeof(OrgCustomLabels), decidedType);

			orgCustomLabels.OT_Type = OrgConstants.CustomLabelType.OverrideExportDoc;
			decidedType = pkgPackageJobTypeDecider.GetTypeForLoad(((INeedRow)orgCustomLabels).Row, Factory);
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IOrgCustomLabels>(), decidedType);

			orgCustomLabels.OT_Type = OrgConstants.CustomLabelType.OverrideImportDoc;
			decidedType = pkgPackageJobTypeDecider.GetTypeForLoad(((INeedRow)orgCustomLabels).Row, Factory);
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IOrgCustomLabels>(), decidedType);

			orgCustomLabels.OT_Type = OrgConstants.CustomLabelType.Document;
			decidedType = pkgPackageJobTypeDecider.GetTypeForLoad(((INeedRow)orgCustomLabels).Row, Factory);
			AssertEquals(typeof(OrgCustomLabels), decidedType);
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(OrgCustomLabels), new OrgCustomLabelsTypeDecider().GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(OrgCustomLabels), new OrgCustomLabelsTypeDecider().GetTypeForNew());
		}
	}
}
