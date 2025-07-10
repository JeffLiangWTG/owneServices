using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ZA.Business.BatchProcessor.MessageProcessors;
using Enterprise.Edifact.D96B.Messages.STATAC;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	class STATACMessageProcessor : ZACApplicationTypeMessageProcessor
	{
		public STATACMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => "STATAC Message";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { SARSEDIMessage.MessageTypes.STATAC };

		protected override bool RequiresPreProcessingCore => true;

		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason, MessageHelper Helper) TryFindLinkedObject(EDIMessage message)
		{
			var branchPK = message.EM_GB;
			MultilingualString discardReason = (NoResString)string.Empty;
			var helper = STATACMessageHelper.New(message as STATACEDIMessage);
			if (helper != null && helper.statacMessage != null)
			{
				var accountNo = helper.FinancialAccountNumber;
				if (accountNo.IsEmpty)
				{
					discardReason = MissingFAN(message.EM_MessageNum);
				}
			}
			else
			{
				discardReason = GetUnableToFindTheLinkedJobMessage("STATAC", message);
			}
			return (branchPK, null, discardReason, helper);
		}

		public override ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
			=> message.EM_GB;

		public override ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var keys = new HashSet<string>();

			var helper = STATACMessageHelper.New(message as STATACEDIMessage);
			var accountNo = helper.FinancialAccountNumber;
			var messageBranchCompanyPK = message.Branch.GB_GC;
			var customsOffice = helper.CustomsOffice;
			var agentCode = helper.AgentCode;
			foreach (SegmentGroup3 sg3 in helper.statacMessage.Group3)
			{
				var sg3Helper = new STATACMessageSG3Helper(sg3);
				if (sg3Helper.Level != STATACMessageSG3Helper.Constants.HeaderLevelIndicatorForDetailMessage
					|| helper.DailyOrDetail != STATACMessageHelper.Constants.Detail)
				{
					keys.Add(CreateKey(accountNo, sg3Helper.TransactionDate, sg3Helper.DueDate, customsOffice, agentCode, messageBranchCompanyPK));
				}
			}
			if (keys.Count == 0)
			{
				keys.Add(CreateKey(accountNo, ZDateTime.Empty, ZDateTime.Empty, customsOffice, agentCode, messageBranchCompanyPK));
			}
			if (keys.Count == 0)
			{
				return SerializationKeysResult.SerialProcessingInReceivedOrder;
			}
			return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, keys);
		}

		static string CreateKey(ZString accountNo, ZDateTime processDate, ZDateTime dueDate, ZString customsOffice, ZString agentCode, ZGuid messageBranchCompanyPK)
		{
			return $"{accountNo}|{processDate.ToISO8601ShortDateString()}|{dueDate.ToISO8601ShortDateString()}|{customsOffice}|{agentCode}|{messageBranchCompanyPK}";
		}

		protected override void ProcessMessageMain(EDIMessage message)
		{
			var successful = false;
			var helper = STATACMessageHelper.New(message as ZAMessage);
			if (helper != null && helper.statacMessage != null)
			{
				if (!helper.FinancialAccountNumber.IsEmpty)
				{
					foreach (SegmentGroup3 sg3 in helper.statacMessage.Group3)
					{
						var sg3Helper = new STATACMessageSG3Helper(sg3);
						if (sg3Helper.Level != STATACMessageSG3Helper.Constants.HeaderLevelIndicatorForDetailMessage
							|| helper.DailyOrDetail != STATACMessageHelper.Constants.Detail)
						{
							var statementHeader = LocateOrCreateCusStatementHeader(message.Factory, message, helper, sg3Helper);
							if (statementHeader != null)
							{
								UpdateOrCreateCusStatementLine(message, sg3Helper, statementHeader);
							}
						}
					}
					successful = true;
				}
			}

			message.EM_Status = successful ? EDIMessage.Status.ProcessedOK : EDIMessage.Status.Discarded;
		}

		public static MultilingualString MissingFAN(string messageNum)
		{
			return ResString.GetMultilingualString("70d3482d-817b-4e99-9ffa-794f6bcec08c", "STATAC Message: #{0} does not contain an Account Number (FAN).", messageNum);
		}

		#region Update Methods

		CusStatementHeader LocateOrCreateCusStatementHeader(BusinessObjectFactory factory, EDIMessage message, STATACMessageHelper helper, STATACMessageSG3Helper sg3Helper)
		{
			CusStatementHeader result = null;
			var accountNo = helper.FinancialAccountNumber;
			var processDate = sg3Helper.TransactionDate;
			var dueDate = sg3Helper.DueDate;
			var customsOffice = helper.CustomsOffice;
			var agentCode = helper.AgentCode;

			if (processDate.IsValid)
			{
				var existingHeader = new CusStatementHeader.Loader(factory).Load(accountNo, customsOffice, agentCode, processDate, dueDate);
				if (existingHeader != null)
				{
					result = existingHeader;
				}
				else
				{
					var newHeader = factory.New<CusStatementHeader>();
					newHeader.B2_AccountNo = accountNo;
					newHeader.B2_ProcessPort = customsOffice;
					newHeader.B2_EntryFilerCode = agentCode;
					newHeader.B2_ProcessDate = processDate;
					newHeader.B2_DueDate = dueDate;
					newHeader.B2_GC = GlbCompany.CurrentCompany.PK;

					result = newHeader;
				}
			}
			else
			{
				Logger.Log(TransactionDateInvalid(message.EM_MessageNum, sg3Helper.TransactionDateString));
			}

			return result;
		}

		public static string TransactionDateInvalid(string messageNum, string transactionDate)
		{
			return Res.GetString("9d7d6f2a-7069-4646-a3f2-fe339a7b0f54", "STATAC Message: #{0} Transaction Date invalid {1}.", messageNum, transactionDate);
		}

		void UpdateOrCreateCusStatementLine(EDIMessage message, STATACMessageSG3Helper sg3helper, CusStatementHeader statementHeader)
		{
			var entryNum = sg3helper.TransactionReference;
			var brokerReference = sg3helper.TransactionDescription;

			if (!entryNum.IsEmpty)
			{
				var statementLine = statementHeader.StatementLines.GetStatementLineFor(entryNum);
				if (statementLine == null)
				{
					statementLine = statementHeader.StatementLines.AddNew();
					statementLine.B3_EntryNum = entryNum;
					statementLine.B3_BrokerReference = brokerReference;
				}

				AddCusStatementLineChargeIfNotExist(sg3helper, statementLine);

				var customsFeesTotal = ZDecimal.Zero;
				foreach (CusStatementLineCharge charge in statementLine.Charges)
				{
					customsFeesTotal += charge.B4_ChargeAmount;
				}
				statementLine.B3_CustomsFeesTotal = customsFeesTotal;
			}
			else
			{
				Logger.Log(MissingEntryNumber(message.EM_MessageNum));
			}
		}

		public static string MissingEntryNumber(string messageNum)
		{
			return Res.GetString("eca8e606-f45a-4256-bc4e-2f4a5c1fb049", "STATAC Message: #{0} Entry Number not provided.", messageNum);
		}

		void AddCusStatementLineChargeIfNotExist(STATACMessageSG3Helper sg3Helper, CusStatementLine statementLine)
		{
			var amount = sg3Helper.Amount;
			ZString chargeType;

			if (IsEntryNumLRNFormat(statementLine))
			{
				chargeType = sg3Helper.Type;
			}
			else if (IsEntryNumPRNFormat(statementLine))
			{
				chargeType = STATACMessageSG3Helper.Constants.PaymentType;
			}
			else
			{
				chargeType = STATACMessageSG3Helper.Constants.OtherType;
			}

			var lineCharge = statementLine.Charges.GetChargeLineFor(chargeType, amount);
			if (lineCharge == null)
			{
				lineCharge = statementLine.Charges.AddNew();
				lineCharge.B4_ChargeType = chargeType;
				lineCharge.B4_ChargeAmount = amount;
			}
		}

		public static bool IsEntryNumLRNFormat(CusStatementLine statementLine)
		{
			var entryNum = statementLine.B3_EntryNum;

			if (entryNum.Length == 25)
			{
				var regEx = new System.Text.RegularExpressions.Regex("^" + statementLine.StatementHeader.B2_EntryFilerCode + "[A-Z]{3}[0-9]{14}$");
				if (regEx.IsMatch(entryNum))
				{
					var dateStr = statementLine.B3_EntryNum.Substring(11, 8);

					return ZDateTime.TryParseExact(dateStr, out _, "yyyyMMdd");
				}
			}

			return false;
		}

		public static bool IsEntryNumPRNFormat(CusStatementLine statementLine)
		{
			var entryNum = statementLine.B3_EntryNum;

			if (entryNum.Length == 19 && statementLine.StatementHeader.B2_AccountNo.Length == 10)
			{
				return entryNum.StartsWith(statementLine.StatementHeader.B2_AccountNo, System.StringComparison.Ordinal);
			}

			return false;
		}

		#endregion
	}
}
