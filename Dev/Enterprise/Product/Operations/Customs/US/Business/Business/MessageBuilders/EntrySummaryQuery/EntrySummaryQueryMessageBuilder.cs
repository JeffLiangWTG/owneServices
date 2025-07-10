using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class EntrySummaryQueryMessageBuilder
	{
		public EntrySummaryQueryMessageBuilder(EntryHeaderMessageSendingAction action)
			: this(action.entry, action.US_CollectionBillInformationCode)
		{
		}

		public EntrySummaryQueryMessageBuilder(IEntrySummaryQueryMessageAttachee entryHeader, ZString collectionBillInformationCode)
		{
			this.entryHeader = entryHeader;
			this.collectionBillInformationCode = collectionBillInformationCode;
		}

		public MQEDIMessage PopulateMessage()
		{
			var enqj1 = new ENQJ1();
			var entryFilerCodesAndNumber = entryHeader.EntryFilerCodesAndNumbers.FirstOrDefault();
			if (entryFilerCodesAndNumber != default)
			{
				enqj1.BrokerNumberOrEntryFilerCode = entryFilerCodesAndNumber.Code;
				enqj1.EntryNumber = entryFilerCodesAndNumber.Number;
			}
			enqj1.CollectionBillInformationCode = ZInt.ParseSafe(collectionBillInformationCode, 0);
			var blockControlGenerator = GetBlockGenerator(enqj1.BrokerNumberOrEntryFilerCode);
			blockControlGenerator.MessageBlocks.Add(enqj1);

			var message = blockControlGenerator.CreateMessage<MQEDIMessage>(entryHeader.Factory);
			SetMessageSubType(message);
			if (entryHeader.Messages != null)
			{
				entryHeader.Messages.Add(message);
			}
			return message;
		}

		protected virtual BlockControlGenerator GetBlockGenerator(ZString entryFilerCode)
		{
			var processingPort = entryHeader.IsRemoteLocationFiling ? entryHeader.PreparerDistrictPort : entryHeader.ProcessingDistrictPort;
			var abiInputBlockControlGenerator = new ABIInputBlockControlGenerator(entryFilerCode, processingPort, entryHeader.ProcessingOfficeCode);

			abiInputBlockControlGenerator.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.QueryEntrySummary;
			return abiInputBlockControlGenerator;
		}

		protected virtual void SetMessageSubType(MQEDIMessage message)
		{
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryQuery;
		}

		protected readonly IEntrySummaryQueryMessageAttachee entryHeader;
		readonly ZString collectionBillInformationCode;
	}
}
