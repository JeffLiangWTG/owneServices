using System;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[System.Runtime.InteropServices.Guid("579aacce-f62d-41b9-b93e-5a850e40b8a0")]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	public class PromotePollingData : ComponentBase, IComponent
	{
		protected override Guid ClassID
		{
			get { return new Guid("f5598ce2-1be3-41fa-aecd-a19ed4b7404e"); }
		}

		protected override string DisplayName
		{
			get { return "Promote Polling Data"; }
		}

		public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			var logger = LoggerHelpers.GetPipelineLogger(pInMsg);
			LoggerHelpers.LogComponentStart(logger, this);
			try
			{
				new PipelineHelpers().CreateSeekableMessageStream(pContext, pInMsg, logger);
				var xDoc = XDocument.Load(pInMsg.BodyPart.GetOriginalDataStream());
				string accountNo = xDoc.XPathSelectElement("/*[local-name()='TypedPollingResultSet0']/*[local-name()='AccountNo']").Value;
				string password = xDoc.XPathSelectElement("/*[local-name()='TypedPollingResultSet0']/*[local-name()='Password']").Value;
				pInMsg.Context.PromoteProperty<CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.PropertySchemas.AccountNo>(accountNo);
				pInMsg.Context.PromoteProperty<CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.PropertySchemas.Password>(password);
				logger.DebugFormat("MessageID: '{0}' AccountNo: '{1}' Password: '{2}'", pInMsg.MessageID, accountNo, password);
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
