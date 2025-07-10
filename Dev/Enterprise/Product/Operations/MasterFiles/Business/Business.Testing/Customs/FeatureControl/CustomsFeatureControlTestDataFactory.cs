using System.Threading;
using System.Threading.Tasks;
using CargoWise.FeatureControl.Abstractions;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing;

public static class CustomsFeatureControlTestHelper
{
	public static Mock<IFeatureControlManager> CreateVietnamManifestFeatureControlMock(bool isVietnamManifestFeatureEnabled)
	{
		var featureControlData = new VietnamManifestFeatureControlData();
		featureControlData.IsVietnamManifestFeatureEnabled = isVietnamManifestFeatureEnabled;

		var mockIFeatureData = new Mock<IFeatureData>();
		mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out featureControlData)).Returns(true);

		var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
		mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.VietnamForwarderManifest, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

		return mockIFeatureControlManager;
	}
}
