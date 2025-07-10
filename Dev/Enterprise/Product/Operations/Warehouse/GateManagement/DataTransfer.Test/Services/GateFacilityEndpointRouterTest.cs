using System;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Application.Testing;
using Enterprise.Warehouse.GateManagement.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	public class GateFacilityEndpointRouterTest : TestCase
	{
		public void TestValidateBooking_WhenNoFacilitySetup_ThrowsHttpNotFound()
		{
			using (SetupFactory("TWH", out var facilityImplementation))
			{
				var request = new GateBookingValidationRequest()
				{
					FacilityTypeCode = "CYD"
				};
				AssertExceptionThrown<HttpResponseException>(() => GateFacilityEndpointRouter.ValidateFacilityBooking(request));
			}
		}

		public void TestValidateBooking_WhenFacilitySetup_CallsFacilityMethod()
		{
			using (SetupFactory("TWH", out var facilityImplementation))
			{
				var request = new GateBookingValidationRequest()
				{
					FacilityTypeCode = "TWH"
				};
				facilityImplementation.Setup(x => x.ValidateBooking(It.IsAny<GateBookingValidationRequest>())).Returns(new Mock<IGateBookingValidationResponse>().Object);
				GateFacilityEndpointRouter.ValidateFacilityBooking(request);
				AssertNoExceptionThrown(() => facilityImplementation.Verify(x => x.ValidateBooking(It.IsAny<GateBookingValidationRequest>()), Times.Once));
			}
		}

		IDisposable SetupFactory(string facilityCode, out Mock<GateManagementFacilityEndpointManager> facilityImplementation)
		{
			facilityImplementation = new Mock<GateManagementFacilityEndpointManager>();
			var mockGateManagementFacilityEndpointManagerList = new KeyObjectHandleDictionaryObject
			{
				{ facilityCode, new TestObjectHandle(facilityImplementation?.Object) }
			};

			return ObjectFactory.Substitute("GateManagementFacilityEndpointManagerList", mockGateManagementFacilityEndpointManagerList);
		}
	}
}
