using System;
using System.Web.Http;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost.GMD
{
	[Authorize]
	[eHubIdentityBasicAuthentication]
	[GMDAuthentication]
	[RoutePrefix("GenericMessageDelivery")]
	public sealed class GenericMessageDeliveryController : ApiController
	{
		[Route("")]
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Http action result")]
		public IHttpActionResult Get() => Ok("Welcome to the Generic Message Delivery Service");

		[Route("create-interchange")]
		[HttpPost]
		public IHttpActionResult CreateInterchange([FromBody] Interchange interchange)
		{
			if (interchange == null)
			{
				return BadRequest((NoResString)"Missing interchange details");
			}

			var creator = ObjectFactory.Get<Messaging.Integration.IGenericMessageDeliveryInterchangeCreator>();
			var validator = new InterchangeCreateRequestValidator(creator.SupportedInterchangeTypes, interchange);

			if (!validator.IsValid(out var message, out var branch))
			{
				return BadRequest(message);
			}

			var userContext = new Environment.UserContext(ZArchitecture.Environment.User.WebUserName, branch.PK.ToGuid(), Guid.Empty);
			using (Env.SetTemporaryUserContext(userContext))
			{
				var data = creator.Create(branch.PK, interchange.SenderId, interchange.RecipientId, interchange.InterchangeType, interchange.Body);

				if (data == null)
				{
					return BadRequest((NoResString)"Unable to create interchange");
				}

				var response = new InterchangeCreateResponse
				{
					Result = (NoResString)"accept",
					InterchangeNumber = data.EI_InterchangeNum,
				};

				return Json(response);
			}
		}
	}
}
