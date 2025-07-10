using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business
{
	//This class lists message templates for all critical validations which have been referenced in all Critical validation related classes.    
	//These Message Templates are Translation enabled
	//So if you are changing any message Template, please make sure that you are updating the GUID for that message in Res.Getstring function.
	//Otheriwse it will show an invalid Translated Error Message to the end user.

	#region SuppressResourceStringsCheckRegion

	public static class CriticalValidationMessageTemplate
	{
		public static ResourceString MatchLinkLinkedTransactionHasNotChanged => ResString.GetMultilingualString("5d5acfb1-be11-4f28-b456-3d063ff8b55b", "A transaction linked to the match link has not changed.");

		public static ResourceString TransactionHeaderSkippedDataRefreshBusUpdateButWasSavedSuccessfully => ResString.GetMultilingualString("C897DF3A-FBAE-4A44-9D05-BECFF9DC8A83", "Transaction header was modified by this user during another operation.");

		public static ResourceString TransactionHeaderInvoiceAmountWasModifiedAfterBeingSaved => ResString.GetMultilingualString("9B9ACD3D-0DD1-4A90-BD79-926776E2E4EC", "Transaction local amount was modified after being saved.");

		public static ResourceString TransactionHeaderOSAmountWasModifiedAfterBeingSaved => ResString.GetMultilingualString("941F3E60-907C-4AEB-A10F-3437A9A8A5C9", "Transaction OS amount was modified after being saved.");

		public static ResourceString TaxRecordSkippedDataRefreshBusUpdateButWasSavedSuccessfully => ResString.GetMultilingualString("C04F4C08-672A-490C-8F17-571DC5EADFDE", "Tax record was modified by this user during another operation.");

		public static ResourceString TaxTransactionWithoutPivots => ResString.GetMultilingualString("E261572F-A675-4C5B-88E9-7C0C12AFE518", "No transaction lines are linked to tax record.");

		public static ResourceString TaxTransactionWithSumOfPivotsLocalTaxAmountsMismatch => ResString.GetMultilingualString("39EE094D-A2AE-48EB-9088-CA904A9BA88F", "Sum of local tax amounts on transaction lines does not match tax record local tax amount.");

		public static ResourceString TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch => ResString.GetMultilingualString("718E559C-C2E6-4FB5-83BD-22DFBE93EC31", "Tax record has tax expense definition mismatch with linked transaction line.");

		public static ResourceString TaxTransactionWithLocalTaxAmountChangedAfterSaving => ResString.GetMultilingualString("F079BF54-5BEC-4535-A40F-EAD9C61E5760", "Local tax amount of tax record was modified after saving.");

		public static ResourceString TaxTransactionWithTaxExpenseDataChangedAfterSaving => ResString.GetMultilingualString("ABB459AD-05F8-4072-A5D7-2273A83547B9", "Tax expense GL Account of tax record was modified after saving.");

		public static ResourceString CancelledTaxTransactionRealisationDateShouldNotBeEmpty => ResString.GetMultilingualString("7780445D-5304-4CAB-8E2E-0D2242A263D5", "Canceled transaction has empty realization date.");

		public static ResourceString JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully => ResString.GetMultilingualString("7cbf77f1-c4a4-425f-bc50-16a2043fd9a5", "Job charge was modified by this user during another operation.");
		public static ResourceString TransactionHeaderWasCriticallyChangedByDataRefreshBus => ResString.GetMultilingualString("7AA907CF-1C01-4FCB-9306-C5E8D05F7BB6", "Transaction was critically changed by this user during another operation. Please close this screen as this operation is not valid any more.");
		public static ResourceString JobChargeOsSellAmountNotEqualLocalSellAmountErrorMessage => ResString.GetMultilingualString("0b877aaf-4e2f-4075-9627-8342473361e1", "OS sell amount should be same as local sell amount when Local Currency is used.");
		public static ResourceString JobChargeOsCostAmountNotEqualLocalCostAmountErrorMessage => ResString.GetMultilingualString("b6d72fd2-eeb5-40ab-b659-9396aef91aed", "OS cost amount should be same as local cost amount when Local Currency is used.");
		public static ResourceString JobChargeNegativeOSSellExRateErrorMessage => ResString.GetMultilingualString("ec584f0b-a454-4e9c-a498-38f220537f71", "You have entered a Local Sell Amount less than the CFX minimum. You will need to either remove or adjust the CFX minimum, or increase the Local Sell Amount to a value greater than the CFX minimum.");
		public static ResourceString JobChargeOsSellAmountNotMatchingSignOfLocalSellAmountErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("8d991ff6-159d-4685-aa37-c63c9565aa5c", "OS Sell Amount and Local Sell Amount should have the same sign.\r\nOS Sell Amount: {0}, Local Sell Amount: {1}.", messageParameters);
		}
		public static ResourceString JobChargeOsCostAmountNotMatchingSignOfLocalCostAmountErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("4f7f5485-b3f7-4e91-8967-607459ccf9ca", "OS Cost Amount and Local Cost Amount should have the same sign.\r\nOS Cost Amount: {0}, Local Cost Amount: {1}.", messageParameters);
		}
		public static ResourceString JobTransactionLineWithMoreThanOneJobCharge_ChargeSideErrorMessage => ResString.GetMultilingualString("51339221-0269-49ef-a910-4a3aed42c894", "Other charges related to the same Transaction line.");
		public static ResourceString JobChargeMoreThanOneFieldLinkToTheSameLineErrorMessage => ResString.GetMultilingualString("88d3d7f7-c2c9-4afb-aea6-9d6ffee25d73", "The AP Line and AR Line of a Job Charge can not be the same transaction line.");
		public static ResourceString NewJobChargeLinkedToWIPAccrualInDbReferredByOtherJobCharges => ResString.GetMultilingualString("FFF6B281-9D7A-44F8-9DF2-1BD79CCB9664", "The new charge linked to a WIP Line or ACR Line which already in database and referred by other charges.");
		public static ResourceString JobChargeLinkedToConsolCostInvoiceHasZeroCostAmountErrorMessage => ResString.GetMultilingualString("13726313-37c3-420d-9dc9-663eedd5507e", "Apportion Split Charge should not have 0 Cost Amount.");
		public static ResourceString OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmountErrorMessage => ResString.GetMultilingualString("f84b8315-9db8-4096-a4ab-b3c3040f4ee9", "Overseas cost amount is not equal to the sum of the apportionment's overseas cost amount.");
		public static ResourceString LocalCostAmountNotEqualToSumOfApportionmentsLocalCostAmountErrorMessage => ResString.GetMultilingualString("65338c57-0824-448b-83ed-c9955f2e10e7", "Local cost amount is not equal to the sum of the apportionment's local cost amount.");
		public static ResourceString OverseasCostTaxAmountNotEqualToSumOfApportionmentsOverseasCostTaxAmountErrorMessage => ResString.GetMultilingualString("9007c267-62db-4d48-bbbc-9228f5029f61", "Overseas cost tax amount is not equal to the sum of the apportionment's Overseas cost tax amount.");
		public static ResourceString CostNotLinkedToJobDueToEmptyParentIDErrorMessage => ResString.GetMultilingualString("22e36a2c-5085-44e2-94cb-bf7b32019f17", "Cost is not linked to a job properly due to Parent ID is empty.");
		public static ResourceString CostNotLinkedToJobDueToEmptyParentTableCodeErrorMessage => ResString.GetMultilingualString("735c7a58-375f-4b2b-af19-3757ebe68337", "Cost is not linked to a job properly due to Parent Table Code is empty.");
		public static ResourceString CashBasisTaxRecognition_AlreadySavedErrorMessage => ResString.GetMultilingualString("f035e4f5-548c-4366-b6d1-7637634643ea", "Cash basis tax recognition record cannot be changed once it saved.");
		public static ResourceString CashBasisTaxRecognition_CurrentCompanyErrorMessage => ResString.GetMultilingualString("e708dded-89fd-4ea9-90c2-687c717ef9c9", "Cash basis tax recognition record must belong to current company.");
		public static ResourceString CashBasisTaxRecognition_TransactionLineErrorMessage => ResString.GetMultilingualString("5b3fa985-3435-4b5b-94d1-b1f9bc71244f", "Cash basis tax recognition record must always have transaction line.");
		public static ResourceString CashBasisTaxRecognitionWithIncorrectTaxRecord_NotCashErrorMessage => ResString.GetMultilingualString("6fd88802-7bf8-4004-9b81-b2d06da1dadf", "Cash Basis Tax Recognition record cannot be created for a line when the line's Tax Basis is not Cash.");
		public static ResourceString CashBasisTaxRecognitionWithIncorrectTaxRecord_NoTaxIDErrorMessage => ResString.GetMultilingualString("46447500-ccff-4573-a7f2-c8c55ee1b927", "Cash Basis Tax Recognition records cannot be created for lines with no Tax ID.");
		public static ResourceString CashBasisTaxRecognitionWithIncorrectPostDateErrorMessage => ResString.GetMultilingualString("05e54a06-65df-4ba9-9b81-3c0263800e35", "Cash Basis Tax Recognition record cannot be created with an empty Post Date.");
		public static ResourceString CashBasisTaxRecognitionWithIncorrectTaxRecord_ZeroTaxBasisErrorMessage => ResString.GetMultilingualString("1f8ec7ef-0e69-42a9-8132-547c03980b99", "Cash basis tax recognition record cannot have zero tax basis amount.");
		public static ResourceString CashBasisTaxRecognitionWithIncorrectTaxRecord_NotZeroRelatedLineTaxAmountErrorMessage => ResString.GetMultilingualString("216e0943-9e31-437a-bca5-ef3c598cf179", "Cash basis tax recognition record cannot have zero tax amount if related line tax amount is not zero.");
		public static ResourceString CashBasisTaxRecognitionWithDifferentSignsErrorMessage => ResString.GetMultilingualString("27343798-c554-4a06-bdd9-c86edfe39ebd", "Cash basis tax recognition record has different signs.");
		public static ResourceString CashBasisTaxRecognitionWithIncorrectPostDate_PaidDateErrorMessage => ResString.GetMultilingualString("c9971c07-e5a1-4026-aeb0-a6ad4dd3f123", "Cash basis tax recognition record with empty match group number and the same amounts as related line must have Post Date equal to fully paid date of related transaction.");
		public static ResourceString CashBasisTaxRecognitionWithIncorrectPostDate_MatchDateErrorMessage => ResString.GetMultilingualString("7837287e-54ac-4027-b612-bd4eb975e918", "The Post date of the Cash Basis Tax Recognition record must be the same as the related Match Group's match date.");
		public static ResourceString CashBasisTaxRecognition_MatchGroupNumberErrorMessage => ResString.GetMultilingualString("488d8225-dce3-42a1-b96d-0ec08f91845d", "Cash basis tax recognition record match group number does not exist.");
		public static ResourceString CashBasisTaxRecognitionWithOppositeToLineSigns_ExistingRecordErrorMessage => ResString.GetMultilingualString("c4ecf5a3-1ae7-4b5f-9551-8810fa17d4fb", "Cash Basis Tax Recognition records with an opposite sign to the line are only permitted when reversing an existing record.");
		public static ResourceString CashBasisTaxRecognitionWithOppositeToLineSigns_unmatchingErrorMessage => ResString.GetMultilingualString("5a21fcb7-03ae-4d50-8e45-e18b441c72db", "Cash Basis Tax Recognition record with an opposite sign to the line is only allowed when unmatching.");
		public static ResourceString CashBasisTaxRecognitionWithIncorrectTotal_ExceedLinedLineAmountErrorMessage => ResString.GetMultilingualString("4f4cdcf3-5aa0-4bc1-b4f2-f7b649135058", "Sum of Tax Base Amounts for Cash basis tax recognition records cannot exceed linked Line Amount.");
		public static ResourceString CashBasisTaxRecognitionWithIncorrectTotal_SignLinkedLineAmountErrorMessage => ResString.GetMultilingualString("149e0c86-1df8-48a9-a811-ec65007a1b9e", "Sum of Cash basis tax recognition records must have the same sign as linked Line Amount.");
		public static ResourceString CashBasisTaxRecognitionWithIncorrectTotal_OriginalTaxAmountErrorMessage => ResString.GetMultilingualString("17f9aeaa-5deb-49bd-8dd6-4735018db5c2", "The sum of all related Cash Basis Tax Recognition records for this Line cannot exceed the Line's original Tax Amount.");
		public static ResourceString CashBasisTaxRecognitionWithIncorrectTotal_LinkedLineTaxAmountErrorMessage => ResString.GetMultilingualString("930F3877-11CD-4B8E-A465-5EFE608016C4", "If the line tax amount sign is positive, the sum of related tax lines must be positive. And vice versa for negative.");
		public static ResourceString CashBasisTaxRecognitionWithEmptyMatchGroupNumber_ZeroValueTransactionFullyPaidErrorMessage => ResString.GetMultilingualString("5821beae-f51b-4b60-95df-aa0e3b5ced90", "Cash Basis Tax Recognition record with an empty Match Group number must have the same amount as the related line. Zero value transactions are always fully paid and cannot be part matched.");
		public static ResourceString CashBasisTaxRecognitionWithEmptyMatchGroupNumber_ZeroValueTransactionErrorMessage => ResString.GetMultilingualString("ef633fe1-b6d6-4211-a01d-b4bf6e159beb", "Cash Basis Tax Recognition record with an empty Match Group number can only be created for zero value transactions.");
		public static ResourceString SavedMachLinkCannotBeModifiedErrorMessage => ResString.GetMultilingualString("5013cb44-5cd9-4ea9-bd3a-eceda9c7e25d", "The Match Link already saved in database should not be modified.");
		public static ResourceString MatchLinkIsNotAMemberOfMatchGroupErrorMessage => ResString.GetMultilingualString("f3ed466b-8fac-4020-9be8-f4701df37894", "The Match Link should always be a member of Match Link Group.");
		public static ResourceString MatchLinkWithoutGroupNumberErrorMessage => ResString.GetMultilingualString("f877e30b-ef95-47ee-9252-abf834446ad6", "All Match Links should have Group Number assigned.");
		public static ResourceString ReversedTransactionsShouldNotHaveLinesLinkedToChargesErrorMessage => ResString.GetMultilingualString("1E6320BA-E727-4323-A6FC-20B0BCDFB62F", "Transaction line of reversed invoice / credit note / job revenue journal should not be linked to job charge.");
		public static ResourceString TransactionLineWithoutTransactionHeaderErrorMessage => ResString.GetMultilingualString("a48a9dd5-b229-47d8-8b82-c0779f50ef23", "Cost, Revenue or Unapproved Cost transaction line that does not have a related transaction header.");
		public static ResourceString JobChargeNegativeRevenueIsNotPermittedErrorMessage => ResString.GetMultilingualString("06023206-51ec-461d-aed3-36863e0c24b4", "Negative revenue charges are not allowed.");
		public static ResourceString JobTransactionLineWithoutRevenueRecognitionTypeErrorMessage => ResString.GetMultilingualString("9d4c76d0-de9a-4240-9740-a9821ec4b2ef", "Job related transaction line that does not have revenue recognition type.");
		public static ResourceString JobTransactionLineWithoutJobChargeErrorMessage(string lineType) => ResString.GetMultilingualString("289ad754-fe59-4099-8de2-5360bdea5470", "{0} transaction line that does not have a related job charge.", lineType);
		public static ResourceString JobTransactionLineWithMoreThanOneJobCharge_LineSideErrorMessage => ResString.GetMultilingualString("c1eef5c7-706c-4bc7-ad32-9f2a56f6f9f7", "Cost, Revenue or Unapproved Cost transaction line that has more than one related job charge.");
		public static ResourceString JobTransactionLineWithoutJobChargeToSaveErrorMessage => ResString.GetMultilingualString("037e7f03-7b99-4292-9388-b37647513ab4", "Cost, Revenue or Unapproved Cost transaction line that does not have a related job charge that can be saved.");
		public static ResourceString WIPMustHaveDebtorErrorMessage => ResString.GetMultilingualString("41928638-0ea3-4e66-b61d-569f0b0f6791", "WIP without a debtor. You must enter a debtor. Your system has been configured so that the 'debtor' is mandatory when entering an unposted sell of non zero value.\r\n\r\nThe registry setting that governs this rule is Accounting > Job Costing > WIP Must Have Debtor Code");
		public static ResourceString AccrualMustHaveCreditorErrorMessage => ResString.GetMultilingualString("8d5b4466-a63c-4a13-a029-e409554c09b7", "Accrual without a creditor. You must enter a creditor. Your system has been configured so that the 'creditor' is mandatory when entering an unposted cost of non zero value.\r\n\r\nThe registry setting that governs this rule is Accounting > Job Costing > Accrual Must Have Creditor Code");
		public static ResourceString TransactionLineLocalAmountNotEqualForeignWithExRate1ErrorMessage => ResString.GetMultilingualString("c1fbb803-7690-4c23-86e0-35aab3167c4d", "Local Line Amount and Tax that are not equal to the Foreign Currency Line Amount and Tax when the exchange rate is 1.");
		public static ResourceString WIPACROrganisationDoesNotMatchOneOnJobChargeErrorMessage => ResString.GetMultilingualString("6253dea5-0b72-4491-ab39-cbb6cfcaf23a", "Transaction line organization does not equal related job charge organization.");
		public static ResourceString SubAccountDetailsCannotBeSetForJobRelatedLinesErrorMessage => ResString.GetMultilingualString("a4a4ff31-e667-49e8-9324-fcca10cfbcce", "Transaction Line which is related to Job and has Sub Account Details set.");
		public static ResourceString TransactionIsAlreadyReversedErrorMessage => ResString.GetMultilingualString("14509dac-5850-46c3-b060-a385ac159ead", "This transaction was already reversed by another user.");
		public static ResourceString TransactionHeaderIncorrectOutstandingAmountErrorMessage => ResString.GetMultilingualString("bf3c52f3-dd07-4495-b070-b9824f093559", "Incorrect outstanding amount.");
		public static ResourceString TransactionHeaderIncorrectOSOutstandingAmountErrorMessage => ResString.GetMultilingualString("5633CA9C-F0B5-4584-81E6-919DFC572201", "Incorrect overseas outstanding amount.");
		public static ResourceString TransactionHeaderLedgerNotCompatibleWithTransactionHeaderTypeErrorMessage => ResString.GetMultilingualString("83b923ad-150b-4f4e-88ae-92e54d382f76", "This Transaction ledger is not compatible with the transaction type");
		public static ResourceString TransactionHeaderGstAmountWasModifiedAfterBeingSavedErrorMessage => ResString.GetMultilingualString("c985b891-0148-4add-9400-cfc704cc9e07", "The tax amount was modified after being saved.");
		public static ResourceString TransactionHeaderPostDateChangesAfterPostedMessage => ResString.GetMultilingualString("163BEFFF-3E36-493f-BCFE-60FE703B9889", "Posted transaction post date can't be changed.");
		public static ResourceString TransactionHeaderWithLinesShouldHaveLinesMessage => ResString.GetMultilingualString("ABA91FF4-B4F5-439d-AB83-2268D72DBC0F", "This transaction should have lines.");
		public static ResourceString TransactionHeaderMustHaveTransactionNumber => ResString.GetMultilingualString("5C02BCA5-D8DD-475b-BE64-2D316C41E7D5", "This transaction must have transaction number.");
		public static ResourceString TransactionNumberOfHeaderMustNotChangeOnceSaved => ResString.GetMultilingualString("56D18D12-6A1F-42EB-9607-1FA94AAA0EC1", "The transaction number was modified after being saved.");
		public static ResourceString INTransactionHeaderHasPostedLinesMessage => ResString.GetMultilingualString("8D368333-CE32-4486-B795-01B0BA0FA0EE", "This transaction has line with posted charge.");
		public static ResourceString TransactionLineTypeNotCompatibleWithTransactionHeaderErrorMessage => ResString.GetMultilingualString("e1ad3a16-1fba-477f-9e24-9a21101f6e17", "This transaction line is not compatible with the transaction header");
		public static ResourceString NonZeroOutstandingAmountOnMiscellaneousTransactionErrorMessage => ResString.GetMultilingualString("0f9bd15a-9ca6-4775-9c47-ca09cdbc51a8", "Non zero outstanding amount on miscellaneous transaction.");
		public static ResourceString ClearedReceiptWithoutBatchNumberErrorMessage => ResString.GetMultilingualString("69c6cfd5-a6cb-49f6-bb73-56a7ca462afc", "Cleared receipt without a batch number.");
		public static ResourceString AutoGLJournalWithoutDueDateErrorMessage => ResString.GetMultilingualString("7273b0e5-9e8a-4937-8a0c-4fdea0b0a371", "Auto/Reversing GL Journal without Reverse/Ending Period (Due Date).");
		public static ResourceString LinesOfBankTranferShouldBalanceToZeroErrorMessage => ResString.GetMultilingualString("5198b7e8-a6f8-46ef-87cd-53bc44e948f0", "Bank transfer where the buy transaction and sell transaction do not balance.");
		public static ResourceString CashBookExchangeTransactionNeedGeneralLedgerAccountErrorMessage => ResString.GetMultilingualString("F4F5A06C-F74D-4002-BD66-12DA1F06B5D8", "The transaction does not have a GL account specified.\r\n\r\nPlease check and fill below registry settings:\r\n1.{0}\r\n2.{1}", ObjectFactory.Get<IAccounting>().Registry.CurrencyAdjustmentExchangeGainAccount.HumanReadableRegistryPath(), ObjectFactory.Get<IAccounting>().Registry.CurrencyAdjustmentExchangeLossAccount.HumanReadableRegistryPath());
		public static ResourceString MissingRevesingTransactionForCanceledTransactionErrorMessage => ResString.GetMultilingualString("f7cb8100-4ded-4cc2-a291-3139e5d12705", "Missing Reversing Transaction for this canceled transaction.");
		public static ResourceString InvalidFullyPaidDateWithRespectToTheOutstandingAmountErrorMessage => ResString.GetMultilingualString("570d67e3-4435-4df1-ad3c-61bf1285718e", "Invalid Fully Paid Date with respect to the outstanding amount.");
		public static ResourceString LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1ErrorMessage(ZString currency, ZDecimal exchangeRate) => ResString.GetMultilingualString("e82046b6-b181-43d0-9c87-223ada6de288", "Local Invoice Amount that is not equal to the Foreign Currency Invoice Amount when we use Local Currency. The Currency is '{0}' and the Exchange Rate is '{1}'.", currency,exchangeRate);
		public static ResourceString TransactionWithEmptyGLAccountField_ClearingJournalErrorMessage => ResString.GetMultilingualString("5e2ea4c4-f0c7-4023-b67b-5f95ca9536d1", "Clearing Journal with empty GL account field.");
		public static ResourceString JobChargeLinkedToPostedConsolCostIsNotCostPostedErrorMessage => ResString.GetMultilingualString("96fac812-a63d-47f1-92f1-f24bdd3d6ba7", "Non Cost posted Apportion Split Charge is linked to posted Consol Cost.");
		public static ResourceString JobChargeLinkedToPostedConsolCostIsPostedToDifferentInvoiceErrorMessage => ResString.GetMultilingualString("fd789c64-b785-485b-96ea-f6d0a8470983", "Consol Cost and linked Apportion Split Charge are posted to different AP Invoices.");
		public static ResourceString JobChargeLinkedToUnpostedConsolCostIsCostPostedErrorMessage => ResString.GetMultilingualString("e2af4367-35e2-46c7-aee4-49ffa9b4594f", "Cost posted Apportion Split Charge is linked to unposted Consol Cost.");
		public static ResourceString PostedConsolCostWithCostUnpostedApportionmentChargeErrorMessage => ResString.GetMultilingualString("408775ef-284a-4714-b91b-9dfda0900c89", "Non Cost posted Apportion Split Charge is linked to posted Consol Cost.");
		public static ResourceString PostedConsolCostWithApportionmentChargePostedToDifferentInvoiceErrorMessage => ResString.GetMultilingualString("978f6683-e685-4578-b7f1-b8a9dc5656f1", "Consol Cost and linked Apportion Split Charge are posted to different AP Invoices.");
		public static ResourceString UnpostedConsolCostWithCostPostedApportionmentChargeErrorMessage => ResString.GetMultilingualString("df397331-2268-4e67-98a2-2c3b54cbeed1", "Cost posted Apportion Split Charge is linked to unposted Consol Cost.");
		public static ResourceString TransactionHeaderBranchDoesNotBelongToTransactionHeaderCompanyErrorMessage => ResString.GetMultilingualString("3e77e791-a987-4d87-a97e-3c25b1d8cf4e", "Transaction header branch does not belong to transaction header company");
		public static ResourceString TransactionMatchGroupNotAllHeaderInSameCompaniesErrorMessage => ResString.GetMultilingualString("B14E0B3E-C273-42C1-BA17-977195C7FFCA", "All transaction header must in same company");
		public static ResourceString TransactionLineBranchDoesNotBelongToTransactionHeaderCompanyErrorMessage => ResString.GetMultilingualString("8406f29e-042c-48dc-960c-e0e28f5f75e8", "Transaction line branch does not belong to transaction header company");
		public static ResourceString CancelledUnapprovedPayableTransactionsShouldNotHaveLinesErrorMessage => ResString.GetMultilingualString("92178fb8-b894-4140-bb10-f77baed3abc9", "Canceled Unapproved Payable Transactions should not have transaction lines.");
		public static ResourceString JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrencyErrorMessage => ResString.GetMultilingualString("e725ad89-233b-4d3e-a209-54221457d41c", "Invalid charge cost exchange rate. The job charge cost exchange rate should be 1.0 when local company currency is equals to the job charge cost currency.");
		public static ResourceString JobChargeOSSellExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeSellCurrencyErrorMessage => ResString.GetMultilingualString("4fbe259d-9fb1-44dc-ac2d-be4dfaa2b80d", "Invalid charge sell exchange rate. The job charge sell exchange rate should be 1.0 when local company currency is equals to the job charge sell currency.");
		public static ResourceString TransactionHeaderBankAccountWasModifiedAfterBeingSavedErrorMessage => ResString.GetMultilingualString("A787521A-022F-4020-911A-4C44BC42C831", "Transaction header bank account was modified after being saved.");
		public static ResourceString TransactionHeaderExchangeRateIsNotGreaterThanZeroErrorMessage => ResString.GetMultilingualString("8BA3FEB5-75B9-4345-8C65-2BA92E7E0736", "This transaction exchange rate is less than or equal to 0.");

		public static ResourceString NotAllowedToPostCreditNoteDueToRegistryConfigurationErrorMessage => ResString.GetMultilingualString("D9CE4936-1A2E-4746-A9FB-FFF6E813A01E", "Credit note cannot be posted due to registry configuration.");

		public static ResourceString GetPostedConsolCostWithNonOverriddenTaxAmountErrorMessage => ResString.GetMultilingualString("8b997111-57e6-4910-8bb8-e8c4fc1481e2", "Posted Consol cost must have overridden GST Amount.");
		public static ResourceString JobChargeInvoiceDetailsNotEqualConsolCostOnesErrorMessage => ResString.GetMultilingualString("41067218-61EB-42C0-A2EE-42D7304870AF", "Apportion Split Charge invoice details are not equal to parent consol cost invoice details.");
		public static ResourceString JobChargeLinkedToClosedJobErrorMessage => ResString.GetMultilingualString("f5a1113a-301f-4395-bb12-a4d6b40a0a3a", "Newly created job charge is being saved on a closed job.");
		public static ResourceString JobChargeLinkedToClosedJobWhenCreatingProfitShareChargesErrorMessage => ResString.GetMultilingualString("60C6DD7F-B572-407D-8CF0-E80AB4875D65", "Cannot close the job when new profit share charges are created automatically. After you close and re-open the form, please re-enter the data, save all changes before closing the job.");
		public static ResourceString JobChargeLinkedToInactiveJobErrorMessage => ResString.GetMultilingualString("8B5BB97F-B9D2-41D2-9FBE-A41BCF904C49", "Job charge is saved with inactive job.");
		public static ResourceString InvalidJobChargeProFormaCostOrProFormaRevenueErrorMessage => ResString.GetMultilingualString("5E6A6F80-AC19-44A6-9CE4-9D049D889B30", "Invalid Job Charge Proforma-cost or Proforma-revenue. Proforma-cost or Proforma-revenue should be true for Rating Header, otherwise, they should be false.");

		public static ResourceString GetTransactionLineHasNoJobWhenRequiredByChargeCodeErrorMessage(ZString chargeCode)
		{
			return ResString.GetMultilingualString("ea8b340e-661e-4f41-a646-f8e6658514bc", "Cost or Unapproved Cost transaction line that does not have a Job, but the line Charge Code '{0}' requires it.", chargeCode);
		}

		public static ResourceString GetTransactionLineAmountExceedMaximumAllowedAmountErrorMessage(string maxAmount)
		{
			return ResString.GetMultilingualString("4ef0388a-7d28-414f-81d6-253ad00f28fd", "The transaction line amount exceed the maximum allowed amount {0} which is defined in the '{1}' registry.", maxAmount, MaximumAllowedAmountRegistryPath);
		}

		public static ResourceString GetTransactionHeaderAmountExceedMaximumAllowedAmountErrorMessage(string maxAmount)
		{
			return ResString.GetMultilingualString("743e2a32-fddd-440a-b947-bd9ffcd2b87b", "The transaction header amount exceed the maximum allowed amount {0} which is defined in the '{1}' registry.", maxAmount, MaximumAllowedAmountRegistryPath);
		}

		static string MaximumAllowedAmountRegistryPath => AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.HumanReadableRegistryPath();

		public static ResourceString GetTransactionLineTaxBranchDifferentFromChargeTaxBranchErrorMessage(string lineTaxBranchCode, string chargeTaxBranchCode) => ResString.GetMultilingualString("B0AD05E1-7D38-4034-BD5B-5B03F24E2294", "Transaction line tax branch '{0}' is different from line charge tax branch '{1}'.", lineTaxBranchCode, chargeTaxBranchCode);

		public static ResourceString GetJobChargeInvoiceDetailsNotEqualConsolCostOnesErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("40e42750-17c6-4ebb-bbc0-7e259280bf4f", "Apportion Split Charge invoice details are not equal to parent consol cost invoice details.\r\n{0}", messageParameters);
		}

		public static ResourceString GetJobChargeInvoiceDetailsNotEqualConsolCostOnes_NeedToBeSyncErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("B74FBF10-2246-4AC1-A346-A79607D05DBC", "Apportion Split Charge invoice details are not equal to parent consol cost invoice details.\r\nPlease go to related consol form, click 'Job Invoicing > Synchronize Cost Invoice Details' menu item and try again.\r\n{0}", messageParameters);
		}

		public static ResourceString GetTransactionLineTaxClassNotEqualJobChargeTaxClassErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("4e02144b-27d0-4693-ab22-5430d377b17d", "Cost, Revenue or Unapproved Cost transaction tax class that does not equal related job charge tax class.\r\n\r\n{0}", messageParameters);
		}

		public static ResourceString GetTransactionLineTaxCodeNotEqualJobChargeTaxCodeErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("a3450bd3-eef9-491c-9d0c-8341f9d40f3d", "Cost, Revenue or Unapproved Cost transaction tax code that does not equal related job charge tax code.\r\n\r\n{0}", messageParameters);
		}

		public static ResourceString GetTransactionLineLocalAmountNotEqualJobChargeLocalAmountErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("e49c1742-9048-4333-9f6f-9d88352240f8", "Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount.\r\n\r\n{0}", messageParameters);
		}

		public static ResourceString GetTransactionLineTaxBranchNotEqualToTransactionHeaderTaxBranchErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("7DE4D51E-C009-4C82-B907-635CC960D20C", "Transaction Line tax branch does not equal to transaction header tax branch.\r\n\r\n{0}", messageParameters);
		}

		public static ResourceString GetTransactionLineSupplyTypeNotEqualJobChargeSupplyTypeErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("1CE1F44D-7C78-4105-9E46-D895D46BFC4B", "Cost, Revenue or Unapproved Cost transaction line supply type that does not equal related job charge supply type.\r\n\r\n{0}", messageParameters);
		}

		public static ResourceString GetJobChargeAmountExceedMaximumAllowedAmountErrorMessage(string maxAmount)
		{
			return ResString.GetMultilingualString("7b8b43fa-7104-40ef-9d63-de3f19fc2dc4", "The job charge amount must be between -{0} and {0} which is defined in the '{1}' registry.", maxAmount, MaximumAllowedAmountRegistryPath);
		}

		public static ResourceString GetJobChargeInvoiceDetailsNotEqualConsolCostOnes_JobChargeErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("3f88ea15-0f9b-4e72-97c8-392e4c9d6365", "Apportion Split Charge invoice details are not equal to parent consol cost invoice details.\r\n{0}", messageParameters);
		}

		public static ResourceString GetJobChargeHasPreviouslyLinkedLineThatMustBeReversedErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("76d12a4c-0732-43f2-b4bb-c18597a84ca7", "Previously linked but currently detached {0} line should be reversed", messageParameters);
		}

		public static ResourceString GetJobChargeLinkedLineShouldNotBeReversedErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("5dd88cf6-8ef2-4f3b-848c-2b896ac37282", "Currently linked {0} line should not be reversed", messageParameters);
		}

		public static ResourceString GetJobChargeAmountNotEqualRelatedLineAmountErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("f3c0b2a9-8158-43d8-ade0-adc84764ea32", "Related {0} amount is not the same as charge amount.", messageParameters);
		}

		public static ResourceString GetJobChargeOrganisationIsNotSameAsLineOneErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("ea78d445-c7f6-4561-8e6e-8f729ddcad5b", "Related {0} organization is not the same as charge organization.", messageParameters);
		}

		public static ResourceString GetJobChargeReferenceToPostedTransactionLineCannotBeChangedErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("17bfb771-cf97-41f9-9dde-89fdaa5c4990", "Posted {0} line reference can't be changed.", messageParameters);
		}

		public static ResourceString GetJobChargeTaxCodeNotEqualRelatedLineTaxCodeErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("a818b2d6-769f-464a-9429-50150c41ec31", "Related {0} line tax code and class are not the same as charge tax code and class.", messageParameters);
		}

		public static ResourceString GetJobChargeRelatedToPostedTransactionLineCannotBeDeletedErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("3dc94fc4-f40b-412f-8476-e8a6813b3a79", "Charge deleted with posted {0}.", messageParameters);
		}

		public static ResourceString GetJobChargeRelatedWIP_ACR_MustBeReversedWhenChargeIsDeletedErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("d6c9314a-c4e1-49ae-b4fb-79820978ff2b", "Charge deleted with not reversed {0}.", messageParameters);
		}

		public static ResourceString GetReversedTransactionLineShouldBeOrNotLinkedToJobCharge_IfReversedErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("e5d95533-a63f-431a-971a-6e9dac9a2405", "reversed {0} should not be linked to a Job Charge", messageParameters);
		}

		public static ResourceString GetReversedTransactionLineShouldBeOrNotLinkedToJobCharge_IfNotReversedErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("0f3d8007-19bf-484c-8bcb-bfe9319cef5c", "{0} should be linked to a Job Charge unless it is Reversed", messageParameters);
		}

		public static ResourceString GetLineShouldBeRelatedToSameJobAsLinkedJobChargeErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("0ce137e0-7daf-4606-a7cb-b36c4fb43c61", "{0} should be related to the same Job as a linked Job Charge", messageParameters);
		}

		public static ResourceString GetLineShouldHaveSameAmountAsJobChargeErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("fa73d529-9839-4187-8fa4-3a3de173e28b", "Related {0} amount is not the same as charge amount.", messageParameters);
		}

		public static ResourceString GetLineCurrencyShouldHaveSameCurrencyAsLocalCurrencyErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("32498c89-91e0-4860-a2a8-18f457a43801", "{0} currency is not the same as local currency.", messageParameters);
		}

		public static ResourceString GetTransactionMatchGroupOutOfBalanceErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("d3e73a02-a825-46ca-8022-041d01b50812", "Match Group {0} is out of balance.", messageParameters);
		}

		public static ResourceString GetMatchLinkWithoutGroupNumber_DeleteTogetherErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("8c4125a2-b765-4bcf-ac05-641e4bb602a7", "All Match Links from the Group {0} should be deleted together.", messageParameters);
		}

		public static ResourceString GetAPTransactionNumberAlreadyUsedForAnotherOrganizationErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("ee19d6ab-831c-422d-85b3-f67da624b6d3", "AP {0} number {1} is already used for the organization: {2}. Please use another transaction number.", messageParameters);
		}

		public static ResourceString GetAPConvertedFromARInvoiceTransactionNumberAlreadyUsedForAnotherOrganizationErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("c757839d-92cb-4a24-a2d8-45d28ed22cb9", "AP {0} number {1} is already used for the organization: {2}. The AP {0} {1} may have been already converted(approved) by another user from the sister company AR transaction.", messageParameters);
		}

		public static ResourceString GetTransactionWithEmptyGLAccountFieldErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("02dbc288-bc6f-4dff-a7f4-ad4fe2999365", "{0} {1} with an empty GL account field.", messageParameters);
		}

		public static ResourceString GetTransactionLineWithoutGLAccountAndChargeCodeErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("a4e74330-2a8d-4004-a9b3-a61e730edf0a", @"The system has attempted to create a transaction line of type {0} but was not successful due to one of the following reasons:
1.The transaction line does not have a GL account specified.
2.The transaction line does not have a charge code specified.
3.The transaction line has a charge code specified but GL account is missing in the charge code configuration.

Please try the following steps to identify and/or resolve the issue:
1. Run File > Validate All menu and check if any validation error is shown.
2. If you are reversing a transaction, check if any of the original transaction line is missing a GL Account value.
3. Check the GL Account setup of the charge code used in Maintain > Account > Charge Codes.", messageParameters);
		}

		public static ResourceString GetSumOfTransactionLineAmountsDoesNotMatchTransactionHeaderInvoiceAmountErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("1685af67-89cd-461e-ab94-a5713084a89e", "Transaction Line Amount Total does not match the Transaction Header Invoice Amount. Transaction Line Amount Total is {0} but Header Invoice Amount is {1}.", messageParameters);
		}

		public static ResourceString GetSumOfTransactionLineAmountsWillNotMatchTransactionHeaderInvoiceAmountBecauseNotAllLinesWillBeSaved(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("860D6101-6B07-49B1-AB3E-44228B52EC60", @"Transaction Line Amount Total will not match the Transaction Header Invoice Amount after saving because not all lines will be saved. Number of all lines: {0}, Number of lines that will not be saved despite having changed amounts: {1}, Number of new lines that will not be saved: {2}.", messageParameters);
		}

		public static ResourceString GetSumOfTransactionLineGSTAmountsDoesNotMatchTransactionHeaderGSTAmountErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("8a24ef0e-4d0f-418f-8401-577d34ef60d8", "Transaction Line GST Total does not match the Transaction Header GST Amount. Transaction Line GST Total is {0} but Header GST Amount is {1}.", messageParameters);
		}

		public static ResourceString GetSumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmountErrorMessage(ZDecimal lineTotal, ZDecimal headerTotal, ZDecimal otherTaxesAmount)
		{
			if (otherTaxesAmount.IsEmpty)
			{
				return ResString.GetMultilingualString("5d293d6f-72cc-4429-b851-329721abe02c", "Transaction Line OS Amount Total does not match the Transaction Header OS Amount. Transaction Line OS Amount Total is {0} but Header OS Amount is {1}.", lineTotal, headerTotal);
			}
			else
			{
				return ResString.GetMultilingualString("34E91AC1-B0FB-4463-A43B-29CC4A933E31", "Transaction Line OS Amount Total and Tax Transaction Amount does not match the Transaction Header OS Amount. Transaction Line OS Amount Total is {0}, Tax Transaction Amount is {1}, but Header OS Amount is {2}.", lineTotal, otherTaxesAmount, headerTotal);
			}
		}

		public static ResourceString GetSumOfTransactionLineAmountsDoesNotEqualToZeroErrorMessage(params object[] messageParameters)
		{
			return ResString.GetMultilingualString("82E5DBF6-ED3F-4044-9403-7FDE6BFEDC46", "Transaction Line Amount Total must be zero for a Header with {0} ledger {1} transaction type but was {2}.", messageParameters);
		}

		public static ResourceString GetJobInvoiceNumberExceedMaximumNumberMessage(params object[] messageParamters)
		{
			return ResString.GetMultilingualString("112C91AD-A90D-4FEC-ACC2-2123E4E59720", "The maximum number of invoices for a job is {0}. You cannot post any more invoices for this job.", messageParamters);
		}

		public static ResourceString NumberFountainBaseDataWasChanged => ResString.GetMultilingualString("7AA93406-79FC-49FB-8185-FB4F9B3D50B6", "Transaction data for transaction number generator was changed after the number is generated.");

		public static ResourceString TransactionLineNegativeAmountOnAccountReceivableTransactionsIsNotAllowedErrorMessage(string countryName, string transactionType)
		{
			return ResString.GetMultilingualString("e081bcc8-8521-468a-9796-6588c20c50f5", "To comply with tax regulations in {0}, you are not allowed to post AR {1} with negative amounts on any lines.", countryName, transactionType);
		}

		public static ResourceString GLJournalEntriesNumberHasBeenAssignedErrorMessage()
		{
			return ResString.GetMultilingualString("0fa3b102-1769-43bc-a7d7-02d0f0c0063c", "An unique reference number has been allocated while you were editing the GL Journal.\r\nSaving has been aborted to protect the allocated reference number.\r\nPlease reverse the GL Journal and re-enter.");
		}

		public static ResourceString BranchOfJobShouldNotBeNull => ResString.GetMultilingualString("5CBAC4E9-7317-4226-B85D-7260DFE3C6CB", "Branch of the Job should not be null.");
		public static ResourceString DepartmentOfJobShouldNotBeNull => ResString.GetMultilingualString("6AE15240-2B5E-47B6-8DFB-F1E1BD0B7737", "Department of the Job should not be null.");

		public static ResourceString TaxTransactionShouldBeCancelledIfAllAmountsAreZeroErrorMessage => ResString.GetMultilingualString("8396d206-049c-4524-9113-f25adeafc564", "Tax Transaction should be canceled if all Tax amounts - OS Base, Local Base, OS Tax and Local Tax are zero");

		public static ResourceString GetMissingGLMovementsRecordErrorMessage(string taxTransactionBasis, string glMovementType) => ResString.GetMultilingualString("406D73CA-2B14-4507-AAB6-B6BEC7FE097A", "Could not find GL Movements record of type {0} when tax transaction basis is {1}.", glMovementType, taxTransactionBasis);

		public static ResourceString GetInvalidTaxTransactionBasisErrorMessage(string taxTransactionBasis) => ResString.GetMultilingualString("9E41C6C9-453A-4678-8814-060FC39EE542", "Invalid Tax Transaction basis {0}", taxTransactionBasis);

		public static ResourceString MatchingBasisTaxTransactionsWithEmptyRealisationDateErrorMessage => ResString.GetMultilingualString("B0A034B0-6324-488D-8C64-11A9B8516CD7", "Transaction has Tax Transaction records with {0} basis but Tax Realization Date is empty", TaxBasisList.Matching.Code);

		public static ResourceString GetInvalidGLMovementForNotionalTaxErrorMessage => ResString.GetMultilingualString("18127ae6-d429-4363-a69e-776f4935dd9c", "GL Movement record must not be created for notional tax record.");

		public static ResourceString GLMovementExistsForLocalTaxAmountZeroErrorMessage => ResString.GetMultilingualString("b71a2bcc-3b91-4316-8d0e-b8ed869414dc", "GL Movement record must not be created when Local Tax Amount is zero.");

		public static ResourceString OSTotalAmountMustNotChangeAfterRoundingLineCreationErrorMessage => ResString.GetMultilingualString("A863298A-3922-458F-B2F1-2EC36C4A57CE", "Transaction OS Total amount must not be changed after rounding line creation.");

		public static ResourceString PostInvoiceWhenComplianceBookWithEmptyPrintingAuthorizationnumberErrorMessage => ResString.GetMultilingualString("4FBD65F6-5CBE-45BC-9AAD-D2860167CB57", "The Compliance Book trying to allocate a Compliance Number to this transaction does not have a Printing Authorization Number, please record this value in the corresponding book.");

		public static ResourceString GetRemittanceReferenceNumberExceedMaxLengthErrorMessage(string referenceNumber) => ResString.GetMultilingualString("DAE44FC9-753C-4311-BD76-1898084F1CB7", "Transaction Remittance Reference '{0}' exceed max length {1}.\r\nPlease check registry settings: Accounting > Receivable Defaults > Default Settings > Invoice Remittance Configuration.", referenceNumber, AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength);

		public static ResourceString GetDirectDebitBatchLocalAmountIsNotEqualToSumOfAllPaymentLocalAmountsErrorMessage(ZDecimal headerTotal, ZDecimal lineTotal, ZDecimal gstAmount)
		{
			return ResString.GetMultilingualString("773d736e-cd77-4b78-b23f-4f0326350867", "Direct Debit Batch Header Local Amount does not match sum of Payment Local Amount and GST Amount. Payment Local Amount Total is {0}, GST Amount is {1}, but Header Local Amount is {2}.", lineTotal, gstAmount, headerTotal);
		}

		public static ResourceString GetDirectDebitBatchOSAmountIsNotEqualToSumOfAllPaymentOSAmountsErrorMessage(ZDecimal headerTotal, ZDecimal lineTotal)
		{
			return ResString.GetMultilingualString("6e2dd1ab-4129-4cfe-a9eb-c6f7e3c8ce7a", "Direct Debit Batch Header OS Amount does not match sum of Payment OS Amount. Payment OS Amount Total is {0}, but Header OS Amount is {1}.", lineTotal, headerTotal);
		}

		public static ResourceString GetDirectDebitBatchOSAmountIsNotEqualToLocalAmountWhenBatchIsInLocalCurrency(ZDecimal headerOSAmount, ZDecimal headerLocalAmount)
		{
			return ResString.GetMultilingualString("8bf4b6e7-c4b0-4f7e-80a0-a57cb78433e3", "Direct Debit Batch Header OS Amount does not match Local Amount. Header OS Amount is {0}, but Header Local Amount is {1}.", headerOSAmount, headerLocalAmount);
		}

		public static ResourceString GetSumOfInvoiceBatchLineInvoiceAmountNotEqualToInvoiceBatchHeaderInvoiceAmountErrorMessage(ZDecimal batchLineAmountTotal, ZDecimal batchHeaderAmount) => ResString.GetMultilingualString("F4715653-0F26-400D-A079-7C9BC7AED1CF", "Sum Of Invoice Batch Line Invoice Amount does not match the Invoice Batch Header Invoice Amount. Sum of Invoice Batch Line Invoice Amount is {0} but Invoice Batch Header Invoice Amount is {1}.", batchLineAmountTotal, batchHeaderAmount);

		public static ResourceString GetSumOfInvoiceBatchLineLocalTotalNotEqualToInvoiceBatchHeaderOutstandingAmountErrorMessage(ZDecimal batchLineAmountTotal, ZDecimal batchHeaderAmount) => ResString.GetMultilingualString("CCA82DDF-4296-4424-945E-49AAD8E8EE0F", "Sum of Invoice Batch Line Local Total does not match the Invoice Batch Header Outstanding Amount. Sum of Invoice Batch Line Local Total is {0} but Invoice Batch Header Outstanding Amount is {1}.", batchLineAmountTotal, batchHeaderAmount);

		public static ResourceString GetSumOfInvoiceBatchLineOSTotalNotEqualToInvoiceBatchHeaderOSTotalErrorMessage(ZDecimal batchLineAmountTotal, ZDecimal batchHeaderAmount) => ResString.GetMultilingualString("BEC80912-2849-4C64-82A6-1C72EA39C246", "Sum of Invoice Batch Line OS Total does not match the Invoice Batch Header OS Total. Sum of Invoice Batch Line OS Total is {0} but Invoice Batch Header OS Total is {1}.", batchLineAmountTotal, batchHeaderAmount);

		public static ResourceString GetSumOfInvoiceBatchLineGSTAmountNotEqualToInvoiceBatchHeaderGSTAmountErrorMessage(ZDecimal batchLineAmountTotal, ZDecimal batchHeaderAmount) => ResString.GetMultilingualString("BD978BF3-048A-4FF0-8DF1-20D199078429", "Sum of Invoice Batch Line GST Amount  does not match the Invoice Batch Header GST Amount. Sum of Invoice Batch Line GST Amount is {0} but Invoice Batch Header GST Amount is {1}.", batchLineAmountTotal, batchHeaderAmount);

		public static ResourceString CancelledInvoiceBatchHeaderInvoiceAmountIsNonZeroErrorMessage => ResString.GetMultilingualString("B3522C70-6150-45EC-8344-115C181AD330", "Canceled Invoice Batch Header Invoice Amount is non-zero.");

		public static ResourceString CancelledInvoiceBatchHeaderOutstandingAmountIsNonZeroErrorMessage => ResString.GetMultilingualString("C29B1BFA-306B-451C-B614-9AC1434B9426", "Canceled Invoice Batch Header Outstanding Amount is non-zero.");

		public static ResourceString CancelledInvoiceBatchHeaderOSTotalIsNonZeroErrorMessage => ResString.GetMultilingualString("F82AB216-35B8-4617-B50F-9B54E8820B8B", "Canceled Invoice Batch Header OS Total is non-zero.");

		public static ResourceString CancelledInvoiceBatchHeaderGSTAmountIsNonZeroErrorMessage => ResString.GetMultilingualString("5DB37AC2-50C9-42E4-A16B-B642DCC680E5", "Canceled Invoice Batch Header GST Amount is non-zero.");

		public static ResourceString AccComplianceDocumentHeaderWithAddressNotRelatedToHeaderErrorMessage => ResString.GetMultilingualString("4FF61EB7-1FC8-4AFC-A92C-BEA69E773711", "The organization header address of the compliance document record does not belong to the transaction's organization.");

		public static ResourceString AccComplianceDocumentNumberAlreadyInUseErrorMessage => ResString.GetMultilingualString("A62347F8-278F-4270-AF43-77E202BF5678", "This Compliance Document Number is already in use. Please try to allocate a number to it again.");
	}

	#endregion
}
