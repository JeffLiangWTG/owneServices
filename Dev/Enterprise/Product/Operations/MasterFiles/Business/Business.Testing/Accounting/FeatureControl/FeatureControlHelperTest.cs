using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class FeatureControlHelperTest : TestCaseWithFactory
	{
		public void TestIsSubTypeFeatureEnabled()
		{
			AssertEquals("Pre-condition", false, FeatureControlHelper.IsKoreaSouthComplianceSubTypeFeatureEnabled);

			var featureControlTestHelper = new FeatureControlTestDataFactory();

			var featureControlManagerMock = featureControlTestHelper.CreateKoreaSouthComplianceSubTypeFeatureControlMock(true);

			using (ObjectFactory.Substitute(featureControlManagerMock.Object))
			{
				AssertEquals(true, FeatureControlHelper.IsKoreaSouthComplianceSubTypeFeatureEnabled);
			}

			featureControlManagerMock = featureControlTestHelper.CreateKoreaSouthComplianceSubTypeFeatureControlMock(false);

			using (ObjectFactory.Substitute(featureControlManagerMock.Object))
			{
				AssertEquals(false, FeatureControlHelper.IsKoreaSouthComplianceSubTypeFeatureEnabled);
			}
		}

		public void TestAdvancePaymentFeatureEnabled()
		{
			AssertEquals("Pre-condition", false, FeatureControlHelper.IsAdvancePaymentFeatureEnabled);

			var featureControlTestHelper = new FeatureControlTestDataFactory();

			var featureControlManagerMock = featureControlTestHelper.CreateAdvancePaymentFeatureControlMock(true);

			using (ObjectFactory.Substitute(featureControlManagerMock.Object))
			{
				AssertEquals(true, FeatureControlHelper.IsAdvancePaymentFeatureEnabled);
			}

			featureControlManagerMock = featureControlTestHelper.CreateAdvancePaymentFeatureControlMock(false);

			using (ObjectFactory.Substitute(featureControlManagerMock.Object))
			{
				AssertEquals(false, FeatureControlHelper.IsAdvancePaymentFeatureEnabled);
			}
		}

		public void TestIsCWD365IntegrationFeatureEnabled()
		{
			AssertEquals("Pre-condition", false, FeatureControlHelper.IsCWD365IntegrationFeatureEnabled);

			var mockIFeatureData = new Mock<IFeatureData>();
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();

			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CWD365IntegrationFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				AssertEquals(true, FeatureControlHelper.IsCWD365IntegrationFeatureEnabled);
			}

			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CWD365IntegrationFeature, CancellationToken.None)).Returns(Task.FromResult((IFeatureData)null));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				AssertEquals(false, FeatureControlHelper.IsCWD365IntegrationFeatureEnabled);
			}
		}
	}
}
