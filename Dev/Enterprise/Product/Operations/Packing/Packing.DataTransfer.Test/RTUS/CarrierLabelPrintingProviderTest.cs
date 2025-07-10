using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.Packing.DataTransfer.Universal.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Http;
using WTG.RTUS.Interface;
using WTG.RTUS.Interface.TestFramework;
using WTG.RTUS.Printing;
using WTG.RTUS.Printing.TestFramework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Testing
{
	class CarrierLabelPrintingProviderTest : PackingTestCaseWithFactory
	{
		#region TestConstruction_ArgumentsArePassedThroughCorrectly

		public void TestConstruction_ArgumentsArePassedThroughCorrectly()
		{
			var onError = new Action<Exception>(delegate
			{ });
			var hubConnectionFactoryMock = new Mock<IHubConnectionFactory>();
			hubConnectionFactoryMock.Setup(f => f.Create(new Uri("http://PrintServer"), null, null, onError, null, null))
				.Returns(HubConnectionFactoryMocker.GetHubProxyConnection().Object);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(onError, new Mock<ICarrierLabelManager>().Object, hubConnectionFactoryMock.Object))
			{
				AssertEquals("Provider should have all valid details.", true, provider.IsRemotePrintingConnectionDetailsProvided);
				hubConnectionFactoryMock.Verify(f => f.Create(new Uri("http://PrintServer"), null, null, onError, null, null));
			}
		}

		public void TestConstruction_ArgumentsArePassedThroughCorrectly_ErrorAction()
		{
			var onError = new Action<Exception>(delegate
			{ });
			var managerMock = new Mock<ICarrierLabelManager>();
			var hubConnectionFactoryMock = new Mock<IHubConnectionFactory>();
			Action<Exception> onErrorPassedIn = null;
			IHubConnectionFactory hubConnectionFactory = null;

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(GetRTUSPrinter))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(onError, managerMock.Object, hubConnectionFactoryMock.Object))
			{
				AssertEquals("Provider should have all valid details.", true, provider.IsRemotePrintingConnectionDetailsProvided);
				AssertEquals("Should have passed through parameters correctly.", hubConnectionFactoryMock.Object, hubConnectionFactory);
				AssertEquals("Should have passed through parameters correctly.", onError, onErrorPassedIn);
			}

			IRTUSPrinter GetRTUSPrinter(object[] args)
			{
				hubConnectionFactory = (IHubConnectionFactory)args[0];
				onErrorPassedIn = (Action<Exception>)args[4];
				return new Mock<IRTUSPrinter>().Object;
			}
		}

		#endregion

		#region TestConstruction_ArgumentsArePassedThroughCorrectly_Integration

		public void TestConstruction_ArgumentsArePassedThroughCorrectly_Integration()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT");
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var processorMock = new Mock<IRTUSProcessor>();
			processorMock.Setup(p => p.PushMessage(RequestType.Booking, It.IsNotNull<IHttpClientFactory>(), It.IsNotNull<Stream>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetSuccessResponse());

			Exception thrownException = null;

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(processorMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(ex => thrownException = ex, new CarrierLabelManager(null, null), null))
			{
				AssertEquals("Provider should have all valid details.", true, provider.IsRemotePrintingConnectionDetailsProvided);

				provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertType<HttpRequestException>(thrownException);
#if NETFRAMEWORK
				AssertEquals("An error occurred while sending the request.", thrownException.Message);
#else
				AssertEquals("No such host is known. (printserver:80)", thrownException.Message);
#endif
			}
		}

