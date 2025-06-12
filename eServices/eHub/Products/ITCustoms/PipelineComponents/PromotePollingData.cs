using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.ITCustoms.BizTalk.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("8446A2B7-144C-4935-B7C2-5D986B0BEA48")]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	public class PromotePollingData : ComponentBase, IComponent
	{
		protected override Guid ClassID
		{
			get { return new Guid("8446A2B7-144C-4935-B7C2-5D986B0BEA48"); }
		}


		protected override string DisplayName
		{
			get { return "Promote ITC Polling Data"; }
		}

		public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			var logger = LoggerHelpers.GetPipelineLogger(pInMsg);
			LoggerHelpers.LogComponentStart(logger, this);
			try
			{
				new PipelineHelpers().CreateSeekableMessageStream(pContext, pInMsg, logger);
				var xDoc = XDocument.Load(pInMsg.BodyPart.GetOriginalDataStream());

				var subject = xDoc.XPathSelectElement("//*[local-name()='TypedPollingResultSet0']/*[local-name()='OverrideEmailSubject']");
				var destination = xDoc.XPathSelectElement("//*[local-name()='TypedPollingResultSet0']/*[local-name()='DestinationParty']");
				var ehId = xDoc.XPathSelectElement("//*[local-name()='TypedPollingResultSet0']/*[local-name()='EH_ID']");
				var filename = xDoc.XPathSelectElement("//*[local-name()='TypedPollingResultSet0']/*[local-name()='IT_Filename']");
				var senderPk = xDoc.XPathSelectElement("//*[local-name()='TypedPollingResultSet0']/*[local-name()='SourcePartyPk']");
				if (subject != null)
					pInMsg.Context.PromoteProperty<CargoWise.eHub.Core.PropertySchemas.OverrideEmailSubject>(subject.Value);
				if (destination != null)
					pInMsg.Context.PromoteProperty<BTS.DestinationParty>(destination.Value);
				if(ehId != null)
					pInMsg.Context.PromoteProperty<CargoWise.eHub.Products.ITCustoms.Biztalk.PropertySchemas.EH_ID>(ehId.Value);
				if(filename != null)
					pInMsg.Context.PromoteProperty<CargoWise.eHub.Products.ITCustoms.Biztalk.PropertySchemas.IT_Filename>(filename.Value);
				if (senderPk != null)
					pInMsg.Context.PromoteProperty<CargoWise.eHub.Products.ITCustoms.Biztalk.PropertySchemas.SourcePartyPk>(senderPk.Value);
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
