using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Application;
using CargoWise.Application.Testing;
using Enterprise.Warehouse.GateManagement.DataTransfer;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Yard.DataTransfer;
using Enterprise.Warehouse.Yard.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	sealed class GateControllerTest : TestCase
	{
		#region Validate Booking - Container Yard

		public void TestValidateDropoffContainerYard_Failed_ShouldReturnBadRequestError()
		{
			TestCaseValidateBookingContainerYard<IYardDropoffValidationProvider, IYardDropoffRequestValidator>(
				controller => controller.ValidateBookingContainerYard(new GateBookingValidationRequest(), "dropoff"),
				false,
				AssertType<BadRequestErrorMessageResult>);
		}

		public void TestValidatePickupContainerYard_Failed_ShouldReturnBadRequestError()
		{
			TestCaseValidateBookingContainerYard<IYardPickupValidationProvider, IYardPickupRequestValidator>(
				controller => controller.ValidateBookingContainerYard(new GateBookingValidationRequest(), "pickup"),
				false,
				AssertType<BadRequestErrorMessageResult>);
		}

		public void TestValidateDropoffContainerYard_Successful_ShouldReturnJsonResponse()
		{
			TestCaseValidateBookingContainerYard<IYardDropoffValidationProvider, IYardDropoffRequestValidator>(
				controller => controller.ValidateBookingContainerYard(new GateBookingValidationRequest(), "dropoff"),
				true,
				(IHttpActionResult result) =>
				{
					var castResult = result as JsonResult<IGateBookingValidationResponse>;
					Assert(typeof(IYardValidationData).IsInstanceOfType(castResult.Content));
				});
		}

		public void TestValidatePickupContainerYard_Successful_ShouldReturnJsonResponse()
		{
			TestCaseValidateBookingContainerYard<IYardPickupValidationProvider, IYardPickupRequestValidator>(
				controller => controller.ValidateBookingContainerYard(new GateBookingValidationRequest(), "pickup"),
				true,
				(IHttpActionResult result) =>
				{
					var castResult = result as JsonResult<IGateBookingValidationResponse>;
					Assert(typeof(IYardValidationData).IsInstanceOfType(castResult.Content));
				});
		}

		void TestCaseValidateBookingContainerYard<TProvider, TValidator>(Func<GateController, IHttpActionResult> validateFunc, bool valid, Action<IHttpActionResult> expectedAssertion)
			where TProvider : class, IYardValidationProvider
			where TValidator : class, IYardValidationRequestValidator
		{
			var mockValidationProvider = new Mock<TProvider>();
			mockValidationProvider
				.Setup(x => x.Get(It.IsAny<IYardDropoffRequest>()))
				.Returns(() => new Mock<YardDropoffData>().Object);
			mockValidationProvider
				.Setup(x => x.Get(It.IsAny<IYardPickupRequest>()))
				.Returns(() => new Mock<YardPickupData>().Object);

			var mockRequestValidator = new Mock<TValidator>();
			string message = string.Empty;
			mockRequestValidator
				.Setup(x => x.IsValid(It.IsAny<IYardValidationRequest>(), out message))
				.Returns(() => valid);

			using (ObjectFactory.Substitute(mockRequestValidator.Object))
			using (ObjectFactory.Substitute(mockValidationProvider.Object))
			{
				using var httpRequest = new HttpRequestMessage();
				using var controller = new GateController();
				controller.Request = httpRequest;
				var result = validateFunc.Invoke(controller);
				expectedAssertion.Invoke(result);
			}
		}

		#endregion

		#region Validate Booking - Transit Warehouse

		public void TestValidateDeliveryTransitWarehouse_Failed_ShouldReturnBadRequestError()
		{
			TestCaseValidateBookingTransitWarehouse<ITWHDeliveryValidationProvider, ITWHValidationRequestValidator>(
				controller => controller.ValidateFacilityBooking(new GateBookingValidationRequest(), "delivery", "twh"),
				false,
				AssertType<BadRequestErrorMessageResult>);
		}

		public void TestValidatePickupTransitWarehouse_Failed_ShouldReturnBadRequestError()
		{
			TestCaseValidateBookingTransitWarehouse<ITWHPickupValidationProvider, ITWHValidationRequestValidator>(
				controller => controller.ValidateFacilityBooking(new GateBookingValidationRequest(), "pickup", "twh"),
				false,
				AssertType<BadRequestErrorMessageResult>);
		}

		public void TestValidateDeliveryTransitWarehouse_Successful_ShouldReturnJsonResponse()
		{
			TestCaseValidateBookingTransitWarehouse<ITWHDeliveryValidationProvider, ITWHValidationRequestValidator>(
				controller => controller.ValidateFacilityBooking(new GateBookingValidationRequest(), "delivery", "twh"),
				true,
				(IHttpActionResult result) =>
				{
					var castResult = result as JsonResult<IGateBookingValidationResponse>;
					Assert(typeof(ITWHValidationResponse).IsInstanceOfType(castResult.Content));
				});
		}

		public void TestValidatePickupTransitWarehouse_Successful_ShouldReturnJsonResponse()
		{
			TestCaseValidateBookingTransitWarehouse<ITWHPickupValidationProvider, ITWHValidationRequestValidator>(
				controller => controller.ValidateFacilityBooking(new GateBookingValidationRequest(), "pickup", "twh"),
				true,
				(IHttpActionResult result) =>
				{
					var castResult = result as JsonResult<IGateBookingValidationResponse>;
					Assert(typeof(ITWHValidationResponse).IsInstanceOfType(castResult.Content));
				});
		}

		void TestCaseValidateBookingTransitWarehouse<TProvider, TValidator>(Func<GateController, IHttpActionResult> validateFunc, bool valid, Action<IHttpActionResult> expectedAssertion)
			where TProvider : class, ITWHValidationProvider
			where TValidator : class, ITWHValidationRequestValidator
		{
			var mockValidationProvider = new Mock<TProvider>();
			mockValidationProvider
				.Setup(x => x.Get(It.IsAny<ITWHValidationRequest>()))
				.Returns(() => new Mock<TWHValidationResponse>().Object);
			mockValidationProvider
				.Setup(x => x.Get(It.IsAny<ITWHValidationRequest>()))
				.Returns(() => new Mock<TWHValidationResponse>().Object);

			var mockRequestValidator = new Mock<TValidator>();
			string message = string.Empty;
			mockRequestValidator
				.Setup(x => x.IsValid(It.IsAny<ITWHValidationRequest>(), out message))
				.Returns(() => valid);

			using (ObjectFactory.Substitute(mockRequestValidator.Object))
			using (ObjectFactory.Substitute(mockValidationProvider.Object))
			{
				using var httpRequest = new HttpRequestMessage();
				using var controller = new GateController();
				controller.Request = httpRequest;
				var result = validateFunc.Invoke(controller);
				expectedAssertion.Invoke(result);
			}
		}

		#endregion

		#region Validate Booking - Generic Facility

		public void TestValidateBooking_NotDropoffOrPickup_ReturnsNotFoundResult()
		{
			var controller = new GateController();
			var twhEndpointManagerMock = new Mock<GateManagementFacilityEndpointManager>();
			twhEndpointManagerMock.Setup(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>())).Returns(new Mock<IGateBookingValidationResponse>().Object);
			var mockGateManagementFacilityEndpointManagerList = new KeyObjectHandleDictionaryObject
			{
				{ "TWH", new TestObjectHandle(twhEndpointManagerMock.Object) }
			};

			using (ObjectFactory.Substitute("GateManagementFacilityEndpointManagerList", mockGateManagementFacilityEndpointManagerList))
			{
				var response = controller.ValidateFacilityBooking(new GateBookingValidationRequest(), "invalid", "twh");
				AssertType<NotFoundResult>(response);
				twhEndpointManagerMock.Verify(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>()), Times.Never, "Should not reach facility method");
			}
		}

		public void TestValidateBooking_DropoffWithInvalidFacility_ReturnsNotFoundResult()
		{
			var controller = new GateController();
			var twhEndpointManagerMock = new Mock<GateManagementFacilityEndpointManager>();
			twhEndpointManagerMock.Setup(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>())).Returns(new Mock<IGateBookingValidationResponse>().Object);
			var mockGateManagementFacilityEndpointManagerList = new KeyObjectHandleDictionaryObject();

			using (ObjectFactory.Substitute("GateManagementFacilityEndpointManagerList", mockGateManagementFacilityEndpointManagerList))
			{
				var response = controller.ValidateFacilityBooking(new GateBookingValidationRequest(), "dropoff", "twh");
				AssertType<NotFoundResult>(response);
				twhEndpointManagerMock.Verify(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>()), Times.Never, "Should not reach facility method");
			}
		}

		public void TestValidateBooking_PickupWithInvalidFacility_ReturnsNotFoundResult()
		{
			var controller = new GateController();
			var twhEndpointManagerMock = new Mock<GateManagementFacilityEndpointManager>();
			twhEndpointManagerMock.Setup(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>())).Returns(new Mock<IGateBookingValidationResponse>().Object);
			var mockGateManagementFacilityEndpointManagerList = new KeyObjectHandleDictionaryObject();

			using (ObjectFactory.Substitute("GateManagementFacilityEndpointManagerList", mockGateManagementFacilityEndpointManagerList))
			{
				var response = controller.ValidateFacilityBooking(new GateBookingValidationRequest(), "pickup", "twh");
				AssertType<NotFoundResult>(response);
				twhEndpointManagerMock.Verify(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>()), Times.Never, "Should not reach facility method");
			}
		}

		public void TestValidateBooking_DropoffWithValidFacilityNotImplementingMethod_ReturnsNotFound()
		{
			var controller = new GateController();
			var twhEndpointManagerMock = new Mock<GateManagementFacilityEndpointManager>();
			twhEndpointManagerMock.Setup(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>()))
				.Returns((GateBookingValidationRequest req) => throw new NotImplementedException());
			var mockGateManagementFacilityEndpointManagerList = new KeyObjectHandleDictionaryObject
			{
				{ "TWH", new TestObjectHandle(twhEndpointManagerMock.Object) }
			};

			using (ObjectFactory.Substitute("GateManagementFacilityEndpointManagerList", mockGateManagementFacilityEndpointManagerList))
			{
				var response = controller.ValidateFacilityBooking(new GateBookingValidationRequest(), "dropoff", "twh");
				AssertType<NotFoundResult>(response);
				twhEndpointManagerMock.Verify(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>()), Times.Once, "Should reach facility method");
			}
		}

		public void TestValidateBooking_PickupWithValidFacilityNotImplementingMethod_ReturnsNotFound()
		{
			var controller = new GateController();
			var twhEndpointManagerMock = new Mock<GateManagementFacilityEndpointManager>();
			twhEndpointManagerMock.Setup(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>()))
				.Returns((GateBookingValidationRequest req) => throw new NotImplementedException());
			var mockGateManagementFacilityEndpointManagerList = new KeyObjectHandleDictionaryObject
			{
				{ "TWH", new TestObjectHandle(twhEndpointManagerMock.Object) }
			};

			using (ObjectFactory.Substitute("GateManagementFacilityEndpointManagerList", mockGateManagementFacilityEndpointManagerList))
			{
				var response = controller.ValidateFacilityBooking(new GateBookingValidationRequest(), "pickup", "twh");
				AssertType<NotFoundResult>(response);
				twhEndpointManagerMock.Verify(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>()), Times.Once, "Should reach facility method");
			}
		}

		public void TestValidateBooking_DropoffWithValidFacility_CallsFacilityMethod()
		{
			var controller = new GateController();
			var twhEndpointManagerMock = new Mock<GateManagementFacilityEndpointManager>();
			twhEndpointManagerMock.Setup(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>())).Returns(new Mock<IGateBookingValidationResponse>().Object);
			var mockGateManagementFacilityEndpointManagerList = new KeyObjectHandleDictionaryObject
			{
				{ "TWH", new TestObjectHandle(twhEndpointManagerMock.Object) }
			};

			using (ObjectFactory.Substitute("GateManagementFacilityEndpointManagerList", mockGateManagementFacilityEndpointManagerList))
			{
				var response = controller.ValidateFacilityBooking(new GateBookingValidationRequest(), "dropoff", "twh");
				AssertType<JsonResult<IGateBookingValidationResponse>>(response);
				twhEndpointManagerMock.Verify(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>()), Times.Once, "Should reach facility method");
			}
		}

		public void TestValidateBooking_PickupWithValidFacility_CallsFacilityMethod()
		{
			var controller = new GateController();
			var twhEndpointManagerMock = new Mock<GateManagementFacilityEndpointManager>();
			twhEndpointManagerMock.Setup(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>())).Returns(new Mock<IGateBookingValidationResponse>().Object);
			var mockGateManagementFacilityEndpointManagerList = new KeyObjectHandleDictionaryObject
			{
				{ "TWH", new TestObjectHandle(twhEndpointManagerMock.Object) }
			};

			using (ObjectFactory.Substitute("GateManagementFacilityEndpointManagerList", mockGateManagementFacilityEndpointManagerList))
			{
				var response = controller.ValidateFacilityBooking(new GateBookingValidationRequest(), "pickup", "twh");
				AssertType<JsonResult<IGateBookingValidationResponse>>(response);
				twhEndpointManagerMock.Verify(x => x.ValidateBooking(It.IsAny<IGateBookingValidationRequest>()), Times.Once, "Should reach facility method");
			}
		}

		#endregion
	}
}
