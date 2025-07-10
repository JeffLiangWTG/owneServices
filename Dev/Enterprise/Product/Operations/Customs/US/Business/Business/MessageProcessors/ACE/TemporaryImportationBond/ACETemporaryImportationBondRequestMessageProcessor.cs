using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosureResponse)]
	public class ACETemporaryImportationBondRequestMessageProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			var entryHeader = OriginalMessageLinker.Link<CusEntryHeader>(Message);
			bool isFailure = false;
			TIBExpiryDateExtension(entryHeader);
			var emailBody = GenerateEmailBody(out isFailure);
			string uri = entryHeader == null ? "" : ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entryHeader);
			string jobNumber = entryHeader == null ? "Unknown" : entryHeader.Declaration.DeclarationReferenceAppendedByFormattedEntryNumber;

			GlbBranch branch = entryHeader != null ? entryHeader.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Request for ACE TIB Extension/Closure", emailBody, isFailure, branch, entryHeader);
		}

		void TIBExpiryDateExtension(CusEntryHeader entryHeader)
		{
			if (!(entryHeader?.Has98130075Articles ?? true) && Message.HasApprovedTIBExtension())
			{
				var numberOfExtensionGranted = entryHeader.ApprovedTIBExtensionMessages.Length;
				if (numberOfExtensionGranted <= 1)
				{
					var newTIBExpiryDate = ((MessageBuilders.ICusEntryHeader)entryHeader).DateOfImportation.AddYears(2 + numberOfExtensionGranted);
					if (newTIBExpiryDate != entryHeader.US_TIBExpiryDate)
					{
						entryHeader.US_TIBExpiryDate = newTIBExpiryDate;
					}
				}
			}
		}

		ZString GenerateEmailBody(out bool isFailure)
		{
			var table = new HtmlTableCreator(new string[] { "Column", "Value" });
			HtmlTableCreator e1Table = null;
			isFailure = false;

			foreach (var block in Message.MessageBlock.MessageBlocks)
			{
				var e0Block = block as ATIBE0;
				if (e0Block != null)
				{
					table.WriteRow("Data Reference", e0Block.ReferenceDataTypeCode);
					table.WriteRow("Occurrence Position", e0Block.OccurrencePosition);
				}
				else
				{
					var e1Block = block as ATIBE1;
					if (e1Block != null)
					{
						if (!isFailure)
						{
							isFailure = e1Block.DispositionTypeCode == ACESeverityList.Codes.Rejected;
						}

						if (e1Table == null)
						{
							e1Table = new HtmlTableCreator(new string[] { "DISPOSITION", "Severity Code", "Condition Code", "Message" });
						}

						e1Table.WriteRow(Factory.GetCachedValue<ACESeverityList>().GetDescriptionFromCode(e1Block.DispositionTypeCode), Factory.GetCachedValue<ACESeverityList>().GetDescriptionFromCode(e1Block.SeverityCode), e1Block.ConditionCode, e1Block.NarrativeText);
					}
				}
			}

			return table.ToHtml() + "<br/><br/>" + e1Table.ToHtml();
		}
	}
}
