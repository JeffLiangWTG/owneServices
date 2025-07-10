using System;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.Common.Testing;
using CargoWise.IO;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.Packing.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Http;
using WTG.NUnit;
using WTG.RTUS.Interface;
using WTG.RTUS.Interface.TestFramework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Testing
{
	class CarrierLabelManagerTest : PackingTestCaseWithFactory
	{
		#region TestConstruction_ArgumentsArePassedThroughCorrectly_Integration

		public void TestConstruction_ArgumentsArePassedThroughCorrectly_Integration()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT");

			using (ICarrierLabelManager manager = new CarrierLabelManager(null, null))
			{
				var response = manager.PushCarrierLabelRequest(DummyRequest.New(), package, RTUSCBA.SmartFreight, new Uri("http://127.0.0.1:2"));
				AssertEquals("Response should not have been successful.", false, response.IsSuccessful);
				AssertStartsWith("Should have some ConnectionRefused error.", "ConnectionRefused", response.ErrorMessageForFailure);
			}
		}

		#endregion

		#region TestDispose

		public void TestDispose()
		{
			var clientFactoryMock = HttpClientFactoryMocker.CreateMockWithOkResponseAndContent(RTUSCBA.SmartFreight, "http://SmartFreight", new byte[] { 0xA, 0xB, 0xC, 0xD }, null);

			using (var manager = new CarrierLabelManager(null, clientFactoryMock.Object))
			{
				clientFactoryMock.Verify(f => f.Dispose(), Times.Never);
				AssertEquals("Should be registered.", true, DisposableLeakListener.Instance.IsRegistered(manager));
			}

			clientFactoryMock.Verify(f => f.Dispose(), Times.Once);
		}

		#endregion

		#region TestObjectFactoryCreation

		public void TestObjectFactoryCreation()
		{
			using (var manager = ObjectFactory.New<ICarrierLabelManager>(null, null))
			{
				AssertType<CarrierLabelManager>(manager);
			}
		}

		#endregion

		#region TestPushCarrierLabelRequest

		public void TestPushCarrierLabelRequest_DoesNotAcceptInvalidArguments()
		{
			var package = Factory.New<PkgPackage>();
			using (ICarrierLabelManager manager = new CarrierLabelManager(null, null))
			{
				AssertExceptionThrown<ArgumentNullException>(() => manager.PushCarrierLabelRequest(DummyRequest.New(), (PkgPackage)null, RTUSCBA.SmartFreight, new Uri("http://SmartFreight")));
				AssertExceptionThrown<ArgumentNullException>(() => manager.PushCarrierLabelRequest(DummyRequest.New(), (ITopLevelDataObject)null, RTUSCBA.SmartFreight, new Uri("http://SmartFreight")));
			}
		}

		public void TestPushCarrierLabelRequest_UniversalShipmentGenerationThrowsException()
		{
			Data.CreatePackingData();

			var mockHandle = new DummyHandle(() => throw new InvalidOperationException("Oops!"));
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT");

				using (ICarrierLabelManager manager = new CarrierLabelManager(null, null))
				{
					AssertExceptionThrown(typeof(InvalidOperationException), "Oops!", () =>
						manager.PushCarrierLabelRequest(DummyRequest.New(), package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight")));
				}
			}
		}

		public void TestPushCarrierLabelRequest_UniversalShipmentGenerationThrowsException_WithExceptionHandler()
		{
			Data.CreatePackingData();

			var exceptionToThrow = new InvalidOperationException("Oops!");
			var mockHandle = new DummyHandle(() => throw exceptionToThrow);
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");

				Exception exceptionThrown = null;

				using (ICarrierLabelManager manager = new CarrierLabelManager(ex => exceptionThrown = ex, null))
				{
					var response = manager.PushCarrierLabelRequest(DummyRequest.New(), package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"));
					AssertEquals("Universal Shipment generation failed.", false, response.IsSuccessful);
					AssertEquals("Failed to generate Universal Shipment from Package 'ABC'.", response.ErrorMessageForFailure);
					AssertEquals("Should have called Exception handler with correct Parameter.", exceptionToThrow, exceptionThrown);
				}
			}
		}

		[TestDate(2019, 04, 01)]
		public void TestPushCarrierLabelRequest_CheckCorrectRequestIsMade()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");

			var request = DummyRequest.New();
			var expectedBytes = GetBytesFromPackageUniversalShipment(package, request);
			var clientFactoryMock = new Mock<IHttpClientFactory>();
			var processorMock = new Mock<IRTUSProcessor>();
			var responseMock = new Mock<IRTUSResponse>();
			processorMock.Setup(p => p.PushMessage(request, clientFactoryMock.Object, It.Is<SubStreamableStream>(s => ValidateBytes(s, expectedBytes)), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(responseMock.Object);

			using (ObjectFactory.Substitute(processorMock.Object))
			using (ICarrierLabelManager manager = new CarrierLabelManager(null, clientFactoryMock.Object))
			{
				var response = manager.PushCarrierLabelRequest(request, package, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"));
				AssertEquals("Response from RTUS should be correct.", responseMock.Object, response);
			}
		}

		public void TestPushCarrierLabelRequest_CheckCorrectRequestIsMade_WhenPassingThroughUniversalShipment()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var packageUniversalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BookingConfirmationReference = "TEST",
				WayBillNumber = "WAYBILL",
			};

			byte[] expectedBytes;

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.New<IXmlWriter>().WriteXML(packageUniversalShipment, stream);
				using (var streamReader = new StreamReader(stream))
				{
					expectedBytes = Encoding.UTF8.GetBytes(streamReader.ReadToEnd());
				}
			}

			var clientFactoryMock = new Mock<IHttpClientFactory>();
			var processorMock = new Mock<IRTUSProcessor>();
			var request = DummyRequest.New();
			var responseMock = new Mock<IRTUSResponse>();
			processorMock.Setup(p => p.PushMessage(request, clientFactoryMock.Object, It.Is<SubStreamableStream>(s => ValidateBytes(s, expectedBytes)), RTUSCBA.SmartFreight, new Uri("http://SmartFreight")))
				.Returns(responseMock.Object);

			using (ObjectFactory.Substitute(processorMock.Object))
			using (var manager = new CarrierLabelManager(null, clientFactoryMock.Object))
			{
				var response = manager.PushCarrierLabelRequest(request, packageUniversalShipment, RTUSCBA.SmartFreight, new Uri("http://SmartFreight"));
				AssertEquals("Response from RTUS should be correct.", responseMock.Object, response);
			}
		}

		bool ValidateBytes(SubStreamableStream stream, byte[] expectedBytes)
		{
			using (var streamReader = new StreamReader(stream))
			{
				return Encoding.UTF8.GetBytes(streamReader.ReadToEnd()).ElementsEqual(expectedBytes);
			}
		}

		[TestDate(2019, 04, 01)]
		[ExpectNoExceptions]
		public void TestPushCarrierLabelRequest_Integration()
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
			var expectedResponse = GetUniversalResponseXML();

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedResponse)))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT");
				var request = DummyRequest.New();
				var expectedBytes = GetBytesFromPackageUniversalShipment(package, request);

				var rtusUrl = "http://SmartFreight";
				var clientFactoryMock = HttpClientFactoryMocker.CreateMockWithOkResponseAndContent(RTUSCBA.SmartFreight, rtusUrl, expectedBytes, responseStream);

				using (ICarrierLabelManager manager = new CarrierLabelManager(null, clientFactoryMock.Object))
				{
					// dummy request cannot have a successful response
					NUnit.Framework.Assert.That(
						delegate
						{
							manager.PushCarrierLabelRequest(request, package, RTUSCBA.SmartFreight, new Uri(rtusUrl));
						}, CustomConstraints.InnermostExceptionThrown(typeof(NotImplementedException)));

					clientFactoryMock.VerifyAll();
				}
			}
		}

		#endregion

		#region Implementation

		static string GetUniversalResponseXML()
		{
			return
$@"<UniversalResponse xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Status>PRS</Status>
	<Data>
	</Data>
</UniversalResponse>";
		}

		static byte[] GetBytesFromPackageUniversalShipment(PkgPackage package, RequestType requestType)
		{
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, package));
				var expectedShipment = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, requestType).GetDataObject(package);
				ObjectFactory.New<IXmlWriter>().WriteXML(expectedShipment, stream);
				using (var streamReader = new StreamReader(stream))
				{
					return Encoding.UTF8.GetBytes(streamReader.ReadToEnd());
				}
			}
		}

		#endregion
	}
}
