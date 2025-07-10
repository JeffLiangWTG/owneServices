using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Services.ServiceHost.GMD;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost.Tests.HttpXmlServices.GenericMessageDelivery
{
	class GenericMessageDeliveryControllerTest : TestCaseWithFactory
	{
		public void TestRouteAttributes()
		{
			var type = typeof(GenericMessageDeliveryController);
			var classAttributes = type.GetCustomAttributes(false);
			var authorizeAttribute = classAttributes.OfType<AuthorizeAttribute>().SingleOrDefault();
			var eHubIdentityBasicAuthenticationAttribute = classAttributes.OfType<eHubIdentityBasicAuthenticationAttribute>().SingleOrDefault();
			var authenticationAttribute = classAttributes.OfType<GMDAuthenticationAttribute>().SingleOrDefault();
			var routeAttribute = classAttributes.OfType<RoutePrefixAttribute>().SingleOrDefault();
			AssertNotNull(authorizeAttribute);
			AssertNotNull(eHubIdentityBasicAuthenticationAttribute);
			AssertNotNull(authenticationAttribute);
			AssertEquals("GenericMessageDelivery", routeAttribute?.Prefix);

			var createInterchangeMethodAttributes = type.GetMethod(nameof(GenericMessageDeliveryController.CreateInterchange))?.GetCustomAttributes(typeof(RouteAttribute), false);
			AssertEquals(1, createInterchangeMethodAttributes.Length);
			AssertEquals("create-interchange", ((RouteAttribute)createInterchangeMethodAttributes[0]).Template);
		}

		public void TestDefaultRouteResponse()
		{
			var controller = new GenericMessageDeliveryController();
			var response = controller.Get() as OkNegotiatedContentResult<string>;
			AssertEquals("Welcome to the Generic Message Delivery Service", response?.Content);
		}

		public void TestCreateInterchangeWithValidInterchange()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DNA";
			company.Branches.AddNew();
			Factory.Save();

			var interchange = new Interchange
			{
				SenderId = "EASYLOG2TEST_EAD",
				RecipientId = "HYEDNACMT",
				InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse,
				Body = "<Message>TEST</Message>"
			};
			var controller = new GenericMessageDeliveryController();
			var response = controller.CreateInterchange(interchange) as JsonResult<InterchangeCreateResponse>;
			var result = response?.Content;

			AssertEquals("accept", result.Result);
			AssertNotNullOrEmpty(result.InterchangeNumber);

			var query = new ZQuery();
			query.AddToFilter(EDIInterchangeSchema.EI_GB, company.Branches[0].PK);
			query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery);
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse);
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, result.InterchangeNumber);
			var savedInterchange = Factory.LoadTop1<EDIInterchange>(query);

			AssertEquals(EDIInterchange.Direction.Receive, savedInterchange.EI_ReceiveTransmit);
			AssertEquals("EASYLOG2TEST_EAD", savedInterchange.EI_From);
			AssertEquals("HYEDNACMT", savedInterchange.EI_To);
			AssertEquals(EDIInterchangeStatusList.Codes.Queued, savedInterchange.EI_Status);
			AssertEquals("<Message>TEST</Message>", savedInterchange.EI_BodyText);
		}

		public void TestCreateInterchangeWithInvalidInterchange()
		{
			var controller = new GenericMessageDeliveryController();
			var response = controller.CreateInterchange(null) as BadRequestErrorMessageResult;
			AssertEquals("Missing interchange details", response?.Message);

			var interchange = new Interchange
			{
				SenderId = "EASYLOG2TEST_EAD",
				RecipientId = "",
				InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse,
				Body = "<Message>TEST</Message>"
			};
			response = controller.CreateInterchange(interchange) as BadRequestErrorMessageResult;
			AssertEquals("Recipient Id is missing", response?.Message);
		}

		protected override void SetUp()
		{
			var registration = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registration.EnterpriseCodeForTest = "HYE";
			registration.ServerCodeForTest = "CMT";
		}
	}
}
