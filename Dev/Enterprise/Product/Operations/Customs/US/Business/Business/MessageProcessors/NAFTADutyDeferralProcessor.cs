using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.NAFTADutyDeferralResponse)]
	public class NAFTADutyDeferralProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			CusEntryHeader entryHeaderLinked = CusEntryHeaderLinker.Link(Message);

			string jobNumber = "Unknown";
			string uri = "";
			if (entryHeaderLinked != null)
			{
				jobNumber = entryHeaderLinked.Declaration.DeclarationReferenceAppendedByFormattedEntryNumber;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entryHeaderLinked);
			}

			StringBuilder html = new StringBuilder();
			HtmlTableCreator creator = new HtmlTableCreator(new string[] { "Error", "Message" });
			bool isFailure = false;
			List<ZString> errorCodes = new List<ZString>();
			foreach (MessageBlock block in messageBlocks)
			{
				NDDDER der = block as NDDDER;
				if (der != null)
				{
					isFailure = true;
					if (errorCodes.Contains(der.ErrorMessageIdentifier))
					{
						errorCodes.Add(der.ErrorMessageIdentifier);
					}
					creator.WriteRow(der.ErrorMessageIdentifier, der.NarrativeMessage);
				}
			}
			if (creator != null)
			{
				html.Append(creator.ToHtml());
			}

			GlbBranch branch = entryHeaderLinked != null ? entryHeaderLinked.Branch : null;
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "NAFTA Duty Deferral", html.ToString(), isFailure, branch, entryHeaderLinked);
		}
	}
}
