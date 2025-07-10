using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Moq;
using WTG.RTUS.Interface;

namespace Enterprise.Packing.DataTransfer.Testing
{
	class RTUSRemotePrintingHelperTest : PackingTestCaseWithFactory
	{
		#region TestUpdatePackageWithRTUSResponse_OnPackageBookedViaRTUS

		public void TestUpdatePackageWithRTUSResponse_OnPackageBookedViaRTUS()
		{
			var packageBookedTime = ZDateTime.Now;
			var packingParent = Factory.New<DummyPackingParent>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);
			var package = packageJob.Packages.AddNew("PLT", "ABC");

			AssertEquals("Precondition", ZDateTime.Empty, packingParent.OnPackageBookedViaRTUS_LastDateTimeReceived);

			RTUSRemotePrintingHelper.UpdatePackageWithRTUSResponse(package, null, "XYZ001", "T01", packageBookedTime, null);

			AssertEquals("Null PackingParent is ignore set value without exception.", ZDateTime.Empty, packingParent.OnPackageBookedViaRTUS_LastDateTimeReceived);

			RTUSRemotePrintingHelper.UpdatePackageWithRTUSResponse(package, packingParent, "XYZ001", "T01", packageBookedTime, null);

			AssertEquals("Should set value.", packageBookedTime, packingParent.OnPackageBookedViaRTUS_LastDateTimeReceived);

			var packageBookedTimeDayAfter = packageBookedTime.AddDays(1);
			RTUSRemotePrintingHelper.UpdatePackageWithRTUSResponse(package, packingParent, "XYZ001", "", packageBookedTimeDayAfter, null);

			AssertEquals("Still should set value even TransportReference is empty.", packageBookedTimeDayAfter, packingParent.OnPackageBookedViaRTUS_LastDateTimeReceived);
		}

		#endregion

		#region TestUpdatePackageWithRTUSResponse_SetType

		public void TestUpdatePackageWithRTUSResponse_SetType()
		{
			var packageBookedTime = ZDateTime.Now;
			var packingParent = Factory.New<DummyPackingParent>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);
			var package = packageJob.Packages.AddNew("PLT", "ABC");

			RTUSRemotePrintingHelper.UpdatePackageWithRTUSResponse(package, null, "XYZ001", "T01", packageBookedTime, "BLK");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertEquals("Should set type to BLK.", "BLK", newFactory.Load<PkgPackage>(package.PK).RTUSBookedType);
		}

		#endregion

		#region TestIsValidRTUSResponse

		public void TestIsValidRTUSResponse_ReferenceNumbers()
		{
			var packingParent = Factory.New<DummyPackingParent>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);

			Assert("Precondition: package job is not finalised.", !packageJob.KJ_IsFinalized);
			var referenceNumbers_NewPackageId = new ReferenceNumbers("ABC", "PKGID", "NEWPKGID", "TRANSPORTREF");
			Assert("RTUS Response is valid with new package id if package job is not finalised.", RTUSRemotePrintingHelper.IsValidRTUSResponse(packageJob, "PKGID", "TRANSPORTREF", referenceNumbers_NewPackageId));

			var referenceNumbers_NewTransportRef = new ReferenceNumbers("ABC", "PKGID", "PKGID", "NEWTRANSPORTREF");
			Assert("RTUS Response is valid with new transport reference if package job is not finalised.", RTUSRemotePrintingHelper.IsValidRTUSResponse(packageJob, "PKGID", "TRANSPORTREF", referenceNumbers_NewTransportRef));

			packageJob.KJ_IsFinalized = true;
			Assert("Precondition: package job is finalised.", packageJob.KJ_IsFinalized);
			Assert("RTUS response is not valid with new package id if package job is finalised.", !RTUSRemotePrintingHelper.IsValidRTUSResponse(packageJob, "PKGID", "TRANSPORTREF", referenceNumbers_NewPackageId));
			Assert("RTUS response is not valid with new transport reference if package job is finalised.", !RTUSRemotePrintingHelper.IsValidRTUSResponse(packageJob, "PKGID", "TRANSPORTREF", referenceNumbers_NewTransportRef));

			var referenceNumbers_NoNewDetails = new ReferenceNumbers("ABC", "PKGID", "PKGID", "TRANSPORTREF");
			Assert("RTUS response is valid for finalised package job if both package id and transport reference are the current values.", RTUSRemotePrintingHelper.IsValidRTUSResponse(packageJob, "PKGID", "TRANSPORTREF", referenceNumbers_NoNewDetails));
		}

		public void TestIsValidRTUSResponse_SingleBookingResponse()
		{
			var packingParent = Factory.New<DummyPackingParent>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);

			Assert("Precondition: package job is not finalised.", !packageJob.KJ_IsFinalized);
			var responseMock = new Mock<ISingleBookingRTUSResponse>();
			responseMock.SetupGet(m => m.TrackingNumber).Returns("NEWPKGID");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			Assert("RTUS Response is valid with new package id if package job is not finalised.", RTUSRemotePrintingHelper.IsValidRTUSResponse(packageJob, "PKGID", "TRANSPORTREF", responseMock.Object));

			responseMock.SetupGet(m => m.TrackingNumber).Returns("PKGID");
			responseMock.SetupGet(m => m.TransportReference).Returns("NEWTRANSPORTREF");
			Assert("RTUS Response is valid with new transport reference if package job is not finalised.", RTUSRemotePrintingHelper.IsValidRTUSResponse(packageJob, "PKGID", "TRANSPORTREF", responseMock.Object));

			packageJob.KJ_IsFinalized = true;
			Assert("Precondition: package job is finalised.", packageJob.KJ_IsFinalized);

			responseMock.SetupGet(m => m.TrackingNumber).Returns("NEWPKGID");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			Assert("RTUS response is not valid with new package id if package job is finalised.", !RTUSRemotePrintingHelper.IsValidRTUSResponse(packageJob, "PKGID", "TRANSPORTREF", responseMock.Object));

			responseMock.SetupGet(m => m.TrackingNumber).Returns("PKGID");
			responseMock.SetupGet(m => m.TransportReference).Returns("NEWTRANSPORTREF");
			Assert("RTUS response is not valid with new transport reference if package job is finalised.", !RTUSRemotePrintingHelper.IsValidRTUSResponse(packageJob, "PKGID", "TRANSPORTREF", responseMock.Object));

			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			Assert("RTUS response is valid for finalised package job if both package id and transport reference are the current values.", RTUSRemotePrintingHelper.IsValidRTUSResponse(packageJob, "PKGID", "TRANSPORTREF", responseMock.Object));
		}

		#endregion
	}
}
