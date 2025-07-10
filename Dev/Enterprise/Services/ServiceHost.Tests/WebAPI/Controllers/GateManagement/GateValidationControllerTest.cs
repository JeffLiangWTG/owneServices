using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.GateManagement.Integration.Interfaces;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.Tests
{
	class GateValidationControllerTest : TestCaseWithFactory
	{
		public void TestControllerUsesGlowTicketAuthentication()
		{
			var glowAuthenticationAttribute = controller.GetType()
				.GetCustomAttributes(typeof(GlowTicketAuthenticationAttribute), inherit: true).FirstOrDefault() as GlowTicketAuthenticationAttribute;

			AssertNotNull(glowAuthenticationAttribute);
		}

		#region	Test ValidateMovement

		public void TestValidateGVM_AccessUserIsStaff()
		{
			SetUpContactPrincipal();

			var response = controller.ValidateGateInMovement(gvm.PK.ToGuid()).ExecuteAsync(CancellationToken.None).Result;
			AssertEquals("Access Forbidden - Must be staff user", response.StatusCode, HttpStatusCode.Forbidden);
		}

		public void TestValidateGVM_GivenGVMDoesNotExist_ReturnValidationFailedMessage()
		{
			var response = controller.ValidateGateInMovement(new Guid()).ExecuteAsync(CancellationToken.None).Result;
			var result = JsonConvert.DeserializeObject<GateInValidationResponse>(response.Content.ReadAsStringAsync().Result);

			AssertEquals("Expected gate-in validation to fail", result.GateInValidationPassed, false);
			AssertEquals("Expected gate-in validation failed message", result.ValidationErrorMessage, "This vehicle movement does not exist");
		}

		public void TestValidateGVM_GivenNoValidationServiceFound_ReturnsMessage()
		{
			var mockGateFacilityValidationServiceList = new KeyObjectHandleDictionaryObject { };
			using (ObjectFactory.Substitute("GateFacilityValidationServiceList", mockGateFacilityValidationServiceList))
			{
				var response = controller.ValidateGateInMovement(gvm.PK.ToGuid()).ExecuteAsync(CancellationToken.None).Result;
				var result = JsonConvert.DeserializeObject<GateInValidationResponse>(response.Content.ReadAsStringAsync().Result);

				AssertEquals("Expected gate-in validation to pass", result.GateInValidationPassed, true);
				AssertEquals("Expected gate-in validation message", result.ValidationErrorMessage, "There is no validation for this facility type");
			}
		}

		public void TestValidateGVM_GivenHasValidationErrors_ReturnsAllErrorMessages()
		{
			var cydValidationServiceMock = new Mock<IGateFacilityValidationService>();
			var trwValidationServiceMock = new Mock<IGateFacilityValidationService>();

			var cydValidationError = "Failed Container Yard Validation";
			cydValidationServiceMock
				.Setup(x => x.ValidateMovement(It.IsAny<ITopLevelDataObject>(), It.IsAny<String>()))
				.Returns([cydValidationError]);

			var trwValidationError = "Failed Transit Warehouse Validation";
			trwValidationServiceMock
				.Setup(x => x.ValidateMovement(It.IsAny<ITopLevelDataObject>(), It.IsAny<String>()))
				.Returns([trwValidationError]);

			var mockGateFacilityValidationServiceList = new KeyObjectHandleDictionaryObject
			{
				{ WarehouseTypes.Codes.ContainerYard, new TestObjectHandle(cydValidationServiceMock.Object) },
				{ WarehouseTypes.Codes.Transit, new TestObjectHandle(trwValidationServiceMock.Object) }
			};

			using (ObjectFactory.Substitute("GateFacilityValidationServiceList", mockGateFacilityValidationServiceList))
			{
				var httpResponse = controller.ValidateGateInMovement(gvm.PK.ToGuid()).ExecuteAsync(CancellationToken.None).Result;
				var gateInValidationResponse = JsonConvert.DeserializeObject<GateInValidationResponse>(httpResponse.Content.ReadAsStringAsync().Result);

				AssertType<GateInValidationResponse>(gateInValidationResponse);
				AssertEquals(gateInValidationResponse.ValidationErrorMessage, string.Join(System.Environment.NewLine, new List<string> { trwValidationError, cydValidationError }.Select(s => " - " + s)));
			}
		}

		public void TestValidateGVM_GivenNoErrors_ReturnsOk()
		{
			var cydValidationServiceMock = new Mock<IGateFacilityValidationService>();
			var trwValidationServiceMock = new Mock<IGateFacilityValidationService>();

			cydValidationServiceMock
				.Setup(x => x.ValidateMovement(It.IsAny<ITopLevelDataObject>(), It.IsAny<String>()))
				.Returns([]);

			trwValidationServiceMock
				.Setup(x => x.ValidateMovement(It.IsAny<ITopLevelDataObject>(), It.IsAny<String>()))
				.Returns([]);

			var mockGateFacilityValidationServiceList = new KeyObjectHandleDictionaryObject
			{
				{ WarehouseTypes.Codes.ContainerYard, new TestObjectHandle(cydValidationServiceMock.Object) },
				{ WarehouseTypes.Codes.Transit, new TestObjectHandle(trwValidationServiceMock.Object) }
			};

			using (ObjectFactory.Substitute("GateFacilityValidationServiceList", mockGateFacilityValidationServiceList))
			{
				var response = controller.ValidateGateInMovement(gvm.PK.ToGuid());
				response.AssertResultContains(HttpStatusCode.OK);
			}
		}

		#endregion

		void SetUpStaffPrincipal()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
		}

		void SetUpContactPrincipal()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);
		}

		protected override void SetUp()
		{
			base.SetUp();
			gvm = Factory.New<IGteVehicleMovement>();
			gvm.GVM_VehicleRegistration = "0123456789";

			AddFacilityTypeToGVM(gvm, WarehouseTypes.Codes.ContainerYard, "WW1");
			AddFacilityTypeToGVM(gvm, WarehouseTypes.Codes.Transit, "WW2");

			Factory.Save();

			controller = new GateValidationController();
			controller.Request = new HttpRequestMessage();
			controller.Request.SetConfiguration(new HttpConfiguration());
			SetUpStaffPrincipal();
		}

		void AddFacilityTypeToGVM(IGteVehicleMovement gvm, string facilityType, string facilityID)
		{
			var whs = Factory.New<IWhsWarehouse>();
			((BusinessObject)whs).FillWithValidTestData();
			whs.WW_WarehouseType = facilityType;
			whs.WW_WarehouseCode = facilityID;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var gbk = Factory.New<IGteBooking>();
			gbk.GBK_WW_Facility = whs.PK;
			gbk.GBK_BookingType = "ADH";
			gbk.GBK_OH_TransportCompany = org.PK;

			var gbm = Factory.New<IGteGateMovementBooking>();
			gbm.GBM_GBK_Booking = gbk.PK;

			var ggm = Factory.New<IGteGateMovement>();
			ggm.GGM_GBM_MovementBooking = gbm.PK;
			ggm.GGM_GVM_VehicleMovement = gvm.PK;

			gvm.GVM_GBK_MainBooking = gbk.PK;
		}

		protected override void TearDown()
		{
			base.TearDown();
			controller?.Dispose();
		}

		IGteVehicleMovement gvm;
		GateValidationController controller;
	}
}
