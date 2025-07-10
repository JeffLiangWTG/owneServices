using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.TemporaryImportationBondDuetoExpire)]
	[TopLevel(typeof(TIBX1))]
	class TemporaryImportBondExpiryNotificationProcessor : ACSABIProcessor
	{
		#region IKeysForBlockingParallelProcessingProvider Members

		protected override ProcessingResult<LinkedBusinessObjectMetaData> TryFindLinkedObject(CBPEDIMessage message)
		{
			var jobNumber = ZString.Empty;
			var companyPK = message.Branch.GB_GC;
			var x1 = message.MessageBlock.MessageBlocks.OfType<TIBX1>().FirstOrDefault();
			if (x1 != null)
			{
				jobNumber = USIUniversalCustomsMessageProcessor.GetEntryJobNumberKey(x1.BrokerNumberOrEntryFilerCode, x1.EntryNumber, companyPK);
			}
			return new LinkedBusinessObjectMetaData(ZString.Empty, ZGuid.Empty, ZGuid.Empty, jobNumber);
		}

		protected override HashSet<string> TryFindSerializationKeys(CBPEDIMessage message, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var companyPK = message.Branch.GB_GC;
			return message.MessageBlock.MessageBlocks
				.OfType<TIBX1>()
				.Skip(1)
				.Select(x1 => USIUniversalCustomsMessageProcessor.GetEntryJobNumberKey(x1.BrokerNumberOrEntryFilerCode, x1.EntryNumber, companyPK))
				.ToHashSet();
		}

		#endregion

		public override void Process()
		{
			Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.TemporaryImportationBondDueToExpire;

			if (messageBlocks.Count > 0)
			{
				string entryFiler = "";
				string entryNumber = "";
				bool messageHasNotLinkedToEntry = true;
				var x1Blocks = Message.GetMessageBlocks<TIBX1>();
				var loader = new CusEntryHeader.Loader(Message.Factory);
				foreach (var x1 in x1Blocks)
				{
					HtmlTableCreator htmlTable = new HtmlTableCreator(new string[] { "ID", "Description" });
					CusEntryHeader entry = null;

					entryNumber = x1.EntryNumber;
					entryFiler = x1.BrokerNumberOrEntryFilerCode;

					if (messageHasNotLinkedToEntry)
					{
						entry = CusEntryHeaderLinker.Link(entryNumber, entryFiler, Message, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
						messageHasNotLinkedToEntry = false;
					}
					else
					{
						entry = loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, entryNumber, entryFiler, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
					}

					if (entry != null)
					{
						entry.US_TIBExpiryDate = x1.TemporaryImportationBondTIBExpirationDate;
						entry.US_TIBNumOfExtensions = x1.TotalNumberOfExtensions;

						htmlTable.WriteRow("Expiry Date", x1.TemporaryImportationBondTIBExpirationDate.ToShortDateString());
						htmlTable.WriteRow("Total Number of Extensions", x1.TotalNumberOfExtensions);
					}
					else
					{
						htmlTable.WriteRow(CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(entryFiler, entryNumber), "No entry found with this entry number");
					}

					string jobNumber, url;

					if (entry != null && entry.Declaration != null)
					{
						jobNumber = entry.Declaration.DeclarationReferenceAppendedByFormattedEntryNumber;
						url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entry);
					}
					else
					{
						jobNumber = "Unknown / " + CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(entryFiler, entryNumber);
						url = "";
					}

					GlbBranch branch = entry != null ? entry.Branch : null;
					GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumber, "Notice for TIB Due to Expire", htmlTable.ToHtml(), false, branch, entry);
				}
			}
		}
	}
}
