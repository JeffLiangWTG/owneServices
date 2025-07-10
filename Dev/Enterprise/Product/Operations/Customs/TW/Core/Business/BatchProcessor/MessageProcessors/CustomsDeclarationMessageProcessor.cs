using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.BusinessObjects.Interfaces;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	class CustomsDeclarationMessageProcessor(LoggingInformation logger) : TWCApplicationTypeMessageProcessor(logger)
	{
		protected override bool ProcessMessageMain(TWMessage message)
		{
			var successful = false;
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				message.EM_MessageInterpretation = TWMessageHelper.NewIncomingHelper(message)?.ToHtml() ?? ZString.Empty;
				UpdateCusEntryHeader(message, entryHeader);
				AddEntryPayInfoFromMessageIfNeeded(message, entryHeader);
				AddCusDispositionIfNeeded(message, entryHeader);
				successful = true;
			}
			message.EM_Status = successful ? TWMessage.Status.ProcessedOK : TWMessage.Status.Discarded;
			return successful;
		}

		void UpdateCusEntryHeader(TWMessage message, CusEntryHeader cusEntryHeader)
		{
			var messageType = message.EM_MessageType;
			UpdateEntryStatus(cusEntryHeader, message.EntryStatus, messageType);
			if (messageType == MessageTypeList.Codes.ERM || messageType == MessageTypeList.Codes.IRM)
			{
				UpdateClearanceStatus(cusEntryHeader, message.ClearanceStatus);
				cusEntryHeader.CH_EntryReleaseDate = message.ReleaseDateTime;
			}
			if (messageType == MessageTypeList.Codes.TPC)
			{
				cusEntryHeader.CH_DutyDueDate = message.DutyDueDateForTPC;
			}
		}

		void UpdateEntryStatus(CusEntryHeader cusEntryHeader, ZString status, ZString messageType)
		{
			if (!status.IsEmpty)
			{
				if (!(messageType == MessageTypeList.Codes.TPC || messageType == MessageTypeList.Codes.TAD) || !cusEntryHeader.Messages.OfType<TWMessage>().Any(x => x.EM_MessageType == MessageTypeList.Codes.IRM))
				{
					cusEntryHeader.CH_EntryStatus = status;
					LogEntryStatusEvent(cusEntryHeader);
				}
			}
		}

		void LogEntryStatusEvent(CusEntryHeader cusEntryHeader)
		{
			if (cusEntryHeader.Declaration is JobDeclaration declaration)
			{
				declaration.Logs.AddNew(Events.CustomsEntryStatus, declaration.JE_EntryStatus);
			}
		}

		void UpdateClearanceStatus(CusEntryHeader cusEntryHeader, ZString status)
		{
			var entryNumber = cusEntryHeader.CusEntryNumber;
			if (entryNumber != null && !status.IsEmpty)
			{
				entryNumber.CE_EntryStatus = status;
				LogClearanceStatusEvent(cusEntryHeader, status);
			}
		}

		void LogClearanceStatusEvent(CusEntryHeader cusEntryHeader, ZString status)
		{
			if (cusEntryHeader.Declaration is JobDeclaration declaration)
			{
				declaration.Logs.AddNew(Events.ClearanceStatusChanged, status);
			}
		}

		#region AddNewEntryPayInfoFromMessages

		void AddEntryPayInfoFromMessageIfNeeded(TWMessage message, CusEntryHeader cusEntryHeader)
		{
			var messageType = message.EM_MessageType;
			if (messageType == MessageTypeList.Codes.TAD)
			{
				if (AddEntryPayInfoFromMessageFromN5111(message, cusEntryHeader))
				{
					LogCustomsReadyToPayEvent(messageType, cusEntryHeader);
				}
			}
			else if (messageType == MessageTypeList.Codes.TPC)
			{
				if (AddEntryPayInfoFromMessageFromN5110(message, cusEntryHeader))
				{
					LogCustomsReadyToPayEvent(messageType, cusEntryHeader);
				}
			}
		}

		void LogCustomsReadyToPayEvent(ZString messageType, CusEntryHeader entryHeader)
		{
			entryHeader.Declaration?.LogsOfDeclarationOrShipment?.AddNew(Events.CustomsReadyToPay, GetCustomsReadyToPayEventReference(messageType));
		}

		ZString GetCustomsReadyToPayEventReference(ZString messageType)
		{
			var result = ZString.Empty;
			if (messageType == MessageTypeList.Codes.TAD)
			{
				result = (NoResString)"N5111 message received";
			}
			else if (messageType == MessageTypeList.Codes.TPC)
			{
				result = (NoResString)"N5110 message received";
			}
			return result;
		}

		bool AddEntryPayInfoFromMessageFromN5110(TWMessage message, CusEntryHeader cusEntryHeader)
		{
			var shouldAddEntryPayInfo = false;
			if (message.IncomingMessageKeyInfomation.Result is CargoWise.Customs.TW.MessageDefinitions.N5110.Response result)
			{
				var declaration = result.Declaration;
				var dutyTaxFeeCollection = declaration?.GoodsShipment?.DutyTaxFee;
				var payment = declaration?.DutyTaxFee?.Payment;
				if (dutyTaxFeeCollection != null && payment != null)
				{
					var entryPayInfos = cusEntryHeader.EntryPayInfos;
					var incomingPayResponseNo = payment.ReferenceId?.Value ?? ZString.Empty;
					if (ZDateTime.TryParseExact(payment.DueDateTime, out var dueDateTime, "yyyy-MM-dd"))
					{
						shouldAddEntryPayInfo = !entryPayInfos.Cast<CusEntryPayInfo>().Any(x => x.C9_IncomingPayResponseNo == incomingPayResponseNo && x.C9_PaymentDate > dueDateTime);
						if (shouldAddEntryPayInfo)
						{
							RemoveExistEntryPayInfoFromMessage(entryPayInfos, incomingPayResponseNo);
							var paymentReasonCode = payment.TwIssueReasonCode?.Value ?? ZString.Empty;
							var bankAccountInfo = result.BankAccount;
							var paymentReference = bankAccountInfo?.Id?.Value ?? ZString.Empty;
							var bankAccount = bankAccountInfo?.ReferenceId?.Value ?? ZString.Empty;
							var paymentParty = cusEntryHeader.Declaration?.JE_PaidBy ?? ZString.Empty;
							var otherChargeDeductionAmount = declaration?.GoodsShipment?.CustomsValuation?.OtherChargeDeductionAmount?.Value ?? ZDecimal.Zero;

							foreach (var fee in dutyTaxFeeCollection)
							{
								var newEntryPayInfo = entryPayInfos.AddNew();
								newEntryPayInfo.C9_PaymentAmount = fee.AdValoremTaxBaseAmount?.Value ?? ZDecimal.Zero;
								newEntryPayInfo.C9_TransactionType = fee.TypeCode?.Value ?? ZString.Empty;
								newEntryPayInfo.C9_IncomingPayResponseNo = incomingPayResponseNo;
								newEntryPayInfo.C9_PaymentDate = dueDateTime;
								newEntryPayInfo.C9_PaymentReference = paymentReference;
								newEntryPayInfo.C9_PaymentParty = paymentParty;
								newEntryPayInfo.C9_BankAccount = bankAccount;
								newEntryPayInfo.C9_PaymentReasonCode = paymentReasonCode;
								newEntryPayInfo.C9_ReceiptDate = ZDate.Today;
								newEntryPayInfo.OtherChargeDeductionAmount = otherChargeDeductionAmount;
							}
						}
					}
				}
			}
			return shouldAddEntryPayInfo;
		}

		bool AddEntryPayInfoFromMessageFromN5111(TWMessage message, CusEntryHeader cusEntryHeader)
		{
			var shouldAddEntryPayInfo = false;
			if (message.IncomingMessageKeyInfomation.Result is CargoWise.Customs.TW.MessageDefinitions.N5111.Response result)
			{
				var declaration = result.Declaration;
				var payment = declaration?.DutyTaxFee?.Payment;
				if (payment != null)
				{
					ZString incomingPayResponseNo = payment.ReferenceId?.Value ?? ZString.Empty;
					var depositTypeCode = payment.TwDepositTypeCode?.Value;
					if (!incomingPayResponseNo.IsEmpty)
					{
						var entryPayInfos = cusEntryHeader.EntryPayInfos;
						if (ZDateTime.TryParseExact(result.IssueDateTime, out var parseResult, "yyyy-MM-dd"))
						{
							var issueDateTime = parseResult.Date;
							shouldAddEntryPayInfo = !entryPayInfos.Cast<CusEntryPayInfo>().Any(x => x.C9_IncomingPayResponseNo == incomingPayResponseNo && x.C9_ReceiptDate > issueDateTime);
							if (shouldAddEntryPayInfo)
							{
								var bankAccountInfo = result.BankAccount;
								RemoveExistEntryPayInfoFromMessage(entryPayInfos, incomingPayResponseNo);
								var newEntryPayInfo = entryPayInfos.AddNew();
								newEntryPayInfo.C9_IncomingPayResponseNo = incomingPayResponseNo;
								newEntryPayInfo.C9_PaymentAmount = payment.PaymentAmount?.Value ?? ZDecimal.Zero;
								newEntryPayInfo.C9_TransactionType = depositTypeCode;
								newEntryPayInfo.C9_PaymentDate = issueDateTime.AddDays(14);
								newEntryPayInfo.C9_PaymentReference = bankAccountInfo?.Id?.Value;
								newEntryPayInfo.C9_PaymentParty = cusEntryHeader.Declaration?.JE_PaidBy ?? ZString.Empty;
								newEntryPayInfo.C9_ReceiptDate = issueDateTime;
								newEntryPayInfo.C9_BankAccount = bankAccountInfo?.ReferenceId?.Value;
								newEntryPayInfo.C9_PaymentReasonCode = payment.TwIssueReasonCode?.Value ?? ZString.Empty;
							}
						}
					}
				}
			}
			return shouldAddEntryPayInfo;
		}

		void RemoveExistEntryPayInfoFromMessage(ICusEntryPayInfoCollection<CusEntryPayInfo> entryPayInfos, ZString incomingPayResponseNo)
		{
			var existEntryPayInfos = entryPayInfos.Cast<CusEntryPayInfo>().Where(x => x.C9_IncomingPayResponseNo == incomingPayResponseNo).ToList();
			existEntryPayInfos.ForEach(x => entryPayInfos.RemoveAndDelete(x));
		}
		#endregion

		void AddCusDispositionIfNeeded(TWMessage twmessage, CusEntryHeader cusEntryHeader)
		{
			switch (twmessage.EM_MessageType)
			{
				case MessageTypeList.Codes.RFM:
					AddCusDispositionFromN5107(twmessage, cusEntryHeader);
					break;
				case MessageTypeList.Codes.ARM:
					AddCusDispositionFromNX5106(twmessage, cusEntryHeader);
					break;
				case MessageTypeList.Codes.ERM:
					AddCusDispositionFromN5204(twmessage, cusEntryHeader);
					break;
				case MessageTypeList.Codes.IRM:
					AddCusDispositionFromN5116(twmessage, cusEntryHeader);
					break;
			}
		}

		void AddCusDispositionFromN5107(TWMessage message, CusEntryHeader cusEntryHeader)
		{
			if (message.IncomingMessageKeyInfomation.Result is CargoWise.Customs.TW.MessageDefinitions.N5107.Response result)
			{
				var statusKey = CusDispositionStatusKeyList.Codes.RFM;
				var dispositions = cusEntryHeader.CusDispositions;
				dispositions.Load();
				var validationCodes = message.GoodsShipmentValidationCode;
				RemoveExistDispositionsFromMessage(dispositions, statusKey);
				validationCodes.ForEach(validationCode => AddCusDisposition(dispositions, statusKey, validationCode, result.IssueDateTime));
			}
		}

		void AddCusDispositionFromNX5106(TWMessage message, CusEntryHeader cusEntryHeader)
		{
			if (message.IncomingMessageKeyInfomation.Result is CargoWise.Customs.TW.MessageDefinitions.NX5106.Response result)
			{
				var statusKey = CusDispositionStatusKeyList.Codes.ARM;
				var dispositions = cusEntryHeader.CusDispositions;
				dispositions.Load();
				var nameCodes = message.GoodsShipmentNameCode;
				RemoveExistDispositionsFromMessage(dispositions, statusKey);
				nameCodes.ForEach(nameCode => AddCusDisposition(dispositions, statusKey, nameCode, result.Declaration.AdditionalInformation?.TwIssueDateTime ?? ZString.Empty));
			}
		}

		void AddCusDispositionFromN5204(TWMessage message, CusEntryHeader cusEntryHeader)
		{
			if (message.IncomingMessageKeyInfomation.Result is CargoWise.Customs.TW.MessageDefinitions.N5204.Response result)
			{
				var statusKey = CusDispositionStatusKeyList.Codes.CLR;
				var dispositions = cusEntryHeader.CusDispositions;
				dispositions.Load();
				var statementCodes = message.StatementCode;
				RemoveExistDispositionsFromMessage(dispositions, statusKey);
				statementCodes.ForEach(statementCode => AddCusDisposition(dispositions, statusKey, statementCode, result.Status?.ReleaseDateTime ?? ZString.Empty));
			}
		}

		void AddCusDispositionFromN5116(TWMessage message, CusEntryHeader cusEntryHeader)
		{
			if (message.IncomingMessageKeyInfomation.Result is CargoWise.Customs.TW.MessageDefinitions.N5116.Response result)
			{
				var statusKey = CusDispositionStatusKeyList.Codes.CLR;
				var dispositions = cusEntryHeader.CusDispositions;
				dispositions.Load();
				var statementCodes = message.StatementCode;
				RemoveExistDispositionsFromMessage(dispositions, statusKey);
				statementCodes.ForEach(statementCode => AddCusDisposition(dispositions, statusKey, statementCode, result.Status?.ReleaseDateTime ?? ZString.Empty));
			}
		}

		void AddCusDisposition(CusDispositionCollection dispositions, ZString statusKey, ZString statusCode, ZString issueDateTimeString)
		{
			if (ZDateTime.TryParseExact(issueDateTimeString, out var issueDateTime, (NoResString)"yyyy-MM-ddTHH:mm:ss") && !issueDateTime.IsEmpty)
			{
				var newDisposition = dispositions.AddNew();
				newDisposition.CDI_Type = Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				newDisposition.CDI_StatusKey = statusKey;
				newDisposition.CDI_Status = statusCode;
				newDisposition.CDI_StatusDate = issueDateTime;
				newDisposition.CDI_Notes = ZString.Empty;
			}
		}

		void RemoveExistDispositionsFromMessage(CusDispositionCollection cusDispositions, ZString statusKey)
		{
			var existDispositions = cusDispositions.Cast<CusDisposition>().Where(x => x.CDI_Type == Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber && x.CDI_StatusKey == statusKey).ToList();
			existDispositions.ForEach(cusDispositions.RemoveAndDelete);
		}

		public override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObject(TWMessage message)
		{
			MultilingualString discardReason = (NoResString)string.Empty;
			var linkedObject = TWMessageHelper.LookForCusEntryHeader(message.Factory, message);
			if (linkedObject == null)
			{
				discardReason = GetUnableToFindTheLinkedJobMessage(message);
				LogNotFindEntryHeaderLogWarning(message);
			}
			return (message.EM_GB, linkedObject, discardReason);
		}
	}
}
