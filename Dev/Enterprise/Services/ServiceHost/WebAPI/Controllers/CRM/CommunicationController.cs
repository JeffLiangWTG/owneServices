using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/communication")]
	public class CommunicationController : ApiController
	{
		[Route("sendreminder/{communicationPK}")]
		[HttpPost]
		[HttpGet]
		public HttpResponseMessage SendReminder([FromUri] Guid communicationPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				try
				{
					if (!Env.Registry.CalendarIntegration)
					{
						return new HttpResponseMessage(HttpStatusCode.Forbidden)
						{
							Content = new StringContent((NoResString)"Cannot send invitation as calendar integration is not enabled."),
						};
					}
					var factory = new BusinessObjectFactory();
					var orgSalesCall = factory.Load<OrgSalesCall>(communicationPK);
					var relatedAttendee = factory.Load<OrgSalesCallAdditionalAttendee>(new ZQuery(OrgSalesCallAdditionalAttendeeSchema.O6_OQ, communicationPK));
					var recipients = GetRecipients(relatedAttendee);
					var notes = ORtfTextUtil.RtfToText(orgSalesCall.OQ_SalesCallNotes);
					var reminder = CommunicationReminder.New(ZDateTime.Empty, orgSalesCall.OQ_NextCall, orgSalesCall, recipients, notes);
					reminder.CreateAppointment();
					return new HttpResponseMessage(HttpStatusCode.OK);
				}
				catch (Exception ex)
				{
					var problemDetails = new ProblemDetails()
					{
						Title = ex.Message,
						Type = ProblemType.BadData,
						Status = (int)HttpStatusCode.InternalServerError
					};

					return new HttpResponseMessage(HttpStatusCode.InternalServerError)
					{
						Content = new StringContent(problemDetails.Serialize(), Encoding.UTF8, ProblemDetails.MediaType),
					};
				}
			}
		}

		IEnumerable<CommunicationReminderRecipient> GetRecipients(OrgSalesCallAdditionalAttendee[] attendees)
		{
			var recipients = new List<CommunicationReminderRecipient>();

			foreach (var attendee in attendees)
			{
				if (attendee.O6_ReceiverReminder && !string.IsNullOrEmpty(attendee.O6_EmailAddress))
				{
					recipients.Add(new CommunicationReminderRecipient(attendee.O6_OQ, attendee.Name, attendee.O6_EmailAddress));
				}
			}

			return recipients;
		}
	}
}
