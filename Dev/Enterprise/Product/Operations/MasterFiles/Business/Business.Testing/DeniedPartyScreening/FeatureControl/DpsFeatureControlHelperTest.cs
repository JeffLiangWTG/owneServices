using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DpsFeatureControlHelperTest : TestCaseWithFactory
	{
		public void TestGetFeatureControlAddressOnlyFlags_WhenFlagsAreEnabled_ShouldReturnTrueValues()
		{
			var dpsFeatureControlHelper = new DpsFeatureControlHelper();
			AssertEquals("Pre-condition", false, dpsFeatureControlHelper.GetFeatureControlAddressOnlyFlags.isAllAddressesIncluded);
			AssertEquals("Pre-condition", false, dpsFeatureControlHelper.GetFeatureControlAddressOnlyFlags.isAddressOnlyScreeningIncluded);

			SetTestFeatureFlagsForAddressOnlyMatching();

			AssertEquals(true, dpsFeatureControlHelper.GetFeatureControlAddressOnlyFlags.isAllAddressesIncluded);
			AssertEquals(true, dpsFeatureControlHelper.GetFeatureControlAddressOnlyFlags.isAddressOnlyScreeningIncluded);
		}

		public void SetTestFeatureFlagsForAddressOnlyMatching()
		{
			var mockFeatureData = new Mock<IFeatureData>();
			var isAllAddressIncluded = new DpsAddressMatchingProfilesFeatureControlData() { IsAllAddressesIncluded = true };
			var mockFeatureManager = new Mock<IFeatureControlManager>();

			ObjectFactory.Substitute(mockFeatureManager.Object);

			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.DpsAddressMatchingLevelFeature, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));
			mockFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out isAllAddressIncluded)).Returns(true);
		}
	}
}
