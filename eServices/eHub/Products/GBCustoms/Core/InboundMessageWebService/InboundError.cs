using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;

using Common.Logging;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService
{
	public class InboundError
	{
		internal Func<Guid> InternalNewGuid = Guid.NewGuid;
		internal Func<DateTime> InternalUtcNow = () => DateTime.UtcNow;
		protected eHubTransactionsContext dbContext;
		protected ILog logger;
		protected string eHubClientID;

		public InboundError(eHubTransactionsContext dbContext, ILog logger, string eHubClientID)
		{
			this.dbContext = dbContext;
			this.logger = logger;
			this.eHubClientID = eHubClientID;
		}

		public void CreateFailedMessage(HttpRequestMessage requestMessage, string subject, Exception ex)
		{
			DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
			{
				var eHubClientPK = dbContext.eHubClients.FirstOrDefault(client => client.CC_ID == eHubClientID).CC_PK;

				var inboxMessage = new eHubInboxMessage
				{
					EI_PK = InternalNewGuid(),
					EI_MessageTrackingID = InternalNewGuid().ToString(),
					EI_EnvelopeTrackingID = string.Empty,
					EI_CC_Sender = eHubClientPK,
					EI_CC_Recipient = eHubClientPK,
					EI_MessageType = "CDS",
					EI_IsFlatFile = false,
					EI_ApplicationCode = "HUB",
					EI_EmailSubjectOverride = string.Empty,
					EI_FileNameOverride = string.Empty,
					EI_InsertUTC = InternalUtcNow(),
					EI_LastUpdateUTC = InternalUtcNow(),
					EI_Status = 255,
					EI_Content = CreateContent(requestMessage, ex).CompressAndEncode().ReadToEnd()
				};

				var error = new eHubError
				{
					EE_PK = InternalNewGuid(),
					EE_Alerted = false,
					EE_DateTimeUTC = InternalUtcNow(),
					EE_Description = $"{subject} - {ex.Message}",
					EE_EI_Inbox = inboxMessage.EI_PK,
					EE_ErrorDetail = CreateErrorDetail(subject, ex),
					EE_ErrorType = "EXP",
					EE_Source = "HUB"
				};

				dbContext.eHubInboxMessages.Add(inboxMessage);
				dbContext.eHubErrors.Add(error);
				dbContext.SaveChanges();
			}, logger);
		}

		internal string CreateErrorDetail(string subject, Exception ex)
		{
			return $"<ErrorDetail>{subject} - {ex.Message}</ErrorDetail>";
		}

		internal Stream CreateContent(HttpRequestMessage requestMessage, Exception ex)
		{
			var contentBuilder = new StringBuilder();
			contentBuilder.AppendLine("Exception Details:");
			contentBuilder.AppendLine(ex.Message);
			contentBuilder.AppendLine(ex.StackTrace);
			contentBuilder.AppendLine();

			contentBuilder.AppendLine("Inbound Message Headers:");
			foreach (var header in requestMessage.Headers)
			{
				contentBuilder.AppendLine($"{header.Key} - {header.Value.FirstOrDefault()}");
			}
			contentBuilder.AppendLine();

			contentBuilder.AppendLine("Inbound Message Body:");
			contentBuilder.AppendLine(requestMessage.Content.ReadAsStringAsync().Result);

			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(contentBuilder);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
}
