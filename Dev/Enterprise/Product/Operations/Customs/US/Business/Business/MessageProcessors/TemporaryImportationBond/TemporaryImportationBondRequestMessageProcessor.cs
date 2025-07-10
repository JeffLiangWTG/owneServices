using CargoWise.Application;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.TemporaryImportationBondEntrySummariesResponse)]
	class TemporaryImportationBondRequestMessageProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			HtmlTableCreator htmlTable = new HtmlTableCreator(new string[] { "ID", "Description" });
			CusEntryHeader entry = null;
			bool isFailure = true;

			if (messageBlocks.Count > 0)
			{
				TIBXO xo = GetFirstMessageBlock<TIBXO>();

				entry = CusEntryHeaderLinker.Link(xo.EntryNumber, xo.BrokerNumberOrEntryFilerCode, Message, CusEntryHeaderMessageTypeList.Codes.EntrySummary);

				isFailure = !xo.ErrorMessageIdentifier.IsEmpty;

				htmlTable.WriteRow(xo.ErrorMessageIdentifier, MessageCalculator.GetLongDescription(xo.ErrorMessageIdentifier, xo.NarrativeMessage));
			}

			if (entry == null)
			{
				htmlTable.WriteRow("-", "No entry is found with this entry number");
			}

			string jobNumber, uri;

			if (entry != null && entry.Declaration != null)
			{
				jobNumber = entry.Declaration.DeclarationReferenceAppendedByFormattedEntryNumber;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entry);
			}
			else
			{
				jobNumber = "Unknown";
				uri = "";
			}

			GlbBranch branch = entry != null ? entry.Branch : null;
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Request for TIB Expiry Extension", htmlTable.ToHtml(), isFailure, branch, entry);
		}
	}
}
