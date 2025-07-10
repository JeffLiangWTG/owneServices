using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing;

public class CustomsFeatureControlHelperTest : TestCaseWithFactory
{
	public void TestIsVietnamManifestFeatureEnabled()
	{
		AssertEquals("Pre-condition", false, CustomsFeatureControlHelper.IsVietnamManifestFeatureEnabled);

		var featureControlManagerMock = CustomsFeatureControlTestHelper.CreateVietnamManifestFeatureControlMock(true);
		using (ObjectFactory.Substitute(featureControlManagerMock.Object))
		{
			AssertEquals(true, CustomsFeatureControlHelper.IsVietnamManifestFeatureEnabled);
		}

		featureControlManagerMock = CustomsFeatureControlTestHelper.CreateVietnamManifestFeatureControlMock(false);
		using (ObjectFactory.Substitute(featureControlManagerMock.Object))
		{
			AssertEquals(false, CustomsFeatureControlHelper.IsVietnamManifestFeatureEnabled);
		}
	}
}
