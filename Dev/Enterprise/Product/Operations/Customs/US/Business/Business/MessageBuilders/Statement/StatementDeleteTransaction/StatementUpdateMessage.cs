using System;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class StatementUpdateMessageBuilder
	{
		public StatementUpdateMessageBuilder(IStatementDeleteTransaction statementUpdateEntity)
		{
			this.statementUpdateEntity = statementUpdateEntity;
		}
		readonly IStatementDeleteTransaction statementUpdateEntity;

		public MQEDIMessage PopulateMessage(int attemptNumber = 0)
		{
			ZString officeCode = USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(statementUpdateEntity.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);
			BlockControlGenerator block = null;

			if (statementUpdateEntity.ShouldGenerateACEStatementMessage)
			{
				block = new ACEInputBlockControlGenerator(GlbCompany.CurrentCompany.PK.ToGuid(), statementUpdateEntity.ProcessingPort, officeCode);
				block.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.StatementUpdate;
			}
			else
			{
				block = new ABIInputBlockControlGenerator(statementUpdateEntity.EntryFilerCode, statementUpdateEntity.ProcessingPort, officeCode);
				block.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction;
			}

			if (statementUpdateEntity.ShouldPopulatePreparerSite && block.B is IABIControlMessageBlockB)
			{
				var messageBlockB = block.B as IABIControlMessageBlockB;
				messageBlockB.PreparerIndicator = "1";
				messageBlockB.PreparerDistrictPort = statementUpdateEntity.PreparerPort;
				messageBlockB.PreparerFilerCode = statementUpdateEntity.EntryFilerCode;
				messageBlockB.PreparerOfficeCode = statementUpdateEntity.PreparerOfficeCode;
			}

			UpdateMessageBlocks(block);

			MQEDIMessage message = block.CreateMessage<MQEDIMessage>(statementUpdateEntity.Factory);
			if (statementUpdateEntity.Branch != null)
			{
				message.EM_GB = statementUpdateEntity.Branch.PK;
			}

			if (message.EM_MessageType == ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction &&
				attemptNumber > 0)
			{
				message.EM_HeldUntilDate = ZDateTime.Now.AddMinutes(GetDelayValue(attemptNumber));
				message.EM_ApplicationReference = (attemptNumber).ToString(System.Globalization.CultureInfo.InvariantCulture);
			}

			statementUpdateEntity.AddMessages(message);
			SetMessageSubType(message);
			return message;
		}

		int GetDelayValue(int attemptNumber)
		{
			int result = 1;
			switch (attemptNumber)
			{
				case 1:
					result = 1;
					break;
				case 2:
				case 3:
				case 4:
					result = 2;
					break;
				case 5:
					result = 5;
					break;
			}
			return result;
		}

		protected void SetMessageSubType(MQEDIMessage message)
		{
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
		}

		protected void UpdateMessageBlocks(BlockControlGenerator block)
		{
			IStatementUpdateInputHBlock h = statementUpdateEntity.ShouldGenerateACEStatementMessage ? new ASTUH() : new ENSH();

			h.DistrictPortOfEntrySummary = statementUpdateEntity.PortOfEntry;
			h.EntryFilerCode = statementUpdateEntity.EntryFilerCode;
			h.EntryNumber = statementUpdateEntity.EntryNumber;
			h.PaymentTypeIndicator = statementUpdateEntity.PaymentType;
			h.PreliminaryStatementPrintDate = statementUpdateEntity.PreliminaryStatementPrintDate.Date;
			h.ClientBranchDesignation = statementUpdateEntity.ClientBranchDesignation;
			h.PeriodicStatementMonth = statementUpdateEntity.PeriodicStatementMonth;

			block.AddMessageBlock((MessageBlock)h);
		}
	}
}
