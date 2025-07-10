using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.Foundation.Http;
using WTG.RTUS.Interface;
using WTG.RTUS.Printing;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PrintCarrierLabelTest : WhsSecureServiceTestCase
	{
		#region TestInvalidPackage

		public void TestInvalidPackage()
		{
			var printer = Helper.CreatePrintQueue("PRINTER");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response1 = webService.PrintCarrierLabel(null, printer.PK.ToGuid());
			AssertSuccessfulResponse(response1, webService);
			AssertEquals("Providing a null Package should result in an Error.", ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("Providing a null Package should result in an Error.", "No Package/Order information provided.", response1.ErrorMessage);

			var packageInfo = new PackageInfo { PackageID = "123", PK = ZGuid.BrettsGuid.ToGuid() };
			var response2 = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
			AssertSuccessfulResponse(response2, webService);
			AssertEquals("Providing an Invalid Package should result in an Error.", ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Providing an Invalid Package should result in an Error.", "Package ID '123' does not exist.", response2.ErrorMessage);
		}

		#endregion

		#region TestInvalidPrinter

		public void TestInvalidPrinter()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
			var response = webService.PrintCarrierLabel(packageInfo, ZGuid.BrettsGuid.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Providing a null Printer should result in an Error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Providing a null Printer should result in an Error.", "Could not find specified Printer.", response.ErrorMessage);
		}

		#endregion

		#region TestInvalidParentJob

		public void TestInvalidParentJob()
		{
			var printer = Helper.CreatePrintQueue("PRINTER");
			var packageJob = Helper.Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = ZGuid.BrettsGuid;
			packageJob.KJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var package = packageJob.Packages.AddNew("PLT", "123");

			Helper.Factory.Save();

			var webService = GetNewWebService();
			var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
			var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Providing a Package with no Parent should result in an Error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Providing a Package with no Parent should result in an Error.", "Package '123' must be a Packing Consolidation Handling Unit or attached to an Order.", response.ErrorMessage);
		}

		#endregion

		#region TestNonWarehouseOrderParentJob

		public void TestNonWarehouseOrderParentJob()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);

			try
			{
				var data = new TestDataForPacking(Helper.Factory);
				data.CreatePackingData();

				var printer = Helper.CreatePrintQueue("PRINTER");
				var package = data.PackageJob.Packages.AddNew("PLT", "123");

				Helper.Factory.Save();

				var webService = GetNewWebService();
				var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
				var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
				AssertSuccessfulResponse(response, webService);
				AssertEquals("Providing a Package with no Warehouse Order Parent should result in an Error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Providing a Package with no Warehouse Order Parent should result in an Error.", "Package '123' must be a Packing Consolidation Handling Unit or attached to an Order.", response.ErrorMessage);
			}
			finally
			{
				DummyBusinessObject.TypeDecider.TypeForLoadOverride = null;
			}
		}

		#endregion

		#region TestCarrierLabelIsPrintedSuccessfully

		public void TestCarrierLabelIsPrintedSuccessfully()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");

			Helper.Factory.Save();

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA type, Uri url, IStmPrintQueue prn) =>
				{
					p.KP_PackageID = "TRACKING";
					p.PackageJob.ParentJob.TransportReference = "TRANSPORTRef";
				}) // simulate setting Tracking Number and Transport Ref
				.Returns(new ReturnResult { Success = true });

			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (SubstituteCarrierLabelProvider(carrierLabelProvider))
			{
				var webService = GetNewWebService();
				var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
				var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
				AssertSuccessfulResponse(response, webService);
				AssertEquals("Successful print should have no Error.", ErrorTypes.None, response.Error);
				AssertEquals("Successful print should have no Error.", null, response.ErrorMessage);
				AssertNotNull("Successful print should return the new Package Details (Package ID is updated).", response.NewPackage);
				AssertEquals("Successful print should return the new Package Details (Package ID is updated).", package.PK, response.NewPackage.PK);
				AssertEquals("Successful print should return the new Package Details (Package ID is updated).", "TRACKING", response.NewPackage.PackageID);
				AssertEquals("Successful print should update order Transport Referece.", "TRANSPORTRef", order.WD_TransportReference);
				AssertEquals("", response.NewPackage.PackType);
				AssertEquals(0m, response.NewPackage.QtyPacked);

				var packageInNewFactory = new BusinessObjectFactory().Load<PkgPackage>(package.PK);
				AssertEquals("Package ID should have been updated.", "TRACKING", packageInNewFactory.KP_PackageID);

				carrierLabelProvider.VerifyAll();
			}
		}

		#endregion

		#region TestCarrierLabelIsPrintedSuccessfully_ConsolidationHandlingUnit

		public void TestCarrierLabelIsPrintedSuccessfully_ConsolidationHandlingUnit()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order1.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order2.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order1, order2);

			var package1 = Helper.CreatePackage("PLT", "123", order1.PackageJob.Packages);
			var package2 = Helper.CreatePackage("PLT", "456", order2.PackageJob.Packages);

			var printer = Helper.CreatePrintQueue("PRINTER");

			var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);

			Helper.Factory.Save();

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == handlingUnitPackage.PK), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Callback((PkgPackage p, RTUSCBA type, Uri url, IStmPrintQueue prn) =>
				{
					p.KP_PackageID = "TRACKING";
					p.PackageJob.ParentJob.TransportReference = "TRANSPORTRef";
				}) // simulate setting Tracking Number and Transport Ref
				.Returns(new ReturnResult { Success = true });

			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (SubstituteCarrierLabelProvider(carrierLabelProvider))
			{
				var webService = GetNewWebService();
				var packageInfo = new PackageInfo { PackageID = "HU", PK = handlingUnitPackage.PK.ToGuid() };
				var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
				AssertSuccessfulResponseWithNoErrors(response, webService);
				AssertNotNull("Successful print should return the new Package Details (Package ID is updated).", response.NewPackage);
				AssertEquals("Successful print should return the new Package Details (Package ID is updated).", handlingUnitPackage.PK, response.NewPackage.PK);
				AssertEquals("Successful print should return the new Package Details (Package ID is updated).", "TRACKING", response.NewPackage.PackageID);
				AssertEquals("Successful print should update order Transport Referece.", "TRANSPORTRef", order1.WD_TransportReference);
				AssertEquals("Successful print should update order Transport Referece.", "TRANSPORTRef", order2.WD_TransportReference);
				AssertEquals("Successful print should update HU Transport Referece.", "TRANSPORTRef", ((IPackingParent)handlingUnit).TransportReference);

				var packageInNewFactory = new BusinessObjectFactory().Load<PkgPackage>(handlingUnitPackage.PK);
				AssertEquals("Handling Unit ID should have been updated.", "TRACKING", packageInNewFactory.KP_PackageID);

				carrierLabelProvider.VerifyAll();
			}
		}

		#endregion

		#region TestCarrierLabelIsPrintedSuccessfully_ConsolidationHandlingUnit_Integration

		public void TestCarrierLabelIsPrintedSuccessfully_ConsolidationHandlingUnit_Integration()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORDER123", data.Part1, 10m);
			order1.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORDER456", data.Part1, 10m);
			order2.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order1, order2);

			var printer = Helper.CreatePrintQueue("PRINTER");
			printer.SQ_ServerName = "PRINTSERVER";

			var package1 = Helper.CreatePackage("PLT", "PKG-123", order1.PackageJob.Packages);
			var package2 = Helper.CreatePackage("PLT", "PKG-456", order2.PackageJob.Packages);

			var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);

			handlingUnitPackage.KP_IsClosed = true;
			handlingUnitPackage.KP_ClosedTimeUtc = DateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "E";

			Helper.Factory.Save();

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsNotNull<IHttpClientFactory>(), It.Is<Stream>(s => IsStreamGeneratedWithCorrectHandlingUnitDetails(s)), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl)))
				.Returns(GetResponse(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "NUMBER", "TRANSPORTREF"));

			var rtusPrinterMock = new Mock<IRTUSPrinter>();
			rtusPrinterMock.Setup(pr => pr.Print(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "PRINTER", "PRINTSERVER")).Returns(true);
			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD"))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(rtusPrinterMock.Object))
			using (var webService = GetNewWebService())
			{
				var packageInfo = new PackageInfo { PackageID = "HU", PK = handlingUnitPackage.PK.ToGuid() };
				var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
				AssertSuccessfulResponseWithNoErrors(response, webService);
				AssertNotNull("Successful print should return the new Package Details (Package ID is updated).", response.NewPackage);
				AssertEquals("Successful print should return the new Package Details (Package ID is updated).", handlingUnitPackage.PK, response.NewPackage.PK);
				AssertEquals("Successful print should return the new Package Details (Package ID is updated).", "NUMBER", response.NewPackage.PackageID);

				AssertEquals("Successful print should update order Transport Referece.", "TRANSPORTREF", order1.WD_TransportReference);
				AssertEquals("Successful print should update order Transport Referece.", "TRANSPORTREF", order2.WD_TransportReference);
				AssertEquals("Successful print should update HU Transport Referece.", "TRANSPORTREF", ((IPackingParent)handlingUnit).TransportReference);

				var newFactory = new BusinessObjectFactory();
				var huInNewFactory = newFactory.Load<PkgPackage>(handlingUnitPackage.PK);
				var pkg1InNewFactory = newFactory.Load<PkgPackage>(package1.PK);
				var pkg2InNewFactory = newFactory.Load<PkgPackage>(package2.PK);

				AssertEquals("Handling Unit ID should have been updated.", "NUMBER", huInNewFactory.KP_PackageID);
				AssertEquals("Outers should not have been updated.", "PKG-123", pkg1InNewFactory.KP_PackageID);
				AssertEquals("Outers should not have been updated.", "PKG-456", pkg2InNewFactory.KP_PackageID);
			}
		}

		bool IsStreamGeneratedWithCorrectHandlingUnitDetails(Stream originalStream)
		{
			using (var streamToRead = new MemoryStream())
			{
				originalStream.CopyTo(streamToRead);
				originalStream.Position = 0;
				streamToRead.Position = 0;

				using (var reader = new StreamReader(streamToRead))
				{
					var uxml = reader.ReadToEnd();

					var whitespaceTrim = new Regex(@"\s+");
					uxml = whitespaceTrim.Replace(uxml, "");

					return uxml.Contains("<OrderNumber>ORDER123</OrderNumber>")
						&& uxml.Contains("<OrderNumber>ORDER456</OrderNumber>")
						&& uxml.Contains(GetDataContextString("PkgPackage", "HU"))
						&& uxml.Contains(GetDataContextString("WarehouseOrder", "W00000002"))
						&& uxml.Contains(GetDataContextString("WarehouseOrder", "W00000003"));
				}
			}
		}

		string GetDataContextString(string type, string key)
		{
			return $"<DataSource><Type>{type}</Type><Key>{key}</Key></DataSource>";
		}

		#endregion

		#region TestCarrierLabelIsReprintedSuccessfully

		public void TestCarrierLabelIsReprintedSuccessfully()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "TRACKING", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");

			Helper.Factory.Save();

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Returns(new ReturnResult { Success = true });

			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (SubstituteCarrierLabelProvider(carrierLabelProvider))
			{
				var webService = GetNewWebService();
				AssertEquals("No saves should have occured yet.", 0, webService.Factory.SaveCount);

				var packageInfo = new PackageInfo { PackageID = "TRACKING", PK = package.PK.ToGuid() };
				var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
				AssertSuccessfulResponse(response, webService);
				AssertEquals("Successful print should have no Error.", ErrorTypes.None, response.Error);
				AssertEquals("Successful print should have no Error.", null, response.ErrorMessage);
				AssertNotNull("Successful print should return the new Package Details (Package ID is updated).", response.NewPackage);
				AssertEquals("Successful print should return the new Package Details (Package ID is updated).", package.PK, response.NewPackage.PK);
				AssertEquals("Successful print should return the new Package Details (Package ID is updated).", "TRACKING", response.NewPackage.PackageID);
				AssertEquals("Reprint should cause no changes and thus there should be no factory save.", 0, webService.Factory.SaveCount);

				var packageInNewFactory = new BusinessObjectFactory().Load<PkgPackage>(package.PK);
				AssertEquals("Package ID should stay the same.", "TRACKING", packageInNewFactory.KP_PackageID);
				AssertEquals("Previous Package ID should not be updated.", "", packageInNewFactory.KP_PreviousPackageID);

				carrierLabelProvider.VerifyAll();
			}
		}

		#endregion

		#region TestCarrierLabelIsPrintedSuccessfully_Integration

		public void TestCarrierLabelIsPrintedSuccessfully_Integration()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORDER123", data.Part1, 10m);
			order.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");
			printer.SQ_ServerName = "PRINTSERVER";

			Helper.Factory.Save();

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsNotNull<IHttpClientFactory>(), It.Is<Stream>(s => IsStreamGeneratedFromCorrectWriter(s)), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl)))
				.Returns(GetResponse(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "NUMBER", "TRANSPORTREF"));

			var rtusPrinterMock = new Mock<IRTUSPrinter>();
			rtusPrinterMock.Setup(pr => pr.Print(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "PRINTER", "PRINTSERVER")).Returns(true);
			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD"))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(rtusPrinterMock.Object))
			using (var webService = GetNewWebService())
			{
				var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
				var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
				AssertSuccessfulResponse(response, webService);
				AssertEquals("Successful print should have no Error.", ErrorTypes.None, response.Error);
				AssertEquals("Successful print should have no Error.", null, response.ErrorMessage);

				rtusProcessorMock.VerifyAll();
				rtusPrinterMock.VerifyAll();
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
					return uxml.Contains("<OrderNumber>ORDER123</OrderNumber>");
				}
			}
		}

		#endregion

		#region TestCarrierLabelFailsToPrint

		public void TestCarrierLabelFailsToPrint()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");

			Helper.Factory.Save();

			var carrierLabelProviderMock = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProviderMock
				.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == package.PK), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Returns(new ReturnResult { Success = false, Message = "Some Error" });

			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (SubstituteCarrierLabelProvider(carrierLabelProviderMock))
			{
				var webService = GetNewWebService();
				var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
				var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
				AssertSuccessfulResponse(response, webService);
				AssertEquals("Failed print should have Error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Failed print should have Error.", "Some Error", response.ErrorMessage);

				carrierLabelProviderMock.VerifyAll();
			}
		}

		#endregion

		#region TestCarrierLabelFailsToPrint_PrintErrorIsException

		public void TestCarrierLabelFailsToPrint_PrintErrorIsException()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORDER123", data.Part1, 10m);
			order.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");
			printer.SQ_ServerName = "PRINTSERVER";
			printer.SQ_DisplayName = "GRAVITY PRINTER";

			Helper.Factory.Save();

			Action<Exception> onError = null;
			var carrierLabelProviderMock = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProviderMock.SetupGet(c => c.IsRemotePrintingConnectionDetailsProvided).Returns(true);
			carrierLabelProviderMock.Setup(c => c.PrintCarrierLabel(It.IsNotNull<PkgPackage>(), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl), It.IsNotNull<IStmPrintQueue>()))
				.Returns(new ReturnResult { Message = "", Success = false }).Callback(() => onError(new Exception("Error 407 - Proxy Authentication Required")));
			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD"))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (SubstituteCarrierLabelProvider(MockFactory))
			using (var webService = GetNewWebService())
			{
				var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
				var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
				AssertSuccessfulResponse(response, webService);
				AssertEquals("Failed print should have Error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Failed print, should have Exception Error Message.",
					"Failed to send Print Request to 'GRAVITY PRINTER'.\r\nDetails below:\r\nError 407 - Proxy Authentication Required", response.ErrorMessage);
			}

			// store exception handler passed in so we can invoke it later to simulate an exception occuring
			ICarrierLabelPrintingProvider MockFactory(object[] args)
			{
				onError = (Action<Exception>)args[0];
				return carrierLabelProviderMock.Object;
			}
		}

		#endregion

		#region TestCarrierLabelFailsToPrint_RemotePrintingConfigurationNotFullySetup

		public void TestCarrierLabelFailsToPrint_RemotePrintingConfigurationNotFullySetup()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORDER123", data.Part1, 10m);
			order.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");
			printer.SQ_ServerName = "PRINTSERVER";

			Helper.Factory.Save();

			var rtusProcessorMock = new Mock<IRTUSProcessor>();
			rtusProcessorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsNotNull<IHttpClientFactory>(), It.Is<Stream>(s => IsStreamGeneratedFromCorrectWriter(s)), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl)))
				.Returns(GetResponse(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "NUMBER", "TRANSPORTREF"));

			var rtusPrinterMock = new Mock<IRTUSPrinter>();
			rtusPrinterMock.Setup(pr => pr.Print(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "PRINTER", "PRINTSERVER")).Returns(true);
			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute(rtusProcessorMock.Object))
			using (ObjectFactory.Substitute(rtusPrinterMock.Object))
			{
				var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };

				using (SetupRegistryForTest("http://PrintServer", "", ""))
				using (var webService = GetNewWebService())
				{
					var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
					AssertSuccessfulResponse(response, webService);
					AssertEquals("Missing Remote Printing Config should show Error.", ErrorTypes.BusinessValidationError, response.Error);
					AssertEquals("Missing Remote Printing Config should show Error.",
						"Unable to Print Carrier Label due to the following: No Remote Printing Username provided in Registry. No Remote Printing Password provided in Registry.", response.ErrorMessage);
				}

				rtusPrinterMock.Verify(pr => pr.Dispose(), Times.Never);

				using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD"))
				using (var webService = GetNewWebService())
				{
					var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
					AssertSuccessfulResponse(response, webService);
					AssertEquals("Printing should succeed.", ErrorTypes.None, response.Error);
					AssertEquals("Printing should succeed.", null, response.ErrorMessage);

					rtusProcessorMock.VerifyAll();
					rtusPrinterMock.VerifyAll();
				}

				rtusPrinterMock.Verify(pr => pr.Dispose(), Times.Once);
			}
		}

		#endregion

		#region TestCarrierLabelFailsToPrint_ExceptionIsThrownDuringProcess

		public void TestCarrierLabelFailsToPrint_ExceptionIsThrownDuringProcess()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");

			Helper.Factory.Save();

			var mockedPackageExporterDictionary = new KeyTypeDictionaryObject { { nameof(ParentJobType.WarehouseOrder), new HandleThatThrowsException() } };
			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			using (SetupRegistryForTest("http://PrintServer", "USERNAME", "PASSWORD"))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (ObjectFactory.Substitute("UniversalPackingExporters", mockedPackageExporterDictionary))
			using (var webService = GetNewWebService())
			{
				var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
				var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
				AssertSuccessfulResponse(response, webService);
				AssertEquals("Exception was thrown during Label Printing.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Exception was thrown during Label Printing, should show correct Error Message.",
					"Failed to generate Universal Shipment from Package '123'.\r\nDetails below:\r\nWHAT HAPPEN!", response.ErrorMessage);
			}
		}

		class HandleThatThrowsException : ObjectHandle
		{
			public override object GetObject(params object[] arguments)
			{
				throw new InvalidOperationException("WHAT HAPPEN!");
			}
		}

		#endregion

		#region TestCarrierLabelFailsToPrint_OrderMissingCBA

		public void TestCarrierLabelFailsToPrint_OrderMissingCBA()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORDER123", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");

			Helper.Factory.Save();

			var webService = GetNewWebService();
			var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
			var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Order missing CBA, response should have Error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Order missing CBA, response should have Error.", "Warehouse Order 'ORDER123' has no Carrier Booking Agent.", response.ErrorMessage);
		}

		#endregion

		#region TestCarrierLabelFailsToPrint_ConsolidationHandlingUnitMissingCBA

		public void TestCarrierLabelFailsToPrint_ConsolidationHandlingUnitMissingCBA()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");

			var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			packingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			var packageInfo = new PackageInfo { PackageID = "HU", PK = handlingUnitPackage.PK.ToGuid() };
			var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
			AssertBusinessValidationError(webService, "Handling Unit 'HU' has no Carrier Booking Agent.", response);
		}

		#endregion

		#region TestCarrierLabelFailsToPrint_ConsolidationHandlingUnitMissingCBA_DifferentCBAs

		public void TestCarrierLabelFailsToPrint_ConsolidationHandlingUnitMissingCBA_DifferentCBAs()
		{
			var smartFreight1 = Helper.CreateClient("SMART1");
			var smartFreight2 = Helper.CreateClient("SMART2");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order1.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight1.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order2.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight2.PK;
			Helper.CreatePickNew(order1, order2);

			var package1 = Helper.CreatePackage("PLT", "123", order1.PackageJob.Packages);
			var package2 = Helper.CreatePackage("PLT", "456", order2.PackageJob.Packages);

			var printer = Helper.CreatePrintQueue("PRINTER");

			var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			var packageInfo = new PackageInfo { PackageID = "HU", PK = handlingUnitPackage.PK.ToGuid() };
			var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
			AssertBusinessValidationError(webService, "Handling Unit 'HU' has no Carrier Booking Agent.", response);
		}

		#endregion

		#region TestCarrierLabelFailsToPrint_RegistryMissingRTUS

		public void TestCarrierLabelFailsToPrint_RegistryMissingRTUS()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");

			Helper.Factory.Save();

			var webService = GetNewWebService();
			var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
			var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Missing RTUS Org in Registry, response should have Error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Missing RTUS Org in Registry, response should have Error.", "No RTUS URL provided for Organization 'SMART'.", response.ErrorMessage);
		}

		#endregion

		#region TestCarrierLabelProviderIsRecalculatedWhenMissingConfigurationIsSet

		public void TestCarrierLabelProviderIsRecalculatedWhenMissingConfigurationIsSet()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");

			Helper.Factory.Save();

			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (var webService = GetNewWebService())
			{
				var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
				var carrierLabelProviderMockWithFailure = new Mock<ICarrierLabelPrintingProvider>();
				carrierLabelProviderMockWithFailure.SetupGet(c => c.IsRemotePrintingConnectionDetailsProvided).Returns(false);
				carrierLabelProviderMockWithFailure.Setup(c => c.PrintCarrierLabel(It.IsNotNull<PkgPackage>(), It.IsAny<RTUSCBA>(), It.IsNotNull<Uri>(), It.IsNotNull<IStmPrintQueue>()))
					.Returns(new ReturnResult { Success = false, Message = "FAIL" });

				using (SubstituteCarrierLabelProvider(carrierLabelProviderMockWithFailure.Object))
				{
					var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
					AssertSuccessfulResponse(response, webService);
					AssertEquals("Missing Remote Printing Config, should fail.", ErrorTypes.BusinessValidationError, response.Error);
				}

				var carrierLabelProviderMockWithSuccess = new Mock<ICarrierLabelPrintingProvider>();
				carrierLabelProviderMockWithSuccess.Setup(c => c.PrintCarrierLabel(It.IsNotNull<PkgPackage>(), It.IsAny<RTUSCBA>(), It.IsNotNull<Uri>(), It.IsNotNull<IStmPrintQueue>()))
					.Returns(new ReturnResult { Success = true });

				using (SubstituteCarrierLabelProvider(carrierLabelProviderMockWithSuccess.Object))
				{
					var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
					AssertSuccessfulResponse(response, webService);
					AssertEquals("Remote Printing Config provided, should succeed.", ErrorTypes.None, response.Error);
				}
			}
		}

		#endregion

		#region TestCarrierLabelManagerIsDisposedWhenMissingConfiguration

		public void TestCarrierLabelManagerIsDisposedWhenMissingConfiguration()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");

			Helper.Factory.Save();

			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (var webService = GetNewWebService())
			{
				var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
				var managerMock = new Mock<ICarrierLabelManager>();

				using (ObjectFactory.Substitute(managerMock.Object))
				{
					managerMock.Verify(m => m.Dispose(), Times.Never);

					var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
					AssertSuccessfulResponse(response, webService);
					AssertEquals("Missing Remote Printing Config, should fail.", ErrorTypes.BusinessValidationError, response.Error);

					managerMock.Verify(m => m.Dispose(), Times.Once);
				}
			}
		}

		#endregion

		#region DBHits

		public void TestCarrierLabelIsPrintedSuccessfully_ConsolidationHandlingUnit_DBHits()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order1.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order2.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			order3.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;

			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 10m);
			order4.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;

			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 10m);
			order5.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;

			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part1, 10m);
			order6.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;

			var order7 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O7", data.Part1, 10m);
			order7.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;

			var order8 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O8", data.Part1, 10m);
			order8.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;

			var order9 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O9", data.Part1, 10m);
			order9.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;

			var order0 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O0", data.Part1, 10m);
			order0.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order1, order2, order3, order4, order5);

			var package1 = Helper.CreatePackage("PLT", "1", order1.PackageJob.Packages);
			var package2 = Helper.CreatePackage("PLT", "2", order2.PackageJob.Packages);
			var package3 = Helper.CreatePackage("PLT", "3", order3.PackageJob.Packages);
			var package4 = Helper.CreatePackage("PLT", "4", order4.PackageJob.Packages);
			var package5 = Helper.CreatePackage("PLT", "5", order5.PackageJob.Packages);
			var package6 = Helper.CreatePackage("PLT", "6", order5.PackageJob.Packages);
			var package7 = Helper.CreatePackage("PLT", "7", order5.PackageJob.Packages);
			var package8 = Helper.CreatePackage("PLT", "8", order5.PackageJob.Packages);
			var package9 = Helper.CreatePackage("PLT", "9", order5.PackageJob.Packages);
			var package0 = Helper.CreatePackage("PLT", "0", order5.PackageJob.Packages);

			var printer = Helper.CreatePrintQueue("PRINTER");

			var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package3, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package4, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package5, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package6, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package7, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package8, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package9, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package0, handlingUnitPackage);

			Helper.Factory.Save();

			var carrierLabelProvider = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProvider.Setup(c => c.PrintCarrierLabel(It.Is<PkgPackage>(p => p.PK == handlingUnitPackage.PK), RTUSCBA.SmartFreight, new Uri(SmartfreightUrl), It.Is<IStmPrintQueue>(pr => pr.PK == printer.PK)))
				.Returns(new ReturnResult { Success = true });

			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			var webService = GetNewWebService(data.Whs1);
			var expectedDBHits = new Dictionary<string, int>()
				{
					{ JobDocAddressSchema.Constants.TableName, 5 },
					{ OrgAddressSchema.Constants.TableName, 2 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ PkgHandlingUnitSchema.Constants.TableName, 1 },
					{ PkgPackageSchema.Constants.TableName, 4 },
					{ PkgPackageJobSchema.Constants.TableName, 2 },
					{ WhsDocketSchema.Constants.TableName, 5 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ PkgPackageHeaderSchema.Constants.TableName, 1 },
					{ StmPrintQueueSchema.Constants.TableName, 1 },
				};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (SubstituteCarrierLabelProvider(carrierLabelProvider))
			{
				var packageInfo = new PackageInfo { PackageID = "HU", PK = handlingUnitPackage.PK.ToGuid() };
				var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}
		}

		#endregion

		#region TestDisposingWebServiceDisposesCarrierLabelProvider

		public void TestDisposingWebServiceDisposesCarrierLabelProvider()
		{
			var smartFreight = Helper.CreateClient("SMART");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.CarrierBookingAgentDocAddress.OrganisationPK = smartFreight.PK;
			Helper.CreatePickNew(order);

			var package = Helper.CreatePackage("PLT", "123", order.PackageJob.Packages);
			var printer = Helper.CreatePrintQueue("PRINTER");

			Helper.Factory.Save();

			var carrierLabelProviderMock = new Mock<ICarrierLabelPrintingProvider>();
			carrierLabelProviderMock.Setup(c => c.PrintCarrierLabel(It.IsNotNull<PkgPackage>(), It.IsAny<RTUSCBA>(), It.IsNotNull<Uri>(), It.IsNotNull<IStmPrintQueue>()))
				.Returns(new ReturnResult { Success = true });

			var rtusCollection = GetRTUSCollectionWithOrg(smartFreight, CBAList.Codes.SmartFreight);

			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (SubstituteCarrierLabelProvider(carrierLabelProviderMock))
			using (var webService = GetNewWebService())
			{
				var packageInfo = new PackageInfo { PackageID = "123", PK = package.PK.ToGuid() };
				var response = webService.PrintCarrierLabel(packageInfo, printer.PK.ToGuid());
				AssertSuccessfulResponse(response, webService);

				carrierLabelProviderMock.Verify(p => p.Dispose(), Times.Never);
			}

			carrierLabelProviderMock.Verify(p => p.Dispose(), Times.Once);
		}

		#endregion

		#region Implementation

		const string SmartfreightUrl = "https://wtg1.p1.app.smartfreight.com/smartfreight/api/ucp/sems/execute/N1ZVOmJ3VjNWcHJQNnVlMTlVUWhqXy1hQ05mdmZIT1ZYVXc3ZUE";

		static IDisposable SubstituteCarrierLabelProvider(Mock<ICarrierLabelPrintingProvider> carrierLabelProviderMock)
		{
			carrierLabelProviderMock.SetupGet(c => c.IsRemotePrintingConnectionDetailsProvided).Returns(true);
			return SubstituteCarrierLabelProvider(carrierLabelProviderMock.Object);
		}

		static IDisposable SubstituteCarrierLabelProvider(ICarrierLabelPrintingProvider carrierLabelProvider)
		{
			return SubstituteCarrierLabelProvider(args => carrierLabelProvider);
		}

		static IDisposable SubstituteCarrierLabelProvider(Func<object[], ICarrierLabelPrintingProvider> mockFactory)
		{
			var disposableList = new DisposableList(2);
			disposableList.Add(ObjectFactory.Substitute(GetCarrierLabelProvider));
			return disposableList;

			ICarrierLabelPrintingProvider GetCarrierLabelProvider(object[] args)
			{
				disposableList.Add((ICarrierLabelManager)args[1]);
				return mockFactory(args);
			}
		}

		ISingleBookingRTUSResponse GetResponse(FileType fileType, byte[] binaryData, string trackingNumber, string transportRef, string errorMessage = "")
		{
			var mock = new Mock<ISingleBookingRTUSResponse>();
			mock.SetupGet(m => m.FileType).Returns(fileType);
			mock.SetupGet(m => m.BinaryData).Returns(binaryData);
			mock.SetupGet(m => m.TrackingNumber).Returns(trackingNumber);
			mock.SetupGet(m => m.TransportReference).Returns(transportRef);
			mock.SetupGet(m => m.ErrorMessageForFailure).Returns(errorMessage);
			mock.SetupGet(m => m.IsSuccessful).Returns(string.IsNullOrEmpty(errorMessage));
			return mock.Object;
		}

		static OrganisationRTUSCollection GetRTUSCollectionWithOrg(OrgHeader org, string cbaCode)
		{
			var rtusCollection = new OrganisationRTUSCollection();
			var rtus = rtusCollection.AddNew();
			rtus.CBACode = cbaCode;
			rtus.OrganisationPK = org.PK;
			rtus.Url = SmartfreightUrl;

			return rtusCollection;
		}

		static IDisposable SetupRegistryForTest(string remotePrintServer, string remotePrintUserName, string remotePrintPassword)
		{
			return new DisposableList(new[]
			{
				TransportRegistry.Instance.RemotePrintServerURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, remotePrintServer),
				WebDataRegistry.Instance.WebServiceUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, remotePrintUserName),
				WebDataRegistry.Instance.WebServicePassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, remotePrintPassword),
			});
		}

		#endregion
	}
}
