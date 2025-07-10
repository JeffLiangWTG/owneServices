using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.RTUS.Interface;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Testing
{
	public class CarrierLabelsBatchPrintingProviderTest : PackingTestCaseWithFactory
	{
		[TestDate(2020, 7, 16, 1, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestPrintCarrierLabels_PDF() => TestPrintCarrierLabelsCore(WTG.RTUS.Interface.FileType.PDF);

		[TestDate(2020, 7, 16, 1, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestPrintCarrierLabels_ZPL() => TestPrintCarrierLabelsCore(WTG.RTUS.Interface.FileType.ZPL);

		void TestPrintCarrierLabelsCore(WTG.RTUS.Interface.FileType fileType)
		{
			Data.CreatePackingData();

			Data.Dummy.JobNoForPackingParent = "D00000001";
			Data.Dummy.TransportReference = "Initial Test Value";
			var package1 = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var package2 = Data.PackageJob.Packages.AddNew("PLT", "DEF");
			var package3 = Data.PackageJob.Packages.AddNew("PLT", "GHI");

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<IBatchBookingRTUSResponse>();
			responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock.SetupGet(r => r.ReferenceNumbers).Returns(new[]
			{
				new ReferenceNumbers("D00000001", "ABC", "PKG1", "TRANSPORTREF"),
				new ReferenceNumbers("D00000001", "DEF", "PKG2", "TRANSPORTREF"),
				new ReferenceNumbers("D00000001", "GHI", "PKG3", "TRANSPORTREF")
			});
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock.SetupGet(m => m.FileType).Returns(fileType);

			var pushCarrierLabelCalled = 0;

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking, It.IsAny<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock.Object);

			var packageToPackingParentInfos = new[]
			{
				new PackageToPackingParentInfo(Data.Dummy, package1, "D00000001"),
				new PackageToPackingParentInfo(Data.Dummy, package2, "D00000001"),
				new PackageToPackingParentInfo(Data.Dummy, package3, "D00000001"),
			};
			var carrierLabelsBatchPrintingInfo = new CarrierLabelsBatchPrintingInfoTest(new[] { new UniversalShipment() as ITopLevelDataObject }, null, packageToPackingParentInfos);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabels(carrierLabelsBatchPrintingInfo, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer.PK, new CancellationToken());
				AssertEquals("Print is successful", true, response.Success);
				AssertEquals("PushCarrieLabel called 1 time only.", 1, pushCarrierLabelCalled);

				AssertEquals("Print is successful.", "PKG1", package1.KP_PackageID);
				AssertEquals("Print is successful.", "PKG2", package2.KP_PackageID);
				AssertEquals("Print is successful.", "PKG3", package3.KP_PackageID);

				AssertEquals("Print is successful.", "TRANSPORTREF", Data.Dummy.TransportReference);

				AssertEquals("Package is sent to RTUS.", true, package1.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, package2.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, package3.IsSentToRTUS);

				managerMock.VerifyAll();

				var query = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
				var otherFactory = new BusinessObjectFactory();
				var printJobs = otherFactory.Load<IStmPrintJob>(query);
				AssertEquals("There should be 1 doc print jobs.", 1, printJobs.Length);
				Assert(nameof(StmPrintJobSchema.SP_Copies), printJobs.All(printJob => printJob.SP_Copies == 1));
				Assert(nameof(StmPrintJobSchema.SP_EmailAttachments), printJobs.All(printJob => printJob.SP_EmailAttachments == $"Label1.{fileType}"));
				Assert(nameof(StmPrintJobSchema.SP_JobType), printJobs.All(printJob => printJob.SP_JobType == "PRN"));
				Assert(nameof(StmPrintJobSchema.SP_RunDateTime), printJobs.All(printJob => printJob.SP_RunDateTime == new ZDateTime(2020, 7, 16, 1, 1, 1)));

				var deliveryGroup = otherFactory.Load<IStmDeliveryGroup>(printJobs.Select(printJob => printJob.SP_SB_DeliveryGroup).Distinct().Single());
				AssertEquals(nameof(deliveryGroup.SB_IsProcessed), true, deliveryGroup.SB_IsProcessed);
			}
		}

		public void TestPrintCarrierLabels_PackagJobFinalised()
		{
			Data.CreatePackingData();

			Data.Dummy.JobNoForPackingParent = "D00000001";
			Data.Dummy.TransportReference = "Initial Test Value";
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			Data.PackageJob.KJ_IsFinalized = true;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<IBatchBookingRTUSResponse>();
			responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock.SetupGet(r => r.ReferenceNumbers).Returns(new[]
			{
				new ReferenceNumbers("D00000001", "ABC", "PKG1", "TRANSPORTREF"),
			});
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking, It.IsAny<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(responseMock.Object);

			var packageToPackingParentInfos = new[]
			{
				new PackageToPackingParentInfo(Data.Dummy, package, "D00000001"),
			};
			var carrierLabelsBatchPrintingInfo = new CarrierLabelsBatchPrintingInfoTest(new[] { new UniversalShipment() as ITopLevelDataObject }, null, packageToPackingParentInfos);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabels(carrierLabelsBatchPrintingInfo, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer.PK, new CancellationToken());
				AssertEquals("Print is not successful", false, response.Success);
				AssertEquals("Print is not successful.", "ABC", package.KP_PackageID);
				AssertEquals("Print is not successful.", "Initial Test Value", Data.Dummy.TransportReference);
				AssertEquals("Print is not successful.", "The Package job is already finalized and the RTUS response attempted to update the package details.", response.Message);
				managerMock.VerifyAll();

				var query = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
				var otherFactory = new BusinessObjectFactory();
				var printJobs = otherFactory.Load<IStmPrintJob>(query);
				AssertEquals("There should be no doc print jobs.", 0, printJobs.Length);
			}
		}

		public void TestPrintCarrierLabels_PackagJobFinalised_PackageNotUpdated()
		{
			Data.CreatePackingData();

			Data.Dummy.JobNoForPackingParent = "D00000001";
			Data.Dummy.TransportReference = "Initial Test Value";
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			Data.PackageJob.KJ_IsFinalized = true;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<IBatchBookingRTUSResponse>();
			responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock.SetupGet(r => r.ReferenceNumbers).Returns(new[]
			{
				new ReferenceNumbers("D00000001", "ABC", "ABC", "Initial Test Value"),
			});
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking, It.IsAny<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(responseMock.Object);

			var packageToPackingParentInfos = new[]
			{
				new PackageToPackingParentInfo(Data.Dummy, package, "D00000001"),
			};
			var carrierLabelsBatchPrintingInfo = new CarrierLabelsBatchPrintingInfoTest(new[] { new UniversalShipment() as ITopLevelDataObject }, null, packageToPackingParentInfos);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabels(carrierLabelsBatchPrintingInfo, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer.PK, new CancellationToken());
				AssertEquals("Print is successful", true, response.Success);
				AssertEquals("Print is successful.", "ABC", package.KP_PackageID);
				AssertEquals("Print is successful.", "Initial Test Value", Data.Dummy.TransportReference);
				AssertEquals("Print is successful.", "", response.Message);
				managerMock.VerifyAll();

				var query = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
				var otherFactory = new BusinessObjectFactory();
				var printJobs = otherFactory.Load<IStmPrintJob>(query);
				AssertEquals("There should be 1 doc print job.", 1, printJobs.Length);
			}
		}

		[TestDate(2020, 7, 16, 1, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestGetCarrierLabelRequestBatches_MultiplePackingParents()
		{
			var packageJob1 = Factory.New<PkgPackageJob>();
			packageJob1.KJ_JobID = "P00000001";
			var dummyParent1 = Factory.New<DummyWithPacking>();
			dummyParent1.JobNoForPackingParent = "D00000001";
			packageJob1.KJ_ParentID = dummyParent1.PK;
			packageJob1.KJ_ParentTableCode = dummyParent1.TablePrefix;

			var packageJob2 = Factory.New<PkgPackageJob>();
			packageJob2.KJ_JobID = "P00000002";
			var dummyParent2 = Factory.New<DummyWithPacking>();
			dummyParent2.JobNoForPackingParent = "D00000002";
			packageJob2.KJ_ParentID = dummyParent2.PK;
			packageJob2.KJ_ParentTableCode = dummyParent2.TablePrefix;

			var package1Dummy1 = packageJob1.Packages.AddNew("PLT", "ABC");
			var package2Dummy1 = packageJob1.Packages.AddNew("PLT", "DEF");
			var package1Dummy2 = packageJob2.Packages.AddNew("PLT", "ABC");
			var package2Dummy2 = packageJob2.Packages.AddNew("PLT", "DEF");

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<IBatchBookingRTUSResponse>();
			responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock.SetupGet(r => r.ReferenceNumbers).Returns(new[]
			{
				new ReferenceNumbers("D00000001", "ABC", "PKG1", "TRANSPORTREF1"),
				new ReferenceNumbers("D00000002", "ABC", "PKG2", "TRANSPORTREF2"),
				new ReferenceNumbers("D00000001", "DEF", "PKG3", "TRANSPORTREF1"),
				new ReferenceNumbers("D00000002", "DEF", "PKG4", "TRANSPORTREF2")
			});
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock.SetupGet(m => m.FileType).Returns(WTG.RTUS.Interface.FileType.PDF);

			var pushCarrierLabelCalled = 0;

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking, It.IsAny<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock.Object);

			var packageToPackingParentInfos = new[]
			{
				new PackageToPackingParentInfo(dummyParent1, package1Dummy1, "D00000001"),
				new PackageToPackingParentInfo(dummyParent1, package2Dummy1, "D00000001"),
				new PackageToPackingParentInfo(dummyParent2, package1Dummy2, "D00000002"),
				new PackageToPackingParentInfo(dummyParent2, package2Dummy2, "D00000002"),
			};
			var carrierLabelsBatchPrintingInfo = new CarrierLabelsBatchPrintingInfoTest(new[] { new UniversalShipment() as ITopLevelDataObject }, null, packageToPackingParentInfos);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabels(carrierLabelsBatchPrintingInfo, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer.PK, new CancellationToken());
				AssertEquals("Print is successful", true, response.Success);
				AssertEquals("PushCarrieLabel called 1 time only.", 1, pushCarrierLabelCalled);

				AssertEquals("Print is successful.", "PKG1", package1Dummy1.KP_PackageID);
				AssertEquals("Print is successful.", "PKG2", package1Dummy2.KP_PackageID);
				AssertEquals("Print is successful.", "PKG3", package2Dummy1.KP_PackageID);
				AssertEquals("Print is successful.", "PKG4", package2Dummy2.KP_PackageID);

				AssertEquals("Print is successful.", "TRANSPORTREF1", dummyParent1.TransportReference);
				AssertEquals("Print is successful.", "TRANSPORTREF2", dummyParent2.TransportReference);

				AssertEquals("Package is sent to RTUS.", true, package1Dummy1.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, package1Dummy2.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, package2Dummy1.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, package2Dummy2.IsSentToRTUS);

				var localTime = new ZDateTime(2020, 7, 16, 1, 1, 1).AddHours(10); // 10 hours UtcOffset
				AssertEquals("Should receive package book time.", localTime, dummyParent1.OnPackageBookedViaRTUS_LastDateTimeReceived);
				AssertEquals("Should receive package book time.", localTime, dummyParent2.OnPackageBookedViaRTUS_LastDateTimeReceived);

				managerMock.VerifyAll();

				var query = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
				var otherFactory = new BusinessObjectFactory();
				var printJobs = otherFactory.Load<IStmPrintJob>(query);
				AssertEquals("There should be 1 doc print jobs.", 1, printJobs.Length);
				Assert(nameof(StmPrintJobSchema.SP_Copies), printJobs.All(printJob => printJob.SP_Copies == 1));
				Assert(nameof(StmPrintJobSchema.SP_EmailAttachments), printJobs.All(printJob => printJob.SP_EmailAttachments == "Label1.PDF"));
				Assert(nameof(StmPrintJobSchema.SP_JobType), printJobs.All(printJob => printJob.SP_JobType == "PRN"));
				Assert(nameof(StmPrintJobSchema.SP_RunDateTime), printJobs.All(printJob => printJob.SP_RunDateTime == new ZDateTime(2020, 7, 16, 1, 1, 1)));

				var deliveryGroup = otherFactory.Load<IStmDeliveryGroup>(printJobs.Select(printJob => printJob.SP_SB_DeliveryGroup).Distinct().Single());
				AssertEquals(nameof(deliveryGroup.SB_IsProcessed), true, deliveryGroup.SB_IsProcessed);
			}
		}

		public void TestGetCarrierLabelRequestBatches_MultiplePackingParents_OnPackageBookedViaRTUS()
		{
			var packageJob1 = Factory.New<PkgPackageJob>();
			packageJob1.KJ_JobID = "P00000001";
			var dummyParent1 = Factory.New<DummyWithPacking>();
			dummyParent1.JobNoForPackingParent = "D00000001";
			packageJob1.KJ_ParentID = dummyParent1.PK;
			packageJob1.KJ_ParentTableCode = dummyParent1.TablePrefix;

			var packageJob2 = Factory.New<PkgPackageJob>();
			packageJob2.KJ_JobID = "P00000002";
			var dummyParent2 = Factory.New<DummyWithPacking>();
			dummyParent2.JobNoForPackingParent = "D00000002";
			packageJob2.KJ_ParentID = dummyParent2.PK;
			packageJob2.KJ_ParentTableCode = dummyParent2.TablePrefix;

			var package1Dummy1 = packageJob1.Packages.AddNew("PLT", "ABC");
			var package2Dummy1 = packageJob1.Packages.AddNew("PLT", "DEF");
			var package1Dummy2 = packageJob2.Packages.AddNew("PLT", "ABC");
			var package2Dummy2 = packageJob2.Packages.AddNew("PLT", "DEF");

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<IBatchBookingRTUSResponse>();
			responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock.SetupGet(r => r.ReferenceNumbers).Returns(new[]
			{
				new ReferenceNumbers("D00000001", "ABC", "PKG1", "TRANSPORTREF1"),
				new ReferenceNumbers("D00000002", "ABC", "PKG2", "TRANSPORTREF2"),
				new ReferenceNumbers("D00000001", "DEF", "PKG3", "TRANSPORTREF1"),
				new ReferenceNumbers("D00000002", "DEF", "PKG4", "TRANSPORTREF2")
			});
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);

			var pushCarrierLabelCalled = 0;

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking, It.IsAny<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock.Object);

			var packageToPackingParentInfos = new[]
			{
				new PackageToPackingParentInfo(dummyParent1, package1Dummy1, "D00000001"),
				new PackageToPackingParentInfo(dummyParent1, package2Dummy1, "D00000001"),
				new PackageToPackingParentInfo(dummyParent2, package1Dummy2, "D00000002"),
				new PackageToPackingParentInfo(dummyParent2, package2Dummy2, "D00000002"),
			};
			var carrierLabelsBatchPrintingInfo = new CarrierLabelsBatchPrintingInfoTest(new[] { new UniversalShipment() as ITopLevelDataObject }, null, packageToPackingParentInfos);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				var parents = new DummyWithPacking[] { dummyParent1, dummyParent2 };

				Assert("Precondition", parents.All(p => p.OnPackageBookedViaRTUS_LastDateTimeReceived.IsEmpty));
				var response = provider.PrintCarrierLabels(carrierLabelsBatchPrintingInfo, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer.PK, new CancellationToken());

				Assert("Both should have value.", parents.All(p => !p.OnPackageBookedViaRTUS_LastDateTimeReceived.IsEmpty));
				AssertEquals("Both should have same value.", dummyParent1.OnPackageBookedViaRTUS_LastDateTimeReceived, dummyParent2.OnPackageBookedViaRTUS_LastDateTimeReceived);
			}
		}

		[TestDate(2020, 7, 16, 1, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestGetCarrierLabelRequestBatches_WithCustomSegments_PDF() => TestGetCarrierLabelRequestBatches_WithCustomSegmentsCore(WTG.RTUS.Interface.FileType.PDF);

		[TestDate(2020, 7, 16, 1, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestGetCarrierLabelRequestBatches_WithCustomSegments_ZPL() => TestGetCarrierLabelRequestBatches_WithCustomSegmentsCore(WTG.RTUS.Interface.FileType.ZPL);

		void TestGetCarrierLabelRequestBatches_WithCustomSegmentsCore(WTG.RTUS.Interface.FileType fileType)
		{
			Data.CreatePackingData();

			Data.Dummy.JobNoForPackingParent = "D00000001";
			Data.Dummy.TransportReference = "Initial Test Value";
			var package1 = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var package2 = Data.PackageJob.Packages.AddNew("PLT", "DEF");
			var package3 = Data.PackageJob.Packages.AddNew("PLT", "GHI");

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock1 = new Mock<IBatchBookingRTUSResponse>();
			responseMock1.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock1.SetupGet(r => r.ReferenceNumbers).Returns(new[] { new ReferenceNumbers("D00000001", "ABC", "PKG1", "TRANSPORTREF") });
			responseMock1.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock1.SetupGet(m => m.FileType).Returns(fileType);

			var responseMock2 = new Mock<IBatchBookingRTUSResponse>();
			responseMock2.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock2.SetupGet(r => r.ReferenceNumbers).Returns(new[] { new ReferenceNumbers("D00000001", "DEF", "PKG2", "TRANSPORTREF") });
			responseMock2.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock2.SetupGet(m => m.FileType).Returns(fileType);

			var responseMock3 = new Mock<IBatchBookingRTUSResponse>();
			responseMock3.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock3.SetupGet(r => r.ReferenceNumbers).Returns(new[] { new ReferenceNumbers("D00000001", "GHI", "PKG3", "TRANSPORTREF") });
			responseMock3.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock3.SetupGet(m => m.FileType).Returns(fileType);

			var pushCarrierLabelCalled = 0;

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking,
				It.Is<UniversalShipment>(shipment => IsUniversalShipmentPassedInFromPackage(shipment, "ABC")), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock1.Object);
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking,
				It.Is<UniversalShipment>(shipment => IsUniversalShipmentPassedInFromPackage(shipment, "DEF")), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock2.Object);
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking,
				It.Is<UniversalShipment>(shipment => IsUniversalShipmentPassedInFromPackage(shipment, "GHI")), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock3.Object);

			var universalShipment1 = BuildMockUniversalShipment("D00000001", "ABC");
			var universalShipment2 = BuildMockUniversalShipment("D00000001", "DEF");
			var universalShipment3 = BuildMockUniversalShipment("D00000001", "GHI");

			var packageToPackingParentInfos = new[]
			{
				new PackageToPackingParentInfo(Data.Dummy, package1, "D00000001"),
				new PackageToPackingParentInfo(Data.Dummy, package2, "D00000001"),
				new PackageToPackingParentInfo(Data.Dummy, package3, "D00000001"),
			};

			var customSegments1 = new DocumentCommandCollection(package2);
			customSegments1.AddFromDatabase(EndOfPalletLabelPK);

			var customSegments2 = new DocumentCommandCollection(package3);
			customSegments2.AddFromDatabase(EndOfPalletLabelPK);
			customSegments2.AddFromDatabase(EndOfAreaLabelPK);

			var customSegmentsCollection = new[] { new KeyValuePair<ZGuid, DocumentCommandCollection>(package2.PK, customSegments1), new KeyValuePair<ZGuid, DocumentCommandCollection>(package3.PK, customSegments2) };
			var carrierLabelsBatchPrintingInfo = new CarrierLabelsBatchPrintingInfoTest(new ITopLevelDataObject[] { universalShipment1, universalShipment2, universalShipment3 }, customSegmentsCollection, packageToPackingParentInfos);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabels(carrierLabelsBatchPrintingInfo, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer.PK, new CancellationToken());
				AssertEquals("Print is successful", true, response.Success);
				AssertEquals("PushCarrieLabel called 3 times.", 3, pushCarrierLabelCalled);

				AssertEquals("Print is successful.", "PKG1", package1.KP_PackageID);
				AssertEquals("Print is successful.", "PKG2", package2.KP_PackageID);
				AssertEquals("Print is successful.", "PKG3", package3.KP_PackageID);

				AssertEquals("Print is successful.", "TRANSPORTREF", Data.Dummy.TransportReference);

				AssertEquals("Package is sent to RTUS.", true, package1.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, package2.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, package3.IsSentToRTUS);

				managerMock.VerifyAll();

				var query = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
				var otherFactory = new BusinessObjectFactory();
				var printJobs = otherFactory.Load<StmPrintJob>(query);
				AssertEquals("There should be 6 docs print jobs, 3 package labels and 3 splitter labels.", 6, printJobs.Length);
				Assert(nameof(StmPrintJobSchema.SP_Copies), printJobs.All(printJob => printJob.SP_Copies == 1));
				AssertSequencesEqual(nameof(StmPrintJobSchema.SP_EmailAttachments),
					new ZString[] { $"Label1.{fileType}", $"Label2.{fileType}", "Label3.PDF", $"Label4.{fileType}", "Label5.PDF", "Label6.PDF", },
					printJobs.OrderBy(job => job.SP_Sequence).Select(printJob => printJob.SP_EmailAttachments));
				Assert(nameof(StmPrintJobSchema.SP_JobType), printJobs.All(printJob => printJob.SP_JobType == "PRN"));
				Assert(nameof(StmPrintJobSchema.SP_RunDateTime), printJobs.All(printJob => printJob.SP_RunDateTime == new ZDateTime(2020, 7, 16, 1, 1, 1)));

				var deliveryGroup = otherFactory.Load<IStmDeliveryGroup>(printJobs.Select(printJob => printJob.SP_SB_DeliveryGroup).Distinct().Single());
				AssertEquals(nameof(deliveryGroup.SB_IsProcessed), true, deliveryGroup.SB_IsProcessed);
			}
		}

		bool IsUniversalShipmentPassedInFromPackage(UniversalShipment shipment, string packageReference)
		{
			var packingLines = shipment.SubShipmentCollection.SelectMany(subshipment => subshipment.PackingLineCollection).Select(packingLine => packingLine.ReferenceNumber.Value.ToString());
			return packingLines.Count() == 1 && packingLines.Single().Equals(packageReference);
		}

		internal static readonly ZGuid EndOfAreaLabelPK = new ZGuid("174bbaf6-6f87-4299-9fe1-243335bc4199");
		internal static readonly ZGuid EndOfPalletLabelPK = new ZGuid("494e1b73-2ea5-4fca-b673-ebca174b4b2d");

		public void TestGetCarrierLabelRequestBatches_FailedToCreatePrintJob()
		{
			Data.CreatePackingData();

			Data.Dummy.JobNoForPackingParent = "D00000001";
			Data.Dummy.TransportReference = "Initial Test Value";
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<IBatchBookingRTUSResponse>();
			responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock.SetupGet(r => r.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(r => r.ReferenceNumbers).Returns(new[]
			{
				new ReferenceNumbers("D00000001", "ABC", "PKG1", "TRANSPORTREF")
			});
			responseMock.SetupGet(m => m.FileType).Returns(WTG.RTUS.Interface.FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);

			var pushCarrierLabelCalled = 0;

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking, It.IsAny<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock.Object);

			var packageToPackingParentInfos = new[]
			{
				new PackageToPackingParentInfo(Data.Dummy, package, "D00000001")
			};
			var carrierLabelsBatchPrintingInfo = new CarrierLabelsBatchPrintingInfoTest(new[] { new UniversalShipment() as ITopLevelDataObject }, null, packageToPackingParentInfos);

			var errorMessage = string.Empty;
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, (exception) => errorMessage = exception.Message))
			{
				var response = provider.PrintCarrierLabels(carrierLabelsBatchPrintingInfo, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), Guid.NewGuid(), new CancellationToken());
				AssertEquals("Print is not successful.", false, response.Success);
				AssertEquals("Print is not successful.", string.Empty, response.Message);
				AssertEquals("Print is not successful, OnError is called.", false, errorMessage.IsNullOrEmpty());

				AssertEquals("Package is sent to RTUS.", true, package.IsSentToRTUS);

				var otherFactory = new BusinessObjectFactory();
				var printJobs = otherFactory.Load<IStmPrintJob>(new ZQuery());
				AssertEquals("There should be no doc print jobs.", false, printJobs.Any());
			}
		}

		public void TestGetCarrierLabelRequestBatches_WrongPackageAndPackingParentInTheRTUSReply()
		{
			Data.CreatePackingData();

			Data.Dummy.JobNoForPackingParent = "D00000001";
			Data.Dummy.TransportReference = "Initial Test Value";
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<IBatchBookingRTUSResponse>();
			responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock.SetupGet(r => r.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(r => r.ReferenceNumbers).Returns(new[]
			{
				new ReferenceNumbers("D00000002", "DEF", "PKG1", "TRANSPORTREF")
			});
			responseMock.SetupGet(m => m.FileType).Returns(WTG.RTUS.Interface.FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);

			var pushCarrierLabelCalled = 0;

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking, It.IsAny<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock.Object);

			var packageToPackingParentInfos = new[]
			{
				new PackageToPackingParentInfo (Data.Dummy, package, "D00000001")
			};
			var carrierLabelsBatchPrintingInfo = new CarrierLabelsBatchPrintingInfoTest(new[] { new UniversalShipment() as ITopLevelDataObject }, null, packageToPackingParentInfos);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabels(carrierLabelsBatchPrintingInfo, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), Guid.NewGuid(), new CancellationToken());
				AssertEquals("Print is not successful.", false, response.Success);
				AssertEquals("Print is not successful.", "Package and Packing Parent information mismatch between RTUS response and request.", response.Message);
				AssertEquals("Package is not sent to RTUS.", false, package.IsSentToRTUS);

				var otherFactory = new BusinessObjectFactory();
				var printJobs = otherFactory.Load<IStmPrintJob>(new ZQuery());
				AssertEquals("There should be no doc print jobs.", false, printJobs.Any());
			}
		}

		public void TestGetCarrierLabelRequestBatches_NullCarrierLabelsbatchPrintingInfo()
		{
			var managerMock = new Mock<ICarrierLabelManager>();
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertExceptionThrown<ArgumentNullException>(() => provider.PrintCarrierLabels(null, RTUSCBA.SmartFreight, new Uri(SmartFreightUrl), ZGuid.Empty, new CancellationToken()));
			}
		}

		public void TestGetCarrierLabelRequestBatches_NullPackageToPackingParentPairCollection()
		{
			Data.CreatePackingData();

			Data.Dummy.JobNoForPackingParent = "D00000001";
			Data.Dummy.TransportReference = "Initial Test Value";
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<IBatchBookingRTUSResponse>();
			responseMock.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock.SetupGet(r => r.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(r => r.ReferenceNumbers).Returns(new[]
			{
				new ReferenceNumbers("D00000001", "ABC", "PKG1", "TRANSPORTREF")
			});
			responseMock.SetupGet(m => m.FileType).Returns(WTG.RTUS.Interface.FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking, It.IsAny<ITopLevelDataObject>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Returns(responseMock.Object);

			var carrierLabelsBatchPrintingInfo = new CarrierLabelsBatchPrintingInfoTest(new[] { new UniversalShipment() as ITopLevelDataObject }, null, null);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				var result = provider.PrintCarrierLabels(carrierLabelsBatchPrintingInfo, RTUSCBA.SmartFreight, new Uri(SmartFreightUrl), ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Print is not successful.", "Package and Packing Parent information mismatch between RTUS response and request.", result.Message);
			}
		}

		public void TestGetCarrierLabelRequestBatches_CancellationTokenTest()
		{
			Data.CreatePackingData();

			Data.Dummy.JobNoForPackingParent = "D00000001";
			Data.Dummy.TransportReference = "Initial Test Value";
			var package1 = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var package2 = Data.PackageJob.Packages.AddNew("PLT", "DEF");

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock1 = new Mock<IBatchBookingRTUSResponse>();
			responseMock1.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock1.SetupGet(r => r.ReferenceNumbers).Returns(new[] { new ReferenceNumbers("D00000001", "ABC", "PKG1", "TRANSPORTREF") });
			responseMock1.SetupGet(m => m.BinaryData).Returns(binaryData);

			var cancellationTokenSource = new CancellationTokenSource();
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.BatchBooking,
				It.Is<UniversalShipment>(shipment => IsUniversalShipmentPassedInFromPackage(shipment, "ABC")), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Callback(() => cancellationTokenSource.Cancel())
				.Returns(responseMock1.Object);

			var universalShipment1 = BuildMockUniversalShipment("D00000001", "ABC");
			var universalShipment2 = BuildMockUniversalShipment("D00000001", "DEF");

			var packageToPackingParentInfos = new[]
			{
				new PackageToPackingParentInfo(Data.Dummy, package1, "D00000001"),
				new PackageToPackingParentInfo(Data.Dummy, package2, "D00000001"),
			};

			var customSegments1 = new DocumentCommandCollection(package2);
			customSegments1.AddFromDatabase(EndOfPalletLabelPK);

			var carrierLabelsBatchPrintingInfo = new CarrierLabelsBatchPrintingInfoTest(new ITopLevelDataObject[] { universalShipment1, universalShipment2 }, null, packageToPackingParentInfos);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertExceptionThrown<OperationCanceledException>("Operation is cancelled.", () =>
					provider.PrintCarrierLabels(carrierLabelsBatchPrintingInfo, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer.PK, cancellationTokenSource.Token));

				AssertEquals("Updated since cancel happened after this package.", "PKG1", package1.KP_PackageID);
				AssertEquals("Not updated due to cancel.", "DEF", package2.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", Data.Dummy.TransportReference);

				AssertEquals("Package is sent to RTUS.", true, package1.IsSentToRTUS);
				AssertEquals("Package is not sent to RTUS.", false, package2.IsSentToRTUS);

				managerMock.VerifyAll();

				var otherFactory = new BusinessObjectFactory();
				var printJobs = otherFactory.Load<IStmPrintJob>(new ZQuery());
				AssertEquals("There should be no doc print jobs.", false, printJobs.Any());
			}
		}

		public void TestDispose()
		{
			var managerMock = new Mock<ICarrierLabelManager>();

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("Provider should have all valid details.", true, provider.IsRemotePrintingConnectionDetailsProvided);
				managerMock.Verify(m => m.Dispose(), Times.Never);
			}

			managerMock.Verify(m => m.Dispose(), Times.Once);
		}

		#region TestGetProvider_InvalidServerUrl

		public void TestGetProvider_InvalidServerUrl()
		{
			var managerMock = new Mock<ICarrierLabelManager>();

			using (SetupRegistryForTest(null, "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var result = provider.PrintCarrierLabels(null, RTUSCBA.Pierbridge, null, ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: Invalid Remote Printing Server URL in Registry ''.", result.Message);
			}

			using (SetupRegistryForTest("", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var result = provider.PrintCarrierLabels(null, RTUSCBA.Pierbridge, null, ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: Invalid Remote Printing Server URL in Registry ''.", result.Message);
			}

			using (SetupRegistryForTest(" ", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var result = provider.PrintCarrierLabels(null, RTUSCBA.Pierbridge, null, ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: Invalid Remote Printing Server URL in Registry ' '.", result.Message);
			}

			using (SetupRegistryForTest("InvalidUrl", "USER", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var result = provider.PrintCarrierLabels(null, RTUSCBA.Pierbridge, null, ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: Invalid Remote Printing Server URL in Registry 'InvalidUrl'.", result.Message);
			}
		}

		#endregion

		#region TestGetProvider_MissingUserName

		public void TestGetProvider_MissingUserName()
		{
			var managerMock = new Mock<ICarrierLabelManager>();

			using (SetupRegistryForTest("http://PrintServer", null, "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var result = provider.PrintCarrierLabels(null, RTUSCBA.Pierbridge, null, ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Username provided in Registry.", result.Message);
			}

			using (SetupRegistryForTest("http://PrintServer", "", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var result = provider.PrintCarrierLabels(null, RTUSCBA.Pierbridge, null, ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Username provided in Registry.", result.Message);
			}

			using (SetupRegistryForTest("http://PrintServer", " ", "PASSWORD"))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var result = provider.PrintCarrierLabels(null, RTUSCBA.Pierbridge, null, ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Username provided in Registry.", result.Message);
			}
		}

		#endregion

		#region TestGetProvider_MissingPassword

		public void TestGetProvider_MissingPassword()
		{
			var managerMock = new Mock<ICarrierLabelManager>();

			using (SetupRegistryForTest("http://PrintServer", "USER", null))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var result = provider.PrintCarrierLabels(null, RTUSCBA.Pierbridge, null, ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Password provided in Registry.", result.Message);
			}

			using (SetupRegistryForTest("http://PrintServer", "USER", ""))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var result = provider.PrintCarrierLabels(null, RTUSCBA.Pierbridge, null, ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Password provided in Registry.", result.Message);
			}

			using (SetupRegistryForTest("http://PrintServer", "USER", " "))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var result = provider.PrintCarrierLabels(null, RTUSCBA.Pierbridge, null, ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Password provided in Registry.", result.Message);
			}
		}

		#endregion

		#region TestGetProvider_AllDetailsInvalid

		public void TestGetProvider_AllDetailsInvalid()
		{
			var managerMock = new Mock<ICarrierLabelManager>();
			using (SetupRegistryForTest("InvalidUrl", "", ""))
			using (var provider = CarrierLabelsBatchPrintingProvider.GetProvider(managerMock.Object, null))
			{
				AssertEquals("No valid configuration data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var result = provider.PrintCarrierLabels(null, RTUSCBA.Pierbridge, null, ZGuid.Empty, new CancellationToken());
				AssertEquals("Print is not successful.", false, result.Success);
				AssertEquals("Message should specify issue.",
"Unable to Print Carrier Label due to the following: Invalid Remote Printing Server URL in Registry 'InvalidUrl'. No Remote Printing Username provided in Registry. No Remote Printing Password provided in Registry.", result.Message);
			}
		}

		#endregion

		#region Implementation

		UniversalShipment BuildMockUniversalShipment(ZString jobNo, ZString packageId)
		{
			var segment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
			};
			segment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			segment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, jobNo);
			var packingParent = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
			};
			packingParent.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			packingParent.DataContext.AddDataSource(DataContextType.DummyBusinessObject, jobNo);
			segment.SubShipmentCollection.Add(packingParent);
			packingParent.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ItemNo = 1,
				ReferenceNumber = packageId,
			});

			return segment;
		}

		const string SmartFreightUrl = "https://wtg1.p1.app.smartfreight.com/smartfreight/api/ucp/sems/execute/N1ZVOmJ3VjNWcHJQNnVlMTlVUWhqXy1hQ05mdmZIT1ZYVXc3ZUE";

		IDisposable SetupRegistryForTest(string remotePrintServer, string remotePrintUserName, string remotePrintPassword)
		{
			return new DisposableList(new[]
			{
				TransportRegistry.Instance.RemotePrintServerURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, remotePrintServer),
				WebDataRegistry.Instance.WebServiceUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, remotePrintUserName),
				WebDataRegistry.Instance.WebServicePassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, remotePrintPassword),
			});
		}

		#endregion

		#region CarrierLabelsBatchPrintingInfoTest

		class CarrierLabelsBatchPrintingInfoTest : ICarrierLabelsBatchPrintingInfo
		{
			public CarrierLabelsBatchPrintingInfoTest(IReadOnlyCollection<ITopLevelDataObject> packageBatchUniversalShipments, IEnumerable<KeyValuePair<ZGuid, DocumentCommandCollection>> customSegments, IEnumerable<PackageToPackingParentInfo> packageToPackingParentInfos)
			{
				PackagesBatchUniversalShipments = packageBatchUniversalShipments;
				CustomSegments = customSegments;
				PackageToPackingParentInfos = packageToPackingParentInfos;
			}

			public IReadOnlyCollection<ITopLevelDataObject> PackagesBatchUniversalShipments { get; }
			public IEnumerable<KeyValuePair<ZGuid, DocumentCommandCollection>> CustomSegments { get; }
			public IEnumerable<PackageToPackingParentInfo> PackageToPackingParentInfos { get; }
		}

		#endregion
	}
}
