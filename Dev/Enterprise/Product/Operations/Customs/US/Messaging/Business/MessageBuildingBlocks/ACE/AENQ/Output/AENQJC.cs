namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("JC")]
	public partial class AENQJC : MessageBlock
	{
		public AENQJC()
			: base("JC")
		{
		}

		/// <summary>
		/// A code indicating the basic control status of the entry summary in ACE. Valid codes are:
		/// 
		/// 1 = Entry summary under trade control.
		/// 2 = Entry summary under CBP control.
		/// 3 = Entry summary inactive in ACE.
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString EntrySummaryControlStatus;

		/// <summary>
		/// A code indicating the current status of the entry summary in ACE. Valid codes are:
		/// 
		/// 1 = Entry summary accepted.
		/// 2 = Entry summary rejected.
		/// 3 = Entry summary canceled.
		/// 4 = Entry summary inactive in ACE.
		/// </summary>
		[MessageBlockString(1, 4, "M")]
		public ZString EntrySummaryStatusCode;

		/// <summary>
		/// The latest date the entry summary was either accepted in ACE or the entry summary status code was changed.
		/// </summary>
		[MessageBlockDate(5, "M", "MMddyy")]
		public ZDate EntrySummaryStatusDate;

		/// <summary>
		/// A code indicating the late filing status of the entry summary. Valid codes are:
		/// 
		/// 0 = Not late.
		/// 1 = Over 10 days late.
		/// 2 = Over 30 days late.
		/// 3 = Over 60 days late.
		/// </summary>
		[MessageBlockString(1, 11, "M")]
		public ZString LateFilingStatusCode;

		/// <summary>
		/// A code indicating the release status of the shipment. Valid codes are:
		/// 
		/// 0 = Not released.
		/// 1 = Released.
		/// 2 = Information not permitted.
		/// 
		/// Release Status Code 2 will be returned if the query date is less than five days from the CBP release/examination date, the entry is for a border port location, and the mode of transportation code is 12, 20, 21,30, 31, 32, 33 or 34.
		/// </summary>
		[MessageBlockString(1, 12, "C")]
		public ZString ReleaseStatusCode;

		/// <summary>
		/// If released, contains the date on which the entry was released by ACS cargo processing.
		/// </summary>
		[MessageBlockDate(13, "C", "MMddyy")]
		public ZDate ReleaseDate;

		/// <summary>
		/// A code indicating the payment/collection status of the entry summary. Valid codes are:
		/// 
		/// 0 = Not paid.
		/// 1 = Partially paid.
		/// 2 = Fully paid.
		/// 3 = Duty free.
		/// 5 = Drawback.
		/// 6 = Authorized.
		/// </summary>
		[MessageBlockString(1, 20, "M")]
		public ZString CollectionStatusCode;

		/// <summary>
		/// The last date on which a payment was made for the entry summary.
		/// </summary>
		[MessageBlockDate(21, "C", "MMddyy")]
		public ZDate CollectionDate;

		/// <summary>
		/// A code which indicates the reason for the extension or suspension of the entry summary liquidation. Valid codes are:
		/// 
		/// 0 = Not extended or suspended.
		/// 1 = Customs extended.
		/// 2 = Importer extended.
		/// 3 = Other extension.
		/// 4 = CVD suspended.
		/// 5 = ADD suspended.
		/// 6 = Court ordered suspended.
		/// 7 = Actual use suspended.
		/// 8 = Other suspension 1.
		/// 9 = AD/CVD Suspended.
		/// </summary>
		[MessageBlockString(1, 27, "M")]
		public ZString ExtensionSuspensionStatusCode;

		/// <summary>
		/// The date of the last extension or suspension of liquidation of the entry summary.
		/// </summary>
		[MessageBlockDate(28, "C", "MMddyy")]
		public ZDate ExtensionSuspensionDate;

		/// <summary>
		/// The date of the last extension or suspension of liquidation as provided to the filer.
		/// </summary>
		[MessageBlockDate(34, "C", "MMddyy")]
		public ZDate ExtensionSuspensionNoticeDate;

		/// <summary>
		/// A code indicating the status of Census warnings for the entry summary based upon line level information. Valid codes are:
		/// 
		/// 0 = No Census warnings exist.
		/// 1 = An unresolved Census warning exists for at least one line item.
		/// 6 = All Census warnings have been resolved.
		/// </summary>
		[MessageBlockString(1, 40, "M")]
		public ZString CensusHeaderStatusCode;

		/// <summary>
		/// A code indicating the invoice status of the entry summary. Valid codes are:
		/// 
		/// Space = Non-EIP, electronic invoice not available.
		/// 1 = Open - default status when the entry summary is received and stored in ACE.
		/// 2 = Received - invoices have been received and stored in ACS.
		/// 3 = Deleted - invoice(s) stored in ACS have been disassociated from this entry summary.
		/// 4 = Invoice requested - this status is set when an automated request for invoices is sent to the filer.
		/// </summary>
		[MessageBlockString(1, 41, "M")]
		public ZString InvoiceStatusCode;

		/// <summary>
		/// A code which indicates if a protest has been filed and if so the status of the protest. Valid codes are:
		/// 
		/// Space = No protest.
		/// OP = Case open.
		/// AP = Approved.
		/// DN = Denied.
		/// SP = Suspended.
		/// PD = Partially denied.
		/// WD = Withdrawn denial of protest.
		/// UT = Untimely denial.
		/// </summary>
		[MessageBlockString(2, 42, "M")]
		public ZString ProtestStatusCode;

		/// <summary>
		/// A code which indicates the status of quota processing. Valid codes are:
		/// 
		/// Space = Quota not processed.
		/// 1 = Quota processed.
		/// 2 = Quota no lines.
		/// 3 = Quota processed, no lines are quota.
		/// 4 = Quota line deleted by CBP.
		/// </summary>
		[MessageBlockString(1, 44, "M")]
		public ZString QuotaStatusCode;

		/// <summary>
		/// Entry filer's identification code for the Trade Agreement reconciliation entry.
		/// </summary>
		[MessageBlockString(3, 45, "C")]
		public ZString TradeAgreementReconciliationFilerCode;

		/// <summary>
		/// Trade agreement reconciliation entry number for the reconciliation entry summary.
		/// </summary>
		[MessageBlockString(8, 50, "C")]
		public ZString TradeAgreementReconciliationEntryNumber;

		/// <summary>
		/// Entry filer's identification code for the Other reconciliation entry.
		/// </summary>
		[MessageBlockString(3, 58, "C")]
		public ZString OtherReconciliationFilerCode;

		/// <summary>
		/// Other reconciliation entry number for the reconciliation entry summary.
		/// </summary>
		[MessageBlockString(8, 63, "C")]
		public ZString OtherReconciliationEntryNumber;
	}

	[OutputBlock("JC", "01")]
	public partial class AENQJC_01 : MessageBlock
	{
		public AENQJC_01()
			: base("JC")
		{
		}

		/// <summary>
		/// A code indicating the basic control status of the entry summary in ACE. Valid codes are:
		/// 
		/// 1 = Entry summary under trade control.
		/// 2 = Entry summary under CBP control.
		/// 3 = Entry summary inactive in ACE.
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString EntrySummaryControlStatus;

		/// <summary>
		/// A code indicating the current status of the entry summary in ACE. Valid codes are:
		/// 
		/// 1 = Entry summary accepted.
		/// 2 = Entry summary rejected.
		/// 3 = Entry summary canceled.
		/// 4 = Entry summary inactive in ACE.
		/// </summary>
		[MessageBlockString(1, 4, "M")]
		public ZString EntrySummaryStatusCode;

		/// <summary>
		/// The latest date the entry summary was either accepted in ACE or the entry summary status code was changed.
		/// </summary>
		[MessageBlockDate(5, "M", "MMddyy")]
		public ZDate EntrySummaryStatusDate;

		/// <summary>
		/// A code indicating the late filing status of the entry summary. Valid codes are:
		/// 
		/// 0 = Not late.
		/// 1 = Over 10 days late.
		/// 2 = Over 30 days late.
		/// 3 = Over 60 days late.
		/// </summary>
		[MessageBlockString(1, 11, "M")]
		public ZString LateFilingStatusCode;

		/// <summary>
		/// A code indicating the release status of the shipment. Valid codes are:
		/// 
		/// 0 = Not released.
		/// 1 = Released.
		/// 2 = Information not permitted.
		/// 
		/// Release Status Code 2 will be returned if the query date is less than five days from the CBP release/examination date, the entry is for a border port location, and the mode of transportation code is 12, 20, 21,30, 31, 32, 33 or 34.
		/// </summary>
		[MessageBlockString(1, 12, "C")]
		public ZString ReleaseStatusCode;

		/// <summary>
		/// If released, contains the date on which the entry was released by ACS cargo processing.
		/// </summary>
		[MessageBlockDate(13, "C", "MMddyy")]
		public ZDate ReleaseDate;

		/// <summary>
		/// A code indicating the payment/collection status of the entry summary. Valid codes are:
		/// 
		/// 0 = Not paid.
		/// 1 = Partially paid.
		/// 2 = Fully paid.
		/// 3 = Duty free.
		/// 5 = Drawback.
		/// 6 = Authorized.
		/// </summary>
		[MessageBlockString(1, 20, "M")]
		public ZString CollectionStatusCode;

		/// <summary>
		/// The last date on which a payment was made for the entry summary.
		/// </summary>
		[MessageBlockDate(21, "C", "MMddyy")]
		public ZDate CollectionDate;

		/// <summary>
		/// The date of the last extension or suspension of liquidation of the entry summary.
		/// </summary>
		[MessageBlockDate(28, "C", "MMddyy")]
		public ZDate ExtensionSuspensionDate;

		/// <summary>
		/// The date of the last extension or suspension of liquidation as provided to the filer.
		/// </summary>
		[MessageBlockDate(34, "C", "MMddyy")]
		public ZDate ExtensionSuspensionNoticeDate;

		/// <summary>
		/// A code indicating the status of Census warnings for the entry summary based upon line level information. Valid codes are:
		/// 
		/// 0 = No Census warnings exist.
		/// 1 = An unresolved Census warning exists for at least one line item.
		/// 6 = All Census warnings have been resolved.
		/// </summary>
		[MessageBlockString(1, 40, "M")]
		public ZString CensusHeaderStatusCode;

		/// <summary>
		/// A code indicating the invoice status of the entry summary. Valid codes are:
		/// 
		/// Space = Non-EIP, electronic invoice not available.
		/// 1 = Open - default status when the entry summary is received and stored in ACE.
		/// 2 = Received - invoices have been received and stored in ACS.
		/// 3 = Deleted - invoice(s) stored in ACS have been disassociated from this entry summary.
		/// 4 = Invoice requested - this status is set when an automated request for invoices is sent to the filer.
		/// </summary>
		[MessageBlockString(1, 41, "M")]
		public ZString InvoiceStatusCode;

		/// <summary>
		/// A code which indicates if a protest has been filed and if so the status of the protest. Valid codes are:
		/// 
		/// Space = No protest.
		/// OP = Case open.
		/// AP = Approved.
		/// DN = Denied.
		/// SP = Suspended.
		/// PD = Partially denied.
		/// WD = Withdrawn denial of protest.
		/// UT = Untimely denial.
		/// </summary>
		[MessageBlockString(2, 42, "M")]
		public ZString ProtestStatusCode;

		/// <summary>
		/// A code which indicates the status of quota processing. Valid codes are:
		/// 
		/// Space = Quota not processed.
		/// 1 = Quota processed.
		/// 2 = Quota no lines.
		/// 3 = Quota processed, no lines are quota.
		/// 4 = Quota line deleted by CBP.
		/// </summary>
		[MessageBlockString(1, 44, "M")]
		public ZString QuotaStatusCode;

		/// <summary>
		/// Entry filer's identification code for the Trade Agreement reconciliation entry.
		/// </summary>
		[MessageBlockString(3, 45, "C")]
		public ZString TradeAgreementReconciliationFilerCode;

		/// <summary>
		/// Trade agreement reconciliation entry number for the reconciliation entry summary.
		/// </summary>
		[MessageBlockString(8, 50, "C")]
		public ZString TradeAgreementReconciliationEntryNumber;

		/// <summary>
		/// Entry filer's identification code for the Other reconciliation entry.
		/// </summary>
		[MessageBlockString(3, 58, "C")]
		public ZString OtherReconciliationFilerCode;

		/// <summary>
		/// Other reconciliation entry number for the reconciliation entry summary.
		/// </summary>
		[MessageBlockString(8, 63, "C")]
		public ZString OtherReconciliationEntryNumber;

		/// <summary>
		/// A code, which indicates the reason for the extension or suspension of the entry summary liquidation.. Valid codes are:
		/// 43 = CVD Suspend.
		/// 44 = ADD Suspend.
		/// 45 = AD/CVD Suspend.
		/// 46 = Court Ordered Suspend.
		/// 47 = Actual Use Suspend.
		/// 48 = Other 1 Suspend.
		/// 49 = Customs Ext.
		/// 50 = Importer Ext.
		/// 51 = Other Ext.
		/// 65 = Subject to EAPA.
		/// 66 = Subject to Court Injunction.
		/// </summary>
		[MessageBlockString(2, 71, "C")]
		public ZString ExtensionSuspensionStatusCode1;

		/// <summary>
		/// A code, which indicates the reason for the extension or suspension of the entry summary liquidation.. Valid codes are:
		/// 43 = CVD Suspend.
		/// 44 = ADD Suspend.
		/// 45 = AD/CVD Suspend.
		/// 46 = Court Ordered Suspend.
		/// 47 = Actual Use Suspend.
		/// 48 = Other 1 Suspend.
		/// 49 = Customs Ext.
		/// 50 = Importer Ext.
		/// 51 = Other Ext.
		/// 65 = Subject to EAPA.
		/// 66 = Subject to Court Injunction.
		/// </summary>
		[MessageBlockString(2, 73, "O")]
		public ZString ExtensionSuspensionStatusCode2;

		/// <summary>
		/// A code, which indicates the reason for the extension or suspension of the entry summary liquidation.. Valid codes are:
		/// 43 = CVD Suspend.
		/// 44 = ADD Suspend.
		/// 45 = AD/CVD Suspend.
		/// 46 = Court Ordered Suspend.
		/// 47 = Actual Use Suspend.
		/// 48 = Other 1 Suspend.
		/// 49 = Customs Ext.
		/// 50 = Importer Ext.
		/// 51 = Other Ext.
		/// 65 = Subject to EAPA.
		/// 66 = Subject to Court Injunction.
		/// </summary>
		[MessageBlockString(2, 75, "O")]
		public ZString ExtensionSuspensionStatusCode3;

		/// <summary>
		/// A code, which indicates the reason for the extension or suspension of the entry summary liquidation.. Valid codes are:
		/// 43 = CVD Suspend.
		/// 44 = ADD Suspend.
		/// 45 = AD/CVD Suspend.
		/// 46 = Court Ordered Suspend.
		/// 47 = Actual Use Suspend.
		/// 48 = Other 1 Suspend.
		/// 49 = Customs Ext.
		/// 50 = Importer Ext.
		/// 51 = Other Ext.
		/// 65 = Subject to EAPA.
		/// 66 = Subject to Court Injunction.
		/// </summary>
		[MessageBlockString(2, 77, "O")]
		public ZString ExtensionSuspensionStatusCode4;
	}
}
