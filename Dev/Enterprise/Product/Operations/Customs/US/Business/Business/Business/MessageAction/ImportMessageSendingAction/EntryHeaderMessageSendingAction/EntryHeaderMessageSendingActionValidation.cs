//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEntryHeaderMessageSendingActionValidation
//
//    This class should be used for overriding validation in AutoEntryHeaderMessageSendingActionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class EntryHeaderMessageSendingActionValidation : USImportMessageSendingActionValidation
	{
		public EntryHeaderMessageSendingActionValidation(EntryHeaderMessageSendingAction parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateUS_SE_ReferenceNo();
			ValidateUS_SE_ReasonCode();
		}

		protected new EntryHeaderMessageSendingAction Parent
		{
			get { return (EntryHeaderMessageSendingAction)base.Parent; }
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void CheckUS_SendMessage()
		{
			base.CheckUS_SendMessage();

			if (Parent.US_SendMessage)
			{
				var entry = Parent.entry;
				var declaration = Parent.entry.Declaration;
				if (declaration != null)
				{
					var messageType = Parent.actions.messageSendingMessageType;

					if (messageType == ImportMessageSendingMessageType.ExtendTIB)
					{
						if (entry.Has98130075Articles)
						{
							Parent.US_SendMessageInfo.AddMessageError(Res.GetString("A8CF1DDB-00BA-40E1-85B9-E3C154306601", "6 month TIB using tariff 9813.00.75 may not be extended."));
						}
						else if (entry.ApprovedTIBExtensionMessages.Length > 1)
						{
							{
								Parent.US_SendMessageInfo.AddMessageError(Res.GetString("76B9853C-CE46-4D39-AFD6-553512D9EEDA"
									, "Two TIB extensions have already been granted on this entry. No more extensions are allowed."));
							}
						}
					}
					else
					{
						bool isEntrySummary = IsEntrySummary;

						if (isEntrySummary || declaration.IsCargoRelease)
						{
							string messageSendingType = isEntrySummary ? "Entry Summary" : "Cargo Release";
							var rlfEntrySummarySecurityError = FormalENSValidator.CheckSendRLFSecurity(Parent.entry, declaration.RegistryCompanyPK, messageSendingType);
							if (!rlfEntrySummarySecurityError.IsEmpty)
							{
								Parent.US_SendMessageInfo.AddError(rlfEntrySummarySecurityError);
							}
						}

						if (isEntrySummary)
						{
							if (declaration.IsACSCargoCertificationMode)
							{
								Parent.US_SendMessageInfo.AddError(NotSupportedByCBP);
							}
							else
							{
								var messageErrors = FormalENSValidator.GetEntrySummaryMessageErrors(Parent.entry, messageType, messageType != ImportMessageSendingMessageType.Deletion && declaration.US_PSC && Parent.PSCReasonCodes.Count == 0);
								messageErrors.ToList().ForEach(x => Parent.US_SendMessageInfo.AddMessageError(x));

								if (Parent.entry != null && !Parent.entry.HasBeenLodgedAtCustoms)
								{
									if (messageType == ImportMessageSendingMessageType.Deletion)
									{
										Parent.US_SendMessageInfo.AddWarning(ZString.Format(NoOriginalMessageAccepted, "Entry Summary", "A delete"));
									}
									else if (messageType == ImportMessageSendingMessageType.Replacement && declaration.US_BRDRefNo.IsEmpty)
									{
										Parent.US_SendMessageInfo.AddMessageError(NoEntrySummaryAccepted);
									}
								}
							}
						}

						if (IsACECargoRelease && declaration.US_ConsolACE)
						{
							Parent.US_SendMessageInfo.AddMessageError(CannotCertifyCRForConsolidatedEntrySummary);
						}

						var isCargoReleaseDeletion = Parent.IsACECargoRelease && messageType == ImportMessageSendingMessageType.Deletion;
						if (isCargoReleaseDeletion)
						{
							if (declaration.FormalEntry != null && declaration.FormalEntry.HasBeenLodgedAtCustoms)
							{
								var ensStatus = declaration.FormalEntry.CH_Status;
								if (!(ensStatus == Common.US.ImportMessageStatusList.Codes.ClearEntrySummaryDelete || ensStatus == Common.US.ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal))
								{
									Parent.US_SendMessageInfo.AddMessageError(ShouldNotDeleteCargoReleaseBeforeEntrySummary);
								}
							}
							else if (declaration.IsBondedWarehousePermitEnabled && PermitHelper.IsPermitInUsed(declaration.Factory, Core.Constants.CountryCodes.UnitedStates, PermitTypeList.Codes.FTZ, PermitEntryLineGrouping.GetOutwardEntryNumber(declaration)))
							{
								Parent.US_SendMessageInfo.AddMessageError(ShouldNotDeleteWhenGoodsWithdrawals);
							}
							else if (declaration.US_SchDEntry.IsEmpty)
							{
								Parent.US_SendMessageInfo.AddMessageError(PortOfEntryCannotBeEmpty);
							}
						}

						var isCargoReleaseReplace = Parent.IsACECargoRelease && messageType == ImportMessageSendingMessageType.Replacement;
						if (Parent.entry != null && !Parent.entry.HasBeenLodgedAtCustoms)
						{
							if (isCargoReleaseDeletion)
							{
								Parent.US_SendMessageInfo.AddWarning(ZString.Format(NoOriginalMessageAccepted, "Cargo Release", "A delete"));
							}
							else if (isCargoReleaseReplace)
							{
								Parent.US_SendMessageInfo.AddWarning(ZString.Format(NoOriginalMessageAccepted, "Cargo Release", "An update or replace"));
							}
						}

						if ((messageType == ImportMessageSendingMessageType.Original || messageType == ImportMessageSendingMessageType.Replacement)
							&& (Parent.IsACECargoRelease || declaration.IsACE && Parent.IsEntrySummary)
							&& Parent.entry.IsWaitingForResponse)
						{
							Parent.US_SendMessageInfo.AddMessageError(ShouldNotOriginalReplacementWhenWaitingForResponse);
						}

						if ((messageType == ImportMessageSendingMessageType.Original || messageType == ImportMessageSendingMessageType.Replacement)
							&& declaration.IsACE && Parent.IsEntrySummary && ((IStatementDeleteTransaction)declaration).IsStatementUpdateMessagePending)
						{
							Parent.US_SendMessageInfo.AddMessageError(ValidationConstants.Statement.ShouldNotSendWhenWaitingForSUResponse);
						}
					}
				}

				if (Parent.entry != null && Parent.entry.EntryLines.Count() > 999)
				{
					Parent.US_SendMessageInfo.AddMessageError(EntryLinesNumberExceeds);
				}
			}
			ValidateUS_SE_ContactName();
			ValidateUS_SE_ContactPhone();
		}

		internal const string NotSupportedByCBP = "This message is no longer supported by CBP.";
		internal const string ShouldNotOriginalReplacementWhenWaitingForResponse = "Message results still pending from last transmission. Please wait for results to be returned before resending.";
		internal const string NoOriginalMessageAccepted = "There is no ACE {0} acceptance message on file. {1} will be rejected if it was never accepted at ABI.";
		internal const string NoEntrySummaryAccepted = "There is no Entry Summary accepted message on file.";
		internal const string EntryLinesNumberExceeds = "Number of Entry Lines exceeds 999, ACE currently does not accept more than 999 entry lines. Please reduce the number of invoice lines and create an additional entry if necessary";
		internal const string PortOfEntryCannotBeEmpty = "You have not entered a Port of Entry.";

		bool IsEntrySummary
		{
			get
			{
				var messageType = Parent.actions.messageSendingMessageType;

				return Parent.IsEntrySummary &&
					(
						messageType == ImportMessageSendingMessageType.Original ||
						messageType == ImportMessageSendingMessageType.Replacement ||
						messageType == ImportMessageSendingMessageType.Deletion
					);
			}
		}

		bool IsACECargoRelease
		{
			get
			{
				var messageType = Parent.actions.messageSendingMessageType;

				return Parent.IsACECargoRelease &&
					(
						messageType == ImportMessageSendingMessageType.Original ||
						messageType == ImportMessageSendingMessageType.Replacement ||
						messageType == ImportMessageSendingMessageType.Deletion
					);
			}
		}

		internal const string ShouldNotDeleteCargoReleaseBeforeEntrySummary = "Cargo Release Delete message may not be sent until Entry Summary has been deleted.";
		internal const string ShouldNotDeleteWhenGoodsWithdrawals = "There have been goods withdrawals from warehouse based on estimates. Please check if the warehouse orders are finalised and cancel those orders first before sending a delete message.";

		internal const string ConsigneeNameAndAddMessageSentAndAcknowleddgedForThisEntry = "'Consignee Name and Address Add' has already been sent and acknowledged for this entry.";
		public const string NoElectronicAmendment = "This entry cannot be amended electronically. ";

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void CheckUS_CertifyCargoRelease()
		{
			base.CheckUS_CertifyCargoRelease();

			var declaration = Parent.Declaration;

			if (Parent.US_SendMessage && Parent.US_CertifyCargoRelease)
			{
				FormalImportAddInfoJobDeclarationValidation.CheckCertifyCargoReleaseForExWarehouse(Parent.US_CertifyCargoReleaseInfo, Parent.entry.IsExWarehouseEntryType);

				if (!declaration.IsRemoteLocationFiling || !declaration.IsACECargoCertificationMode)
				{
					if (declaration.JE_EntryAuthorisationDate.IsValid)
					{
						Parent.US_CertifyCargoReleaseInfo.AddMessageError(CannotCertifyCRForReleasedEntry);
					}
					else if (declaration.DispositionCodes.Count > 0)
					{
						Parent.US_CertifyCargoReleaseInfo.AddMessageError(CargoReleaseHasAlreadyBeenCertified);
					}
				}

				if (declaration.IsACECargoCertificationMode)
				{
					if (declaration.US_ImmediateDelivery)
					{
						Parent.US_CertifyCargoReleaseInfo.AddMessageError(CannotCertifyCRForImmediateDelivery);
					}

					if (declaration.US_PGAExpeditedRelease)
					{
						Parent.US_CertifyCargoReleaseInfo.AddMessageError(ACEImportAddInfoJobDeclarationValidation.PGAReleaseIndicator);
					}
				}

				foreach (EntryHeaderMessageSendingAction action in Parent.actions.EntryHeaderActions)
				{
					if (action != this && Parent.entry.IsRelatedTo(action.entry) && (action.US_CertifyCargoRelease || action.IsACECargoRelease && action.US_SendMessage))
					{
						Parent.US_CertifyCargoReleaseInfo.AddMessageError(YouMayCertifyAtOneTypeOfEntry);
						break;
					}
				}

				if (declaration.IsACE)
				{
					if (declaration.US_ConsolACE)
					{
						Parent.US_CertifyCargoReleaseInfo.AddMessageError(CannotCertifyCRForConsolidatedEntrySummary);
					}

					if (declaration.US_PSC)
					{
						Parent.US_CertifyCargoReleaseInfo.AddMessageError(ValidationConstants.PSC.CannotCertifyForPSC);
					}

					var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
					if (declaration.IsACECargoCertificationMode && Parent.actions.messageSendingMessageType == ImportMessageSendingMessageType.Original && seEntry != null && seEntry.HasBeenLodgedAtCustoms)
					{
						Parent.US_CertifyCargoReleaseInfo.AddMessageError(OriginalACECRAccepted);
					}
				}

				if (ShouldCheckReplacementMessageForExmOrHld)
				{
					Parent.US_CertifyCargoReleaseInfo.AddWarning(ReplacementWarningMessageForExm);
				}
				else if (ShouldCheckReplacementMessageForNotREL)
				{
					Parent.US_CertifyCargoReleaseInfo.AddWarning(ReplacementWarningMessageForNOTREL);
				}
				else if (ShouldCheckReplacementMessageForBillREL)
				{
					Parent.US_CertifyCargoReleaseInfo.AddMessageError(ReplacementErrorMessageForREL);
				}
			}

			if (Parent.US_SendMessage && !Parent.US_CertifyCargoRelease && Parent.IsEntrySummary && declaration.IsRemoteLocationFiling && !declaration.IsACECargoCertificationMode)
			{
				if (!declaration.HasCargoReleaseBeenCertified && !declaration.US_EnableCRL)
				{
					Parent.US_CertifyCargoReleaseInfo.AddMessageError(FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
				}
			}
			ValidateUS_SE_ContactName();
			ValidateUS_SE_ContactPhone();
		}

		internal const string CargoReleaseHasAlreadyBeenCertified = "Selectivity has already been performed. If you try to Certify Cargo Release, customs will reject the message.";
		internal const string YouMayCertifyAtOneTypeOfEntry = "Cargo Release may be certified through either Entry Summary or Cargo Release, but not both.";
		internal const string CannotCertifyCRForReleasedEntry = "Cargo Release can not be certified for an Entry that has been released.";
		internal const string CannotCertifyCRForConsolidatedEntrySummary = "Consolidated Entry is indicated. No cargo release should be sent in this entry.";
		internal const string OriginalACECRAccepted = "Original ACE CR is accepted. Replace/Update should be sent, not Original";
		internal const string CannotCertifyCRForImmediateDelivery = "Cargo cannot be certified from entry summary for an immediate delivery entry.";

		protected override void CheckUS_CollectionBillInformationCode()
		{
			base.CheckUS_CollectionBillInformationCode();
			if (Parent.US_SendMessage && Parent.actions.IsEntrySummaryQuery && !Parent.Declaration.IsACE)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CollectionBillInformationCodeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.US_CollectionBillInformationCodeInfo, Parent.Lookups.CollectionBillInformationCodesList);
			}
		}

		protected override void CheckUS_JobReadyForPosting()
		{
			base.CheckUS_JobReadyForPosting();

			if (Parent.US_JobReadyForPosting)
			{
				JobDeclaration declaration = Parent.entry.Declaration;

				AccountingIntegrationOptions options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetFallBackValueAtAllLevels(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);

				if (options.PreApprovalBillingJob)
				{
					IAccountingAP_ARInvoiceQuery query = GetAP_ARInvoiceQuery();

					ZGuid[] customsChargeCodes = new Registry.Business.Customs.US.EntryChargeTypeList().GetAllChargeCodePKsOf(declaration.Branch.GB_GC);

					AP_ARInvoiceQueryResult queryResult = query.GetPostingDetails(declaration, customsChargeCodes, options.APPostDSB, options.ARPostDSB);

					if (queryResult.NumberOfARInvoicesToBeIssued > 1)
					{
						Parent.US_JobReadyForPostingInfo.AddWarning(string.Format(MoreThanOneARInvoicesWillBeIssued, queryResult.NumberOfARInvoicesToBeIssued));
					}

					if (queryResult.NumberOfUnpostedARCharges == 0 && queryResult.NumberOfUnpostedAPCharges == 0)
					{
						Parent.US_JobReadyForPostingInfo.AddWarning(NoChargesToBePosted);
					}
					else
					{
						if (queryResult.NumberOfUnpostedARCharges != queryResult.NumberOfUnpostedARChargesReadyForPosting)
						{
							Parent.US_JobReadyForPostingInfo.AddWarning(string.Format(UnmatchedARCharges, queryResult.NumberOfUnpostedARCharges, queryResult.NumberOfUnpostedARChargesReadyForPosting));
						}

						if (queryResult.NumberOfUnpostedAPCharges != queryResult.NumberOfUnpostedAPChargesReadyForPosting)
						{
							Parent.US_JobReadyForPostingInfo.AddWarning(string.Format(UnmatchedAPCharges, queryResult.NumberOfUnpostedAPCharges, queryResult.NumberOfUnpostedAPChargesReadyForPosting));
						}
					}
				}
			}
		}

		protected override void CheckUS_Paid()
		{
			base.CheckUS_Paid();

			if (Parent.IsPaidRelevant)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_PaidInfo, Parent.Lookups.YesNoList);

				if (Parent.US_Paid.IsEmpty)
				{
					Parent.US_PaidInfo.AddMessageError(PaidIndicatorToBeSpecified);
				}

				if (Parent.US_Paid == YesNoDefaultList.Codes.No && Parent.entry.Declaration.US_PaymentDueDate.IsInThePastDatePartOnly)
				{
					Parent.US_PaidInfo.AddWarning(PaymentDueDateHasPassedButIsIndicatedAsNotPaid);
				}
			}
		}
		internal const string PaidIndicatorToBeSpecified = "Please indicate whether this entry has been paid or not. Statement details will not be sent to customs if the entry has been paid.";
		internal const string PaymentDueDateHasPassedButIsIndicatedAsNotPaid = "Payment Due Date has passed, but this is indicated as not paid yet.";

		protected override void CheckUS_PSCExplanation()
		{
			base.CheckUS_PSCExplanation();

			bool isEntrySummary = Parent.entry.IsFormalEntry &&
						(Parent.actions.messageSendingMessageType == ImportMessageSendingMessageType.Original ||
						Parent.actions.messageSendingMessageType == ImportMessageSendingMessageType.Replacement);

			if (isEntrySummary && Parent.US_SendMessage && Parent.entry.Declaration.US_PSC)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PSCExplanationInfo, "PSC explanation");

				ValidateUS_SendMessage();
			}
		}

		protected override void CheckUS_SE_ActionType()
		{
			base.CheckUS_SE_ActionType();
			if (IsSEAmendment && Parent.US_SendMessage)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_SE_ActionTypeInfo, Parent.Lookups.ActionTypeList);
			}

			if (Parent.IsACECargoRelease)
			{
				if (Parent.US_SE_ActionType == ACECargoReleaseActionType.Codes.Replace)
				{
					if (ShouldCheckReplacementMessageForExmOrHld)
					{
						Parent.US_SE_ActionTypeInfo.AddWarning(ReplacementWarningMessageForExm);
					}
					else if (ShouldCheckReplacementMessageForNotREL)
					{
						Parent.US_SE_ActionTypeInfo.AddWarning(ReplacementWarningMessageForNOTREL);
					}
					else if (ShouldCheckReplacementMessageForBillREL)
					{
						Parent.US_SE_ActionTypeInfo.AddMessageError(ReplacementErrorMessageForREL);
					}
				}
				else if (Parent.US_SE_ActionType == ACECargoReleaseActionType.Codes.Update)
				{
					if (ShouldCheckUpdateMessageForREL)
					{
						Parent.US_SE_ActionTypeInfo.AddMessageError(UpdateErrorMessageForREL);
					}
				}
			}
		}

		internal const string ReplacementWarningMessageForExm = "Entry is pending a CBP Review, sending a REPLACE Cargo Release message at this time will result in a Correction Request which must be manually adjudicated by an authorized CBP User.";
		internal const string ReplacementWarningMessageForNOTREL = "Shipment has arrived but is not yet released, sending a REPLACE Cargo Release message at this time will result in a Correction Request which must be manually adjudicated by an authorized CBP User.";
		internal const string ReplacementErrorMessageForREL = "Cargo Release cannot be Replaced at this time.  Send ACE CR Message Update Message to update the Cargo Release.";
		internal const string UpdateErrorMessageForREL = "Cargo Release cannot be Updated  this time.  The Release Date is more than 15 days in the past.";

		bool ShouldCheckReplacementOrUpdateMessage
		{
			get
			{
				var declaration = Parent.Declaration;
				return declaration != null
					   && declaration.IsACE
					   && Parent.actions.messageSendingMessageType == ImportMessageSendingMessageType.Replacement;
			}
		}

		bool ShouldCheckReplacementMessageForExmOrHld
		{
			get { return ShouldCheckReplacementOrUpdateMessage && CRLReleaseStatusList.IsExmOrHld(Parent.Declaration.ReleaseStatus); }
		}

		bool ShouldCheckReplacementMessageForNotREL
		{
			get { return ShouldCheckReplacementOrUpdateMessage && Parent.Declaration.Bills.HasAtLeastHaveReceivedArrivedStatus && !CRLReleaseStatusList.IsREL(Parent.Declaration.ReleaseStatus); }
		}

		bool ShouldCheckReplacementMessageForBillREL
		{
			get { return ShouldCheckReplacementOrUpdateMessage && Parent.Declaration.Bills.HasAtLeastHaveReceivedArrivedStatus && CRLReleaseStatusList.IsREL(Parent.Declaration.ReleaseStatus); }
		}

		bool ShouldCheckUpdateMessageForREL
		{
			get { return ShouldCheckReplacementOrUpdateMessage && CRLReleaseStatusList.IsREL(Parent.Declaration.ReleaseStatus) && Parent.Declaration.JE_EntryAuthorisationDate.IsValid && Parent.Declaration.JE_EntryAuthorisationDate.AddDays(15) < ZDate.Today; }
		}

		protected override void CheckUS_AcknowledgeAndSign()
		{
			base.CheckUS_AcknowledgeAndSign();

			bool isEntrySummary = Parent.entry.IsFormalEntry &&
						(Parent.actions.messageSendingMessageType == ImportMessageSendingMessageType.Original ||
						Parent.actions.messageSendingMessageType == ImportMessageSendingMessageType.Replacement);

			if (isEntrySummary && Parent.US_SendMessage && !Parent.US_AcknowledgeAndSign && Parent.entry.Declaration.IsACE)
			{
				Parent.US_AcknowledgeAndSignInfo.AddMessageError(CannotSendUnlessAcknowledgedAndSigned);
			}
		}
		public const string CannotSendUnlessAcknowledgedAndSigned = "Should be acknowledged and signed.";

		public const string MoreThanOneARInvoicesWillBeIssued = "{0} AR invoices will be created. Both Debtor and Invoice Type on all AR Charges should be identical to ensure that only 1 AR Invoice is created";
		public const string UnmatchedARCharges = "Some AR charges on the Billing job will not be posted. Please ensure that the Revenue information is entered for these charges (Amount and Debtor etc).";
		public const string UnmatchedAPCharges = "Some AP charges on the Billing job will not be posted. Please ensure that the Cost information is entered for these charges (AP Invoice Number, Invoice Date and Payment Date etc).";
		public const string NoChargesToBePosted = "No charges were found for posting. Please check the data you entered on the Billing tab.";

		protected override void CheckUS_SE_ContactName()
		{
			base.CheckUS_SE_ContactName();
			if (Parent.US_SendMessage)
			{
				if (Parent.IsSE13DataRelevant)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SE_ContactNameInfo);
				}
			}
		}

		protected override void CheckUS_SE_ContactPhone()
		{
			base.CheckUS_SE_ContactPhone();
			if (Parent.US_SendMessage)
			{
				if (Parent.IsSE13DataRelevant)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SE_ContactPhoneInfo);
				}
			}
		}

		public void ValidateUS_SE_ReasonCode()
		{
			ValidateCalculatedProperty(Parent.US_SE_ReasonCodeInfo);
		}

		protected void CheckUS_SE_ReasonCode()
		{
			if (IsSEWithdrawal)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_SE_ReasonCodeInfo, Parent.Lookups.ReasonCodeList);
				ValidateUS_SE_ReferenceNo();
			}
		}

		public void ValidateUS_SE_ReferenceNo()
		{
			ValidateCalculatedProperty(Parent.US_SE_ReferenceNoInfo);
		}

		protected void CheckUS_SE_ReferenceNo()
		{
			if (IsSEWithdrawal && ReasonCodeList.IsReferenceNoRequired(Parent.US_SE_ReasonCode))
			{
				if (Parent.US_SE_ReferenceNo.IsEmpty)
				{
					Parent.US_SE_ReferenceNoInfo.AddMessageError(RefNoRequired);
				}
				else if (Parent.ReferenceIdentifierQualifier == ReferenceIdentifierCodeList.Codes.ReplacementEntryNumber && !IsValidEntryNumber(Parent.US_SE_ReferenceNo))
				{
					Parent.US_SE_ReferenceNoInfo.AddMessageError(EntryNumberRightFormat);
				}
			}
		}
		internal const string RefNoRequired = "Reference Number is required if Reason Code is entered. Reference Number should be: in-bond number if Reson Code is '02', entry number if Reason Code is '03' or FTZ admission number if Reson Code is '04'.";
		internal const string EntryNumberRightFormat = "Replacement Entry Number should be in the format, FFFNNNNNNNN where FFF represents the Filer Code and N is a number.";

		ZBool IsValidEntryNumber(ZString entryNumber)
		{
			return Regex.IsMatch(entryNumber, @"^[A-Z0-9]{3}[0-9]{8}$", RegexOptions.IgnoreCase);
		}

		ZBool IsSEAmendment
		{
			get { return Parent.IsACECargoRelease && Parent.actions.IsAmendment; }
		}

		ZBool IsSEWithdrawal
		{
			get { return Parent.IsACECargoRelease && Parent.actions.IsWithdrawal; }
		}

#if DEBUG
		public IAccountingAP_ARInvoiceQuery AP_ARInvoiceQueryForTesting;
#endif

		IAccountingAP_ARInvoiceQuery GetAP_ARInvoiceQuery()
		{
			IAccountingAP_ARInvoiceQuery result = ObjectFactory.Get<IAccountingAP_ARInvoiceQuery>();
#if DEBUG
			if (AP_ARInvoiceQueryForTesting != null)
			{
				result = AP_ARInvoiceQueryForTesting;
			}
#endif

			return result;
		}

		protected override void CheckUS_SE_DISIDRefNo()
		{
			base.CheckUS_SE_DISIDRefNo();
			if (Parent.US_SE_DISIndicator && Parent.US_SE_DISIDRefNo.IsEmpty)
			{
				Parent.US_SE_DISIDRefNoInfo.AddMessageError(DISIDRefNoRequired);
			}
		}

		internal const string DISIDRefNoRequired = "A DIS reference number is required.";

		protected override void CheckUS_SE_DISIndicator()
		{
			base.CheckUS_SE_DISIndicator();
			ValidateUS_SE_DISIDRefNo();
		}
	}
}
