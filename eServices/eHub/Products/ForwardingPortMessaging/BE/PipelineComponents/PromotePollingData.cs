using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.BizTalk.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("06DD9212-C697-42E8-B86D-60219868FA95")]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	public class PromotePollingData : ComponentBase, IComponent
	{
		protected override Guid ClassID => new Guid("06DD9212-C697-42E8-B86D-60219868FA95");

		protected override string DisplayName => "Promote BE CPoint Polling Data";

		public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			var logger = LoggerHelpers.GetPipelineLogger(pInMsg);
			LoggerHelpers.LogComponentStart(logger, this);
			try
			{
				new PipelineHelpers().CreateSeekableMessageStream(pContext, pInMsg, logger);
				var xDoc = XDocument.Load(pInMsg.BodyPart.GetOriginalDataStream());

				var messageType = xDoc.XPathSelectElement("//*[local-name()='TypedPollingResultSet0']/*[local-name()='MessageType']");
				var isProd = xDoc.XPathSelectElement("//*[local-name()='TypedPollingResultSet0']/*[local-name()='IsProd']");
				var username = xDoc.XPathSelectElement("//*[local-name()='TypedPollingResultSet0']/*[local-name()='UserName']");

				if (isProd != null)
					pInMsg.Context.PromoteProperty<PropertySchemas.IsProd>(isProd.Value);
				if (messageType != null)
					pInMsg.Context.PromoteProperty<PropertySchemas.MessageType>(messageType.Value);
				if (username != null)
				{
					pInMsg.Context.PromoteProperty<PropertySchemas.UserName>(username.Value);
					pInMsg.Context.Write("TriggerMutex", "http://cargowise.com/ehub/core/mutex", "FPM_BE_CPOINT_POLLING_" + username.Value);
				}


				pInMsg.BodyPart.GetOriginalDataStream().Position = 0;
				return pInMsg;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				throw;
			}
			finally
			{
				LoggerHelpers.LogComponentStart(logger, this);
			}
		}
	}
}