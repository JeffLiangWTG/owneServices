using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	public abstract class AutomatedClearinghouseMessageProcessor : ACSABIProcessor
	{
		public sealed override void Process()
		{
			var statement = OriginalMessageLinker.Link<CusStatementHeader>(Message);
			GlbBranch branch;

			var originalMessage = Message.OriginalMessage;
			var paymentBlock = originalMessage != null ? originalMessage.MessageBlock.MessageBlocks.OfType<IPaymentAuthorisation>().FirstOrDefault() : null;
			var responses = new List<Tuple<ZString, ZString, ZString, ZDecimal, ZDate, ZString, ZString>>();
			var paymentAuthorizationDate = ZDate.Empty;
			foreach (var block in messageBlocks)
			{
				var paymentResponse = block as IPaymentAuthorisationResponse;
				if (paymentResponse != null)
				{
					if (statement == null && !paymentResponse.StatementFiler.IsEmpty && !paymentResponse.StatementBillNumber.IsEmpty)
					{
						branch = Message.Branch ?? Message.Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
						if (branch != null)
						{
							statement = new CusStatementHeader.Loader(Message.Factory).LoadWithStatementNumber(paymentResponse.StatementFiler, paymentResponse.StatementBillNumber, branch.GB_GC);
						}
					}

					if (paymentAuthorizationDate.IsEmpty && !paymentResponse.DateAccepted.IsEmpty)
					{
						paymentAuthorizationDate = paymentResponse.DateAccepted;
					}

					var paymentResponseDetail = new PaymentResponseDetail(statement, paymentBlock, paymentResponse, MessageCalculator);
					responses.Add(new Tuple<ZString, ZString, ZString, ZDecimal, ZDate, ZString, ZString>(paymentResponseDetail.StatementNumber, paymentResponseDetail.StatementFiler, paymentResponse.PaymentFiler, paymentResponseDetail.PaymentAmount, paymentAuthorizationDate, paymentResponseDetail.AcceptanceErrorDetail, paymentResponseDetail.PayersUnitNumber));
				}
			}

			var isFailure = !messageBlocks.OfType<IPaymentAuthorisationResponse>().Any(x => x.DateAccepted.IsValid || x.DispositionTypeCode == ACESeverityList.Codes.Accepted);
			CalculatePaymentStatus(statement, paymentBlock, isFailure, paymentAuthorizationDate);

			var url = statement == null ? "" : ObjectFactory.Get<IShowEditFormUrlCreator>().Create(statement);
			var jobNumber = statement == null ? ZString.Empty : statement.B2_StatementNumber;
			branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumber, MessageTypeDescription, GetBodyDetail(responses, statement != null), isFailure, branch, statement);
		}

		void CalculatePaymentStatus(CusStatementHeader statement, IPaymentAuthorisation paymentBlock, ZBool isFailure, ZDateTime paymentAuthorizationDate)
		{
			if (statement != null)
			{
				statement.ServiceTaskLogger = Logger;
				var isNegation = paymentBlock != null && paymentBlock.NegationCode == YesNoDefaultList.Codes.Yes;
				if (isFailure)
				{
					if (isNegation)
					{
						statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationDeletionFailed;
					}
					else
					{
						statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentFailed;
					}
				}
				else
				{
					if (isNegation)
					{
						statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationDeleted;
					}
					else
					{
						statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
					}
				}

				if (!isFailure && !isNegation)
				{
					statement.PaymentDateChangedByMessageProcessor = true;
					statement.B2_PaymentAuthorizationDate = paymentAuthorizationDate;
				}
			}
		}

		string GetBodyDetail(List<Tuple<ZString, ZString, ZString, ZDecimal, ZDate, ZString, ZString>> responses, bool statementFound)
		{
			var creator = new HtmlTableCreator(new string[] {
				"Statement/Bill No.",
				"Statement Filer",
				"Payment Filer",
				"Payment Amount",
				"Date Accepted",
				"Acceptance/Error Details",
				"Payer's Unit No." });

			foreach (Tuple<ZString, ZString, ZString, ZDecimal, ZDate, ZString, ZString> qu in responses)
			{
				creator.WriteRow(new object[] {
				qu.Item1,
				qu.Item2,
				qu.Item3,
				qu.Item4,
				qu.Item5,
				qu.Item6,
				qu.Item7 });
			}

			var statementNotFoundError = "";

			if (!statementFound)
			{
				statementNotFoundError = "<br><br>There is no statement record found this message is responding to";
			}

			return creator.ToHtml() + statementNotFoundError;
		}

		protected abstract string MessageTypeDescription { get; }

		class PaymentResponseDetail
		{
			public PaymentResponseDetail(CusStatementHeader statement, IPaymentAuthorisation paymentAuthorisation, IPaymentAuthorisationResponse paymentAuthorisationResponse, MessageErrorCalculator messageErrorCalculator)
			{
				this._statement = statement;
				this._paymentAuthorisation = paymentAuthorisation;
				this._paymentAuthorisationResponse = paymentAuthorisationResponse;
				this._messageErrorCalculator = messageErrorCalculator;
			}
			readonly CusStatementHeader _statement;
			readonly IPaymentAuthorisation _paymentAuthorisation;
			readonly IPaymentAuthorisationResponse _paymentAuthorisationResponse;
			readonly MessageErrorCalculator _messageErrorCalculator;

			public ZString StatementNumber
			{
				get
				{
					return _paymentAuthorisationResponse != null && !_paymentAuthorisationResponse.StatementBillNumber.IsEmpty
					  ? _paymentAuthorisationResponse.StatementBillNumber : _statement != null ? _statement.B2_StatementNumber : ZString.Empty;
				}
			}

			public ZString StatementFiler
			{
				get
				{
					return _paymentAuthorisationResponse != null && !_paymentAuthorisationResponse.StatementFiler.IsEmpty
						? _paymentAuthorisationResponse.StatementFiler : _statement != null ? _statement.B2_EntryFilerCode : ZString.Empty;
				}
			}

			public ZString PaymentFiler
			{
				get { return _paymentAuthorisationResponse != null && !_paymentAuthorisationResponse.PaymentFiler.IsEmpty ? _paymentAuthorisationResponse.PaymentFiler : ZString.Empty; }
			}

			public ZDecimal PaymentAmount
			{
				get
				{
					return _paymentAuthorisationResponse != null && !_paymentAuthorisationResponse.PaymentAmount.IsEmpty
						? _paymentAuthorisationResponse.PaymentAmount : _paymentAuthorisation != null ? _paymentAuthorisation.PaymentAmount : ZDecimal.Zero;
				}
			}

			public ZString PayersUnitNumber
			{
				get
				{
					return _paymentAuthorisationResponse != null && !_paymentAuthorisationResponse.PayerUnitNumber.IsEmpty
						? _paymentAuthorisationResponse.PayerUnitNumber : _paymentAuthorisation != null ? _paymentAuthorisation.PayersUnitNumber : ZString.Empty;
				}
			}

			public ZString AcceptanceErrorDetail
			{
				get { return _paymentAuthorisationResponse != null ? _paymentAuthorisationResponse.AcceptanceErrorCode + " - " + _messageErrorCalculator.GetLongDescription(_paymentAuthorisationResponse.AcceptanceErrorCode, _paymentAuthorisationResponse.AcceptanceErrorMessage) : string.Empty; }
			}
		}
	}
}
