using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Web;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportCommon.Registry;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Http;
using WTG.RTUS.Interface;
using WTG.RTUS.Printing;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(DataContentTypes))]

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PrintCarrierLabelsProcessorTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("", () => new PrintCarrierLabelsProcessor(null, ZGuid.Empty));
		}

		public void TestBookAndPrint_BatchProcessingEnabled()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 28m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			var pick = helper.CreatePickNew(order);
			Factory.Save();

			var package1 = order.PackageJob.Packages.AddNew("CTN");
			package1.KP_PackageID = "ABC";
			var package2 = order.PackageJob.Packages.AddNew("CTN");
			package2.KP_PackageID = "DEF";

			var packages = order.PackageJob.GetAllPackagesOnJob();
			AssertEquals("Precondition: should be 2 packages.", 2, packages.Length);
			Factory.Save();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var whsCarrierLabelBatchPrintingProviderMock = new Mock<ICarrierLabelsBatchPrintingProvider>();
			whsCarrierLabelBatchPrintingProviderMock.SetupGet(mock => mock.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);
			whsCarrierLabelBatchPrintingProviderMock.Setup(mock => mock.PrintCarrierLabels(
					It.IsAny<ICarrierLabelsBatchPrintingInfo>(),
					RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl),
					It.Is<ZGuid>(pr => pr == printer.PK),
					It.IsAny<CancellationToken>()))
				.Callback((ICarrierLabelsBatchPrintingInfo p, RTUSCBA type, Uri url, ZGuid pr, CancellationToken token) =>
				{
					AssertEquals(1, p.PackagesBatchUniversalShipments.Count);
					AssertEquals(0, p.CustomSegments.Count());

					AssertEquals("ABC", p.PackageToPackingParentInfos.ElementAt(0).Package.KP_PackageID);
					AssertEquals("DEF", p.PackageToPackingParentInfos.ElementAt(1).Package.KP_PackageID);
					Assert(p.PackageToPackingParentInfos.All(p => p.PackingParent.JobNo == "O1"));
					Assert(p.PackageToPackingParentInfos.All(p => p.PackingParentId == order.WD_DocketID));

					package1.KP_PackageID = "NUMBER1";
					package1.IsSentToRTUS = true;
					package2.KP_PackageID = "NUMBER2";
					package2.IsSentToRTUS = true;
					order.WD_TransportReference = "TRANSPORTREF";
				})
				.Returns(new ReturnResult { Success = true });

			var carrierLabelManagerMock = new Mock<ICarrierLabelManager>();

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManagerMock.Object))
			using (ObjectFactory.Substitute(whsCarrierLabelBatchPrintingProviderMock.Object))
			{
				AssertEquals("Precondition: PackageId", "ABC", package1.KP_PackageID);
				AssertEquals("Precondition: PackageId", "DEF", package2.KP_PackageID);
				AssertEquals("Precondition: IsSentToRTUS", false, package1.IsSentToRTUS);
				AssertEquals("Precondition: IsSentToRTUS", false, package2.IsSentToRTUS);

				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, package1.HasErrors);
				AssertEquals("Print is successful.", false, package2.HasErrors);

				AssertEquals("Print is successful.", "NUMBER1", package1.KP_PackageID);
				AssertEquals("Print is successful.", "NUMBER2", package2.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				var notes = order.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated a Failure Log.", false, notes.Length > 0);

				AssertEquals("Package is sent to RTUS.", true, package1.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, package2.IsSentToRTUS);

				whsCarrierLabelBatchPrintingProviderMock.VerifyAll();
			}
		}

		public void TestBookAndPrint_BatchProcessingEnabled_MultipleOrders()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 14m);
			order1.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 14m);
			order2.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			var pick = helper.CreatePickNew(order1, order2);
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew("CTN");
			package1.KP_PackageID = "ABC";
			var package2 = order2.PackageJob.Packages.AddNew("CTN");
			package2.KP_PackageID = "DEF";
			Factory.Save();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var whsCarrierLabelBatchPrintingProviderMock = new Mock<ICarrierLabelsBatchPrintingProvider>();
			whsCarrierLabelBatchPrintingProviderMock.SetupGet(mock => mock.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);
			whsCarrierLabelBatchPrintingProviderMock.Setup(mock => mock.PrintCarrierLabels(
					It.IsAny<ICarrierLabelsBatchPrintingInfo>(),
					RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl),
					It.Is<ZGuid>(pr => pr == printer.PK),
					It.IsAny<CancellationToken>()))
				.Callback((ICarrierLabelsBatchPrintingInfo p, RTUSCBA type, Uri url, ZGuid pr, CancellationToken token) =>
				{
					AssertEquals(1, p.PackagesBatchUniversalShipments.Count);
					AssertEquals(0, p.CustomSegments.Count());

					var order1PackageToPackingParentInfo = p.PackageToPackingParentInfos.Single(p => p.Package.KP_PackageID == "ABC");
					AssertEquals("O1", order1PackageToPackingParentInfo.PackingParent.JobNo);
					AssertEquals(order1.WD_DocketID, order1PackageToPackingParentInfo.PackingParentId);
					var order2PackageToPackingParentInfo = p.PackageToPackingParentInfos.Single(p => p.Package.KP_PackageID == "DEF");
					AssertEquals("O2", order2PackageToPackingParentInfo.PackingParent.JobNo);
					AssertEquals(order2.WD_DocketID, order2PackageToPackingParentInfo.PackingParentId);

					package1.KP_PackageID = "NUMBER1";
					package1.IsSentToRTUS = true;
					package2.KP_PackageID = "NUMBER2";
					package2.IsSentToRTUS = true;
					order1.WD_TransportReference = "TRANSPORTREF";
				})
				.Returns(new ReturnResult { Success = true });

			var carrierLabelManagerMock = new Mock<ICarrierLabelManager>();

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
						 Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManagerMock.Object))
			using (ObjectFactory.Substitute(whsCarrierLabelBatchPrintingProviderMock.Object))
			{
				AssertEquals("Precondition: PackageId", "ABC", package1.KP_PackageID);
				AssertEquals("Precondition: PackageId", "DEF", package2.KP_PackageID);
				AssertEquals("Precondition: IsSentToRTUS", false, package1.IsSentToRTUS);
				AssertEquals("Precondition: IsSentToRTUS", false, package2.IsSentToRTUS);

				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, package1.HasErrors);
				AssertEquals("Print is successful.", false, package2.HasErrors);

				AssertEquals("Print is successful.", "NUMBER1", package1.KP_PackageID);
				AssertEquals("Print is successful.", "NUMBER2", package2.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", order1.WD_TransportReference);

				var notes = order1.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated a Failure Log.", false, notes.Length > 0);

				AssertEquals("Package is sent to RTUS.", true, package1.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, package2.IsSentToRTUS);

				whsCarrierLabelBatchPrintingProviderMock.VerifyAll();
			}
		}

		public void TestBookAndPrint_BatchProcessingEnabled_WithSplitterDocs()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType =
				UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 28m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			var packageForSplitCase = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			var pickLineForSplitCase = pick.GetAllPickLines().Single(pl => pl.WZ_F3_NKAllocatedPackType == "UNT");
			packingHelper.CreatePackageDivot(packageForSplitCase, pickLineForSplitCase);

			var packages = order.PackageJob.GetAllPackagesOnJob();
			AssertEquals("Precondition: should be 3 packages: for pallet, for case and for split cases.", 3,
				packages.Length);
			Factory.Save();

			var palletPackage = packages.Single(package => package.KP_F3_NKPackType == "PLT");
			palletPackage.KP_PackageID = "PKG-PLT";
			var casePackage = packages.Single(package => package.KP_F3_NKPackType == "CAS");
			casePackage.KP_PackageID = "PKG-CAS";
			var splitCasePackage = packages.Single(package => package.KP_F3_NKPackType == "UNT");
			splitCasePackage.KP_PackageID = "PKG-SPC";

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var whsCarrierLabelBatchPrintingProviderMock = new Mock<ICarrierLabelsBatchPrintingProvider>();
			whsCarrierLabelBatchPrintingProviderMock.SetupGet(mock => mock.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);
			whsCarrierLabelBatchPrintingProviderMock.Setup(mock => mock.PrintCarrierLabels(
					It.IsAny<ICarrierLabelsBatchPrintingInfo>(),
					RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl),
					It.Is<ZGuid>(pr => pr == printer.PK),
					It.IsAny<CancellationToken>()))
				.Callback((ICarrierLabelsBatchPrintingInfo p, RTUSCBA type, Uri url, ZGuid pr, CancellationToken token) =>
				{
					AssertEquals(3, p.PackagesBatchUniversalShipments.Count);
					AssertEquals(3, p.CustomSegments.Count());

					var palletPackageCustomSegments = p.CustomSegments.Single(p => p.Key == palletPackage.PK).Value;
					AssertEquals(1, palletPackageCustomSegments.Count);
					AssertEquals("End of Pallet", palletPackageCustomSegments.Cast<IDocumentCommand>().Select(p => p.SU_MenuName).Single());

					var casePackageCustomSegments = p.CustomSegments.Single(p => p.Key == casePackage.PK).Value;
					AssertEquals(1, casePackageCustomSegments.Count);
					AssertEquals("End of Case", casePackageCustomSegments.Cast<IDocumentCommand>().Select(p => p.SU_MenuName).Single());

					var splitCaseCustomSegments = p.CustomSegments.Single(p => p.Key == splitCasePackage.PK).Value;
					AssertEquals(2, splitCaseCustomSegments.Count);
					AssertContainsExactElementsInAnyOrder(new[] { "End of Split Case", "End of Area" }, splitCaseCustomSegments.Cast<IDocumentCommand>().Select(p => p.SU_MenuName));

					AssertEquals("PKG-PLT", p.PackageToPackingParentInfos.ElementAt(0).Package.KP_PackageID);
					AssertEquals("PKG-CAS", p.PackageToPackingParentInfos.ElementAt(1).Package.KP_PackageID);
					AssertEquals("PKG-SPC", p.PackageToPackingParentInfos.ElementAt(2).Package.KP_PackageID);

					Assert(p.PackageToPackingParentInfos.All(p => p.PackingParent.JobNo == "O1"));
					Assert(p.PackageToPackingParentInfos.All(p => p.PackingParentId == order.WD_DocketID));

					palletPackage.KP_PackageID = "NUMBER1";
					palletPackage.IsSentToRTUS = true;
					casePackage.KP_PackageID = "NUMBER2";
					casePackage.IsSentToRTUS = true;
					splitCasePackage.KP_PackageID = "NUMBER3";
					splitCasePackage.IsSentToRTUS = true;
					order.WD_TransportReference = "TRANSPORTREF";
				})
				.Returns(new ReturnResult { Success = true });

			var carrierLabelManagerMock = new Mock<ICarrierLabelManager>();

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
						 Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManagerMock.Object))
			using (ObjectFactory.Substitute(whsCarrierLabelBatchPrintingProviderMock.Object))
			{
				AssertEquals("Precondition: PackageId", "PKG-PLT", palletPackage.KP_PackageID);
				AssertEquals("Precondition: PackageId", "PKG-CAS", casePackage.KP_PackageID);
				AssertEquals("Precondition: PackageId", "PKG-SPC", splitCasePackage.KP_PackageID);
				AssertEquals("Precondition: IsSentToRTUS", false, palletPackage.IsSentToRTUS);
				AssertEquals("Precondition: IsSentToRTUS", false, casePackage.IsSentToRTUS);
				AssertEquals("Precondition: IsSentToRTUS", false, splitCasePackage.IsSentToRTUS);

				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, palletPackage.HasErrors);
				AssertEquals("Print is successful.", false, casePackage.HasErrors);
				AssertEquals("Print is successful.", false, splitCasePackage.HasErrors);

				AssertEquals("Print is successful.", "NUMBER1", palletPackage.KP_PackageID);
				AssertEquals("Print is successful.", "NUMBER2", casePackage.KP_PackageID);
				AssertEquals("Print is successful.", "NUMBER3", splitCasePackage.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				var notes = order.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated a Failure Log.", false, notes.Length > 0);

				AssertEquals("Package is sent to RTUS.", true, palletPackage.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, casePackage.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, splitCasePackage.IsSentToRTUS);

				whsCarrierLabelBatchPrintingProviderMock.VerifyAll();
			}
		}

		public void TestBookAndPrint_BatchProcessingEnabled_NoRemotePrintingConnectionDetailsProvided()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 28m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			var pick = helper.CreatePickNew(order);
			Factory.Save();

			var package1 = order.PackageJob.Packages.AddNew("CTN");
			package1.KP_PackageID = "ABC";
			var package2 = order.PackageJob.Packages.AddNew("CTN");
			package2.KP_PackageID = "DEF";

			var packages = order.PackageJob.GetAllPackagesOnJob();
			AssertEquals("Precondition: should be 2 packages.", 2, packages.Length);
			Factory.Save();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var whsCarrierLabelBatchPrintingProviderMock = new Mock<ICarrierLabelsBatchPrintingProvider>();
			whsCarrierLabelBatchPrintingProviderMock.SetupGet(mock => mock.IsRemotePrintingConnectionDetailsProvided)
				.Returns(false);
			whsCarrierLabelBatchPrintingProviderMock.Setup(mock => mock.PrintCarrierLabels(
					It.IsAny<ICarrierLabelsBatchPrintingInfo>(),
					RTUSCBA.SmartFreight,
					null,
					ZGuid.Empty,
					It.IsAny<CancellationToken>()))
				.Returns(new ReturnResult { Message = "Some error." });

			var carrierLabelManagerMock = new Mock<ICarrierLabelManager>();

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManagerMock.Object))
			using (ObjectFactory.Substitute(whsCarrierLabelBatchPrintingProviderMock.Object))
			{
				AssertNoExceptionThrown(() => new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer()));
				var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
				AssertEquals("Print is not successful.", true, logs.Length > 0);
				AssertEquals("Should log the error",
					$"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, Some error.",
					logs[0].DisplayEventReference);
			}
		}

		[TestDate(2020, 7, 16, 1, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestBookAndPrint_Integration_BatchProcessingEnabled()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType =
				UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 28m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			var packageForSplitCase = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			var pickLineForSplitCase = pick.GetAllPickLines().Single(pl => pl.WZ_F3_NKAllocatedPackType == "UNT");
			packingHelper.CreatePackageDivot(packageForSplitCase, pickLineForSplitCase);

			var packages = order.PackageJob.GetAllPackagesOnJob();
			AssertEquals("Precondition: should be 3 packages: for pallet, for case and for split cases.", 3,
				packages.Length);
			Factory.Save();

			var palletPackage = packages.Single(package => package.KP_F3_NKPackType == "PLT");
			palletPackage.KP_PackageID = "PKG-PLT";
			var casePackage = packages.Single(package => package.KP_F3_NKPackType == "CAS");
			casePackage.KP_PackageID = "PKG-CAS";
			var splitCasePackage = packages.Single(package => package.KP_F3_NKPackType == "UNT");
			splitCasePackage.KP_PackageID = "PKG-SPC";

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock1 = new Mock<IBatchBookingRTUSResponse>();
			responseMock1.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock1.SetupGet(r => r.ReferenceNumbers)
				.Returns(new[] { new ReferenceNumbers(order.WD_DocketID, "PKG-PLT", "PKG1", "TRANSPORTREF") });
			responseMock1.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock1.SetupGet(m => m.FileType).Returns(FileType.PDF);

			var responseMock2 = new Mock<IBatchBookingRTUSResponse>();
			responseMock2.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock2.SetupGet(r => r.ReferenceNumbers)
				.Returns(new[] { new ReferenceNumbers(order.WD_DocketID, "PKG-CAS", "PKG2", "TRANSPORTREF") });
			responseMock2.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock2.SetupGet(m => m.FileType).Returns(FileType.PDF);

			var responseMock3 = new Mock<IBatchBookingRTUSResponse>();
			responseMock3.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock3.SetupGet(r => r.ReferenceNumbers)
				.Returns(new[] { new ReferenceNumbers(order.WD_DocketID, "PKG-SPC", "PKG3", "TRANSPORTREF") });
			responseMock3.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock3.SetupGet(m => m.FileType).Returns(FileType.PDF);

			var pushCarrierLabelCalled = 0;
			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.BatchBooking, It.IsAny<IHttpClientFactory>(),
					It.Is<Stream>(s => IsStreamGeneratedFromRequest(s, "<ReferenceNumber>PKG-PLT</ReferenceNumber>")),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock1.Object);

			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.BatchBooking, It.IsAny<IHttpClientFactory>(),
					It.Is<Stream>(s => IsStreamGeneratedFromRequest(s, "<ReferenceNumber>PKG-CAS</ReferenceNumber>")),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock2.Object);

			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.BatchBooking, It.IsAny<IHttpClientFactory>(),
					It.Is<Stream>(s => IsStreamGeneratedFromRequest(s, "<ReferenceNumber>PKG-SPC</ReferenceNumber>")),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock3.Object);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, palletPackage.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, casePackage.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, splitCasePackage.IsSentToRTUS);
				AssertEquals("Precondition: Package ID.", "PKG-PLT", palletPackage.KP_PackageID);
				AssertEquals("Precondition: Package ID.", "PKG-CAS", casePackage.KP_PackageID);
				AssertEquals("Precondition: Package ID.", "PKG-SPC", splitCasePackage.KP_PackageID);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("PushCarrieLabel called 3 times.", 3, pushCarrierLabelCalled);
				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, palletPackage.HasErrors);
				AssertEquals("Print is successful.", false, casePackage.HasErrors);
				AssertEquals("Print is successful.", false, splitCasePackage.HasErrors);

				AssertEquals("Print is successful.", "PKG1", palletPackage.KP_PackageID);
				AssertEquals("Print is successful.", "PKG2", casePackage.KP_PackageID);
				AssertEquals("Print is successful.", "PKG3", splitCasePackage.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				var notes = order.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated a Failure Log.", false, notes.Length > 0);

				AssertEquals("Package is sent to RTUS.", true, palletPackage.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, casePackage.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, splitCasePackage.IsSentToRTUS);

				rtusProcessorMock.VerifyAll();

				var query = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
				var otherFactory = new BusinessObjectFactory();
				var printJobs = otherFactory.Load<StmPrintJob>(query);
				AssertEquals("There should be 7 docs print jobs, 3 package labels and 4 splitter labels.", 7,
					printJobs.Length);
				Assert(nameof(StmPrintJobSchema.SP_Copies), printJobs.All(printJob => printJob.SP_Copies == 1));
				AssertSequencesEqual(nameof(StmPrintJobSchema.SP_EmailAttachments),
					new ZString[]
					{
						"Label1.PDF", "Label2.PDF", "Label3.PDF", "Label4.PDF", "Label5.PDF", "Label6.PDF",
						"Label7.PDF"
					},
					printJobs.OrderBy(printJob => printJob.SP_Sequence)
						.Select(printJob => printJob.SP_EmailAttachments));
				Assert(nameof(StmPrintJobSchema.SP_JobType), printJobs.All(printJob => printJob.SP_JobType == "PRN"));
				Assert(nameof(StmPrintJobSchema.SP_RunDateTime),
					printJobs.All(printJob => printJob.SP_RunDateTime == new ZDateTime(2020, 7, 16, 1, 1, 1)));

				var deliveryGroup =
					otherFactory.Load<IStmDeliveryGroup>(printJobs.Select(printJob => printJob.SP_SB_DeliveryGroup)
						.Distinct().Single());
				AssertEquals(nameof(deliveryGroup.SB_IsProcessed), true, deliveryGroup.SB_IsProcessed);
			}
		}

		static bool IsStreamGeneratedFromRequest(Stream originalStream, string packageReferenceString)
		{
			using (var streamToRead = new MemoryStream())
			{
				originalStream.CopyTo(streamToRead);
				originalStream.Position = 0;
				streamToRead.Position = 0;

				using (var reader = new StreamReader(streamToRead))
				{
					var uxml = reader.ReadToEnd();
					return uxml.Contains(packageReferenceString);
				}
			}
		}

		public void TestBookAndPrint_PrintThrowsErrorOnAnotherThread_BatchProcessingEnabled()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 10);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var packageCAS = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "CAS");
			var packagePLT = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "PLT");

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var whsCarrierLabelBatchPrintingProviderMock = new Mock<ICarrierLabelsBatchPrintingProvider>();
			whsCarrierLabelBatchPrintingProviderMock.SetupGet(mock => mock.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(whsCarrierLabelBatchPrintingProviderMock.Object))
			{
				var carrierLabelsProcessor = new PrintCarrierLabelsProcessor(pick, printer.PK);
				Thread otherThread = null;
				whsCarrierLabelBatchPrintingProviderMock.Setup(mock => mock.PrintCarrierLabels(
						It.IsAny<ICarrierLabelsBatchPrintingInfo>(),
						RTUSCBA.SmartFreight,
						new Uri(SmartFreightUrl),
						It.Is<ZGuid>(pr => pr == printer.PK),
						It.IsAny<CancellationToken>()))
					.Callback(() =>
					{
						otherThread = new Thread(() =>
						{
							using (Db.DisposableActionForDbConnection())
							{
								carrierLabelsProcessor.OnError.Invoke(
									new InvalidOperationException(
										"This is an invalid operation exception on another thread."));
							}
						});

						otherThread.Start();
						otherThread.Join();
					})
					.Returns(new ReturnResult { Message = "Error!" });

				AssertEquals("Precondition: Package is not sent to RTUS.", false, packageCAS.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packagePLT.IsSentToRTUS);
				carrierLabelsProcessor.Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				var errorLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code);
				errorLogQuery.AddToFilter(StmALogSchema.SL_Reference,
					"This is an invalid operation exception on another thread.|RES=Failed to print carrier label|TYP=Bulk Carrier Label Booking");

				var logs = pick.Logs.Find(errorLogQuery);
				AssertEquals("Print is not successful.", true, logs.Length > 0);
				AssertEquals("Printing exception error log added on pick only once.", 1, logs.Length);

				AssertEquals("Package is not sent to RTUS.", false, packageCAS.IsSentToRTUS);
				AssertEquals("Package is not sent to RTUS.", false, packagePLT.IsSentToRTUS);

				AssertEquals("No exceptions were thrown.", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestBookAndPrint_PackagesPrintedInCorrectOrder_BatchProcessingEnabled()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 10);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var packageCAS = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "CAS");
			packageCAS.KP_PackageID = "PKG-CAS";
			var packagePLT = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "PLT");
			packagePLT.KP_PackageID = "PKG-PLT";

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var whsCarrierLabelBatchPrintingProviderMock = new Mock<ICarrierLabelsBatchPrintingProvider>();
			whsCarrierLabelBatchPrintingProviderMock.SetupGet(mock => mock.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);
			whsCarrierLabelBatchPrintingProviderMock.Setup(mock => mock.PrintCarrierLabels(
					It.IsAny<ICarrierLabelsBatchPrintingInfo>(),
					RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl),
					It.Is<ZGuid>(pr => pr == printer.PK),
					It.IsAny<CancellationToken>()))
				.Callback(
					(ICarrierLabelsBatchPrintingInfo p, RTUSCBA type, Uri url, ZGuid pr, CancellationToken token) =>
					{
						AssertEquals(2, p.PackagesBatchUniversalShipments.Count);
						AssertEquals("PKG-PLT", p.PackageToPackingParentInfos.ElementAt(0).Package.KP_PackageID);
						AssertEquals("PKG-CAS", p.PackageToPackingParentInfos.ElementAt(1).Package.KP_PackageID);

						packagePLT.KP_PackageID = "NUMBER1";
						packagePLT.IsSentToRTUS = true;
						packageCAS.KP_PackageID = "NUMBER2";
						packageCAS.IsSentToRTUS = true;
						order.WD_TransportReference = "TRANSPORTREF";
					})
				.Returns(new ReturnResult { Success = true });

			var carrierLabelManagerMock = new Mock<ICarrierLabelManager>();
			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManagerMock.Object))
			using (ObjectFactory.Substitute(whsCarrierLabelBatchPrintingProviderMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packageCAS.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packagePLT.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, packageCAS.HasErrors);
				AssertEquals("Print is successful.", false, packagePLT.HasErrors);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);
				AssertEquals("Print is successful.", "NUMBER1", packagePLT.KP_PackageID);
				AssertEquals("Print is successful.", "NUMBER2", packageCAS.KP_PackageID);

				whsCarrierLabelBatchPrintingProviderMock.VerifyAll();
			}
		}

		[TestDate(2020, 7, 16, 1, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestBookAndPrint_InnerPackagesNotIncluded()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 25m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			var packages = order.PackageJob.GetAllPackagesOnJob();
			AssertEquals("Precondition: should be 2 packages: for pallet and for case.", 2, packages.Length);
			Factory.Save();

			var palletPackage = packages.Single(package => package.KP_F3_NKPackType == "PLT");
			palletPackage.KP_PackageID = "PKG-PLT";
			var innerPackageOnPallet = palletPackage.Packages.AddNew();
			var casePackage = packages.Single(package => package.KP_F3_NKPackType == "CAS");
			casePackage.KP_PackageID = "PKG-CAS";

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock1 = new Mock<IBatchBookingRTUSResponse>();
			responseMock1.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock1.SetupGet(r => r.ReferenceNumbers)
				.Returns(new[] { new ReferenceNumbers(order.WD_DocketID, "PKG-PLT", "PKG1", "TRANSPORTREF") });
			responseMock1.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock1.SetupGet(m => m.FileType).Returns(FileType.PDF);

			var responseMock2 = new Mock<IBatchBookingRTUSResponse>();
			responseMock2.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock2.SetupGet(r => r.ReferenceNumbers)
				.Returns(new[] { new ReferenceNumbers(order.WD_DocketID, "PKG-CAS", "PKG2", "TRANSPORTREF") });
			responseMock2.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock2.SetupGet(m => m.FileType).Returns(FileType.PDF);

			var pushCarrierLabelCalled = 0;
			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.BatchBooking, It.IsAny<IHttpClientFactory>(),
					It.Is<Stream>(s => IsStreamGeneratedFromRequest(s, "<ReferenceNumber>PKG-PLT</ReferenceNumber>")),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock1.Object);

			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.BatchBooking, It.IsAny<IHttpClientFactory>(),
					It.Is<Stream>(s => IsStreamGeneratedFromRequest(s, "<ReferenceNumber>PKG-CAS</ReferenceNumber>")),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock2.Object);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, palletPackage.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, casePackage.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, innerPackageOnPallet.IsSentToRTUS);
				AssertEquals("Precondition: Package ID.", "PKG-PLT", palletPackage.KP_PackageID);
				AssertEquals("Precondition: Package ID.", "PKG-CAS", casePackage.KP_PackageID);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("PushCarrieLabel called 2 times.", 2, pushCarrierLabelCalled);
				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, palletPackage.HasErrors);
				AssertEquals("Print is successful.", false, casePackage.HasErrors);

				AssertEquals("Print is successful.", "PKG1", palletPackage.KP_PackageID);
				AssertEquals("Print is successful.", "PKG2", casePackage.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				var notes = order.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated a Failure Log.", false, notes.Length > 0);

				AssertEquals("Package is sent to RTUS.", true, palletPackage.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, casePackage.IsSentToRTUS);
				AssertEquals("Package is not sent to RTUS.", false, innerPackageOnPallet.IsSentToRTUS);

				rtusProcessorMock.VerifyAll();

				var query = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
				var otherFactory = new BusinessObjectFactory();
				var printJobs = otherFactory.Load<IStmPrintJob>(query);
				AssertEquals("There should be 5 docs print jobs, 2 package labels and 3 splitter labels.", 5,
					printJobs.Length);
				Assert(nameof(StmPrintJobSchema.SP_Copies), printJobs.All(printJob => printJob.SP_Copies == 1));
				AssertContainsExactElementsInAnyOrder(nameof(StmPrintJobSchema.SP_EmailAttachments),
					new[] { "Label1.PDF", "Label2.PDF", "Label3.PDF", "Label4.PDF", "Label5.PDF" },
					printJobs.Select(printJob => printJob.SP_EmailAttachments));
				Assert(nameof(StmPrintJobSchema.SP_JobType), printJobs.All(printJob => printJob.SP_JobType == "PRN"));
				Assert(nameof(StmPrintJobSchema.SP_RunDateTime),
					printJobs.All(printJob => printJob.SP_RunDateTime == new ZDateTime(2020, 7, 16, 1, 1, 1)));

				var deliveryGroup =
					otherFactory.Load<IStmDeliveryGroup>(printJobs.Select(printJob => printJob.SP_SB_DeliveryGroup)
						.Distinct().Single());
				AssertEquals(nameof(deliveryGroup.SB_IsProcessed), true, deliveryGroup.SB_IsProcessed);
			}
		}

		public void TestBookAndPrint_CancellationTokenIsPassedIn()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 10);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var packageCAS = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "CAS");
			packageCAS.KP_PackageID = "PKG-CAS";
			var packagePLT = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "PLT");
			packagePLT.KP_PackageID = "PKG-PLT";

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var cancellationTokenSource = new CancellationTokenSource();
			var tokenPassedIn = default(CancellationToken);
			var whsCarrierLabelBatchPrintingProviderMock = new Mock<ICarrierLabelsBatchPrintingProvider>();
			whsCarrierLabelBatchPrintingProviderMock.SetupGet(mock => mock.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);
			whsCarrierLabelBatchPrintingProviderMock.Setup(mock => mock.PrintCarrierLabels(
					It.IsAny<ICarrierLabelsBatchPrintingInfo>(),
					RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl),
					It.Is<ZGuid>(pr => pr == printer.PK),
					It.IsAny<CancellationToken>()))
				.Callback(
					(ICarrierLabelsBatchPrintingInfo p, RTUSCBA type, Uri url, ZGuid pr, CancellationToken token) =>
					{
						tokenPassedIn = token;
					})
				.Returns(new ReturnResult { Success = true });

			var carrierLabelManagerMock = new Mock<ICarrierLabelManager>();
			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManagerMock.Object))
			using (ObjectFactory.Substitute(whsCarrierLabelBatchPrintingProviderMock.Object))
			{
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer(),
					cancellationTokenSource.Token);
				AssertNotEquals(tokenPassedIn, default(CancellationToken));
				AssertEquals(tokenPassedIn, cancellationTokenSource.Token);
				whsCarrierLabelBatchPrintingProviderMock.VerifyAll();
			}
		}

		public void TestBookAndPrint_NoPrinterPassedIn()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			trigger.ProcessTaskNotifications.AddNew();
			Factory.Save();

			AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
			new PrintCarrierLabelsProcessor(pick, ZGuid.Empty).Process(new TestNotificationBuffer());

			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Print is not successful.", true, logs.Length > 0);
			AssertEquals("Should have no printer error.",
				"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, No printer is selected to print carrier labels.",
				logs[0].DisplayEventReference);

			var newFactory = new BusinessObjectFactory();
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
			AssertEquals("Package is not sent to RTUS.", false, packageInNewFactory.IsSentToRTUS);
		}

		public void TestBookAndPrint_NoCarrierBookingAgent()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order1.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order1.CarrierBookingAgent.PK);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			AssertNull(order2.CarrierBookingAgent);

			var pick = Helper.CreatePickByAttachingOrders(order1, order2);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var packages = pick.OuterPackages;
			var packageForOrder1 = packages.Single(package => package.PackageJob.ParentJob.PK == order1.PK);
			var packageForOrder2 = packages.Single(package => package.PackageJob.ParentJob.PK == order2.PK);

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packageForOrder1.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packageForOrder2.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
				AssertEquals("Print is not successful.", true, logs.Length > 0);
				AssertEquals("Should have a 'no carrier booking agent' error.", true,
					logs.Any(log =>
						log.DisplayEventReference ==
						$"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, Not all orders has a Carrier Booking Agent."));
			}
		}

		public void TestBookAndPrint_FailedToSubscribe()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType =
				UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 28m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			var packageForSplitCase = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			var pickLineForSplitCase = pick.GetAllPickLines().Single(pl => pl.WZ_F3_NKAllocatedPackType == "UNT");
			packingHelper.CreatePackageDivot(packageForSplitCase, pickLineForSplitCase);

			var packages = order.PackageJob.GetAllPackagesOnJob();
			AssertEquals("Precondition: should be 3 packages: for pallet, for case and for split cases.", 3,
				packages.Length);
			Factory.Save();

			var palletPackage = packages.Single(package => package.KP_F3_NKPackType == "PLT");
			palletPackage.KP_PackageID = string.Empty;
			var casePackage = packages.Single(package => package.KP_F3_NKPackType == "CAS");
			casePackage.KP_PackageID = string.Empty;
			var splitCasePackage = packages.Single(package => package.KP_F3_NKPackType == "UNT");
			splitCasePackage.KP_PackageID = string.Empty;

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var rtusSubscriberMock = new Mock<IWhsCarrierLabelsBatchPrintingSubscriber>();
			rtusSubscriberMock.Setup(mock => mock.SubscribePackages(It.IsAny<WhsPick>(),
					It.IsAny<IEnumerable<WhsOrderToPackageItemNumbers>>(), It.IsAny<IReadOnlyCollection<int>>()))
				.Returns(new ReturnResult { Success = false, Message = "Error!" });

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusSubscriberMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, palletPackage.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, casePackage.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, splitCasePackage.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				AssertEquals("Package is not sent to RTUS.", false, palletPackage.IsSentToRTUS);
				AssertEquals("Package is not sent to RTUS.", false, casePackage.IsSentToRTUS);
				AssertEquals("Package is not sent to RTUS.", false, splitCasePackage.IsSentToRTUS);

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
				AssertEquals("Print is not successful.", true, logs.Length > 0);
				AssertEquals("Should have a subscription error.", true,
					logs.Any(log =>
						log.ReferenceFreeText ==
						"Failed to subscribe packages to RTUS batch booking due to: 'Error!'."));
			}
		}

		public void TestBookAndPrint_RegistryMissingRTUS_BatchProcessingEnabled()
		{
			var smartFreight = Helper.CreateClient();
			smartFreight.OH_Code = "SMART";

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
				AssertEquals("Print is not successful.", true, logs.Length > 0);
				AssertEquals("Should have a no carrier booking agent error.",
					$"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, No RTUS URL provided for Organization 'SMART'.",
					logs[0].DisplayEventReference);

				var newFactory = new BusinessObjectFactory();
				var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
				AssertEquals("Package is not sent to RTUS.", false, packageInNewFactory.IsSentToRTUS);
			}
		}

		public void TestBookAndPrint_GetRTUSOptionIsCalledOnceOnly()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 10);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order1.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order1.CarrierBookingAgent.PK);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order2.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order2.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickByAttachingOrders(order1, order2);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var packageCAS = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "CAS");
			packageCAS.KP_PackageID = "PKG-CAS";
			var packagePLT = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "PLT");
			packagePLT.KP_PackageID = "PKG-PLT";

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var getRTUSOptionCalled = 0;
			var registryMock = new Mock<ITransportRegistry>();
			registryMock.Setup(r => r.GetRTUSOption(smartFreight.PK.ToGuid()))
				.Callback(() => getRTUSOptionCalled++)
				.Returns(new OrganisationRTUSOption
				{
					OrganisationPK = smartFreight.PK,
					CBACode = "SMA",
					Url = SmartFreightUrl
				});

			var boolTestRegistryItem = new BooleanRegistryItem(
				"Test",
				(NoResString)"Test",
				(NoResString)"Test",
				(NoResString)"Test",
				RegistryStorageFlags.System,
				true
			);

			registryMock.SetupGet(r => r.EnableRTUSBatchProcessing)
				.Returns(boolTestRegistryItem);

			var whsCarrierLabelBatchPrintingProviderMock = new Mock<ICarrierLabelsBatchPrintingProvider>();
			whsCarrierLabelBatchPrintingProviderMock.SetupGet(mock => mock.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);
			whsCarrierLabelBatchPrintingProviderMock.Setup(mock => mock.PrintCarrierLabels(
					It.IsAny<ICarrierLabelsBatchPrintingInfo>(),
					RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl),
					It.Is<ZGuid>(pr => pr == printer.PK),
					It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					packagePLT.KP_PackageID = "NUMBER1";
					packagePLT.IsSentToRTUS = true;
					packageCAS.KP_PackageID = "NUMBER2";
					packageCAS.IsSentToRTUS = true;
					order1.WD_TransportReference = "TRANSPORTREF";
					order2.WD_TransportReference = "TRANSPORTREF";
				})
				.Returns(new ReturnResult { Success = true });

			var carrierLabelManagerMock = new Mock<ICarrierLabelManager>();

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(registryMock.Object))
			using (ObjectFactory.Substitute(carrierLabelManagerMock.Object))
			using (ObjectFactory.Substitute(whsCarrierLabelBatchPrintingProviderMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packageCAS.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packagePLT.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, packageCAS.HasErrors);
				AssertEquals("Print is successful.", false, packagePLT.HasErrors);
				AssertEquals("Print is successful.", "TRANSPORTREF", order1.WD_TransportReference);
				AssertEquals("Print is successful.", "TRANSPORTREF", order2.WD_TransportReference);

				AssertEquals("Package is sent to RTUS.", true, packagePLT.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, packageCAS.IsSentToRTUS);

				AssertEquals("GetRTUSOption only got called once.", getRTUSOptionCalled, 1);
			}
		}

		public void TestBookAndPrint_DifferentCBAs()
		{
			var otherBookingAgent = Helper.CreateClient("XXX");
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order1.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order1.CarrierBookingAgent.PK);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order2.CarrierBookingAgentDocAddress.E2_OA_Address = otherBookingAgent.MainAddress.PK;
			AssertEquals(otherBookingAgent.PK, order2.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package1 = pick.OuterPackages.Single(package => package.PackageJob.ParentJob.PK == order1.PK);
			package1.KP_PackageID = "PKG1";
			var package2 = pick.OuterPackages.Single(package => package.PackageJob.ParentJob.PK == order2.PK);
			package2.KP_PackageID = "PKG2";

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			AssertEquals("Precondition: Package is not sent to RTUS.", false, package1.IsSentToRTUS);
			AssertEquals("Precondition: Package is not sent to RTUS.", false, package2.IsSentToRTUS);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			{
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
				AssertEquals("Print is not successful.", true, logs.Length > 0);
				AssertEquals("Should have a cannot have different carrier booking agents error.",
					$"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, Not all orders have the same carrier booking agents.",
					logs[0].DisplayEventReference);

				var newFactory = new BusinessObjectFactory();
				var package1InNewFactory = newFactory.Load<PkgPackage>(package1.PK);
				AssertEquals("Package is not sent to RTUS.", false, package1InNewFactory.IsSentToRTUS);

				var package2InNewFactory = newFactory.Load<PkgPackage>(package2.PK);
				AssertEquals("Package is not sent to RTUS.", false, package2InNewFactory.IsSentToRTUS);
			}
		}

		public void TestBookAndPrint_MultipleRequests_ErrorOnOneRequest()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType =
				UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 28m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			var packageForSplitCase = packingHelper.CreatePackage(order.PackageJob, "PKG-SPC", 1, "UNT");
			var pickLineForSplitCase = pick.GetAllPickLines().Single(pl => pl.WZ_F3_NKAllocatedPackType == "UNT");
			packingHelper.CreatePackageDivot(packageForSplitCase, pickLineForSplitCase);

			var packages = order.PackageJob.GetAllPackagesOnJob();
			AssertEquals("Precondition: should be 3 packages: for pallet, for case and for split cases.", 3,
				packages.Length);
			Factory.Save();

			var palletPackage = packages.Single(package => package.KP_F3_NKPackType == "PLT");
			palletPackage.KP_PackageID = "PKG-PLT";
			var casePackage = packages.Single(package => package.KP_F3_NKPackType == "CAS");
			casePackage.KP_PackageID = "PKG-CAS";
			var splitCasePackage = packages.Single(package => package.KP_F3_NKPackType == "UNT");

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock1 = new Mock<IBatchBookingRTUSResponse>();
			responseMock1.SetupGet(r => r.IsSuccessful).Returns(true);
			responseMock1.SetupGet(r => r.ReferenceNumbers)
				.Returns(new[] { new ReferenceNumbers(order.WD_DocketID, "PKG-PLT", "PKG1", "TRANSPORTREF") });
			responseMock1.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock1.SetupGet(m => m.FileType).Returns(FileType.PDF);

			var responseMock2 = new Mock<IBatchBookingRTUSResponse>();
			responseMock2.SetupGet(r => r.IsSuccessful).Returns(false);
			responseMock2.SetupGet(r => r.ErrorMessageForFailure).Returns("Error!");

			var pushCarrierLabelCalled = 0;
			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.BatchBooking, It.IsAny<IHttpClientFactory>(),
					It.Is<Stream>(s => IsStreamGeneratedFromRequest(s, "<ReferenceNumber>PKG-PLT</ReferenceNumber>")),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock1.Object);

			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.BatchBooking, It.IsAny<IHttpClientFactory>(),
					It.Is<Stream>(s => IsStreamGeneratedFromRequest(s, "<ReferenceNumber>PKG-CAS</ReferenceNumber>")),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Callback(() => pushCarrierLabelCalled++)
				.Returns(responseMock2.Object);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, palletPackage.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, casePackage.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, splitCasePackage.IsSentToRTUS);
				AssertEquals("Precondition: Package ID.", "PKG-PLT", palletPackage.KP_PackageID);
				AssertEquals("Precondition: Package ID.", "PKG-CAS", casePackage.KP_PackageID);
				AssertEquals("Precondition: Package ID.", "PKG-SPC", splitCasePackage.KP_PackageID);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("PushCarrieLabel called 2 times only.", 2, pushCarrierLabelCalled);

				var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
				AssertEquals("Print is not successful.", true, logs.Length > 0);
				AssertEquals("Should have a cannot have different carrier booking agents error.",
					$"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, Error!",
					logs[0].DisplayEventReference);

				AssertEquals("Package id is updated as RTUS request is successful.", "PKG1",
					palletPackage.KP_PackageID);
				AssertEquals("Package id is not updated as RTUS request is not successful.", "PKG-CAS",
					casePackage.KP_PackageID);
				AssertEquals("Package id is not updated as prior RTUS request is not successful.", "PKG-SPC",
					splitCasePackage.KP_PackageID);
				AssertEquals("Transport reference updated from successful response.", "TRANSPORTREF",
					order.WD_TransportReference);

				AssertEquals("Package is sent to RTUS.", true, palletPackage.IsSentToRTUS);
				AssertEquals("Package is not sent to RTUS.", false, casePackage.IsSentToRTUS);
				AssertEquals("Package is not sent to RTUS.", false, splitCasePackage.IsSentToRTUS);

				rtusProcessorMock.VerifyAll();

				var query = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
				var otherFactory = new BusinessObjectFactory();
				var printJobs = otherFactory.Load<IStmPrintJob>(query);
				AssertEquals("No print jobs are created.", false, printJobs.Length > 0);
			}
		}

		#region RTUS Single Processing

		public void TestBookAndPrint_BatchProcessingDisabled()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA cba, Uri url, IStmPrintQueue prn) =>
				{
					p.KP_PackageID = "TRACKING"; // simulate setting Tracking Number
					p.IsSentToRTUS = true;
					p.PackageJob.ParentJob.TransportReference = "TRANSPORTREF";
				})
				.Returns(new ReturnResult { Success = true });

			carrierLabelProvider.Setup(c => c.Print(It.IsAny<FileType>(), It.IsAny<byte[]>(),
					It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Returns(true);

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose())
				.Callback(() => carrierLabelManager.Object.Dispose());

			carrierLabelProvider.Setup(c => c.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				AssertNotEquals("Precondition: Package ID is different, not 'TRACKING'.", "TRACKING",
					package.KP_PackageID);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, package.HasErrors);
				AssertEquals("Print is successful.", "TRACKING", package.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				var notes = order.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated a Failure Log.", false, notes.Length > 0);

				AssertEquals("Package is sent to RTUS.", true, package.IsSentToRTUS);

				carrierLabelProvider.VerifyAll();
			}
		}

		public void TestBookAndPrint_Integration_BatchProcessingDisabled()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<ISingleBookingRTUSResponse>();
			responseMock.SetupGet(m => m.FileType).Returns(FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock.SetupGet(m => m.TrackingNumber).Returns("NUMBER");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			responseMock.SetupGet(m => m.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(m => m.IsSuccessful).Returns(true);

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsAny<IHttpClientFactory>(),
					It.Is<Stream>(s => IsStreamGeneratedFromCorrectWriter(s)), RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl)))
				.Returns(responseMock.Object);

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(p => p.Print(FileType.PDF, It.IsAny<byte[]>(), "PRINTER", "PRINTSERVER"))
				.Returns(true);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(printerMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				AssertNotEquals("Precondition: Package ID is different, not 'NUMBER'.", "NUMBER", package.KP_PackageID);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, package.HasErrors);
				AssertEquals("Print is successful.", "NUMBER", package.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				AssertEquals("Package is sent to RTUS.", true, package.IsSentToRTUS);
				AssertEquals("Printer is saved to the package for reprint purposes.", printer.PK,
					package.RTUSLabelPrinterPK);
			}
		}

		static bool IsStreamGeneratedFromCorrectWriter(Stream originalStream)
		{
			using (var streamToRead = new MemoryStream())
			{
				originalStream.CopyTo(streamToRead);
				originalStream.Position = 0;
				streamToRead.Position = 0;

				using (var reader = new StreamReader(streamToRead))
				{
					var uxml = reader.ReadToEnd();
					return uxml.Contains("<OrderNumber>O1</OrderNumber>");
				}
			}
		}

		public void TestBookAndPrint_PrintThrowsErrorOnAnotherThread_BatchProcessingDisabled()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 10);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var packageCAS = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "CAS");
			var packagePLT = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "PLT");

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<ISingleBookingRTUSResponse>();
			responseMock.SetupGet(m => m.FileType).Returns(FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock.SetupGet(m => m.TrackingNumber).Returns("NUMBER");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			responseMock.SetupGet(m => m.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(m => m.IsSuccessful).Returns(true);

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsAny<IHttpClientFactory>(),
					It.IsAny<Stream>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Returns(responseMock.Object);

			var printerMock = new Mock<IRTUSPrinter>();
			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(printerMock.Object))
			{
				var carrierLabelsProcessor = new PrintCarrierLabelsProcessor(pick, printer.PK);
				Thread otherThread = null;
				printerMock.Setup(p => p.Print(FileType.PDF, It.IsAny<byte[]>(), "PRINTER", "PRINTSERVER"))
					.Callback(() =>
					{
						otherThread = new Thread(() =>
						{
							using (Db.DisposableActionForDbConnection())
							{
								carrierLabelsProcessor.OnError.Invoke(
									new InvalidOperationException(
										"This is an invalid operation exception on another thread."));
							}
						});

						otherThread.Start();
						otherThread.Join();
					})
					.Returns(false);

				AssertEquals("Precondition: Package is not sent to RTUS.", false, packageCAS.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packagePLT.IsSentToRTUS);
				carrierLabelsProcessor.Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				var errorLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code);
				errorLogQuery.AddToFilter(StmALogSchema.SL_Reference,
					"This is an invalid operation exception on another thread.|RES=Failed to print carrier label|TYP=Bulk Carrier Label Booking");

				var logs = pick.Logs.Find(errorLogQuery);
				AssertEquals("Print is not successful.", true, logs.Length > 0);
				AssertEquals("Printing exception error log added on pick only once.", 1, logs.Length);

				AssertEquals("Package is sent to RTUS.", true, packageCAS.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, packagePLT.IsSentToRTUS);

				AssertEquals("No exceptions were thrown.", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestBookAndPrint_PackagesPrintedInCorrectOrder_BatchProcessingDisabled()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 10);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var packageCAS = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "CAS");
			var packagePLT = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "PLT");

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var printedPackages = new List<PkgPackage>();
			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.IsAny<PkgPackage>(), RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA type, Uri url, IStmPrintQueue prn) =>
				{
					printedPackages.Add(p);
					p.IsSentToRTUS = true;
					p.PackageJob.ParentJob.TransportReference = "TRANSPORTREF";
				})
				.Returns(new ReturnResult { Success = true });

			carrierLabelProvider.Setup(c => c.Print(It.IsAny<FileType>(), It.IsAny<byte[]>(),
					It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Returns(true);

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose())
				.Callback(() => carrierLabelManager.Object.Dispose());

			carrierLabelProvider.Setup(c => c.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packageCAS.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packagePLT.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, packageCAS.HasErrors);
				AssertEquals("Print is successful.", false, packagePLT.HasErrors);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				AssertEquals("Packages are printed in correct order - PLT 1st.", packagePLT.PK, printedPackages[0].PK);
				AssertEquals("Packages are printed in correct order - CAS 2nd.", packageCAS.PK, printedPackages[1].PK);

				AssertEquals("Package is sent to RTUS.", true, packagePLT.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, packageCAS.IsSentToRTUS);

				carrierLabelProvider.VerifyAll();
			}
		}

		public void TestBookAndPrint_SplitterDocumentsArePrinted_BatchProcessingDisabled()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<ISingleBookingRTUSResponse>();
			responseMock.SetupGet(m => m.FileType).Returns(FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock.SetupGet(m => m.TrackingNumber).Returns("NUMBER");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			responseMock.SetupGet(m => m.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(m => m.IsSuccessful).Returns(true);

			var documentUtility = new DocumentUtility(Factory, false);
			var endOfCaseDocument = Factory.Load<IDocumentCommand>(new ZGuid("33307ec0-bd70-4ccd-8267-58f761e6278f"));
			var endOfCaseInBytes =
				documentUtility.GetDocument(package, endOfCaseDocument.SU_MenuName, null, DataContentTypes.Pdf);

			var endOfAreaDocument = Factory.Load<IDocumentCommand>(new ZGuid("174bbaf6-6f87-4299-9fe1-243335bc4199"));
			var endOfAresInBytes =
				documentUtility.GetDocument(package, endOfAreaDocument.SU_MenuName, null, DataContentTypes.Pdf);

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsAny<IHttpClientFactory>(),
					It.Is<Stream>(s => IsStreamGeneratedFromCorrectWriter(s)), RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl)))
				.Returns(responseMock.Object);

			var timesPrintIsCalled = 0;
			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(p => p.Print(FileType.PDF, binaryData, "PRINTER", "PRINTSERVER"))
				.Callback(() => timesPrintIsCalled++)
				.Returns(true);

			var endOfAreaIsPrinted = false;
			printerMock.Setup(p => p.Print(FileType.PDF,
					It.Is<byte[]>(binData => endOfAresInBytes.Length == binData.Length), "PRINTER", "PRINTSERVER"))
				.Callback(() =>
				{
					timesPrintIsCalled++;
					endOfAreaIsPrinted = true;
				})
				.Returns(true);

			var endOfCaseIsPrinted = false;
			printerMock.Setup(p => p.Print(FileType.PDF,
					It.Is<byte[]>(binData => endOfCaseInBytes.Length == binData.Length), "PRINTER", "PRINTSERVER"))
				.Callback(() =>
				{
					timesPrintIsCalled++;
					endOfCaseIsPrinted = true;
				})
				.Returns(true);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(printerMock.Object))
			{
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, package.HasErrors);
				AssertEquals("Print is successful.", "NUMBER", package.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);
				AssertEquals("Label + splitter docs printed.", 3, timesPrintIsCalled);
				AssertEquals("Splitter document is printed.", true, endOfCaseIsPrinted);
				AssertEquals("Splitter document is printed.", true, endOfAreaIsPrinted);
			}
		}

		public void TestBookAndPrint_ResponseFailed()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order1.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order1.CarrierBookingAgent.PK);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order2.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order2.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickByAttachingOrders(order1, order2);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var packages = pick.OuterPackages;
			var packageForOrder1 = packages.Single(package => package.PackageJob.ParentJob.PK == order1.PK);
			var packageForOrder2 = packages.Single(package => package.PackageJob.ParentJob.PK == order2.PK);

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == packageForOrder1.PK),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA type, Uri url, IStmPrintQueue prn) =>
				{
					p.KP_PackageID = "TRACKING"; // simulate setting Tracking Number
					p.IsSentToRTUS = true;
					p.PackageJob.ParentJob.TransportReference = "TRANSPORTREF";
				})
				.Returns(new ReturnResult { Success = true });

			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == packageForOrder2.PK),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Returns(new ReturnResult { Success = false, Message = "Error!" });

			carrierLabelProvider.Setup(c => c.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose())
				.Callback(() => carrierLabelManager.Object.Dispose());

			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packageForOrder1.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packageForOrder2.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
				AssertEquals("Print is not successful for all packages.", true, logs.Length > 0);
				AssertEquals("Should have a 'not all packages booked and printed' error.", true,
					logs.Any(log => log.DisplayEventReference ==
									"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, Not all Packages were successfully booked with a Carrier and had their Label Printed, see Notes on the relevant Orders for details."));

				var notesOnOrder2 = order2.GetNotes()
					.FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated a Failure Log.", 1, notesOnOrder2.Length);
				AssertEquals("Should have generated a Failure Log.",
					"Carrier label printing failed for package 'O2-001'. Error: Error!", notesOnOrder2[0].ST_NoteText);
				carrierLabelProvider.VerifyAll();
			}
		}

		public void TestBookAndPrint_PrintFailed()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickByAttachingOrders(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<ISingleBookingRTUSResponse>();
			responseMock.SetupGet(m => m.FileType).Returns(FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock.SetupGet(m => m.TrackingNumber).Returns("NUMBER");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			responseMock.SetupGet(m => m.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(m => m.IsSuccessful).Returns(true);

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsAny<IHttpClientFactory>(),
					It.Is<Stream>(s => IsStreamGeneratedFromCorrectWriter(s)), RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl)))
				.Returns(responseMock.Object);

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(p => p.Print(FileType.PDF, binaryData, "PRINTER", "PRINTSERVER"))
				.Returns(false);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(printerMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
				AssertEquals("Print is not successful for all packages.", true, logs.Length > 0);
				AssertEquals("Should have a 'not all packages booked and printed' error.", true,
					logs.Any(log => log.DisplayEventReference ==
									"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, Not all Packages were successfully booked with a Carrier and had their Label Printed, see Notes on the relevant Orders for details."));

				var notesOnOrder2 = order.GetNotes()
					.FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should have generated a Failure Log.", 1, notesOnOrder2.Length);
				AssertEquals("Should have generated a Failure Log.",
					"Carrier label printing failed for package 'NUMBER'. Error: Print Request for Package 'NUMBER' failed.",
					notesOnOrder2[0].ST_NoteText);
			}
		}

		public void TestBookAndPrint_RegistryMissingRTUS_BatchProcessingDisabled()
		{
			var smartFreight = Helper.CreateClient();
			smartFreight.OH_Code = "SMART";

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
				AssertEquals("Print is not successful.", true, logs.Length > 0);
				AssertEquals("Should have a no carrier booking agent error.",
					$"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, Order 'O1' - No RTUS URL provided for Organization 'SMART'.",
					logs[0].DisplayEventReference);

				var newFactory = new BusinessObjectFactory();
				var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);
				AssertEquals("Package is not sent to RTUS.", false, packageInNewFactory.IsSentToRTUS);
			}
		}

		public void TestBookAndPrint_SplitterDocumentsFailsToPrint()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order1.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order1.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order1);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<ISingleBookingRTUSResponse>();
			responseMock.SetupGet(m => m.FileType).Returns(FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock.SetupGet(m => m.TrackingNumber).Returns("NUMBER");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			responseMock.SetupGet(m => m.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(m => m.IsSuccessful).Returns(true);

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsAny<IHttpClientFactory>(),
					It.IsAny<Stream>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Returns(responseMock.Object);

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(p => p.Print(FileType.PDF, binaryData, "PRINTER", "PRINTSERVER"))
				.Returns(true);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(printerMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
				AssertEquals("Print is not successful for all packages.", true, logs.Length > 0);
				AssertEquals("Should have a 'failed to print label break' error.", true,
					logs.Any(log =>
						log.DisplayEventReference ==
						"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, Failed to print label break 'End of Case' after package 'NUMBER'."));
				AssertEquals("Should have a 'failed to print label break' error.", true,
					logs.Any(log =>
						log.DisplayEventReference ==
						"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, Failed to print label break 'End of Area' after package 'NUMBER'."));

				AssertEquals("Package is sent to RTUS.", true, package.IsSentToRTUS);
			}
		}

		public void TestBookAndPrint_SplitterDocumentsDoesNotPrint()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<ISingleBookingRTUSResponse>();
			responseMock.SetupGet(m => m.FileType).Returns(FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock.SetupGet(m => m.TrackingNumber).Returns("NUMBER");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			responseMock.SetupGet(m => m.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(m => m.IsSuccessful).Returns(false);

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsAny<IHttpClientFactory>(),
					It.Is<Stream>(s => IsStreamGeneratedFromCorrectWriter(s)), RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl)))
				.Returns(responseMock.Object);

			var timesPrintIsCalled = 0;
			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(p => p.Print(FileType.PDF, It.IsAny<byte[]>(), "PRINTER", "PRINTSERVER"))
				.Callback(() => timesPrintIsCalled++)
				.Returns(true);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(printerMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is not successful.", true, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Label + splitter docs printed.", 0, timesPrintIsCalled);
				AssertEquals("Package is not sent to RTUS.", false, package.IsSentToRTUS);
			}
		}

		public void TestBookAndPrint_RTUSOptionIsCachedForSameParentJob()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 10);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickByAttachingOrders(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var packageCAS = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "CAS");
			var packagePLT = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "PLT");

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var getRTUSOptionCalled = 0;
			var registryMock = new Mock<ITransportRegistry>();
			registryMock.Setup(r => r.GetRTUSOption(smartFreight.PK.ToGuid()))
				.Callback(() => getRTUSOptionCalled++)
				.Returns(new OrganisationRTUSOption
				{
					OrganisationPK = smartFreight.PK,
					CBACode = "SMA",
					Url = SmartFreightUrl
				});

			var boolTestRegistryItem = new BooleanRegistryItem(
				"Test",
				(NoResString)"Test",
				(NoResString)"Test",
				(NoResString)"Test",
				RegistryStorageFlags.System,
				false
			);

			registryMock.SetupGet(r => r.EnableRTUSBatchProcessing)
				.Returns(boolTestRegistryItem);

			var printedPackages = new List<PkgPackage>();
			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.IsAny<PkgPackage>(), RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA type, Uri url, IStmPrintQueue prn) =>
				{
					printedPackages.Add(p);
					p.IsSentToRTUS = true;
					p.PackageJob.ParentJob.TransportReference = "TRANSPORTREF";
				})
				.Returns(new ReturnResult { Success = true });

			carrierLabelProvider.Setup(c => c.Print(It.IsAny<FileType>(), It.IsAny<byte[]>(),
					It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Returns(true);

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose())
				.Callback(() => carrierLabelManager.Object.Dispose());

			carrierLabelProvider.Setup(c => c.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(registryMock.Object))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packageCAS.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packagePLT.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, packageCAS.HasErrors);
				AssertEquals("Print is successful.", false, packagePLT.HasErrors);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				AssertEquals("Packages are printed in correct order - PLT 1st.", packagePLT.PK, printedPackages[0].PK);
				AssertEquals("Packages are printed in correct order - CAS 2nd.", packageCAS.PK, printedPackages[1].PK);

				AssertEquals("Package is sent to RTUS.", true, packagePLT.IsSentToRTUS);
				AssertEquals("Package is sent to RTUS.", true, packageCAS.IsSentToRTUS);

				carrierLabelProvider.VerifyAll();

				AssertEquals("GetRTUSOption only got called once.", getRTUSOptionCalled, 1);
			}
		}

		public void TestBookAndPrint_OnPackageCarrierLabelPrinted()
		{
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA type, Uri url, IStmPrintQueue prn) =>
				{
					p.IsSentToRTUS = true;
				})
				.Returns(new ReturnResult { Success = true });

			carrierLabelProvider.Setup(c => c.Print(It.IsAny<FileType>(), It.IsAny<byte[]>(),
					It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Returns(true);

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose())
				.Callback(() => carrierLabelManager.Object.Dispose());

			carrierLabelProvider.Setup(c => c.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				var isPackageCarrierLabelPrintedFailFired = false;
				var isPackageCarrierLabelPrintedFired = false;
				var isPackageCarrierLabelPrintStartFired = false;
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);

				var printCarrierLabelsProcessor = new PrintCarrierLabelsProcessor(pick, printer.PK);
				printCarrierLabelsProcessor.PackageCarrierLabelPrinted +=
					(s, e) => isPackageCarrierLabelPrintedFired = true;
				printCarrierLabelsProcessor.PackageCarrierLabelPrintingStarted +=
					(s, e) => isPackageCarrierLabelPrintStartFired = true;
				printCarrierLabelsProcessor.PackageCarrierLabelPrintFail +=
					(s, e) => isPackageCarrierLabelPrintedFailFired = true;
				printCarrierLabelsProcessor.Process(new TestNotificationBuffer());

				AssertEquals("Print is successful.", false, package.HasErrors);
				AssertEquals("Print success event is fired.", true, isPackageCarrierLabelPrintedFired);
				AssertEquals("Print start event is fired.", true, isPackageCarrierLabelPrintStartFired);
				AssertEquals("Print failed event is not fired.", false, isPackageCarrierLabelPrintedFailFired);
				AssertEquals("Package is sent to RTUS.", true, package.IsSentToRTUS);

				carrierLabelProvider.VerifyAll();
			}
		}

		public void TestBookAndPrint_OnPackageCarrierLabelPrintFail()
		{
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = helper.CreatePickByAttachingOrders(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Returns(new ReturnResult { Success = false, Message = "Error!" });

			carrierLabelProvider.Setup(c => c.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose())
				.Callback(() => carrierLabelManager.Object.Dispose());

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				var isPackageCarrierLabelPrintedFailFired = false;
				var isPackageCarrierLabelPrintedFired = false;
				var isPackageCarrierLabelPrintStartFired = false;
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);

				var printCarrierLabelsProcessor = new PrintCarrierLabelsProcessor(pick, printer.PK);
				printCarrierLabelsProcessor.PackageCarrierLabelPrinted +=
					(s, e) => isPackageCarrierLabelPrintedFired = true;
				printCarrierLabelsProcessor.PackageCarrierLabelPrintingStarted +=
					(s, e) => isPackageCarrierLabelPrintStartFired = true;
				printCarrierLabelsProcessor.PackageCarrierLabelPrintFail +=
					(s, e) => isPackageCarrierLabelPrintedFailFired = true;
				printCarrierLabelsProcessor.Process(new TestNotificationBuffer());

				AssertEquals("Print success event is fired.", false, isPackageCarrierLabelPrintedFired);
				AssertEquals("Print start event is fired.", true, isPackageCarrierLabelPrintStartFired);
				AssertEquals("Print failed event is not fired.", true, isPackageCarrierLabelPrintedFailFired);
				AssertEquals("Package is not sent to RTUS.", false, package.IsSentToRTUS);
				carrierLabelProvider.VerifyAll();
			}
		}

		public void TestBookAndPrint_CancellationTokenCancelRequested()
		{
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 10);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType =
				UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 15m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var packageCAS = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "CAS");
			var packagePLT = pick.OuterPackages.Single(pkg => pkg.KP_F3_NKPackType == "PLT");

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var cancellationTokenSource = new CancellationTokenSource();
			var printedPackages = new List<PkgPackage>();
			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.IsAny<PkgPackage>(), RTUSCBA.SmartFreight,
					new Uri(SmartFreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA type, Uri url, IStmPrintQueue prn) =>
				{
					printedPackages.Add(p);
					p.IsSentToRTUS = true;
					p.PackageJob.ParentJob.TransportReference = "TRANSPORTREF";
					cancellationTokenSource.Cancel();
				})
				.Returns(new ReturnResult { Success = true });

			carrierLabelProvider.Setup(c => c.Print(It.IsAny<FileType>(), It.IsAny<byte[]>(),
					It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Returns(true);

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose())
				.Callback(() => carrierLabelManager.Object.Dispose());

			carrierLabelProvider.Setup(c => c.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packageCAS.IsSentToRTUS);
				AssertEquals("Precondition: Package is not sent to RTUS.", false, packagePLT.IsSentToRTUS);
				AssertExceptionThrown<OperationCanceledException>("Operation is cancelled.",
					() => new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer(),
						cancellationTokenSource.Token));

				AssertEquals("Package is sent to RTUS.", true, packagePLT.IsSentToRTUS);
				AssertEquals("Package is not sent to RTUS.", false, packageCAS.IsSentToRTUS);

				carrierLabelProvider.VerifyAll();
			}
		}

		public void TestBookAndPrint_NoCarrierBookingAgent_BatchBookingDisabled()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order1.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order1.CarrierBookingAgent.PK);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			AssertNull(order2.CarrierBookingAgent);

			var pick = Helper.CreatePickByAttachingOrders(order1, order2);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var packages = pick.OuterPackages;
			var packageForOrder1 = packages.Single(package => package.PackageJob.ParentJob.PK == order1.PK);
			var packageForOrder2 = packages.Single(package => package.PackageJob.ParentJob.PK == order2.PK);

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			AssertEquals("Precondition: Package is not sent to RTUS.", false, packageForOrder1.IsSentToRTUS);
			AssertEquals("Precondition: Package is not sent to RTUS.", false, packageForOrder2.IsSentToRTUS);
			new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

			var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

			var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Print is not successful.", true, logs.Length > 0);
			AssertEquals("Should have a 'no carrier booking agent' error.", true,
				logs.Any(log =>
					log.DisplayEventReference ==
					$"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, Order 'O2' has no Carrier Booking Agent."));
		}

		public void TestBookAndPrint_Integration_OrderLineLinkExported()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();
			var line = order.Lines[0];
			var releaseLine = line.ReleaseLines.Single();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' };
			var responseMock = new Mock<ISingleBookingRTUSResponse>();
			responseMock.SetupGet(m => m.FileType).Returns(FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock.SetupGet(m => m.TrackingNumber).Returns("NUMBER");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			responseMock.SetupGet(m => m.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(m => m.IsSuccessful).Returns(true);

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsAny<IHttpClientFactory>(),
					It.IsAny<Stream>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Callback((RequestType requestType, IHttpClientFactory clientFactory, Stream stream, RTUSCBA rtuscba,
					Uri uri) =>
				{
					using (var streamToRead = new MemoryStream())
					{
						stream.CopyTo(streamToRead);
						stream.Position = 0;
						streamToRead.Position = 0;

						using (var reader = new StreamReader(streamToRead))
						{
							var uxml = reader.ReadToEnd();
							Assert(uxml.Contains("<CommercialInvoiceCollection>"));
							Assert(uxml.Contains("<CommercialInvoiceLine>"));
							Assert(uxml.Contains("<OrderLineLink>0</OrderLineLink>"));
						}
					}
				})
				.Returns(responseMock.Object);

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(p => p.Print(FileType.PDF, It.IsAny<byte[]>(), "PRINTER", "PRINTSERVER"))
				.Returns(true);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(printerMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				AssertNotEquals("Precondition: Package ID is different, not 'NUMBER'.", "NUMBER", package.KP_PackageID);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, package.HasErrors);
				AssertEquals("Print is successful.", "NUMBER", package.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				AssertEquals("Package is sent to RTUS.", true, package.IsSentToRTUS);
				AssertEquals("Printer is saved to the package for reprint purposes.", printer.PK,
					package.RTUSLabelPrinterPK);
			}
		}

		#endregion

		#region TestBookAndPrint_PrintPackageLabels

		public void TestBookAndPrint_PrintPackageLabels_EmptyDocPack()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA cba, Uri url, IStmPrintQueue prn) =>
				{
					p.KP_PackageID = "TRACKING"; // simulate setting Tracking Number
					p.IsSentToRTUS = true;
					p.PackageJob.ParentJob.TransportReference = "TRANSPORTREF";
				})
				.Returns(new ReturnResult { Success = true });

			carrierLabelProvider.Setup(c => c.Print(It.IsAny<FileType>(), It.IsAny<byte[]>(),
					It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Returns(true);

			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose())
				.Callback(() => carrierLabelManager.Object.Dispose());

			carrierLabelProvider.Setup(c => c.IsRemotePrintingConnectionDetailsProvided)
				.Returns(true);

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				AssertNotEquals("Precondition: Package ID is different, not 'TRACKING'.", "TRACKING",
					package.KP_PackageID);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, package.HasErrors);
				AssertEquals("Print is successful.", "TRACKING", package.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				var notes = order.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated a Failure Log.", false, notes.Length > 0);

				AssertEquals("Package is sent to RTUS.", true, package.IsSentToRTUS);

				carrierLabelProvider.VerifyAll();
				// Print is called 2 times for the Document Separator.
				carrierLabelProvider.Verify(c => c.Print(It.Is<FileType>(f => f == FileType.PDF), It.IsAny<byte[]>(), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)), Times.Exactly(2));
			}
		}

		public void TestBookAndPrint_PrintPackageLabels_NonEmptyDocPack()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			// Basic Label
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");
			var basicLabelMenuItem = Factory.Load<StmMenuItem>(newDocumentMenuPK);
			// Setup document pack to have linked document
			var docCommand = DocumentEngine.DocumentCommand.GetDocumentCommand(Factory, package, "Documents with Carrier Label");
			docCommand.Parent = package;

			var stmMenuMenuPivot = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = newDocumentMenuPK;
			stmMenuMenuPivot.SF_SU_Inward = docCommand.PK;

			// Delivery Label
			var secondDocumentPK = new Guid("448526C4-F8F3-474B-AD8A-25C57AE160F6");
			var secondDocumentMenuItem = Factory.Load<StmMenuItem>(secondDocumentPK);

			var stmMenuMenuPivot2 = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot2.SF_SU_Outward = secondDocumentPK;
			stmMenuMenuPivot2.SF_SU_Inward = docCommand.PK;

			Factory.Save();

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA cba, Uri url, IStmPrintQueue prn) =>
				{
					p.KP_PackageID = "TRACKING"; // simulate setting Tracking Number
					p.IsSentToRTUS = true;
					p.PackageJob.ParentJob.TransportReference = "TRANSPORTREF";
				})
				.Returns(new ReturnResult { Success = true });
			carrierLabelProvider.Setup(c => c.Print(It.IsAny<FileType>(), It.IsAny<byte[]>(), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((FileType fileType, byte[] docToPrintInBytes, IStmPrintQueue printerPK) =>
				{
					AssertNotEquals(Array.Empty<byte>(), docToPrintInBytes);
				})
				.Returns(true);
			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose()).Callback(() => carrierLabelManager.Object.Dispose());
			carrierLabelProvider.Setup(c => c.IsRemotePrintingConnectionDetailsProvided).Returns(true);

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				AssertNotEquals("Precondition: Package ID is different, not 'TRACKING'.", "TRACKING",
					package.KP_PackageID);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, package.HasErrors);
				AssertEquals("Print is successful.", "TRACKING", package.KP_PackageID);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				var notes = order.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated a Failure Log.", false, notes.Length > 0);

				AssertEquals("Package is sent to RTUS.", true, package.IsSentToRTUS);

				carrierLabelProvider.VerifyAll();
				// Print is called 3 times, 2 from the Document Separator, and 1 from printing the doc pack.
				carrierLabelProvider.Verify(c => c.Print(It.Is<FileType>(f => f == FileType.PDF), It.IsAny<byte[]>(), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)), Times.Exactly(3));
				carrierLabelProvider.VerifyNoOtherCalls();
			}
		}

		public void TestBookAndPrint_PrintPackageLabels_NonEmptyDocPack_FailsToPrintDocPackDocs()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.KP_PackageID = "ABC";

			var packingHelper = new PackingTestHelper(Factory);
			var pickLine = pick.GetAllPickLines().Single();
			packingHelper.CreatePackageDivot(package, pickLine);
			Factory.Save();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			// Basic Label
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");
			var basicLabelMenuItem = Factory.Load<StmMenuItem>(newDocumentMenuPK);
			// Setup document pack to have linked document
			var docCommand = DocumentEngine.DocumentCommand.GetDocumentCommand(Factory, package, "Documents with Carrier Label");
			docCommand.Parent = package;

			var stmMenuMenuPivot = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = newDocumentMenuPK;
			stmMenuMenuPivot.SF_SU_Inward = docCommand.PK;

			// Delivery Label
			var secondDocumentPK = new Guid("448526C4-F8F3-474B-AD8A-25C57AE160F6");
			var secondDocumentMenuItem = Factory.Load<StmMenuItem>(secondDocumentPK);

			var stmMenuMenuPivot2 = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot2.SF_SU_Outward = secondDocumentPK;
			stmMenuMenuPivot2.SF_SU_Inward = docCommand.PK;

			Factory.Save();

			var binaryData = new byte[] { (byte)'A', (byte)'B' }; //
			var responseMock = new Mock<ISingleBookingRTUSResponse>();
			responseMock.SetupGet(m => m.FileType).Returns(FileType.PDF);
			responseMock.SetupGet(m => m.BinaryData).Returns(binaryData);
			responseMock.SetupGet(m => m.TrackingNumber).Returns("NUMBER");
			responseMock.SetupGet(m => m.TransportReference).Returns("TRANSPORTREF");
			responseMock.SetupGet(m => m.ErrorMessageForFailure).Returns("");
			responseMock.SetupGet(m => m.IsSuccessful).Returns(true);

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsAny<IHttpClientFactory>(),
					It.IsAny<Stream>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
				.Returns(responseMock.Object);

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(p => p.Print(FileType.PDF, binaryData, "PRINTER", "PRINTSERVER"))
				.Returns(true);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD", false))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
						 Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(printerMock.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				var logs = pick.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
				AssertEquals("Print is not successful for all documents to print.", true, logs.Length > 0);
				AssertEquals("Should have a 'failed to print doc pack documents' error.", true,
					logs.Any(log =>
						log.DisplayEventReference ==
						"Error Report: Bulk Carrier Label Booking, Failed to print carrier label, Failed to print documents from the 'Documents with Carrier Label' document pack for package 'NUMBER'."));

				AssertEquals("Package is sent to RTUS.", true, package.IsSentToRTUS);
			}
		}

		public void TestBookAndPrint_PrintPackageLabels_NonEmptyDocPack_WillNotCallBatchBookingRegardlessOfRegistry()
		{
			var smartFreight = Helper.CreateClient();
			var registry = ObjectFactory.Get<ITransportRegistry>();
			var rtusCollection = (RegistryBusinessObjectCollectionTemplate)registry.OrganisationRTUSOptions.Value;
			var rtusOption = rtusCollection.AddNew();
			rtusOption["CBACode"] = "SMA"; // SmartFreight
			rtusOption["OrganisationPK"] = smartFreight.PK;
			rtusOption["Url"] = SmartFreightUrl;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateProductUnit(data.Part1, "CAS", 5);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType =
				UOMPackTypesList.Codes.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.CarrierBookingAgentDocAddress.E2_OA_Address = smartFreight.MainAddress.PK;
			AssertEquals(smartFreight.PK, order.CarrierBookingAgent.PK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("Pick is cartonized.", true, pick.WP_IsCartonised);
			var package = pick.OuterPackages.Single();

			var trigger = pick.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SQ = printer.PK;
			Factory.Save();

			// Basic Label
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");
			var basicLabelMenuItem = Factory.Load<StmMenuItem>(newDocumentMenuPK);
			// Setup document pack to have linked document
			var docCommand = DocumentEngine.DocumentCommand.GetDocumentCommand(Factory, package, "Documents with Carrier Label");
			docCommand.Parent = package;

			var stmMenuMenuPivot = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = newDocumentMenuPK;
			stmMenuMenuPivot.SF_SU_Inward = docCommand.PK;

			// Delivery Label
			var secondDocumentPK = new Guid("448526C4-F8F3-474B-AD8A-25C57AE160F6");
			var secondDocumentMenuItem = Factory.Load<StmMenuItem>(secondDocumentPK);

			var stmMenuMenuPivot2 = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot2.SF_SU_Outward = secondDocumentPK;
			stmMenuMenuPivot2.SF_SU_Inward = docCommand.PK;

			Factory.Save();

			var whsCarrierLabelBatchPrintingProviderMock = new Mock<ICarrierLabelsBatchPrintingProvider>();
			whsCarrierLabelBatchPrintingProviderMock.SetupGet(mock => mock.IsRemotePrintingConnectionDetailsProvided).Returns(true);
			whsCarrierLabelBatchPrintingProviderMock.Setup(mock => mock.PrintCarrierLabels(It.IsAny<ICarrierLabelsBatchPrintingInfo>(),
				RTUSCBA.SmartFreight,
				new Uri(SmartFreightUrl),
				It.Is<ZGuid>(pr => pr == printer.PK),
				It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					package.IsSentToRTUS = true;
				})
				.Returns(new ReturnResult { Success = true });

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.IsAny<PkgPackage>(),
					RTUSCBA.SmartFreight, new Uri(SmartFreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA cba, Uri url, IStmPrintQueue prn) =>
				{
					p.IsSentToRTUS = true;
					p.PackageJob.ParentJob.TransportReference = "TRANSPORTREF";
				})
				.Returns(new ReturnResult { Success = true });

			carrierLabelProvider.Setup(c => c.Print(It.IsAny<FileType>(), It.IsAny<byte[]>(), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK))).Returns(true);
			var carrierLabelManager = new Mock<ICarrierLabelManager>();
			carrierLabelProvider.Setup(c => c.Dispose()).Callback(() => carrierLabelManager.Object.Dispose());
			carrierLabelProvider.Setup(c => c.IsRemotePrintingConnectionDetailsProvided).Returns(true);

			using (registry.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (((RegistryItemWrapper)registry.OrganisationRTUSOptions).SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(carrierLabelManager.Object))
			using (ObjectFactory.Substitute(carrierLabelProvider.Object))
			{
				AssertEquals("Precondition: Package is not sent to RTUS.", false, package.IsSentToRTUS);
				new PrintCarrierLabelsProcessor(pick, printer.PK).Process(new TestNotificationBuffer());

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, pick.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ErrorReportCode);

				AssertEquals("Print is successful.", false, pick.Logs.Find(logQuery).Length > 0);
				AssertEquals("Print is successful.", false, package.HasErrors);
				AssertEquals("Print is successful.", "TRANSPORTREF", order.WD_TransportReference);

				var notes = order.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.RTUSRequestLog.Description);
				AssertEquals("Should not have generated a Failure Log.", false, notes.Length > 0);

				AssertEquals("Package is sent to RTUS.", true, package.IsSentToRTUS);

				carrierLabelProvider.VerifyAll();
				// Print is called 3 times, 2 from the Document Separator, and 1 from printing the doc pack.
				carrierLabelProvider.Verify(c => c.Print(It.Is<FileType>(f => f == FileType.PDF), It.IsAny<byte[]>(), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)), Times.Exactly(3));
				carrierLabelProvider.VerifyNoOtherCalls();

				whsCarrierLabelBatchPrintingProviderMock.Verify(
					c => c.PrintCarrierLabels(It.IsAny<ICarrierLabelsBatchPrintingInfo>(), It.IsAny<RTUSCBA>(), It.IsAny<Uri>(), It.IsAny<ZGuid>(), It.IsAny<CancellationToken>()
				), Times.Never);
				whsCarrierLabelBatchPrintingProviderMock.Verify(c => c.IsRemotePrintingConnectionDetailsProvided, Times.Never);
			}
		}

		#endregion

		const string SmartFreightUrl =
			"https://wtg1.p1.app.smartfreight.com/smartfreight/api/ucp/sems/execute/N1ZVOmJ3VjNWcHJQNnVlMTlVUWhqXy1hQ05mdmZIT1ZYVXc3ZUE";

		IDisposable SetupRegistryForTest(string remotePrintServer, string remotePrintUserName,
			string remotePrintPassword, bool isBatchProcessingEnabled)
		{
			return new DisposableList(new[]
			{
				TransportRegistry.Instance.EnableRTUSBatchProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty,
					Guid.Empty, isBatchProcessingEnabled),
				TransportRegistry.Instance.RemotePrintServerURL.SetTemporaryValue(Guid.Empty, Guid.Empty,
					Guid.Empty, remotePrintServer),
				WebDataRegistry.Instance.WebServiceUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					remotePrintUserName),
				WebDataRegistry.Instance.WebServicePassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					remotePrintPassword)
			});
		}
	}
}
