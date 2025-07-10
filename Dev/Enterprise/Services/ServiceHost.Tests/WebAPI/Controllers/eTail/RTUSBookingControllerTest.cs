using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Common;
using Enterprise.eTail.Integration;
#if NETFRAMEWORK
using System.Web.Http.Results;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class RTUSBookingControllerTest : TestCase
	{
		public void TestBookLastMileCarrier_SuccessfulWithPrinting()
		{
			var testPrinter = new Guid();
			var mockPrinter = GetPrinterMock(testPrinter, string.Empty);

			var successfulGuid = new Guid();

			var testData1 = new ReadOnlyCollection<byte>(Encoding.UTF8.GetBytes("I'm a label and I'm sticky"));
			var testData2 = new ReadOnlyCollection<byte>(Encoding.UTF8.GetBytes("I'm another label and I'm stickier"));

			var responseCollection = new LastMileCarrierBookingResponseCollectionForTest();
			responseCollection.Add(CreateResponse("PDF", "I'm hungry", true, testData1, "TrackingNumberForBooking"));
			responseCollection.Add(CreateResponse("PDF", "I'm hungry2", true, testData2, "TrackingNumberForBooking2"));

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.BookLastMileCarrier(successfulGuid, true))
				.Returns(responseCollection);

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			using (ObjectFactory.Substitute(mockPrinter.Object))
			{
				var result = controller.BookLastMileCarrier(successfulGuid, "Z0", testPrinter);
				AssertType<OkResult>(result);
				mockProvider.Verify(m => m.BookLastMileCarrier(successfulGuid, true));
			}
		}

		public void TestBookLastMileCarrier_SuccessfulWithoutPrinting()
		{
			var successfulGuid = new Guid();

			var testData1 = new ReadOnlyCollection<byte>(Encoding.UTF8.GetBytes("I'm a label and I'm sticky"));
			var testData2 = new ReadOnlyCollection<byte>(Encoding.UTF8.GetBytes("I'm another label and I'm stickier"));

			var responseCollection = new LastMileCarrierBookingResponseCollectionForTest();
			responseCollection.Add(CreateResponse("PDF", "I'm hungry", true, testData1, "TrackingNumberForBooking"));
			responseCollection.Add(CreateResponse("PDF", "I'm hungry2", true, testData2, "TrackingNumberForBooking2"));

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.BookLastMileCarrier(successfulGuid, true)).Returns(responseCollection);

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			{
				var result = controller.BookLastMileCarrier(Guid.Empty, "Z0", null);
				AssertType<OkResult>(result);
				mockProvider.Verify(m => m.BookLastMileCarrier(successfulGuid, true));
			}
		}

		public void TestBookLastMileCarrier_SuccessfulButBadLabel()
		{
			var guidForNoLableData = new Guid();
			var expectedContent = "Booking was successful but failed to get a valid label file for TrackingNumberForBooking.";

			var responseCollection = new LastMileCarrierBookingResponseCollectionForTest();
			responseCollection.Add(CreateResponse(string.Empty, expectedContent, true, null, "TrackingNumberForBooking"));

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.BookLastMileCarrier(guidForNoLableData, true)).Returns(responseCollection);

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			{
				controller.BookLastMileCarrier(guidForNoLableData, "Z0", Guid.NewGuid())
					.AssertResultContains(HttpStatusCode.OK, expectedContent);
				mockProvider.Verify(m => m.BookLastMileCarrier(guidForNoLableData, true));
			}
		}

		public void TestBookLastMileCarrier_FailedBooking()
		{
			var guidForFail = new Guid();
			var expectedContent = @"I'm hungry
I'm hungry2";

			var responseCollection = new LastMileCarrierBookingResponseCollectionForTest();
			responseCollection.Add(CreateResponse(string.Empty, "I'm hungry", false, null, "TrackingNumberForBooking"));
			responseCollection.Add(CreateResponse(string.Empty, "I'm hungry2", false, null, "TrackingNumberForBooking2"));

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.BookLastMileCarrier(guidForFail, true)).Returns(responseCollection);

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			{
				controller.BookLastMileCarrier(guidForFail, "Z0", Guid.NewGuid())
					.AssertResultContains(HttpStatusCode.InternalServerError, expectedContent);
				mockProvider.Verify(m => m.BookLastMileCarrier(guidForFail, true));
			}
		}

		public void TestBookLastMileCarrier_Partial_FailedBooking()
		{
			var expectedContent = "I'm hungry2";
			var guidForPartialFail = new Guid();

			var testData1 = new ReadOnlyCollection<byte>(Encoding.UTF8.GetBytes("I'm a label and I'm sticky"));
			var testData2 = new ReadOnlyCollection<byte>(Encoding.UTF8.GetBytes("I'm another label and I'm stickier"));

			var responseCollection = new LastMileCarrierBookingResponseCollectionForTest();
			responseCollection.Add(CreateResponse("PDF", "I'm hungry", true, testData1, "TrackingNumberForBooking"));
			responseCollection.Add(CreateResponse("PDF", "I'm hungry2", false, testData2, "TrackingNumberForBooking2"));

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.BookLastMileCarrier(guidForPartialFail, true)).Returns(responseCollection);

			var printerPK = new Guid();
			var mockPrinter = GetPrinterMock(printerPK, string.Empty);

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			using (ObjectFactory.Substitute(mockPrinter.Object))
			{
				controller.BookLastMileCarrier(guidForPartialFail, "Z0", printerPK)
					.AssertResultContains(HttpStatusCode.InternalServerError, expectedContent);
				mockProvider.Verify(m => m.BookLastMileCarrier(guidForPartialFail, true));
			}
		}

		public void TestBookLastMileCarrier_Partial_FailedBooking_WithBadPrinter()
		{
			var printerPK = new Guid();
			var printerErrorMessage = "Cannot find printer [" + printerPK.ToString() + "]";
			var mockPrinter = GetPrinterMock(printerPK, printerErrorMessage, false);

			var expectedContent = @"I'm hungry2
Booking was successful but failed to print label: Cannot find printer [" + printerPK.ToString() + "]";
			var guidForPartialFailWithBadPrinter = new Guid();

			var testData1 = new ReadOnlyCollection<byte>(Encoding.UTF8.GetBytes("I'm a label and I'm sticky"));
			var testData2 = new ReadOnlyCollection<byte>(Encoding.UTF8.GetBytes("I'm another label and I'm stickier"));

			var responseCollection = new LastMileCarrierBookingResponseCollectionForTest();
			responseCollection.Add(CreateResponse("PDF", "I'm hungry", true, testData1, "TrackingNumberForBooking"));
			responseCollection.Add(CreateResponse("PDF", "I'm hungry2", false, testData2, "TrackingNumberForBooking2"));

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.BookLastMileCarrier(guidForPartialFailWithBadPrinter, true)).Returns(responseCollection);

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			using (ObjectFactory.Substitute(mockPrinter.Object))
			{
				controller.BookLastMileCarrier(guidForPartialFailWithBadPrinter, "Z0", printerPK)
					.AssertResultContains(HttpStatusCode.InternalServerError, expectedContent);
				mockProvider.Verify(m => m.BookLastMileCarrier(guidForPartialFailWithBadPrinter, true));
			}
		}

		public void TestBookLastMileCarrier_Partial_FailedBooking_WithBadLabel()
		{
			var printerPK = new Guid();
			var mockPrinter = GetPrinterMock(printerPK, string.Empty);

			var expectedContent = @"I'm hungry2
Booking was successful but failed to get a valid label file for TrackingNumberForBooking.";
			var guidForPartialFailWithBadLabel = new Guid();

			var responseCollection = new LastMileCarrierBookingResponseCollectionForTest();
			responseCollection.Add(CreateResponse(string.Empty, "I'm hungry", true, null, "TrackingNumberForBooking"));
			responseCollection.Add(CreateResponse(string.Empty, "I'm hungry2", false, null, "TrackingNumberForBooking2"));

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.BookLastMileCarrier(guidForPartialFailWithBadLabel, true)).Returns(responseCollection);

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			using (ObjectFactory.Substitute(mockPrinter.Object))
			{
				controller.BookLastMileCarrier(guidForPartialFailWithBadLabel, "Z0", Guid.NewGuid())
					.AssertResultContains(HttpStatusCode.InternalServerError, expectedContent);
				mockProvider.Verify(m => m.BookLastMileCarrier(guidForPartialFailWithBadLabel, true));
			}
		}

		public void TestBookLastMileCarrier_NullResponse()
		{
			var expectedContent = "Booking failed: booking service returned an empty response";
			var guidForNullResponse = new Guid();

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.BookLastMileCarrier(guidForNullResponse, true))
				.Returns((ILastMileCarrierBookingResponseCollection)null);

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			{
				controller.BookLastMileCarrier(guidForNullResponse, "Z0", Guid.NewGuid())
					.AssertResultContains(HttpStatusCode.InternalServerError, expectedContent);
				mockProvider.Verify(m => m.BookLastMileCarrier(guidForNullResponse, true));
			}
		}

		public void TestBookLastMileCarrier_FailedToFindProvider()
		{
			var expectedContent = "Unexpected error happened: Cannot get RTUS booking provider for table [Z1].";

			using (SubstituteRTUSBookingProvider("Z0", null))
			{
				controller.BookLastMileCarrier(Guid.Empty, "Z1", Guid.NewGuid())
					.AssertResultContains(HttpStatusCode.InternalServerError, expectedContent);
			}
		}

		public void TestBookLastMileCarrier_Exception()
		{
			var expectedContent = "Unexpected error happened: Boom!";
			var guidForException = new Guid();
			var mockProvider = new Mock<ILastMileCarrierBookingService>();

			mockProvider.Setup(m => m.BookLastMileCarrier(guidForException, true))
				.Throws(new InvalidOperationException("Boom!"));

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			{
				controller.BookLastMileCarrier(guidForException, "Z0", Guid.NewGuid())
					.AssertResultContains(HttpStatusCode.InternalServerError, expectedContent);
				mockProvider.Verify(v => v.BookLastMileCarrier(guidForException, true));
			}
		}

		public void TestCancelBooking_Successful()
		{
			var successfulGuid = new Guid();

			var responseCollection = new LastMileCarrierBookingResponseCollectionForTest();
			var response = CreateResponse("PDF", "", true, null, "TrackingNumberForBooking");
			responseCollection.Add(response);

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.CancelBooking(successfulGuid))
				.Returns(responseCollection);

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			{
				var result = controller.CancelBooking(Guid.Empty, "Z0");
				AssertType<OkResult>(result);
				mockProvider.Verify(m => m.CancelBooking(successfulGuid));
			}
		}

		public void TestCancelBooking_Failed()
		{
			var expectedContent = "I'm tired";
			var guidForFail = new Guid();

			var responseCollection = new LastMileCarrierBookingResponseCollectionForTest();
			var response = CreateResponse("PDF", expectedContent, false, null, "TrackingNumberForBooking");
			responseCollection.Add(response);

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.CancelBooking(guidForFail))
				.Returns(responseCollection);

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			{
				controller.CancelBooking(guidForFail, "Z0")
					.AssertResultContains(HttpStatusCode.InternalServerError, expectedContent);
				mockProvider.Verify(m => m.CancelBooking(guidForFail));
			}
		}

		public void TestCancelBooking_NullResponse()
		{
			var expectedContent = "Canceling failed: cancel booking service returned an empty response";
			var guidForNullResponse = new Guid();

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.CancelBooking(guidForNullResponse))
				.Returns((ILastMileCarrierBookingResponseCollection)null);

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			{
				controller.CancelBooking(guidForNullResponse, "Z0")
					.AssertResultContains(HttpStatusCode.InternalServerError, expectedContent);

				mockProvider.Verify(m => m.CancelBooking(guidForNullResponse));
			}
		}

		public void TestCancelBooking_Exception()
		{
			var expectedContent = "Unexpected error happened: Boom!";
			var guidForException = new Guid();

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.CancelBooking(guidForException))
				.Throws(new InvalidOperationException("Boom!"));

			using (SubstituteRTUSBookingProvider("Z0", mockProvider.Object))
			{
				controller.CancelBooking(guidForException, "Z0")
					.AssertResultContains(HttpStatusCode.InternalServerError, expectedContent);

				mockProvider.Verify(m => m.CancelBooking(guidForException));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			controller = ControllerHelper.GetController<RTUSBookingController>();
		}

		RTUSBookingController controller;

		IDisposable SubstituteRTUSBookingProvider(string key, ILastMileCarrierBookingService provider)
		{
			var bookingProviders = new KeyObjectHandleDictionaryObject
			{
				{ key, new TestObjectHandle(provider) }
			};

			return ObjectFactory.Substitute("RTUSBookingProviders", bookingProviders);
		}

		LastMileCarrierBookingResponseForTest CreateResponse(string fileType, string error, bool successful, ReadOnlyCollection<byte> binaryData, string trackingNumber)
		{
			var response = new LastMileCarrierBookingResponseForTest();
			response.FileType = fileType;
			response.ErrorMessage = error;
			response.Successful = successful;
			response.BinaryData = binaryData;
			response.TrackingNumber = trackingNumber;

			return response;
		}

		Mock<ILabelPrintingService> GetPrinterMock(Guid printerPK, string errorMesssage, bool successful = true)
		{
			var mockPrinter = new Mock<ILabelPrintingService>();

			mockPrinter.Setup(m => m.TryPrintLabel(It.IsAny<byte[]>(), It.IsAny<string>(), printerPK, out errorMesssage))
				.Returns(successful);

			return mockPrinter;
		}
	}

	class LastMileCarrierBookingResponseCollectionForTest : List<ILastMileCarrierBookingResponse>, ILastMileCarrierBookingResponseCollection
	{
		public bool HasError => this.Any(i => !i.Successful);

		public string ErrorMessage
		{
			get
			{
				var failedResponses = this.Where(i => !i.Successful);
				var sb = new StringBuilder();

				failedResponses?.ForEach((r) =>
				{
					if (!string.IsNullOrWhiteSpace(r.ErrorMessage))
					{
						sb.AppendLine(r.ErrorMessage.Trim());
					}
				});

				return sb.ToString().Trim();
			}
		}
	}

	class LastMileCarrierBookingResponseForTest : ILastMileCarrierBookingResponse
	{
		public bool Successful { get; set; }
		public ReadOnlyCollection<byte> BinaryData { get; set; }
		public string FileType { get; set; }
		public string TrackingNumber { get; set; }
		public string ErrorMessage { get; set; }
	}
}
