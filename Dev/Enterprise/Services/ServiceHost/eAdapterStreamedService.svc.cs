using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using CargoWise.Data;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using Enterprise.eHubMessaging.Business.DownloadHandler;

namespace Enterprise.Services.ServiceHost
{
#pragma warning disable CW1043
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:EAdaptorNamingRule", Justification = "Baseline issue")]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, Namespace = "http://cargowise.com/eAdapter", AddressFilterMode = AddressFilterMode.Any)]
	[eAdaptorStreamServiceErrorBehavior]
	public class eAdapterStreamedService : IeAdapterInboundStreamedService
#pragma warning restore CW1043
	{
		public bool Ping()
		{
			return true;
		}

		public void SendStream(SendStreamRequest request)
		{
			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					var senderId = GetSenderID();
					if (request.Messages == null)
					{
						throw new ArgumentException("Invalid Request XML. Please check if the body is valid.");
					}
					foreach (var message in request.Messages)
					{
						GetMessageHandler(message.SchemaName).SaveMessageFromAdapter(ToeHubMessage(message, senderId));
					}
				}
			}
			catch (Exception ex)
			{
				eAdaptorStreamServiceErrorReporter.ReportUnhandledException(ex);
				throw new FaultException(ex.Message);
			}
		}

		protected virtual string GetSenderID()
		{
			if (OperationContext.Current == null || OperationContext.Current.ServiceSecurityContext == null || OperationContext.Current.ServiceSecurityContext.PrimaryIdentity == null)
			{
				throw new FaultException("OperationContext.Current is null. Use HTTPS.");
			}

			return OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
		}

		IMessageHandler GetMessageHandler(string schemaName)
		{
			return HandlerFactory.GetHandler(schemaName);
		}

		static IeHubMessage ToeHubMessage(eHubGatewayMessage message, string senderId)
		{
			return new eHubMessage(
				message.MessageTrackingID,
				senderId,
				message.ClientID,
				message.SchemaType,
				message.ApplicationCode,
				message.SchemaName,
				message.MessageStream.DecodeAndDecompress(),
				message.EmailSubject,
				message.FileName);
		}

		public RetrieveStreamResponse ProcessStream(SendStreamRequest request)
		{
			var retrieveResponse = new RetrieveStreamResponse();
			retrieveResponse.TrackingID = Guid.NewGuid().ToString();

			List<eHubGatewayMessage> outputMessages = new List<eHubGatewayMessage>();
			foreach (var message in request.Messages)
			{
				outputMessages.Add(ProcessRequestMessage(message));
			}

			retrieveResponse.Messages = outputMessages.ToArray();
			return retrieveResponse;
		}

		eHubGatewayMessage ProcessRequestMessage(eHubGatewayMessage message)
		{
			//TODO: Doris to Modify
			var stream = message.MessageStream.DecodeAndDecompress();

			var result = new eHubGatewayMessage()
			{
				ClientID = GetSenderID(),
				MessageTrackingID = message.MessageTrackingID,
				MessageStream = stream.CompressAndEncode(),
			};

			return result;
		}
	}
}
