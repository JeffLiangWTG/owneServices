using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public static class DeclarationMessageHelper
	{
		public static (IEnumerable<HtmlTableCreator> htmlTables, ZString title, bool isSucceed) ReadAnswerAndUpdateEntry(CusEntryHeader entryHeader, InnerXmlControlAnswerObject answerObject)
		{
			var isSuccess = true;
			var title = ZString.Empty;
			var htmlTables = new List<HtmlTableCreator>();

			entryHeader.CH_Status = TRMessageStatusCodeList.Codes.Accepted;
			entryHeader.CH_EntryStatus = GetEntryStatusWithAnswer(entryHeader.CH_EntryStatus, answerObject);

			if (answerObject.Errors.Count > 0)
			{
				isSuccess = false;
				var htmlTableCreatorByErrors = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_ErrorCodeAndDescription);
				foreach (var error in answerObject.Errors)
				{
					htmlTableCreatorByErrors.WriteRow(new string[] { error.Code.ToString(), error.Description });
				}
				if (title.IsEmpty)
				{
					title = Res.GetString("BBFC855C-18EC-4A6B-95F4-C5E0170D3B1D", "Customs Declaration message for job {0} has been rejected.", entryHeader?.Declaration?.JobNumber ?? ZString.Empty);
				}

				htmlTables.Add(htmlTableCreatorByErrors);
			}
			else
			{
				var messageLabelContent = ZString.Empty;
				if (answerObject.Questions.Count > 0)
				{
					messageLabelContent = (NoResString)"Entry Questions,";
					var htmlTableCreatorByQuestions = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.TableHeaderWithQuestionsFields);

					foreach (var question in answerObject.Questions)
					{
						htmlTableCreatorByQuestions.WriteRow(new string[] { question.Code.ToString(), question.Description, question.LineNumber.ToString(), question.Type });
						CreateEntryQuestionsContent(entryHeader, question.Code, question.Description, question.LineNumber, question.Type);
					}

					htmlTables.Add(htmlTableCreatorByQuestions);
				}

				if (answerObject.Documents.Count > 0)
				{
					messageLabelContent = messageLabelContent + (NoResString)"Documents,";
					var htmlTableCreatorByDocuments = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.TableHeaderWithDocumentsFields);

					var documents = answerObject.Documents.Where(x => x.Code != TRMessageConstants.IncorrectSupportingCodeReceivedByCustoms0103);

					foreach (var document in documents)
					{
						htmlTableCreatorByDocuments.WriteRow(new string[] { document.LineNumber.ToString(), document.Code, document.Description });
						CreateDocumentContent(entryHeader, document.LineNumber, document.Code);
					}

					htmlTables.Add(htmlTableCreatorByDocuments);
				}

				if (answerObject.Taxes.Count > 0)
				{
					messageLabelContent = messageLabelContent + (NoResString)"Taxes,";
					var htmlTableCreatorByTaxes = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.TableHeaderWithTaxesFields);

					foreach (var tax in answerObject.Taxes)
					{
						htmlTableCreatorByTaxes.WriteRow(new string[] { tax.LineNumber.ToString(), tax.Code, tax.Description, tax.Amount.ToString(), tax.Rate.ToString(), tax.PaymentType, tax.BaseAmount.ToString() });
						CreateOrUpdateTaxesContent(entryHeader, tax.LineNumber, tax.Code, tax.Amount, tax.Rate, tax.PaymentType, tax.BaseAmount);
					}

					htmlTables.Add(htmlTableCreatorByTaxes);
				}

				if (title.IsEmpty)
				{
					title = Res.GetString("25313D52-E524-4222-87ED-1EAABD373B87", "Customs Declaration message for job {0} has {1}.", entryHeader?.Declaration?.JobNumber ?? ZString.Empty, messageLabelContent.TrimEnd(','));
				}
			}

			return (htmlTables, title, isSuccess);
		}

		public static ZString GetEntryStatusWithAnswer(string currentEntryStatus, InnerXmlControlAnswerObject answerObject)
		{
			if (answerObject.Errors.Count > 0)
			{
				return currentEntryStatus;
			}

			if (answerObject.Questions.Any(q =>
					q.Type == InnerXmlControlAnswerObject.ConstantsTagName.QuestionType.Question &&
					q.Answers.PositiveAnswers.Count == 0 &&
					q.Answers.NegativeAnswers.Count == 0))
			{
				return EntryStatusTypeList.Codes.QUE;
			}

			if (answerObject.Documents.Any(d => string.IsNullOrEmpty(d.Verification)))
			{
				return EntryStatusTypeList.Codes.SDO;
			}

			if (answerObject.Questions.Any(q =>
					q.Type == InnerXmlControlAnswerObject.ConstantsTagName.QuestionType.Description &&
					q.Answers.PositiveAnswers.Count == 0 &&
					q.Answers.NegativeAnswers.Count == 0))
			{
				return EntryStatusTypeList.Codes.WRN;
			}

			return currentEntryStatus;
		}

		public static ZString GetEntryStatus(this IMessageAttachee attachee, string messageType)
		{
			var result = EntryStatusTypeList.Codes.NOS;
			var header = attachee as CusEntryHeader;

			if (header == null)
			{
				return result;
			}

			var messageStatus = header.CH_Status;
			var currentEntryStatus = header.CH_EntryStatus;

			if (messageStatus == TRMessageStatusCodeList.Codes.NotSent)
			{
				return EntryStatusTypeList.Codes.NOS;
			}

			if (messageStatus == TRMessageStatusCodeList.Codes.Error)
			{
				return currentEntryStatus;
			}

			if (StatusEntryStatusMapping.TryGetValue($"{messageType}.{messageStatus}", out var entryStatus))
			{
				return entryStatus;
			}

			if (messageStatus == TRMessageStatusCodeList.Codes.Accepted)
			{
				return EntryStatusTypeList.Codes.REG;
			}

			return result;
		}

		public static Dictionary<string, string> StatusEntryStatusMapping =
			new Dictionary<string, string>
			{
				{ $"{nameof(TRMessageTypes.Codes.DKO)}.{TRMessageStatusCodeList.Codes.Sent}", EntryStatusTypeList.Codes.SCM },
				{ $"{nameof(TRMessageTypes.Codes.DKO)}.{TRMessageStatusCodeList.Codes.Accepted}", EntryStatusTypeList.Codes.ACM },
				{ $"{nameof(TRMessageTypes.Codes.DK1)}.{TRMessageStatusCodeList.Codes.Sent}", EntryStatusTypeList.Codes.QCM },
				{ $"{nameof(TRMessageTypes.Codes.DK1)}.{TRMessageStatusCodeList.Codes.Accepted}", EntryStatusTypeList.Codes.CMR },
				{ $"{nameof(TRMessageTypes.Codes.DTE)}.{TRMessageStatusCodeList.Codes.Sent}", EntryStatusTypeList.Codes.SRM },
				{ $"{nameof(TRMessageTypes.Codes.DTE)}.{TRMessageStatusCodeList.Codes.Accepted}", EntryStatusTypeList.Codes.RMA },
				{ $"{nameof(TRMessageTypes.Codes.DT1)}.{TRMessageStatusCodeList.Codes.Sent}", EntryStatusTypeList.Codes.QRM },
				{ $"{nameof(TRMessageTypes.Codes.DT1)}.{TRMessageStatusCodeList.Codes.Accepted}", EntryStatusTypeList.Codes.RMR },
				{ $"{nameof(TRMessageTypes.Codes.DT2)}.{TRMessageStatusCodeList.Codes.Sent}", EntryStatusTypeList.Codes.QUR },
				{ $"{nameof(TRMessageTypes.Codes.DT2)}.{TRMessageStatusCodeList.Codes.Accepted}", EntryStatusTypeList.Codes.QUA },
				{ $"{nameof(TRMessageTypes.Codes.DT3)}.{TRMessageStatusCodeList.Codes.Sent}", EntryStatusTypeList.Codes.GQR },
				{ $"{nameof(TRMessageTypes.Codes.DT3)}.{TRMessageStatusCodeList.Codes.Accepted}", EntryStatusTypeList.Codes.AGQ },
				{ $"{nameof(TRMessageTypes.Codes.EUR)}.{TRMessageStatusCodeList.Codes.Accepted}", EntryStatusTypeList.Codes.REG },
			};

		public static void CreateEntryQuestionsContent(CusEntryHeader entryHeader, ZInt questionCode, ZString description, ZInt lineNumber, ZString entryQuestionType)
		{
			if ((entryQuestionType == InnerXmlControlAnswerObject.ConstantsTagName.QuestionType.Question ||
				 entryQuestionType == InnerXmlControlAnswerObject.ConstantsTagName.QuestionType.Description) &&
				!questionCode.IsEmpty)
			{
				var questionType = entryQuestionType == InnerXmlControlAnswerObject.ConstantsTagName.QuestionType.Question ? QuestionTypeList.Codes.Q : QuestionTypeList.Codes.W;
				UniversalReferenceDataHelper.GatherRefCusCodeList(entryHeader.Factory, questionCode.ToString(), description, questionType);

				if (lineNumber == 0)
				{
					if (!entryHeader.CPDecCollection.Cast<CusEntryCPDec>().Any(x => x.ON_CPDecNum == questionCode))
					{
						var headerCPDec = entryHeader.CPDecCollection.AddNew();
						headerCPDec.ON_CPDecNum = questionCode;
						headerCPDec.ON_QuestionType = questionType;
					}
				}
				else
				{
					var cusEntryLine = entryHeader.AllEntryLines.FindByLineNumber(lineNumber);
					if (cusEntryLine != null)
					{
						if (!cusEntryLine.CPDecCollection.Cast<CusEntryCPDec>().Any(x => x.ON_CPDecNum == questionCode))
						{
							var lineCPDec = cusEntryLine.CPDecCollection.AddNew();
							lineCPDec.ON_CPDecNum = questionCode;
							lineCPDec.ON_QuestionType = questionType;
						}
					}
				}
			}
		}

		public static void CreateDocumentContent(CusEntryHeader entryHeader, ZInt lineNumber, ZString documentCode)
		{
			var cusEntryLine = entryHeader.AllEntryLines.FindByLineNumber(lineNumber);
			if (cusEntryLine != null)
			{
				var invoiceLine = cusEntryLine.RandomLine;
				if (invoiceLine != null && !invoiceLine.SupportingDocuments.OfType<SupportingDocument>().Any(supportingDocument => supportingDocument.CSI_Code == documentCode))
				{
					var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
					supportingDocument.CSI_Code = documentCode;
				}
			}
		}

		public static void CreateOrUpdateTaxesContent(CusEntryHeader entryHeader, ZInt lineNumber, ZString nationalFeeTypeCode, ZDecimal amount, ZDecimal rate, ZString paymentType, ZDecimal baseAmount)
		{
			var cusEntryLine = entryHeader.AllEntryLines.FindByLineNumber(lineNumber);
			if (cusEntryLine != null)
			{
				ZString chargeTypeCode = DeclarationHelper.NationalVatType.Code == nationalFeeTypeCode ? Customs.Business.FeeTypeList.Codes.B00 : nationalFeeTypeCode;
				var entryLineFee = cusEntryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == chargeTypeCode);
				if (entryLineFee == null)
				{
					entryLineFee = cusEntryLine.Fees.AddNew() as CusEntryLineFee;
					entryLineFee.CF_ChargeType = chargeTypeCode;
					entryLineFee.NationalFeeTypeCode = nationalFeeTypeCode;
					entryLineFee.CF_RateOverrideReasonCode = EU.Business.RateOverrideReasonList.Codes.Additional;
				}
				else
				{
					entryLineFee.CF_RateOverrideReasonCode = EU.Business.RateOverrideReasonList.Codes.Override;
				}

				entryLineFee.CF_Source = Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CW1;
				entryLineFee.CF_MethodOfCalculation = MethodOfCalculationList.Codes.Gumruk;
				entryLineFee.CF_MethodOfPayment = paymentType;
				entryLineFee.CF_BaseValue = baseAmount;
				entryLineFee.CF_Rate = rate;
				entryLineFee.CF_ChargeAmount = amount;

				CreateOrUpdateLinkedInvoiceLines(entryLineFee);
			}
		}

		public static void CreateOrUpdateLinkedInvoiceLines(CusEntryLineFee entryLineFee)
		{
			var invoiceLineTax = (JobComInvoiceLineTax)null;
			var taxBaseAmount = ZDecimal.Zero;
			var taxAmount = ZDecimal.Zero;
			var chargeTypeCode = entryLineFee.CF_ChargeType;
			var remainingBaseValue = entryLineFee.CF_BaseValue;
			var remainingChargeAmount = entryLineFee.CF_ChargeAmount;
			var taxRate = entryLineFee.CF_Rate;
			var entryLine = entryLineFee.EntryLine;
			var totalInvoiceAmount = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(invoiceLine => invoiceLine.JI_CustomsValue);
			if (totalInvoiceAmount > 0)
			{
				foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
				{
					invoiceLineTax = invoiceLine.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault(tax => tax.JLT_Type == chargeTypeCode);
					if (invoiceLineTax == null)
					{
						invoiceLineTax = invoiceLine.Taxes.AddNew();
						invoiceLineTax.JLT_Type = chargeTypeCode;
						invoiceLineTax.NationalType = entryLineFee.NationalFeeTypeCode;
						invoiceLineTax.JLT_RateOverrideReasonCode = EU.Business.RateOverrideReasonList.Codes.Additional;
					}
					else
					{
						invoiceLineTax.JLT_RateOverrideReasonCode = EU.Business.RateOverrideReasonList.Codes.Override;
					}

					using (invoiceLineTax.SuspendOnCalculation())
					{
						invoiceLineTax.JLT_MethodOfPayment = entryLineFee.CF_MethodOfPayment;
						invoiceLineTax.JLT_MethodOfCalculation = entryLineFee.CF_MethodOfCalculation;

						taxBaseAmount = ZDecimal.Zero;
						taxAmount = ZDecimal.Zero;
						if (entryLineFee.CF_BaseValue > 0)
						{
							taxBaseAmount = (invoiceLine.JI_CustomsValue / totalInvoiceAmount) * entryLineFee.CF_BaseValue;
							taxAmount = taxRate > 0 ? (taxBaseAmount / 100) * taxRate : ZDecimal.Zero;
						}
						else
						{
							taxAmount = (invoiceLine.JI_CustomsValue / totalInvoiceAmount) * entryLineFee.CF_ChargeAmount;
							taxBaseAmount = taxRate > 0 ? taxAmount / (taxRate / 100) : ZDecimal.Zero;
						}

						invoiceLineTax.JLT_BaseValue = taxBaseAmount.Round(2);
						invoiceLineTax.JLT_Rate = taxRate.Round(2);
						invoiceLineTax.JLT_Amount = taxAmount.Round(2);
					}
					remainingBaseValue -= invoiceLineTax.JLT_BaseValue;
					remainingChargeAmount -= invoiceLineTax.JLT_Amount;
				}

				#region Remaining - Last record

				if (invoiceLineTax != null && (remainingBaseValue > 0 || remainingChargeAmount > 0))
				{
					using (invoiceLineTax.SuspendOnCalculation())
					{
						if (remainingBaseValue > 0) { invoiceLineTax.JLT_BaseValue += remainingBaseValue; }
						if (remainingChargeAmount > 0) { invoiceLineTax.JLT_Amount += remainingChargeAmount; }
					}
				}

				#endregion
			}
		}

		public static ZBool GetIsNotWRNorSDOrQUE(this IMessageAttachee attachee, string currentEntryStatus)
		{
			return !(currentEntryStatus == EntryStatusTypeList.Codes.WRN || currentEntryStatus == EntryStatusTypeList.Codes.SDO || currentEntryStatus == EntryStatusTypeList.Codes.QUE);
		}
	}
}
