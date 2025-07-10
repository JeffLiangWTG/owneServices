using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Moq;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing;

public class CO2eFeatureControlHelperTest : TestCaseWithFactory
{
	public void TestCarbonEmissionGreenhouseGasFeatureControl()
	{
		var featureControlMock = new Mock<IFeatureControlManager>();
		featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CarbonEmissionGreenhouseGasFeature, CancellationToken.None)).Returns(Task.FromResult((IFeatureData)null));
		using (ObjectFactory.Substitute(featureControlMock.Object))
		{
			AssertEquals(expected: false, new CO2eFeatureControlHelper().Enabled);
		}

		var featureDataMock = new Mock<IFeatureData>();
		featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CarbonEmissionGreenhouseGasFeature, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		using (ObjectFactory.Substitute(featureControlMock.Object))
		{
			Assert(new CO2eFeatureControlHelper().Enabled);
		}
	}

	public void TestCarbonEmissionDashboardPortalFeatureControl()
	{
		var featureControlMock = new Mock<IFeatureControlManager>();
		featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CarbonEmissionDashboardPortalFeature, CancellationToken.None)).Returns(Task.FromResult((IFeatureData)null));
		using (ObjectFactory.Substitute(featureControlMock.Object))
		{
			AssertEquals(expected: false, new CO2eFeatureControlHelper().DashboardEnabled);
		}

		var featureDataMock = new Mock<IFeatureData>();
		featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CarbonEmissionDashboardPortalFeature, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		using (ObjectFactory.Substitute(featureControlMock.Object))
		{
			Assert(new CO2eFeatureControlHelper().DashboardEnabled);
		}
	}
}