#endregion

		#region TestConstruction_DoesNotAcceptInvalidArguments

		public void TestConstruction_DoesNotAcceptInvalidArguments()
		{
			AssertExceptionThrown<ArgumentNullException>(() => CarrierLabelPrintingProvider.GetProvider(delegate { }, null, new Mock<IHubConnectionFactory>().Object));
		}

		#endregion

		#region TestDispose

		public void TestDispose()
		{
			var managerMock = new Mock<ICarrierLabelManager>();
			var hubConnectionFactoryMock = HubConnectionFactoryMocker.GetHubConnectionFactory(new Uri("http://PrintServer"));
			var rtusPrinterMock = new Mock<IRTUSPrinter>();

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(rtusPrinterMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, hubConnectionFactoryMock.Object))
			{
				AssertEquals("Provider should have all valid details.", true, provider.IsRemotePrintingConnectionDetailsProvided);
				managerMock.Verify(m => m.Dispose(), Times.Never);
				rtusPrinterMock.Verify(f => f.Dispose(), Times.Never);
			}

			managerMock.Verify(m => m.Dispose(), Times.Once);
			rtusPrinterMock.Verify(f => f.Dispose(), Times.Once);
		}

		public void TestDispose_Integration()
		{
			var clientFactoryMock = HttpClientFactoryMocker.CreateMockWithOkResponseAndContent(RTUSCBA.SmartFreight, "http://SmartFreight", new byte[] { 0xA, 0xB, 0xC, 0xD }, null);
			var carrierLabelManager = new CarrierLabelManager(null, clientFactoryMock.Object);
			var hubConnectionFactoryMock = HubConnectionFactoryMocker.GetHubConnectionFactory(new Uri("http://PrintServer"));
			var rtusPrinterMock = new Mock<IRTUSPrinter>();

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(rtusPrinterMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, carrierLabelManager, hubConnectionFactoryMock.Object))
			{
				AssertEquals("Provider should have all valid details.", true, provider.IsRemotePrintingConnectionDetailsProvided);
				clientFactoryMock.Verify(f => f.Dispose(), Times.Never);
				rtusPrinterMock.Verify(f => f.Dispose(), Times.Never);
				AssertEquals("Should be registered.", true, DisposableLeakListener.Instance.IsRegistered(provider));
			}

			clientFactoryMock.Verify(f => f.Dispose(), Times.Once);
			rtusPrinterMock.Verify(f => f.Dispose(), Times.Once);
		}

		#endregion

		#region TestGetProvider_InvalidServerUrl

		public void TestGetProvider_InvalidServerUrl()
		{
			var managerMock = new Mock<ICarrierLabelManager>();

			using (SetupRegistryForTest(null, "USER", "PASSWORD"))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null);
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var response = provider.PrintCarrierLabel(null, RTUSCBA.Pierbridge, null, null);
				AssertEquals("Printing cannot be successful when something is invalid.", false, response.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: Invalid Remote Printing Server URL in Registry ''.", response.Message);
			}

			using (SetupRegistryForTest("", "USER", "PASSWORD"))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null);
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var response = provider.PrintCarrierLabel(null, RTUSCBA.Pierbridge, null, null);
				AssertEquals("Printing cannot be successful when something is invalid.", false, response.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: Invalid Remote Printing Server URL in Registry ''.", response.Message);
			}

			using (SetupRegistryForTest(" ", "USER", "PASSWORD"))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null);
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var response = provider.PrintCarrierLabel(null, RTUSCBA.Pierbridge, null, null);
				AssertEquals("Printing cannot be successful when something is invalid.", false, response.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: Invalid Remote Printing Server URL in Registry ' '.", response.Message);
			}

			using (SetupRegistryForTest("InvalidUrl", "USER", "PASSWORD"))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null);
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var response = provider.PrintCarrierLabel(null, RTUSCBA.Pierbridge, null, null);
				AssertEquals("Printing cannot be successful when something is invalid.", false, response.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: Invalid Remote Printing Server URL in Registry 'InvalidUrl'.", response.Message);
			}
		}

		#endregion

		#region TestGetProvider_MissingUserName

		public void TestGetProvider_MissingUserName()
		{
			var managerMock = new Mock<ICarrierLabelManager>();

			using (SetupRegistryForTest("http://PrintServer", null, "PASSWORD"))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null);
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var response = provider.PrintCarrierLabel(null, RTUSCBA.Pierbridge, null, null);
				AssertEquals("Printing cannot be successful when something is invalid.", false, response.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Username provided in Registry.", response.Message);
			}

			using (SetupRegistryForTest("http://PrintServer", "", "PASSWORD"))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null);
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var response = provider.PrintCarrierLabel(null, RTUSCBA.Pierbridge, null, null);
				AssertEquals("Printing cannot be successful when something is invalid.", false, response.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Username provided in Registry.", response.Message);
			}

			using (SetupRegistryForTest("http://PrintServer", " ", "PASSWORD"))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null);
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var response = provider.PrintCarrierLabel(null, RTUSCBA.Pierbridge, null, null);
				AssertEquals("Printing cannot be successful when something is invalid.", false, response.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Username provided in Registry.", response.Message);
			}
		}

		#endregion

		#region TestGetProvider_MissingPassword

		public void TestGetProvider_MissingPassword()
		{
			var managerMock = new Mock<ICarrierLabelManager>();

			using (SetupRegistryForTest("http://PrintServer", "USER", null))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null);
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var response = provider.PrintCarrierLabel(null, RTUSCBA.Pierbridge, null, null);
				AssertEquals("Printing cannot be successful when something is invalid.", false, response.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Password provided in Registry.", response.Message);
			}

			using (SetupRegistryForTest("http://PrintServer", "USER", ""))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null);
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var response = provider.PrintCarrierLabel(null, RTUSCBA.Pierbridge, null, null);
				AssertEquals("Printing cannot be successful when something is invalid.", false, response.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Password provided in Registry.", response.Message);
			}

			using (SetupRegistryForTest("http://PrintServer", "USER", " "))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null);
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var response = provider.PrintCarrierLabel(null, RTUSCBA.Pierbridge, null, null);
				AssertEquals("Printing cannot be successful when something is invalid.", false, response.Success);
				AssertEquals("Message should specify issue.", "Unable to Print Carrier Label due to the following: No Remote Printing Password provided in Registry.", response.Message);
			}
		}

		#endregion

		#region TestGetProvider_AllDetailsInvalid

		public void TestGetProvider_AllDetailsInvalid()
		{
			using (SetupRegistryForTest("InvalidUrl", "", ""))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, new Mock<ICarrierLabelManager>().Object, null);
				AssertEquals("No valid configuration data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				var response = provider.PrintCarrierLabel(null, RTUSCBA.Pierbridge, null, null);
				AssertEquals("Printing cannot be successful when something is invalid.", false, response.Success);
				AssertEquals("Message should specify issue.",
"Unable to Print Carrier Label due to the following: Invalid Remote Printing Server URL in Registry 'InvalidUrl'. No Remote Printing Username provided in Registry. No Remote Printing Password provided in Registry.", response.Message);
			}
		}

		#endregion

		#region TestObjectFactoryCreation

		public void TestObjectFactoryCreation()
		{
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = ObjectFactory.Get<ICarrierLabelPrintingProvider>(nameof(ICarrierLabelPrintingProvider), null, new Mock<ICarrierLabelManager>().Object, null))
			{
				AssertType<CarrierLabelPrintingProvider>(provider);
				AssertEquals("Provider should have all valid details.", true, provider.IsRemotePrintingConnectionDetailsProvided);
			}
		}

		#endregion

		#region TestPrintCarrierLabel

		public void TestPrintCarrierLabel_DoesNotAcceptInvalidArguments()
		{
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			{
				var package = Factory.New<PkgPackage>();
				var printer = Factory.New<IStmPrintQueue>();
				using (var provider = CarrierLabelPrintingProvider.GetProvider(null, new Mock<ICarrierLabelManager>().Object, null))
				{
					AssertExceptionThrown<ArgumentNullException>(() => provider.PrintCarrierLabel(null, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer));
					AssertExceptionThrown<ArgumentNullException>(() => provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), null));
				}
			}
		}

		public void TestPrintCarrierLabel_LabelNotReturned()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var printer = Factory.New<IStmPrintQueue>();

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, null, "TRACKING", "TransportRef", "NO LABEL!"));

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("Response from RTUS was missing data.", false, response.Success);
				AssertEquals("No Tracking Number should have been returned.", "ABC", package.KP_PackageID);
				AssertEquals("NO LABEL!", response.Message);
			}
		}

		public void TestPrintCarrierLabel_TrackingNumberNotReturned()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var printer = Factory.New<IStmPrintQueue>();

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, null, "TransportRef", "NO TRACKING!"));

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("Response from RTUS was missing data.", false, response.Success);
				AssertEquals("No Tracking Number should have been returned.", "ABC", package.KP_PackageID);
				AssertEquals("NO TRACKING!", response.Message);
			}
		}

		public void TestPrintCarrierLabel_TransportReferenceNotReturned()
		{
			Data.CreatePackingData();

			Data.Dummy.TransportReference = "Initial Test Value";
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER")).Returns(true);

			AssertEquals("Precondition: Transport Ref on PackingParent initial value", "Initial Test Value", Data.Dummy.TransportReference);

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, labelBytes, "Tracking", null));

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("'Print' should be successful.", true, response.Success);
				AssertEquals("Null tranport reference shouldn't overwrite existing value.", "Initial Test Value", Data.Dummy.TransportReference);
			}
		}

		public void TestPrintCarrierLabel_TransportReferenceWhitespace()
		{
			Data.CreatePackingData();

			Data.Dummy.TransportReference = "Initial Test Value";
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER")).Returns(true);

			AssertEquals("Precondition: Transport Ref on PackingParent initial value", "Initial Test Value", Data.Dummy.TransportReference);

			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "Tracking", null));

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("'Print' should be successful.", true, response.Success);
				AssertEquals("Whitespace tranport reference shouldn't overwrite existing value.", "Initial Test Value", Data.Dummy.TransportReference);
			}
		}

		public void TestPrintCarrierLabel_PrintingFailed()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, labelBytes, "NUMBER", "TRANSPORTREF"));

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER")).Returns(false);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("Printing was not successful.", false, response.Success);
				AssertEquals("No Error Message set. Errors from printing would be in the form of exceptions.", "", response.Message);
				AssertEquals("Even if Printing was not successful, if Booking was successful, Tracking Number should be set.", "NUMBER", package.KP_PackageID);
				AssertEquals("Even if Printing was not successful, if Booking was successful, Previous Package ID should be set.", "ABC", package.KP_PreviousPackageID);
				AssertEquals("Even if Printing was not successful, if Booking was successful, IsSentToRTUS should be true.", true, package.IsSentToRTUS);
				AssertEquals("Even if Printing was not successful, if Booking was successful, PackageJob.ParentJob should set transport reference.", "TRANSPORTREF", Data.Dummy.TransportReference);

				printerMock.VerifyAll();
			}
		}

		public void TestPrintCarrierLabel_RequestSuccessful()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, labelBytes, "NUMBER", "TRANSPORTREF"));

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER")).Returns(true);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("Printing was successful.", true, response.Success);
				AssertEquals("Printing was successful.", "", response.Message);
				AssertEquals("If Booking was successful, Tracking Number should be set.", "NUMBER", package.KP_PackageID);
				AssertEquals("If Booking was successful, Previous Package ID should be set.", "ABC", package.KP_PreviousPackageID);
				AssertEquals("If Booking was successful, IsSentToRTUS should be true.", true, package.IsSentToRTUS);
				AssertEquals("If Booking was successful, PackageJob.ParentJob should set transport reference.", "TRANSPORTREF", Data.Dummy.TransportReference);

				managerMock.VerifyAll();
				printerMock.VerifyAll();
			}
		}

		public void TestPrintCarrierLabel_RePrintingSucceeded()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "NUMBER");
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, labelBytes, "NUMBER", "TRANSPORTREF"));

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER")).Returns(true);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("Printing was successful.", true, response.Success);
				AssertEquals("Printing was successful.", "", response.Message);
				AssertEquals("Package ID should stay the same.", "NUMBER", package.KP_PackageID);
				AssertEquals("PackageJob.ParentJob should should be set the same value.", "TRANSPORTREF", Data.Dummy.TransportReference);
				AssertEquals("Previous Package ID should not be updated.", "", package.KP_PreviousPackageID);
				printerMock.VerifyAll();
			}
		}

		[TestDate(2019, 04, 01)]
		public void TestPrintCarrierLabel_Integration()
		{
			Data.CreatePackingData();

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			var expectedBinaryData = new byte[] { (byte)'A', (byte)'B' };
			var expectedResponse = GetUniversalResponseXML("parentID", "packageID", "RYAN123", "TransportRef", expectedBinaryData);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedResponse)))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT");
				var printer = Factory.New<IStmPrintQueue>();
				printer.QueueName = "LASERPRINTER";
				printer.SQ_ServerName = "SKYNET";
				var expectedBytes = GetBytesFromPackageUniversalShipment(package);

				var remoteServerUrl = "http://PrintServer";
				var rtusUrl = "http://SmartFreight";
				var clientFactoryMock = HttpClientFactoryMocker.CreateMockWithOkResponseAndContent(RTUSCBA.SmartFreight, rtusUrl, expectedBytes, responseStream);
				var manager = new CarrierLabelManager(null, clientFactoryMock.Object);
				var hubProxyMock = HubConnectionFactoryMocker.GetHubProxy("LASERPRINTER", "SKYNET", FileType.PDF, expectedBinaryData, success: true);
				var hubProxyConnectionMock = HubConnectionFactoryMocker.GetHubProxyConnection(hubProxyMock);
				var hubConnectionFactoryMock = HubConnectionFactoryMocker.GetHubConnectionFactory(new Uri(remoteServerUrl), hubProxyConnectionMock);

				using (SetupRegistryForTest(remoteServerUrl, "USER", "PASSWORD"))
				using (var provider = CarrierLabelPrintingProvider.GetProvider(null, manager, hubConnectionFactoryMock.Object))
				{
					var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri(rtusUrl), printer);
					AssertEquals("Printing was successful.", true, response.Success);
					AssertEquals("Tracking Number should be returned.", "RYAN123", package.KP_PackageID);
					AssertEquals("Transport Reference should be returned.", "TransportRef", Data.Dummy.TransportReference);
					AssertEquals("Printing was successful.", "", response.Message);

					var credentials = (NetworkCredential)hubProxyConnectionMock.Object.Credentials;
					AssertEquals("Credentials should have been set up correctly.", "USER", credentials.UserName);
					AssertEquals("Credentials should have been set up correctly.", "PASSWORD", credentials.Password);
					hubProxyMock.VerifyAll();
					hubProxyConnectionMock.VerifyAll();
				}
			}
		}

		public void TestPrintCarrierLabel_WorksWithLegacyServerUrlSuffix()
		{
			var hubConnectionFactoryMock = HubConnectionFactoryMocker.GetHubConnectionFactory(new Uri("http://PrintServer"));

			using (SetupRegistryForTest("http://PrintServer/RemotePrintingService.asmx", "USER", "PASSWORD"))
			using (var provider = ObjectFactory.Get<ICarrierLabelPrintingProvider>(nameof(ICarrierLabelPrintingProvider), null, new Mock<ICarrierLabelManager>().Object, hubConnectionFactoryMock.Object))
			{
				AssertEquals("Provider should have all valid details.", true, provider.IsRemotePrintingConnectionDetailsProvided);
				hubConnectionFactoryMock.Verify(f => f.Create(new Uri("http://PrintServer"), null, null, null, null, null), Times.Once());
			}
		}

		public void TestPrintCarrierLabel_PrinterStoredAgainstPackage()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, labelBytes, "NUMBER", "TRANSPORTREF"));

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER")).Returns(true);

			AssertEquals("Precondition", ZGuid.Empty, package.RTUSLabelPrinterPK);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("Printing was successful.", true, response.Success);
				AssertEquals("Printing was successful.", "", response.Message);
				AssertEquals("RTUS Printer PK stored against the package.", printer.PK, package.RTUSLabelPrinterPK);
			}
		}

		public void TestPrintCarrierLabel_PrinterStoredAgainstPackage_PreviousPrinterPKOverridden()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			package.IsSentToRTUS = true;
			var printer1 = Factory.New<IStmPrintQueue>();
			package.RTUSLabelPrinterPK = printer1.PK;
			AssertEquals("Precondition", printer1.PK, package.RTUSLabelPrinterPK);

			var printer2 = Factory.New<IStmPrintQueue>();
			printer2.QueueName = "PRINTER";
			printer2.SQ_ServerName = "PRINTSERVER";

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, labelBytes, "NUMBER", "TRANSPORTREF"));

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER")).Returns(true);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer2);
				AssertEquals("Printing was successful.", true, response.Success);
				AssertEquals("Printing was successful.", "", response.Message);
				AssertEquals("New RTUS Printer PK stored against the package.", printer2.PK, package.RTUSLabelPrinterPK);
			}
		}

		public void TestPrintCarrierLabel_PrinterStoredAgainstPackage_PrintFailed()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, labelBytes, "NUMBER", "TRANSPORTREF"));

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER")).Returns(false);

			AssertEquals("Precondition", ZGuid.Empty, package.RTUSLabelPrinterPK);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("Printing was not successful.", false, response.Success);
				AssertEquals("No Error Message set. Errors from printing would be in the form of exceptions.", "", response.Message);
				AssertEquals("RTUS Printer PK is not stored against the package.", ZGuid.Empty, package.RTUSLabelPrinterPK);
			}
		}

		public void TestPrintCarrierLabel_ParentShipmentGeneration_MultiplePackageJobs()
		{
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			var dummy = Factory.New<DummyWithPacking>();
			dummy.CarrierBookingAgent = smartFreight;

			var rtusCollection = new OrganisationRTUSCollection();
			var rtusOption = rtusCollection.AddNew();
			rtusOption.CBACode = "SMA"; // SmartFreight
			rtusOption.OrganisationPK = smartFreight.PK;
			rtusOption.Url = "http://SmartFreight";

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST1"
			};
			var mockHandle = new DummyHandle(() =>
			{
				var shipmentToReturn = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

				using (var memoryStream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.New<IXmlWriter>().WriteXML(parentShipment, memoryStream);

					memoryStream.Position = 0;
					ObjectFactory.Get<IXmlReader>().ReadXML(shipmentToReturn, memoryStream, new DummyLogger());
				}

				return shipmentToReturn;
			});

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER")).Returns(true);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, dummy))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var parentShipmentsPassedIn = new List<ITopLevelDataObject>();
				var smartFreightUrl = new Uri("http://SmartFreight");

				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, smartFreightUrl))
					.Callback((RequestType request, ITopLevelDataObject shipmentDataObject, RTUSCBA type, Uri url) => parentShipmentsPassedIn.Add(shipmentDataObject))
					.Returns(GetResponse(FileType.PDF, labelBytes, "NUMBER", "TRANSPORTREF"));

				var packageJob1 = Factory.New<PkgPackageJob>();
				packageJob1.KJ_JobID = "P00000001";
				packageJob1.KJ_ParentID = dummy.PK;
				packageJob1.KJ_ParentTableCode = dummy.TablePrefix;
				var package1 = packageJob1.Packages.AddNew("PLT", "ABC");
				var result = provider.PrintCarrierLabel(package1, RTUSCBA.SmartFreight, smartFreightUrl, printer);
				AssertEquals("Print is successful.", true, result.Success);

				parentShipment.BookingConfirmationReference = "TEST2";
				var packageJob2 = Factory.New<PkgPackageJob>();
				packageJob2.KJ_JobID = "P00000002";
				packageJob2.KJ_ParentID = dummy.PK;
				packageJob2.KJ_ParentTableCode = dummy.TablePrefix;
				var package2 = packageJob2.Packages.AddNew("PLT", "XYZ");
				result = provider.PrintCarrierLabel(package2, RTUSCBA.SmartFreight, smartFreightUrl, printer);
				AssertEquals("Print is successful.", true, result.Success);

				AssertEquals("Should have made two requests.", 2, parentShipmentsPassedIn.Count);

				var shipment1 = parentShipmentsPassedIn[0];
				AssertEquals("Should have the correct Data Source Key.", "ABC", shipment1.DataContext.GetMatchingDataSource(DataContextType.PkgPackage).Key);
				AssertEquals("Generated universal shipment is correct.", "TEST1", ((UniversalShipment)shipment1).BookingConfirmationReference);

				var shipment2 = parentShipmentsPassedIn[1];
				AssertEquals("Should have the correct Data Source Key.", "XYZ", shipment2.DataContext.GetMatchingDataSource(DataContextType.PkgPackage).Key);
				AssertEquals("Generation should not be cached for different package jobs.", "TEST2", ((UniversalShipment)shipment2).BookingConfirmationReference);
			}
		}

		public void TestPrintCarrierLabel_ParentShipmentGenerationIsCached()
		{
			Data.CreatePackingData();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var rtusCollection = new OrganisationRTUSCollection();
			var rtusOption = rtusCollection.AddNew();
			rtusOption.CBACode = "SMA"; // SmartFreight
			rtusOption.OrganisationPK = smartFreight.PK;
			rtusOption.Url = "http://SmartFreight";

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() =>
			{
				var shipmentToReturn = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

				using (var memoryStream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.New<IXmlWriter>().WriteXML(parentShipment, memoryStream);

					memoryStream.Position = 0;
					ObjectFactory.Get<IXmlReader>().ReadXML(shipmentToReturn, memoryStream, new DummyLogger());
				}

				return shipmentToReturn;
			});

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER")).Returns(true);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var parentShipmentsPassedIn = new List<ITopLevelDataObject>();
				var smartFreightUrl = new Uri("http://SmartFreight");

				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, smartFreightUrl))
					.Callback((RequestType request, ITopLevelDataObject shipmentDataObject, RTUSCBA type, Uri url) => parentShipmentsPassedIn.Add(shipmentDataObject))
					.Returns(GetResponse(FileType.PDF, labelBytes, "NUMBER", "TRANSPORTREF"));

				var package1 = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				var result = provider.PrintCarrierLabel(package1, RTUSCBA.SmartFreight, smartFreightUrl, printer);
				AssertEquals("Print is successful.", true, result.Success);

				parentShipment.BookingConfirmationReference = "NO WAY!";
				var package2 = Data.PackageJob.Packages.AddNew("PLT", "XYZ");
				result = provider.PrintCarrierLabel(package2, RTUSCBA.SmartFreight, smartFreightUrl, printer);
				AssertEquals("Print is successful.", true, result.Success);

				AssertEquals("Should have made two requests.", 2, parentShipmentsPassedIn.Count);

				var shipment1 = parentShipmentsPassedIn[0];
				AssertNotEquals("Should not be the same instance as the initial generation.", parentShipment, shipment1);
				AssertEquals("Should have the correct Data Source Key.", "ABC", shipment1.DataContext.GetMatchingDataSource(DataContextType.PkgPackage).Key);
				AssertEquals("Generation should be cached and not reflect any change afterward.", "TEST", ((UniversalShipment)shipment1).BookingConfirmationReference);

				var shipment2 = parentShipmentsPassedIn[1];
				AssertNotEquals("Should not be the same instance as the initial generation.", parentShipment, shipment2);
				AssertEquals("Should have the correct Data Source Key.", "XYZ", shipment2.DataContext.GetMatchingDataSource(DataContextType.PkgPackage).Key);
				AssertEquals("Generation should be cached and not reflect any change afterward.", "TEST", ((UniversalShipment)shipment2).BookingConfirmationReference);
			}
		}

		public void TestPrintCarrierLabel_PackageJobFinalised()
		{
			Data.CreatePackingData();
			Data.Dummy.TransportReference = "OLDREF";
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			Data.PackageJob.KJ_IsFinalized = true;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, labelBytes, "NUMBER", "TRANSPORTREF"));

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("Printing was not successful.", false, response.Success);
				AssertEquals("Printing was not successful.", "The Package job is already finalized and the RTUS response attempted to update the package details.", response.Message);
				AssertEquals("Printing was not successful.", "ABC", package.KP_PackageID);
				AssertEquals("Printing was not successful.", "OLDREF", Data.Dummy.TransportReference);
			}
		}

		public void TestPrintCarrierLabel_PackageJobFinalised_PackageNotUpdated()
		{
			Data.CreatePackingData();
			Data.Dummy.TransportReference = "OLDREF";
			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			Data.PackageJob.KJ_IsFinalized = true;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, labelBytes, "ABC", "OLDREF"));

			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER")).Returns(true);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("Printing was successful.", true, response.Success);
				AssertEquals("Printing was successful.", "", response.Message);
				AssertEquals("Printing was successful.", "ABC", package.KP_PackageID);
				AssertEquals("Printing was successful.", "OLDREF", Data.Dummy.TransportReference);
			}
		}

		#endregion

		#region TestPrintCarrierLabel_WithStandardRemotePrint

		public void TestPrintCarrierLabel_WithStandardRemotePrint()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "Tracking", null));

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			var printerMock = new Mock<IRTUSPrinter>(MockBehavior.Strict);

			using (TransportRegistry.Instance.UseStandardRemotePrintingForRTUS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("'Print' should be successful.", true, response.Success);
				printerMock.Verify(p => p.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER"), Times.Never);

				var otherFactory = new BusinessObjectFactory();
				var printJob = otherFactory.Load<IStmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK)).Single();
				AssertArrayEqualsByElements(labelBytes, printJob.SP_CustomProperties);
			}
		}

		public void TestPrintCarrierLabel_WithStandardRemotePrint_OnError()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "Tracking", null));

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			Exception thrownException = null;

			using (TransportRegistry.Instance.UseStandardRemotePrintingForRTUS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(OnError, managerMock.Object, null))
			{
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("'Print' should not be successful.", false, response.Success);
				AssertContains("The INSERT statement conflicted with the FOREIGN KEY constraint \"StmPrintJob__FK2_StmPrintQueue_CRR_120N\".", thrownException.Message);
			}

			void OnError(Exception exception)
			{
				thrownException = exception;
			}
		}

		#endregion

		#region TestPrintCarrierLabel_OnPackageBookedViaRTUS

		[TestDate(2021, 04, 01, 10, 11, 12)]
		public void TestPrintCarrierLabel_OnPackageBookedViaRTUS()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var parentPackage = (DummyWithPacking)package.PackageJob.ParentJob;

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();
			managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Booking, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(GetResponse(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "Tracking", null));

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			var printerMock = new Mock<IRTUSPrinter>(MockBehavior.Strict);

			using (TransportRegistry.Instance.UseStandardRemotePrintingForRTUS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				Assert("Precondition", parentPackage.OnPackageBookedViaRTUS_LastDateTimeReceived.IsEmpty);
				var response = provider.PrintCarrierLabel(package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"), printer);
				AssertEquals("'Print' should be successful.", true, response.Success);
				AssertEquals("Should update package booked date.", new ZDateTime(2021, 04, 01, 10, 11, 12), parentPackage.OnPackageBookedViaRTUS_LastDateTimeReceived);
			}
		}

		#endregion

		#region TestPrint

		public void TestPrint()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();

			var printCalled = false;
			var printerMock = new Mock<IRTUSPrinter>();
			printerMock.Setup(pr => pr.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER"))
				.Callback(() => printCalled = true)
				.Returns(true);

			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				var response = provider.Print(FileType.PDF, labelBytes, printer);
				printerMock.VerifyAll();

				AssertEquals("Print was called.", true, printCalled);
			}
		}

		public void TestPrint_EmptyCarrierLabelPrintingProvider()
		{
			var managerMock = new Mock<ICarrierLabelManager>();
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			var labelBytes = new byte[] { (byte)'A', (byte)'B' };

			using (SetupRegistryForTest(null, "USER", "PASSWORD"))
			{
				var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null);
				AssertEquals("There is at least one invalid bit of data.", false, provider.IsRemotePrintingConnectionDetailsProvided);

				AssertExceptionThrown<InvalidOperationException>("Print will throw an exception if remote printing connection details are not provided.",
					() => provider.Print(FileType.PDF, labelBytes, printer));
			}
		}

		public void TestPrint_WithStandardRemotePrint()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";
			Factory.Save();

			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();

			var printerMock = new Mock<IRTUSPrinter>(MockBehavior.Strict);

			using (TransportRegistry.Instance.UseStandardRemotePrintingForRTUS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (ObjectFactory.Substitute(printerMock.Object))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(null, managerMock.Object, null))
			{
				AssertEquals(true, provider.Print(FileType.PDF, labelBytes, printer));
				printerMock.Verify(p => p.Print(FileType.PDF, labelBytes, "PRINTER", "PRINTSERVER"), Times.Never);

				var otherFactory = new BusinessObjectFactory();
				var printJob = otherFactory.Load<IStmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK)).Single();
				AssertArrayEqualsByElements(labelBytes, printJob.SP_CustomProperties);
			}
		}

		public void TestPrint_WithStandardRemotePrint_OnError()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			printer.SQ_ServerName = "PRINTSERVER";

			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");

			var labelBytes = new byte[] { (byte)'A', (byte)'B' };
			var managerMock = new Mock<ICarrierLabelManager>();

			Exception thrownException = null;

			using (TransportRegistry.Instance.UseStandardRemotePrintingForRTUS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SetupRegistryForTest("http://PrintServer", "USER", "PASSWORD"))
			using (var provider = CarrierLabelPrintingProvider.GetProvider(OnError, managerMock.Object, null))
			{
				AssertEquals(false, provider.Print(FileType.PDF, labelBytes, printer));
				AssertContains("The INSERT statement conflicted with the FOREIGN KEY constraint \"StmPrintJob__FK2_StmPrintQueue_CRR_120N\".", thrownException.Message);
			}

			void OnError(Exception exception)
			{
				thrownException = exception;
			}
		}

		#endregion

		#region Implementation

		static string GetUniversalResponseXML(string parentID, string packageID, string trackingNumber, string transportRef, byte[] binaryData)
		{
			return
$@"<UniversalResponse xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Status>PRS</Status>
	<Data>
		<Event>
			<EventReference>|RFN={trackingNumber}|OLD={packageID}|CRF={parentID}</EventReference>
			<AttachedDocumentCollection>
				<AttachedDocument>
					<Type>
						<Code>PDF</Code>
					</Type>
					<ImageData>
					{Convert.ToBase64String(binaryData)}
					</ImageData>
				</AttachedDocument>
			</AttachedDocumentCollection>
			<ContextCollection>
				<Context>
					<Type>TransportReference</Type>
					<Value>{transportRef}</Value>
				</Context>
			</ContextCollection>
		</Event>
	</Data>
</UniversalResponse>";
		}

		ISingleBookingRTUSResponse GetSuccessResponse()
		{
			return GetResponse(FileType.PDF, new byte[] { (byte)'A', (byte)'B' }, "NUMBER", "TRANSPORTREF");
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

		static byte[] GetBytesFromPackageUniversalShipment(PkgPackage package)
		{
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, package));
				var expectedShipment = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, RequestType.Booking).GetDataObject(package);
				ObjectFactory.New<IXmlWriter>().WriteXML(expectedShipment, stream);
				using (var streamReader = new StreamReader(stream))
				{
					return Encoding.UTF8.GetBytes(streamReader.ReadToEnd());
				}
			}
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
