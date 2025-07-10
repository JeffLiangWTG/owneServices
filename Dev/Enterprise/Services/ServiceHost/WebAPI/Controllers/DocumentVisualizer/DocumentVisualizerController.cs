using System;
using System.Collections;
using System.Linq;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public sealed class DocumentVisualizerController : ApiController
	{
		[Route("api/DocumentVisualizer/SendMessage/{messageID}/{tablePrefix}/{jobPK}")]
		[HttpGet]
		public IHttpActionResult SendMessage([FromUri] string messageID, [FromUri] string tablePrefix, [FromUri] Guid jobPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			using (var disposableManager = new DisposableManager())
			{
				if (string.IsNullOrWhiteSpace(messageID)
					|| string.IsNullOrWhiteSpace(tablePrefix)
					|| jobPK == Guid.Empty)
				{
					var message = $"Non empty {nameof(messageID)}, {nameof(tablePrefix)} and {nameof(jobPK)} are required"; // exception message.
					return BadRequest(message);
				}

				var factory = new BusinessObjectFactory();
				factory.AddDisposableService(disposableManager);
				factory.NameForDebugging = $"{nameof(DocumentVisualizerController)}.{nameof(SendMessage)}";
				factory.RefreshEnabled = false;

				var bizObj = factory.Load(tablePrefix, jobPK);

				if (bizObj == null)
				{
					return NotFound();
				}

				var allMessageSendersRegistration = ObjectFactory.Get<Hashtable>("DocDataObjectWithoutUIMessageSendersProvider");

				if (allMessageSendersRegistration == null
					|| !(allMessageSendersRegistration[messageID] is ObjectHandle senderRegistration)
					|| !(senderRegistration.GetObject() is IDocDataObjectWithoutUIMessageSender messageSender))
				{
					return NotFound();
				}

				var notifications = new NotificationsHandler();
				var hasMessageBeenSent = messageSender.SendMessage(bizObj, notifications);

				var messages = notifications
					?.Notifications
					?.Select(n => new ValidationMessage
					{
						MessageType = n.Type.EnumValueName,
						Message = n.Message
					})
					?.ToArray();

				var result = new SendMessageResponse
				{
					Success = hasMessageBeenSent,
					Messages = messages
				};

				return Json(result);
			}
		}
	}
}
