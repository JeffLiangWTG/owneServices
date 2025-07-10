namespace Enterprise.Customs.NZ.Business
{
	using System;
	using System.Globalization;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Schema;

	[Serializable]
	class DiagnosticMessageException : ApplicationException
	{
		public string FailStatus { get; private set; }

		public DiagnosticMessageException(string failStatus, string message)
			: base(message)
		{
			FailStatus = failStatus;
		}

#if NETFRAMEWORK
		protected DiagnosticMessageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	class DiagnosticCycleController
	{
		public DiagnosticCycleController(ZGuid testEdiMessagePk, ZDateTime messageCreateTime, string companyNzBrokerageId)
		{
			this.testEdiMessagePk = testEdiMessagePk;
			this.messageCreateTime = messageCreateTime;
			this.companyNzBrokerageId = companyNzBrokerageId;
		}

		public string CheckNextDiagnosticCycleStep(string currentStatus)
		{
			var result = currentStatus;
			var newFactory = GetNewBusinessObjectFactory();

			switch (currentStatus)
			{
				case DiagnosticStatusListNZ.Codes.TestMessageAtQUEStatus:
					if (CheckIfEdiMessageStatusChangedFromQUE(newFactory))
					{
						result = DiagnosticStatusListNZ.Codes.LookingForOutgoingItem;
					}

					break;

				case DiagnosticStatusListNZ.Codes.LookingForOutgoingItem:
					if (CheckOutInterchangeSent(newFactory))
					{
						result = DiagnosticStatusListNZ.Codes.OutgoingItemAtQUEStatus;
					}

					break;

				case DiagnosticStatusListNZ.Codes.OutgoingItemAtQUEStatus:
					result = DiagnosticStatusListNZ.Codes.WaitingForTestMessageResponse;
					break;

				case DiagnosticStatusListNZ.Codes.WaitingForTestMessageResponse:
					if (CheckInboundResponseReceived(newFactory))
					{
						result = DiagnosticStatusListNZ.Codes.InComingResponseReceived;
					}

					break;

				case DiagnosticStatusListNZ.Codes.InComingResponseReceived:
					if (CheckInterchangeHasGeneratedMessages(newFactory))
					{
						result = DiagnosticStatusListNZ.Codes.InInterchangeProcessed;
					}

					break;

				case DiagnosticStatusListNZ.Codes.InInterchangeProcessed:
					if (CheckResponseEdiMessageProcessed(newFactory))
					{
						result = DiagnosticStatusListNZ.Codes.OKSuccess;
					}

					break;
			}

			return result;
		}

		bool CheckIfEdiMessageStatusChangedFromQUE(BusinessObjectFactory factory)
		{
			bool result = false;

			var messageToTrack = factory.Load<EDIMessage>(new ZGuid(testEdiMessagePk));

			if (messageToTrack == null)
			{
				throw new ApplicationException("Test Message to track cannot be found");
			}
			else if (messageToTrack.EM_Status == EDIMessage.Status.Sent)
			{
				if (messageToTrack.Interchange == null)
				{
					throw new DiagnosticMessageException(DiagnosticStatusListNZ.Codes.NoOutInterchange, "");
				}
				else
				{
					outInterchangePk = messageToTrack.Interchange.PK;
					result = true;
				}
			}
			else if (messageToTrack.EM_Status != EDIMessage.Status.Queued)
			{
				throw new DiagnosticMessageException(DiagnosticStatusListNZ.Codes.TestMessageStatusInvalid, string.Format("Test Message status not set to expected SNT, but is {0}", messageToTrack.EM_Status));
			}

			return result;
		}

		bool CheckOutInterchangeSent(BusinessObjectFactory factory)
		{
			bool result = false;

			var outInterchange = factory.Load<EDIInterchange>(new ZGuid(outInterchangePk));

			if (outInterchange == null)
			{
				throw new DiagnosticMessageException("FAL", "Out Interchange cannot be found");
			}
			else if (outInterchange.EI_Status == EDIInterchange.Status.Sent)
			{
				result = true;
			}
			else if (outInterchange.EI_Status == EDIInterchange.Status.SyntaxRejected)
			{
				throw new DiagnosticMessageException(DiagnosticStatusListNZ.Codes.OutInterchangeRejected, "");
			}
			else if (outInterchange.EI_Status != EDIInterchange.Status.Queued && outInterchange.EI_Status != EDIInterchange.Status.eHubQueued && outInterchange.EI_Status != EDIInterchange.Status.eHubPending)
			{
				throw new DiagnosticMessageException(DiagnosticStatusListNZ.Codes.OutInterchangeStatusInvalid, string.Format(CultureInfo.CurrentCulture, "Out Interchange status not set to expected QUE or SNT, but is {0}", outInterchange.EI_Status));
			}

			return result;
		}

		bool CheckInboundResponseReceived(BusinessObjectFactory factory)
		{
			bool result = false;
			var replyInterchange = LoadReplyEdiInterchange(factory);
			if (replyInterchange != null)
			{
				result = true;
			}

			return result;
		}

		bool CheckInterchangeHasGeneratedMessages(BusinessObjectFactory factory)
		{
			bool result = false;
			var interchangeMessages = LoadReplyEdiInterchange(factory)?.ContainedMessages;
			if (interchangeMessages != null)
			{
				if (interchangeMessages.Count >= 1)
				{
					result = interchangeMessages[0] != null;
				}
			}
			else
			{
				throw new DiagnosticMessageException("FAL", "The interchange timed out, or doesn't exist.");
			}

			return result;
		}

		bool CheckResponseEdiMessageProcessed(BusinessObjectFactory factory)
		{
			bool result = false;
			var interchange = LoadReplyEdiInterchange(factory);
			EDIMessage message = null;

			if (interchange != null)
			{
				if (interchange.ContainedMessages.Count >= 1)
				{
					message = interchange.ContainedMessages[0];
				}

				if (message == null)
				{
					throw new DiagnosticMessageException(DiagnosticStatusListNZ.Codes.WaitingForTestMessageResponse, "");
				}

				var replyEdiMessage = factory.Load<EDIMessage>(message.PK);

				if (replyEdiMessage == null)
				{
					throw new ApplicationException("Response EDI message cannot be found");
				}
				else if (replyEdiMessage.EM_Status == EDIMessage.Status.Recognised)
				{
					result = true;
				}
				else if (replyEdiMessage.EM_Status != EDIMessage.Status.Queued)
				{
					throw new DiagnosticMessageException(DiagnosticStatusListNZ.Codes.InInterchangeStatusInvalid, string.Format("Response EDI message status set to unexpected value {0}", replyEdiMessage.EM_Status));
				}
			}
			else
			{
				throw new DiagnosticMessageException("FAL", "The interchange timed out, or doesn't exist.");
			}

			return result;
		}

		EDIInterchange LoadReplyEdiInterchange(BusinessObjectFactory factory)
		{
			var messageToTrack = factory.Load<EDIMessage>(new ZGuid(testEdiMessagePk));

			var inInterchangeQuery = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, "NZC");
			inInterchangeQuery.AddToFilter(JoinCondition.And, EDIInterchangeSchema.EI_InterchangeType, "NZC");
			inInterchangeQuery.AddToFilter(JoinCondition.And, EDIInterchangeSchema.EI_ReceiveTransmit, "RCV");
			inInterchangeQuery.AddToFilter(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, messageCreateTime);
			inInterchangeQuery.AddToFilter(JoinCondition.And, EDIInterchangeSchema.EI_To, companyNzBrokerageId);
			inInterchangeQuery.AddToFilter(JoinCondition.And, EDIInterchangeSchema.EI_BodyText, SQLComparisonOperator.Contains, messageToTrack.EM_ApplicationReference);

#if DEBUG
			inInterchangeQuery.AddToFilter(JoinCondition.And, EDIInterchangeSchema.EI_From, EDIInterchange.InterchangePartyIDs.NZCustomsTestMailbox);
#else
			inInterchangeQuery.AddToFilter(JoinCondition.And, EDIInterchangeSchema.EI_From, EDIInterchange.InterchangePartyIDs.NZCustomsLiveMailbox);
#endif

			return factory.LoadTop1<EDIInterchange>(inInterchangeQuery);
		}

		/// <summary>
		/// Virtual for testing only
		/// </summary>
		protected virtual BusinessObjectFactory GetNewBusinessObjectFactory()
		{
			return new BusinessObjectFactory();
		}

		ZGuid outInterchangePk;
		readonly ZGuid testEdiMessagePk;
		readonly ZDateTime messageCreateTime;
		readonly string companyNzBrokerageId;
	}
}
