//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPaymentApprovalLookups
//
//    This class should be used for overriding collections in AutoAccPaymentApprovalLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccPaymentApprovalLookups : AutoAccPaymentApprovalLookups
	{
		public AccPaymentApprovalLookups(AutoAccPaymentApproval parent)
			: base(parent)
		{
		}

		public static string AccountsReceivableDescription
		{
			get { return Res.GetString("2dde07f4-df6c-4a4e-b5e5-28aee0971952", "Accounts Receivable"); }
		}
		public static string AccountsPayableDescription
		{
			get { return Res.GetString("b98eb517-a19e-475d-9925-99ba165cce4b", "Accounts Payable"); }
		}
		public static string AwaitingApprovalDescription
		{
			get { return Res.GetString("11efe04a-5b7d-4b64-813f-55b07e56aec6", "Awaiting Approval"); }
		}
		public static string FullyApprovedDescription
		{
			get { return Res.GetString("5fe1da2f-bbc6-4a04-ae6c-bd6068e42183", "Fully Approved"); }
		}
		public static string PostedDescription
		{
			get { return Res.GetString("1222c4d1-7b90-4305-bbce-135433587880", "Posted"); }
		}
		public static string RejectedDescription
		{
			get { return Res.GetString("58878535-7c57-4820-8a09-841a5d638ece", "Rejected"); }
		}
		public static string CancelledDescription
		{
			get { return Res.GetString("8204976F-B3FF-4036-BE02-7E26B21FD60B", "Canceled"); }
		}
		public static string DraftDescription
		{
			get { return Res.GetString("bacb6193-9030-474d-9671-a8cda6c8870a", "Draft"); }
		}

		public override OrgHeaderCollection Headers
		{
			get { return new CreditorCollection(Factory); }
		}

		public GlbStaffCollection CreatingUsers
		{
			get { return new GlbStaffCollection(Factory); }
		}

		public CodeDescriptionPairList AV_LedgerList
		{
			get
			{
				CodeDescriptionPairList fAV_LedgerList = new CodeDescriptionPairList();
				fAV_LedgerList.AddPair(LedgerTypes.AccountsPayable, AccountsPayableDescription);
				fAV_LedgerList.AddPair(LedgerTypes.AccountsReceivable, AccountsReceivableDescription);

				return fAV_LedgerList;
			}
		}

		public CodeDescriptionPairList AV_StatusList => aV_StatusList ?? (aV_StatusList = GetStatusList());
		CodeDescriptionPairList aV_StatusList;

		public static CodeDescriptionPairList GetStatusList()
		{
			CodeDescriptionPairList fAV_StatusList = new CodeDescriptionPairList();
			fAV_StatusList.AddPair(PaymentApprovalStatus.AwaitingApproval, AwaitingApprovalDescription);
			fAV_StatusList.AddPair(PaymentApprovalStatus.FullyApproved, FullyApprovedDescription);
			fAV_StatusList.AddPair(PaymentApprovalStatus.Posted, PostedDescription);
			fAV_StatusList.AddPair(PaymentApprovalStatus.Rejected, RejectedDescription);
			fAV_StatusList.AddPair(PaymentApprovalStatus.Cancelled, CancelledDescription);
			fAV_StatusList.AddPair(PaymentApprovalStatus.Draft, DraftDescription);

			return fAV_StatusList;
		}

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get { return new GlbDepartmentCollection(Factory); }
		}

		#endregion
	}
}
