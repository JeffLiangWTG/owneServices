namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("10")]
	[OutputBlock("10")]
	public abstract partial class AENS10 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS10()
			: base("10")
		{
		}

		/// <summary>
		/// The action requested for this Entry Summary transaction:
		/// 
		/// A = Add or entirely Replace an Entry Summary. 
		/// R = Add or entirely Replace an Entry Summary. 
		/// D = Delete/remove an Entry Summary.
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString SummaryFilingActionRequestCode;

		/// <summary>
		/// Entry Filer's identification code (as assigned by CBP).
		/// </summary>
		[MessageBlockString(3, 4, "M")]
		public ZString EntryFilerCode;

		/// <summary>
		/// Unique identifying number assigned to the Entry by the Filer. 
		/// 
		/// The check digit must be computed using the formula found in 'AE Table 1 - Check Digit Computation Formula'.
		/// </summary>
		[MessageBlockString(8, 9, "M")]
		public ZString EntryNumber;

		/// <summary>
		/// The code for the U.S. port that the merchandise is entered (under either an 'entry' or an 'immediate delivery permit').
		/// </summary>
		[MessageBlockString(4, 18, "M")]
		public ZString DistrictPortOfEntry;

		/// <summary>
		/// Filer/Preparer's internal Entry Summary identifier. 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(9, 22, "C")]
		public ZString BrokerReferenceNumber;

		/// <summary>
		/// A code identifying the type of entry being submitted. 
		/// 
		/// Note: Only types '01', '03', '11', '51', and '52' are supported at this time. Additional Entry Types shall be made available in the future. 
		/// 
		/// See 'AE Table 2 - Entry Type Codes' for a list of codes supported in this transaction. See Usage Note '(e) Entry Type Data Reporting Considerations' for more information.
		/// </summary>
		[MessageBlockString(2, 34, "M")]
		public ZString EntryTypeCode;

		/// <summary>
		/// A code identifying the mode of transportation by which the merchandise entered the U.S. port of arrival from the last foreign country. 
		/// 
		/// See 'AE Table 3 - Mode of Transportation Codes' for a list of codes supported in this transaction. 
		/// 
		/// Space fill if not allowed or not required/not reported.
		/// </summary>
		[MessageBlockString(2, 36, "C")]
		public ZString ModeOfTransportationMOTCode;

		/// <summary>
		/// An indication that the bond has been waived in accordance with C.R. 142.4 (c) or other regulation.
		/// 
		/// 0 = Bond waived/no bond required. Additionally, a 'waiver' reason may be required to be stated in 10-Record Bond Waiver Reason Code. 
		/// 
		/// Space fill if a bond is required. See Usage Note '(i) Bond & Surety Reporting' for more information.
		/// </summary>
		[MessageBlockString(1, 38, "C")]
		public ZString BondWaiverIndicator;

		/// <summary>
		/// The Filer's electronic signature. An ACE Entry Summary is designated as paperless until such time (if any) that CBP requests that a paper document be submitted. 
		/// 
		/// X = Filer's Electronic Signature.
		/// 
		/// The Electronic Signature is MANDATORY if the 10-Record Summary Filing Action Request Code is an A (Add) or R (Replace).
		/// </summary>
		[MessageBlockString(1, 39, "C")]
		public ZString ElectronicSignature;

		/// <summary>
		/// An indication that the Filer is instructing CBP to use the Entry Summary information for the purpose of Cargo Release (i.e., create/process an Entry for the release of the cargo). 
		/// 
		/// Y = Certify for ACS Cargo Release. 
		/// A = Certify for ACE Cargo Release. 
		/// 
		/// See Usage Note '(ee) Certifying for Cargo Release' for more information. Space fill if not used; Certify for Cargo Release not requested.
		/// </summary>
		[MessageBlockString(1, 40, "C")]
		public ZString CargoReleaseCertificationRequestIndicator;

		/// <summary>
		/// An indication that the Entry Summary conforms to the requirements for Electronic Invoice Processing (EIP) program: an electronic invoice(s) that accounts for ALL articles claimed on the Entry Summary has EITHER - a) been submitted electronically to CBP, or b) is available for electronic submission when requested by CBP. 
		/// 
		/// Y = Electronic Invoice Available. 
		/// 
		/// Space fill if not used; electronic invoice not available.
		/// 
		/// See Usage Note '(t) Electronic Invoice Processing (EIP) Program Reporting Requirements' for new EIP restrictions and information concerning the status/future of the Automated Invoice Interface (AII) program and the current requirements for its use.
		/// </summary>
		[MessageBlockString(1, 41, "C")]
		public ZString ElectronicInvoiceIndicator;

		/// <summary>
		/// An indication that the Entry Summary accounts for (i.e., 'consolidates') the cargo referenced in more than one Cargo Release (entry) transaction. 
		/// 
		/// Y = Consolidated Entry Summary.
		/// 
		/// See Usage Note '(z) Consolidating Releases Under One Entry Summary' for more information. Space fill if not used; not consolidated.
		/// </summary>
		[MessageBlockString(1, 42, "C")]
		public ZString ConsolidatedSummaryIndicator;

		/// <summary>
		/// An indication that the articles referenced in the summary are for personal use or designated as a commercial sample.
		/// 
		/// P = Personal Shipment.
		/// X = Sample Commercial Shipment. 
		/// 
		/// Space fill if a Regular Commercial Shipment.
		/// </summary>
		[MessageBlockString(1, 43, "C")]
		public ZString ShipmentUsageTypeCode;

		/// <summary>
		/// An indication that both Entry Summary & Entry are filed at time of entry (required for importation of quota class merchandise); i.e., a live entry.
		/// 
		/// Y = Entry Summary filing is a live entry.
		/// 
		/// Space fill if not used; not a live entry.
		/// </summary>
		[MessageBlockString(1, 44, "C")]
		public ZString LiveEntryIndicator;

		/// <summary>
		/// An indication that an arrangement has been made with CBP's National Finance Center (NFC) to defer payment of Internal Revenue (IR) tax to be assessed on this Entry Summary, if any, to a periodic basis. 
		///  
		/// 1 = Conventional IR tax payment deferral has been arranged.
		/// 2 = Electronic Funds Transfer (EFT) IR tax payment deferral has been arranged.
		/// 
		/// Space fill if not used, if no IR tax payment deferral has been arranged and/or if no IR tax applicable for this Entry Summary.
		/// </summary>
		[MessageBlockString(1, 45, "C")]
		public ZString DeferredTaxPaymentCode;

		/// <summary>
		/// An indication that certain issues (such as value, classification, etc.) are to be subject to a future 'reconciliation' under an eligible Trade Agreement. 
		/// 
		/// Y = Issues to be subject to a future Trade Agreement reconciliation.
		/// 
		/// Space fill if not used; issues not subject to a future Trade Agreement reconciliation.
		/// </summary>
		[MessageBlockString(1, 46, "C")]
		public ZString TradeAgreementReconciliationIndicator;

		/// <summary>
		/// A code that identifies the specific issue(s) to be reconciled in the future. 
		/// 
		/// 001 = Value.
		/// 002 = Classification.
		/// 003 = Chapter 9802.
		/// 004 = Value & Classification.
		/// 005 = Value and Chapter 9802.
		/// 006 = Classification & Chapter 9802.
		/// 007 = Value, Classification & Chapter 9802.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(3, 47, "C")]
		public ZString ReconciliationIssueCode;

		/// <summary>
		/// A code that specifies the intended payment method for the Entry Summary. If statement is the chosen method, the code further specifies the type of payment (Daily or Periodic) and how the system will batch the entry summaries for statement processing. 
		/// 
		/// 1 = Individual payment (NOT on a statement). 
		/// 
		/// DAILY STATEMENT CODES
		/// 
		/// 2 = Entry summary will be batched by preliminary statement print date and filer code. 
		/// 3 = Entry summary will be batched by preliminary statement print date, filer code, and full importer of record number.
		/// 5 = Entry summary will be batched by preliminary statement print date, filer code, and importer of record number without suffix.
		/// 
		/// PERIODIC MONTHLY STATEMENT CODES
		/// 
		/// 6 = Entry summary will be batched by preliminary statement print date and filer code. 
		/// 7 = Entry summary will be batched by preliminary statement print date, filer code, and full importer of record number. 
		/// 
		/// 8 = Entry summary will be batched by preliminary statement print date, filer code, and importer of record number without suffix.
		/// 
		/// Note: If the entry summary includes a Statement Client Branch Identifier, after each of these actions, the system will subsequently batch by Statement Client Branch Identifier.
		/// 
		/// Space fill if statement fields not allowed.
		/// </summary>
		[MessageBlockString(1, 51, "C")]
		public ZString PaymentTypeCode;

		/// <summary>
		/// The date that the Entry Summary is to appear on the Preliminary Statement. 
		/// 
		/// Space fill if not on Statement or if statement fields not allowed. See Usage Note '(j) Basic Article Classification and Tariff Considerations - Duty Rate Date Matrix Hierarchy'.
		/// </summary>
		[MessageBlockDate(52, "C", "MMddyy")]
		public ZDate PreliminaryStatementPrintDate;

		/// <summary>
		/// The calendar month that the Entry Summary is accounted for on the Preliminary Periodic Monthly Statement. (MM Format). 
		/// 
		/// Space fill if not on Periodic Monthly Statement or if statement fields not allowed.
		/// </summary>
		[MessageBlockString(2, 58, "C")]
		public ZString PeriodicStatementMonth;

		/// <summary>
		/// A Filer assigned code that further groups Entry Summaries on a statement. 
		/// 
		/// Space fill if the participant does not use or if statement fields not allowed.
		/// </summary>
		[MessageBlockString(2, 60, "C")]
		public ZString StatementClientBranchIdentifier;

		/// <summary>
		/// When required, the specific reason that the bond has been waived per the 10-Record Bond Waiver Indicator claim. 
		/// 
		/// See Usage Note '(i) Bond & Surety Reporting' for more information. See 'AE Table 4 - Bond Waiver Reason Codes'. Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(3, 62, "C")]
		public ZString BondWaiverReasonCode;

		/// <summary>
		/// An indication that the filing is a Post Summary Correction (PSC).
		/// 
		/// Y = Post Summary Correction.
		/// 
		/// See Usage Note '(gg) Filing a Post Summary Correction' for more information. Space fill if the filing is a conventional Entry Summary.
		/// </summary>
		[MessageBlockString(1, 65, "C")]
		public ZString PostSummaryCorrectionIndicator;

		/// <summary>
		/// An indication that the filer/importer is requesting that CBP accelerate the liquidation for the Post Summary Correction. 
		/// 
		/// Y = Accelerated Liquidation Requested.
		/// 
		/// See Usage Note '(gg) Filing a Post Summary Correction' for more information. Space fill if the filing is a conventional Entry Summary or if PSC accelerated liquidation is not requested.
		/// </summary>
		[MessageBlockString(1, 66, "C")]
		public ZString AcceleratedLiquidationRequestIndicator;

		/// <summary>
		/// An indication that the Importer of Record reported in the 11-Record is a Known Importer.
		/// 
		/// Y = Importer of Record is a Known Importer.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(1, 67, "O")]
		public ZString KnownImporterIndicator;

		/// <summary>
		/// An indication that the cargo referenced in this entry summary has been released via an expedited entry program (i.e., FAST, BRASS, or Rail Line Release) AND that the PGA dataset records required for the entry have been included in this submission.
		/// 
		/// Y = Expedited release / PGA data included in submission.
		/// 
		/// See Usage Note ‘(dd) Articles Released Via an Expedited Release Program Subject to PGA Reporting’ for more information. Space fill if not used.
		/// </summary>
		[MessageBlockString(1, 68, "C")]
		public ZString PGAExpeditedReleaseIndicator;

		/// <summary>
		/// An indication that the Temporary Importation Under Bond (TIB) conforms to the conditions required for this type of Entry Summary.
		/// 
		/// Y = 'I certify that the articles are to be used according to the terms, conditions and provisos of the HTS subheading as declared herein and applies to the articles entered, that the articles will not be used for any other use and that the articles are not imported for sale or sale upon approval. I declare that the articles will be exported or destroyed within the applicable 6-month or 1-year period from the date of importation, unless extended.'
		/// 
		/// Space fill if not a TIB.
		/// </summary>
		[MessageBlockString(1, 69, "C")]
		public ZString TIBDeclarationIndicator;
	}
}
