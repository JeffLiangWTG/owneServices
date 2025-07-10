using System;
using CargoWise.Types;
using Moq;
using WTG.RTUS.Interface;

namespace Enterprise.Packing.Business.Testing
{
	class CancellationResponseTest : PackingTestCaseWithFactory
	{
		public void TestConstructor_DoesNotAcceptInvalidArguments()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CancellationResponse(ZGuid.NewZGuid(), null, null, "ABC"));
#if NETFRAMEWORK
			AssertExceptionThrown(typeof(ArgumentException), "Package ID should not be empty.\r\nParameter name: packageID",
				() => new CancellationResponse(ZGuid.NewZGuid(), null, new Mock<IRTUSResponse>().Object, ""));
#else
			AssertExceptionThrown(typeof(ArgumentException), "Package ID should not be empty. (Parameter 'packageID')",
				() => new CancellationResponse(ZGuid.NewZGuid(), null, new Mock<IRTUSResponse>().Object, ""));
#endif
		}

		public void TestProperties()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var packingParent = new Mock<IPackingParent>();
			var rtusResponse = new Mock<IRTUSResponse>();
			rtusResponse.SetupGet(r => r.IsSuccessful).Returns(true);
			rtusResponse.SetupGet(r => r.ErrorMessageForFailure).Returns("No Error");

			var response = new CancellationResponse(packageJob.PK, packingParent.Object, rtusResponse.Object, "ABC");
			AssertEquals(nameof(response.PackageJobPK), packageJob.PK, response.PackageJobPK);
			AssertEquals(nameof(response.PackingParent), packingParent.Object, response.PackingParent);
			AssertEquals(nameof(response.IsSuccessful), true, response.IsSuccessful);
			AssertEquals(nameof(response.ErrorMessageForFailure), "No Error", response.ErrorMessageForFailure);
			AssertEquals(nameof(response.PackageID), "ABC", response.PackageID);
		}

		public void TestProperties_NullPackingParent()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var rtusResponse = new Mock<IRTUSResponse>();
			rtusResponse.SetupGet(r => r.IsSuccessful).Returns(true);
			rtusResponse.SetupGet(r => r.ErrorMessageForFailure).Returns("No Error");

			var response = new CancellationResponse(packageJob.PK, null, rtusResponse.Object, "ABC");
			AssertEquals(nameof(response.PackageJobPK), packageJob.PK, response.PackageJobPK);
			AssertEquals(nameof(response.PackingParent), null, response.PackingParent);
			AssertEquals(nameof(response.IsSuccessful), true, response.IsSuccessful);
			AssertEquals(nameof(response.ErrorMessageForFailure), "No Error", response.ErrorMessageForFailure);
			AssertEquals(nameof(response.PackageID), "ABC", response.PackageID);
		}
	}
}
