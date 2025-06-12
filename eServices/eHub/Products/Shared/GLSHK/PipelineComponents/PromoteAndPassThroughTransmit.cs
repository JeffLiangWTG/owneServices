using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.Shared.GLSHK.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("0C1179CA-F869-4AA7-92C0-8DC012547973")]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	public class PromoteAndPassThroughTransmit : ComponentBase, IComponent
	{
		protected override Guid ClassID
		{
			get { return new Guid("0C1179CA-F869-4AA7-92C0-8DC012547973"); }
		}


		protected override string DisplayName
		{
			get { return "Promote and Pass Through Transmit"; }
		}

		public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			var logger = LoggerHelpers.GetPipelineLogger(pInMsg);
			LoggerHelpers.LogComponentStart(logger, this);
			try
			{
				pInMsg.Context.Write("DocumentSpecName", "http://schemas.microsoft.com/BizTalk/2003/xmlnorm-properties", string.Empty);
				pInMsg.Context.Write("InterchangeID", "http://schemas.microsoft.com/BizTalk/2003/system-properties", Guid.NewGuid().ToString().ToUpper());
				PromoteIfExistAndNotPromoted(pInMsg, "ReceivePortName", "http://schemas.microsoft.com/BizTalk/2003/system-properties");

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

		void PromoteIfExistAndNotPromoted(IBaseMessage pInMsg, string strName, string strNamespace)
		{
			var obj = pInMsg.Context.Read(strName, strNamespace);
			if (obj != null)
			{
				var isPromoted = pInMsg.Context.IsPromoted(strName, strNamespace);
				if (!isPromoted)
				{
					pInMsg.Context.Promote(strName, strNamespace, obj);
				}
			}
		}
	}
}
