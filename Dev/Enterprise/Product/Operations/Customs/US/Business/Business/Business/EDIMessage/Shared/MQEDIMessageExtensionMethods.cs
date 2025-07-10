using System;
using System.Linq;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Business
{
	static class MQEDIMessageExtensionMethods
	{
		public static void CalculateRelatedPropertiesAfterENSSending(this MQEDIMessage message, CusEntryHeader entry)
		{
			if (entry != null && entry.IsFormalEntry)
			{
				var statusCalculator = new EntrySummaryMessageStatusCalculator(entry);
				statusCalculator.CalculateStatus(message, ABIResponseStatus.Undefined);
				entry.Messages.Add(message);
				entry.PopulateEntrySubmittedDateIfRequired();
				var declaration = entry.Declaration;
				declaration.PopulatePaymentDueDateIfNeeded();
				message.UpdateFDAMsgStatusIfRelevant(declaration);
				entry.CreateDocPrintingDetails(message.PK);
			}
		}

		public static void UpdateFDAMsgStatusIfRelevant(this MQEDIMessage message, JobDeclaration declaration)
		{
			if (!declaration.CanHavePGAFDA && declaration.US_CertifyCargoRelease && message.HasFDADetails)
			{
				declaration.FDAMsgStatus = FDAStatusList.Codes.AWA;
			}
		}

		public static bool IsSTUMessageAndAccepted(this MQEDIMessage message)
		{
			bool result = false;

			if (message != null)
			{
				var responseMessage = message.RelatedMessage;

				if (responseMessage != null)
				{
					if (message.EM_MessageType == ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction)
					{
						var ensH = message.MessageBlock.MessageBlocks.OfType<IStatementUpdateInputHBlock>().FirstOrDefault();
						var ensH1 = responseMessage.MessageBlock.MessageBlocks.OfType<ENSH1>().FirstOrDefault();

						if (ensH1 != null && !KnownSTUErrors.Contains(ensH1.ErrorMessageIdentifier.ToString()))
						{
							if (ensH != null)
							{
								if (KnownSTUMessagesAccepted.Contains(ensH1.ErrorMessageIdentifier.ToString()))
								{
									result = true;
								}
								else if (ensH1.PaymentTypeIndicator == ensH.PaymentTypeIndicator)
								{
									if (ensH.PaymentTypeIndicator.ToString() == PaymentTypeList.Codes.IndividualBasis)
									{
										result = true;
									}
									else
									{
										result = ensH1.PreliminaryStatementPrintDate == ensH.PreliminaryStatementPrintDate
												 && ensH1.ClientBranchDesignation == ensH.ClientBranchDesignation;
									}
								}
							}
						}
					}
					else if (message.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.StatementUpdate)
					{
						var ensH2 = responseMessage.MessageBlock.MessageBlocks.OfType<ASTUH2>().FirstOrDefault();
						if (ensH2 != null && !KnownACESTUMessagesError.Equals(ensH2.SeverityCode))
						{
							result = true;
						}
					}
				}
			}
			return result;
		}

		const string KnownACESTUMessagesError = "F";

		static string[] KnownSTUMessagesAccepted
		{
			get
			{
				return knownSTUMessagesAccepted ?? (knownSTUMessagesAccepted = new string[]
						{
							"2GB"
						})
						;
			}
		}
		[ThreadStatic]
		static string[] knownSTUMessagesAccepted;

		static string[] KnownSTUErrors
		{
			get
			{
				return knownSTUErrors ?? (knownSTUErrors = new string[]
						{
							"355",
							"46D", "4BI",
							"691",
							"96T", "96U", "96V",
							"ADF",
							"C04",
							"HP1", "HP2", "HP3", "HP4", "HP5", "HP6", "HP7", "HP8",
							"Q41","Q43",
							"VAW",
						})
						;
			}
		}

		[ThreadStatic]
		static string[] knownSTUErrors;
	}
}
