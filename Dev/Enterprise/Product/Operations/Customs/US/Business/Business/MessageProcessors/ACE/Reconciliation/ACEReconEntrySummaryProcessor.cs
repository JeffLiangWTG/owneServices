using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummaryResponse)]
	public class ACEReconEntrySummaryProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			var reconEntry = OriginalMessageLinker.Link<CusEntryHeader>(Message);
			var url = ZString.Empty;
			JobDeclaration jobDeclaration = null;
			if (reconEntry != null)
			{
				jobDeclaration = reconEntry.Declaration;
				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(jobDeclaration);
			}

			var hasRejected = false;
			var emailBody = ProcessBlocksAndGenerateEmailBody(ref hasRejected);

			if (jobDeclaration != null)
			{
				var reconDeclaration = ReconDeclaration.Get(jobDeclaration);
				CalculateStatus(jobDeclaration, reconEntry, hasRejected);

				if (!hasRejected && jobDeclaration.JE_EntryAuthorisationDate.IsEmpty)
				{
					jobDeclaration.JE_EntryAuthorisationDate = ZDateTime.Now;
				}

				if (!hasRejected && Message.OriginalMessage != null)
				{
					var blocks = Message.OriginalMessage.GetMessageBlocks<AREC10>();
					var block = blocks[0];
					reconDeclaration.US_R_ImporterIDLodged = block.ImporterOfRecordNumber;
				}

				if (reconEntry.CH_Status == ReconMessageStatusList.Codes.ClearReconDelete && jobDeclaration.US_Paid == YesNoList.Codes.Yes)
				{
					jobDeclaration.US_Paid = ZString.Empty;
				}
			}

			var branch = jobDeclaration != null ? jobDeclaration.Branch : null;
			string jobNumber = jobDeclaration == null ? "Unknown" : jobDeclaration.DeclarationReferenceAppendedByFormattedEntryNumber;
			GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumber, "ACE Reconciliation Entry Summary", emailBody, hasRejected, branch, jobDeclaration);
		}

		string ProcessBlocksAndGenerateEmailBody(ref bool hasRejected)
		{
			var dispositionTable = new HtmlTableCreator(new string[] { "Disposition", "Condition Code", "Description" });

			foreach (var messageBlock in Message.MessageBlock.MessageBlocks)
			{
				var e1Block = messageBlock as ARECE1;
				if (e1Block != null)
				{
					hasRejected = e1Block.DispositionTypeCode == ACESeverityList.Codes.Rejected;
					dispositionTable.WriteRow(Factory.GetCachedValue<ACESeverityList>().GetDescriptionFromCode(e1Block.DispositionTypeCode), e1Block.ConditionCode, e1Block.NarrativeText);
				}
			}

			return dispositionTable.ToHtml();
		}

		void CalculateStatus(JobDeclaration declaration, CusEntryHeader reconEntry, ZBool hasFailure)
		{
			if (declaration != null)
			{
				var responseStatus = ABIResponseStatus.Cleared;

				if (hasFailure)
				{
					responseStatus = ABIResponseStatus.Rejected;
				}

				var status = new ReconMessageStatusCalculator().Calculate(Message, responseStatus, false);
				if (!status.IsEmpty)
				{
					reconEntry.CH_Status = status;
					declaration.JE_MessageStatus = reconEntry.CH_Status;
					if (!hasFailure)
					{
						declaration.JE_EntryStatus = reconEntry.CH_Status;
					}
				}
			}
		}
	}
}
