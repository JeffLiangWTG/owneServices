using System;
using System.Collections.Generic;
using System.ServiceModel;
using CargoWise.eHub.Products.NZCustoms.Common;
using CargoWise.eHub.Products.NZCustoms.SoapWithAttachments;
using CargoWise.eHub.Products.NZCustoms.Client.RequestLodgeResponse_v1;
using CargoWise.eHub.Products.NZCustoms.Client.SubmitLodgement_v2;
using Common.Logging;
using LodgementBinary = CargoWise.eHub.Products.NZCustoms.Client.SubmitLodgement_v2.Binary;

namespace CargoWise.eHub.Products.NZCustoms.Client
{
	public class ServiceApi : CargoWise.eHub.Products.NZCustoms.Client.IServiceApi
	{
		readonly ILog logger;
		const string SubmitLodgementEndpointName = "SubmitLodgement";
		const string RequestLodgeResponseEndpointName = "RequestLodgeResponse";

		public ServiceApi(ILog logger)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;
		}

		public SubmitLodgementResponse SendLodgementRequest(SubmitLodgementRequest request)
		{
			SubmitLodgementPortTypeClient client = new SubmitLodgementPortTypeClient(SubmitLodgementEndpointName);

			byte[] attachmentContents = null;
			if (request.attachment != null)
			{
				attachmentContents = request.attachment.Value;
				request.attachment.Value = null;
			}

			using (new OperationContextScope(client.InnerChannel))
			{
				if (attachmentContents != null)
				{
					logger.DebugFormat("Attachment size Api {0}", attachmentContents.Length);
					OperationContext.Current.OutgoingMessageProperties.Add
						(
							SwaEncoderConstants.AttachmentProperty,
							attachmentContents
						);
				}
				SubmitLodgementResponse response = client.RunAndClose(t => ((SubmitLodgementPortType)t).ExecuteSubmitLodgement(request));

				if (OperationContext.Current.IncomingMessageProperties.ContainsKey(SwaEncoderConstants.AttachmentProperty))
				{
					if (response.attachment == null)
					{
						response.attachment = new LodgementBinary();
					}
					response.attachment.Value = (byte[])OperationContext.Current.IncomingMessageProperties[SwaEncoderConstants.AttachmentProperty];
				}
				return response;
			}
		}

		public RequestLodgeResponseResponse SendPullRequest(RequestLodgeResponseRequest requestLodgeResponseRequest)
		{
			using (var client = new RequestLodgeResponsePortTypeClient(RequestLodgeResponseEndpointName))
			{
				RequestLodgeResponseResponse responseResponse;
				using (new OperationContextScope(client.InnerChannel))
				{
					responseResponse = client.RunAndClose(t => ((RequestLodgeResponsePortType)t).ExecuteRequestResponse(requestLodgeResponseRequest));
					if (logger.IsDebugEnabled) logger.DebugFormat("SendPullRequest IncomingMessageProperties count", OperationContext.Current.IncomingMessageProperties.Count);

					if (OperationContext.Current.IncomingMessageProperties.ContainsKey(SwaEncoderConstants.AttachmentCount))
					{
						int attachmentCount = (int)OperationContext.Current.IncomingMessageProperties[SwaEncoderConstants.AttachmentCount];
						if (logger.IsDebugEnabled) logger.DebugFormat("SendPullRequest AttachmentCount", attachmentCount);

						var attachments = new List<Attachment>();
						for (int i = 1; i < attachmentCount; i++)
						{
							var attachment = new Attachment();
							attachment.Content = Convert.ToBase64String((byte[])OperationContext.Current.IncomingMessageProperties[string.Format(SwaEncoderConstants.AttachmentPropertyFormat, SwaEncoderConstants.AttachmentProperty, i)]);
							attachment.Filename =
								(string)
									OperationContext.Current.IncomingMessageProperties[
										string.Format(SwaEncoderConstants.AttachmentPropertyFormat,
											SwaEncoderConstants.AttachmentNameProperty, i)];
							;
							attachment.ContentType =
								(string)
									OperationContext.Current.IncomingMessageProperties[
										string.Format(SwaEncoderConstants.AttachmentPropertyFormat,
											SwaEncoderConstants.AttachmentTypeProperty, i)];

							attachments.Add(attachment);
						}

						if (responseResponse.attachments == null) responseResponse.attachments = attachments.ToArray();

					}
				}

				return responseResponse;
			}
		}
	}
}
