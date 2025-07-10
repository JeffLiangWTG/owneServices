using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Moq;

namespace Enterprise.Freight.Business.Testing;
public class DeliveryDueDateFeatureControlHelperTest : TestCaseWithFactory
{
	public void TestFeatureEnabled_ReturnsTrue()
	{
		SetupFeatureControlRule(enabled: true);

		var helper = new DeliveryDueDateFeatureControlHelper();
		Assert(helper.Enabled);
	}

	public void TestFeatureDisabled_ReturnsFalse()
	{
		SetupFeatureControlRule(enabled: false);
		var helper = new DeliveryDueDateFeatureControlHelper();
		AssertEquals(expected: false, helper.Enabled);
	}

	void SetupFeatureControlRule(bool enabled)
	{
		var featureControlMock = new Mock<IFeatureControlManager>();
		featureControlMock
			.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.ForwardingDeliveryDueDateFeature, CancellationToken.None))
			.Returns(Task.FromResult(enabled ? new Mock<IFeatureData>().Object : null));
		ObjectFactory.Substitute(featureControlMock.Object);
	}
}
