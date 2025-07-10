using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.Packing.DataTransfer.Universal.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Http;
using WTG.RTUS.Interface;
using WTG.RTUS.Interface.TestFramework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Testing
{
	class CarrierLabelCancellationTest : PackingTestCaseWithFactory
	{
		#region TestCancelPackages

		public void TestCancelPackages_DoesNotAcceptInvalidArguments()
		{
			ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(null);
			AssertExceptionThrown<ArgumentNullException>(() => cancellation.CancelPackages(null));
#if NETFRAMEWORK
			AssertExceptionThrown(typeof(ArgumentException), "Cannot have null Package elements in Collection.\r\nParameter name: packagesToCancel", () => cancellation.CancelPackages(new PkgPackage[] { null }));
#else
			var ex = AssertExceptionThrown<ArgumentException>(() => cancellation.CancelPackages(new PkgPackage[] { null }));
			Assert(ex.Message.Equals("Cannot have null Package elements in Collection. (Parameter 'packagesToCancel')"));
			Assert(ex.ParamName.Equals("packagesToCancel"));
#endif
		}

		public void TestCancelPackages_DoesNotCancelPackagesThatWereNotPassedIn()
		{
			Data.CreatePackingData();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var rtusCollection = new OrganisationRTUSCollection();
			var rtusOption = rtusCollection.AddNew();
			rtusOption.CBACode = "SMA"; // SmartFreight
			rtusOption.OrganisationPK = smartFreight.PK;
			rtusOption.Url = SmartFreightUrl;

			var managerMock = new Mock<ICarrierLabelManager>();
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(null);
				cancellation.SubscribePackageForCancellation(package);

				// pass in Package that wasn't subscribed, there should be no cancellations.
				var responses = cancellation.CancelPackages(new[] { Factory.New<PkgPackage>() });
				managerMock.Verify(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsAny<ITopLevelDataObject>(), It.IsAny<RTUSCBA>(), It.IsAny<Uri>()), Times.Never);
				AssertEquals("No Responses since nothing was cancelled.", 0, responses.Count());
			}
		}

		public void TestCancelPackages_IgnoreEmptyPackageID()
		{
			var dummyParent1 = Factory.New<DummyWithPacking>();
			var dummyParent2 = Factory.New<DummyWithPacking>();
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(dummyParent1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(dummyParent2);

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			dummyParent1.CarrierBookingAgent = smartFreight;
			dummyParent2.CarrierBookingAgent = smartFreight;

			var package1 = packageJob1.Packages.AddNew("PLT", "ABC");
			var package2 = packageJob2.Packages.AddNew("PLT", "");
			Factory.Save();

			var rtusCollection = new OrganisationRTUSCollection();
			var rtusOption = rtusCollection.AddNew();
			rtusOption.CBACode = "SMA";
			rtusOption.OrganisationPK = smartFreight.PK;
			rtusOption.Url = SmartFreightUrl;

			ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(null);

			var managerMock = new Mock<ICarrierLabelManager>();
			var mockHandle = new DummyHandle(p =>
			{
				var dataContext = DataContextFactory.New();
				dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
				return p.KP_PackageID == "ABC"
					? new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { BookingConfirmationReference = "SUCCESS", DataContext = dataContext, }
					: new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { BookingConfirmationReference = "EMPTY", DataContext = dataContext, };
			});

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, new[] { dummyParent1, dummyParent2 }))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			{
				cancellation.SubscribePackageForCancellation(package1);
				cancellation.SubscribePackageForCancellation(package2);

				var successResponse = new Mock<IRTUSResponse>();
				successResponse.SetupGet(r => r.IsSuccessful).Returns(true);

				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<UniversalShipment>(d => d.BookingConfirmationReference.GetValueOrDefault() == "SUCCESS"), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(successResponse.Object);

				var responses = cancellation.CancelPackages(new[] { package1, package2 });
				AssertEquals(1, responses.Count());

				var response1 = responses.Single();
				AssertEquals("Correct Package Job should be associated with each response.", packageJob1.PK, response1.PackageJobPK);
				AssertEquals("Correct Packing Parent should be associated with each response.", dummyParent1, response1.PackingParent);
				AssertEquals("Correct Package ID should be associated with each response.", "ABC", response1.PackageID);

				managerMock.Verify(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<UniversalShipment>(d => d.BookingConfirmationReference.GetValueOrDefault() == "SUCCESS"), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)));
				managerMock.Verify(m => m.Dispose());
				managerMock.VerifyNoOtherCalls();
			}
		}

		public void TestCancelPackages_NoCarrierBookingAgent()
		{
			Data.CreatePackingData();

			var managerMock = new Mock<ICarrierLabelManager>();
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (ObjectFactory.Substitute(managerMock.Object))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(null);
				cancellation.SubscribePackageForCancellation(package);

				var responses = cancellation.CancelPackages(new[] { package });
				managerMock.Verify(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsAny<ITopLevelDataObject>(), It.IsAny<RTUSCBA>(), It.IsAny<Uri>()), Times.Never);

				var errorResponse = responses.Single();
				AssertEquals("Response from RTUS should be failure since there was no Carrier Booking Agent.", false, errorResponse.IsSuccessful);
				AssertEquals("Response from RTUS should be failure since there was no Carrier Booking Agent.",
					"Failed to cancel Package 'ABC' because no Carrier Booking Agent is specified.", errorResponse.ErrorMessageForFailure);
				AssertEquals("Correct Package Job should be associated with each response.", Data.PackageJob.PK, errorResponse.PackageJobPK);
				AssertEquals("Correct Packing Parent should be associated with each response.", Data.Dummy, errorResponse.PackingParent);
				AssertEquals("Correct Package ID should be associated with each response.", "ABC", errorResponse.PackageID);
			}
		}

		public void TestCancelPackages_MissingRTUSInfoInRegistry()
		{
			Data.CreatePackingData();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			smartFreight.OH_Code = "SMART";
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var managerMock = new Mock<ICarrierLabelManager>();
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (ObjectFactory.Substitute(managerMock.Object))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(null);
				cancellation.SubscribePackageForCancellation(package);

				var responses = cancellation.CancelPackages(new[] { package });
				managerMock.Verify(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsAny<ITopLevelDataObject>(), It.IsAny<RTUSCBA>(), It.IsAny<Uri>()), Times.Never);

				var errorResponse = responses.Single();
				AssertEquals("Response from RTUS should be failure since there was no Carrier Booking Agent.", false, errorResponse.IsSuccessful);
				AssertEquals("Response from RTUS should be failure since there was no Carrier Booking Agent.",
					"Failed to cancel Package 'ABC' because no RTUS URL was provided for Organization 'SMART'.", errorResponse.ErrorMessageForFailure);
				AssertEquals("Correct Package Job should be associated with each response.", Data.PackageJob.PK, errorResponse.PackageJobPK);
				AssertEquals("Correct Packing Parent should be associated with each response.", Data.Dummy, errorResponse.PackingParent);
				AssertEquals("Correct Package ID should be associated with each response.", "ABC", errorResponse.PackageID);
			}
		}

		public void TestCancelPackages_UniversalShipmentGenerationFailed()
		{
			Data.CreatePackingData();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var rtusCollection = new OrganisationRTUSCollection();
			var rtusOption = rtusCollection.AddNew();
			rtusOption.CBACode = "SMA"; // SmartFreight
			rtusOption.OrganisationPK = smartFreight.PK;
			rtusOption.Url = SmartFreightUrl;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(null);

			var managerMock = new Mock<ICarrierLabelManager>();
			var mockHandle = new DummyHandle(() => throw new InvalidOperationException("Oops!"));

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			{
				cancellation.SubscribePackageForCancellation(package);
				var responses = cancellation.CancelPackages(new[] { package });
				AssertEquals("Failure of Universal Shipment generation is logged beforehand, should have no responses.", 0, responses.Count());
				managerMock.Verify(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsAny<ITopLevelDataObject>(), It.IsAny<RTUSCBA>(), It.IsAny<Uri>()), Times.Never);
			}
		}

		public void TestCancelPackages_MultiplePackagesCancelled()
		{
			var dummyParent1 = Factory.New<DummyWithPacking>();
			var dummyParent2 = Factory.New<DummyWithPacking>();
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(dummyParent1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(dummyParent2);

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			dummyParent1.CarrierBookingAgent = smartFreight;
			dummyParent2.CarrierBookingAgent = smartFreight;

			var package1 = packageJob1.Packages.AddNew("PLT", "ABC");
			var package2 = packageJob2.Packages.AddNew("PLT", "XYZ");
			Factory.Save();

			var rtusCollection = new OrganisationRTUSCollection();
			var rtusOption = rtusCollection.AddNew();
			rtusOption.CBACode = "SMA"; // SmartFreight
			rtusOption.OrganisationPK = smartFreight.PK;
			rtusOption.Url = SmartFreightUrl;

			ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(null);

			var managerMock = new Mock<ICarrierLabelManager>();
			var mockHandle = new DummyHandle(p =>
			{
				var dataContext = DataContextFactory.New();
				dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
				return p.KP_PackageID == "ABC"
					? new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { BookingConfirmationReference = "SUCCESS", DataContext = dataContext, }
					: new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { BookingConfirmationReference = "FAIL", DataContext = dataContext, };
			});

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, new[] { dummyParent1, dummyParent2 }))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			{
				cancellation.SubscribePackageForCancellation(package1);
				cancellation.SubscribePackageForCancellation(package2);

				var successResponse = new Mock<IRTUSResponse>();
				successResponse.SetupGet(r => r.IsSuccessful).Returns(true);

				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<UniversalShipment>(d => d.BookingConfirmationReference.GetValueOrDefault() == "SUCCESS"), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(successResponse.Object);

				var failResponse = new Mock<IRTUSResponse>();
				failResponse.SetupGet(r => r.IsSuccessful).Returns(false);
				failResponse.SetupGet(r => r.ErrorMessageForFailure).Returns("ERROR");
				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<UniversalShipment>(d => d.BookingConfirmationReference.GetValueOrDefault() == "FAIL"), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(failResponse.Object);

				var responses = cancellation.CancelPackages(new[] { package1, package2 });
				var response1 = responses.Single(r => r.IsSuccessful);
				AssertEquals("Correct Package Job should be associated with each response.", packageJob1.PK, response1.PackageJobPK);
				AssertEquals("Correct Packing Parent should be associated with each response.", dummyParent1, response1.PackingParent);
				AssertEquals("Correct Package ID should be associated with each response.", "ABC", response1.PackageID);

				var response2 = responses.Single(r => !r.IsSuccessful);
				AssertEquals("Correct Package Job should be associated with each response.", packageJob2.PK, response2.PackageJobPK);
				AssertEquals("Correct Packing Parent should be associated with each response.", dummyParent2, response2.PackingParent);
				AssertEquals("Correct Package ID should be associated with each response.", "XYZ", response2.PackageID);
				AssertEquals("Failure Error Message should be correct.", "ERROR", response2.ErrorMessageForFailure);
			}
		}

		[TestDate(2019, 04, 01)]
		public void TestCancelPackages_CheckCorrectRequestIsMade()
		{
			Data.CreatePackingData();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var rtusCollection = new OrganisationRTUSCollection();
			var rtusOption = rtusCollection.AddNew();
			rtusOption.CBACode = "SMA"; // SmartFreight
			rtusOption.OrganisationPK = smartFreight.PK;
			rtusOption.Url = SmartFreightUrl;

			var managerMock = new Mock<ICarrierLabelManager>();
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				var expectedBytes = GetBytesFromPackageUniversalShipment(package);
				var responseMock = new Mock<IRTUSResponse>();
				responseMock.SetupGet(r => r.IsSuccessful).Returns(true);

				Func<UniversalShipment, bool> funcWrapper = d => IsUniversalShipmentEqualToBytes(d, expectedBytes);
				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.Is<UniversalShipment>(d => funcWrapper(d)), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(responseMock.Object);

				ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(null);
				cancellation.SubscribePackageForCancellation(package);

				managerMock.Verify(m => m.Dispose(), Times.Never);

				var responses = cancellation.CancelPackages(new[] { package });
				var response = responses.Single();
				AssertEquals("Response from RTUS should be correct.", true, response.IsSuccessful);
				AssertEquals("Correct Package Job should be associated with each response.", Data.PackageJob.PK, response.PackageJobPK);
				AssertEquals("Correct Packing Parent should be associated with each response.", Data.Dummy, response.PackingParent);
				AssertEquals("Correct Package ID should be associated with each response.", "ABC", response.PackageID);

				managerMock.Verify(m => m.Dispose(), Times.Once);
				managerMock.VerifyAll();
			}

			bool IsUniversalShipmentEqualToBytes(UniversalShipment packageUniversalShipment, byte[] expectedBytes)
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					ObjectFactory.New<IXmlWriter>().WriteXML(packageUniversalShipment, stream);
					using (var streamReader = new StreamReader(stream))
					{
						return Encoding.UTF8.GetBytes(streamReader.ReadToEnd()).ElementsEqual(expectedBytes);
					}
				}
			}
		}

		[TestDate(2019, 04, 01)]
		public void TestCancelPackages_Integration()
		{
			Data.CreatePackingData();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var rtusCollection = new OrganisationRTUSCollection();
			var rtusOption = rtusCollection.AddNew();
			rtusOption.CBACode = "SMA"; // SmartFreight
			rtusOption.OrganisationPK = smartFreight.PK;
			rtusOption.Url = SmartFreightUrl;

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				var clientFactoryMock = new Mock<IHttpClientFactory>();

				ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(clientFactoryMock.Object);
				cancellation.SubscribePackageForCancellation(package);

				var expectedBytes = GetBytesFromPackageUniversalShipment(package);
				var responseMock = new Mock<IRTUSResponse>();
				responseMock.Setup(r => r.IsSuccessful).Returns(true);

				var rtusProcessorMock = new Mock<IRTUSProcessor>();
				rtusProcessorMock
					.Setup(p => p.PushMessage(RequestType.Cancellation, clientFactoryMock.Object, It.Is<SubStreamableStream>(s => ValidateBytes(s, expectedBytes)), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Returns(responseMock.Object);

				using (ObjectFactory.Substitute(rtusProcessorMock.Object))
				{
					var responses = cancellation.CancelPackages(new[] { package });
					var response = responses.Single();
					AssertEquals("Response from RTUS should be correct.", true, response.IsSuccessful);
					AssertEquals("Correct Package Job should be associated with each response.", Data.PackageJob.PK, response.PackageJobPK);
					AssertEquals("Correct Packing Parent should be associated with each response.", Data.Dummy, response.PackingParent);
					AssertEquals("Correct Package ID should be associated with each response.", "ABC", response.PackageID);

					rtusProcessorMock.VerifyAll();
				}
			}
		}

		bool ValidateBytes(SubStreamableStream stream, byte[] expectedBytes)
		{
			using (var streamReader = new StreamReader(stream))
			{
				return Encoding.UTF8.GetBytes(streamReader.ReadToEnd()).ElementsEqual(expectedBytes);
			}
		}

		public void TestCancelPackages_ParentShipmentGenerationIsCached()
		{
			Data.CreatePackingData();

			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var rtusCollection = new OrganisationRTUSCollection();
			var rtusOption = rtusCollection.AddNew();
			rtusOption.CBACode = "SMA"; // SmartFreight
			rtusOption.OrganisationPK = smartFreight.PK;
			rtusOption.Url = SmartFreightUrl;

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

			ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(null);
			var managerMock = new Mock<ICarrierLabelManager>();

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			using (ObjectFactory.Substitute(managerMock.Object))
			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			{
				var package1 = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				cancellation.SubscribePackageForCancellation(package1);

				parentShipment.BookingConfirmationReference = "NO WAY!";
				var package2 = Data.PackageJob.Packages.AddNew("PLT", "XYZ");
				cancellation.SubscribePackageForCancellation(package2);

				var parentShipmentsPassedIn = new List<ITopLevelDataObject>();
				managerMock.Setup(m => m.PushCarrierLabelRequest(RequestType.Cancellation, It.IsNotNull<UniversalShipment>(), RTUSCBA.SmartFreight, new Uri(SmartFreightUrl)))
					.Callback((RequestType request, ITopLevelDataObject shipmentDataObject, RTUSCBA type, Uri url) => parentShipmentsPassedIn.Add(shipmentDataObject))
					.Returns(new Mock<IRTUSResponse>().Object);

				cancellation.CancelPackages(new[] { package1, package2 });
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

#endregion

		#region TestSubscribePackageBeforeDelete

		public void TestSubscribePackageBeforeDelete_DoesNotAcceptInvalidArguments()
		{
			ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(null);
			AssertExceptionThrown<ArgumentNullException>(() => cancellation.SubscribePackageForCancellation(null));
		}

		public void TestSubscribePackageBeforeDelete_UniversalShipmentGenerationHandlesException()
		{
			Data.CreatePackingData();

			var exceptionToThrow = new InvalidOperationException("Oops!");
			var mockHandle = new DummyHandle(() => throw exceptionToThrow);
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");

				ICarrierLabelCancellation cancellation = new CarrierLabelCancellation(null);
				var result = cancellation.SubscribePackageForCancellation(package);
				AssertEquals("Should have handled the exception and return the exception message.", false, result.Success);
				AssertEquals("Should have handled the exception and return the exception message.", "Failed to generate Universal Shipment from Package 'ABC'.\r\nDetails below:\r\nOops!", result.Message);
			}
		}

		#endregion

		#region TestObjectFactoryCreation

		public void TestObjectFactoryCreation()
		{
			var cancellation = ObjectFactory.New<ICarrierLabelCancellation>(null, null);
			AssertType<CarrierLabelCancellation>(cancellation);
		}

		#endregion

		#region Implementation

		const string SmartFreightUrl = "https://wtg1.p1.app.smartfreight.com/smartfreight/api/ucp/sems/execute/N1ZVOmJ3VjNWcHJQNnVlMTlVUWhqXy1hQ05mdmZIT1ZYVXc3ZUE";

		static byte[] GetBytesFromPackageUniversalShipment(PkgPackage package)
		{
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, package));
				var expectedShipment = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, RequestType.Cancellation).GetDataObject(package);
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
