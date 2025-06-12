using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Common;

namespace DmsGateway
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
	// NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
	public class eHubStreamedService : IeHubStreamedService
	{
		public void SendStream(SendStreamRequest request)
		{
			var user = OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
			foreach (var message in request.Messages)
			{
				message.MessageStream.Position = 0;
				var content = new StreamReader(message.MessageStream).ReadToEnd();
			}
		}

		public RetrieveStreamResponse RetrieveStream()
		{
			var user = OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
			var trackingID = Guid.NewGuid().ToString();
			var outputStream = new VirtualStream();
			OperationContext.Current.OperationCompleted += (sender, args) => outputStream?.Dispose();

			using (var encoderStream = new CryptoStream(outputStream, new ToBase64Transform(), CryptoStreamMode.Write, true))
			using (var gzipStream = new GZipStream(encoderStream, CompressionMode.Compress, true))
			using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello World")))
			{
				inputStream.CopyTo(gzipStream);
				outputStream.Position = 0;

				var retrieveResponse = new RetrieveStreamResponse()
				{
					TrackingID = trackingID,
					Messages = new eHubGatewayMessage[]
					{
						new eHubGatewayMessage
						{
							ClientID = "DMS",
							ApplicationCode = "DMS",
							MessageTrackingID = Guid.NewGuid(),
							MessageStream = outputStream
						}
					}
				};
				return retrieveResponse;
			}
		}

		private void OperationContext_OperationCompleted(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		public void FinaliseBatch(string trackingID)
		{
			var user = OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
		}

		public Dictionary<string, MessageStatus> GetMessageStatuses(string[] trackingIDs)
		{
			throw new NotImplementedException();
		}

		public bool Ping()
		{
			throw new NotImplementedException();
		}
	}
}
