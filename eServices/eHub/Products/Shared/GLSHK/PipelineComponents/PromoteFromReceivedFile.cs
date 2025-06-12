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
	[Guid("DA5F9324-5866-4444-9C2C-E6F8235B71BC")]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	public class PromoteFromReceivedFile : ComponentBase, IComponent
	{
		protected override Guid ClassID
		{
			get { return new Guid("DA5F9324-5866-4444-9C2C-E6F8235B71BC"); }
		}


		protected override string DisplayName
		{
			get { return "Promote from received file"; }
		}

		public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			var logger = LoggerHelpers.GetPipelineLogger(pInMsg);
			LoggerHelpers.LogComponentStart(logger, this);
			try
			{
				new PipelineHelpers().CreateSeekableMessageStream(pContext, pInMsg, logger);
				var xDoc = XDocument.Load(pInMsg.BodyPart.GetOriginalDataStream());

				var file = xDoc.XPathSelectElement("//*[local-name()='File']");
				if (file != null)
				{
					var name = file.Attribute("Name");
					if (name != null) pInMsg.Context.PromoteProperty<FILE.ReceivedFileName>(name.Value);
					var plainFileContent = DecodeFromBase64String(file.Value);
					var destinationParty = GetDestinationParty(plainFileContent);
					pInMsg.Context.PromoteProperty<BTS.DestinationParty>(destinationParty);
					pInMsg.Context.PromoteProperty<BTS.SourceParty>("GLSHK");
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

		string DecodeFromBase64String(string input)
		{
			byte[] data = Convert.FromBase64String(input);
			return Encoding.UTF8.GetString(data);
		}

		string GetDestinationParty(string input)
		{
			var pattern = @"UNB\+UNOA:.+\+\d{6}:\d{4}\+(?<InterchangeNumber>\d+)\+.+'UNH";
			var isMatched = System.Text.RegularExpressions.Regex.IsMatch(input, pattern);

			if (isMatched) return "GLSHK_HKC";
			else return "GLSHK_AIR";
		}
	}
}
