//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusStatementLineValidation
//
//    This class should be used for overriding validation in AutoCusStatementLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class CusStatementLineValidation : Customs.Business.CusStatementLineValidation
	{
		public CusStatementLineValidation(CusStatementLine parent)
			: base(parent)
		{
		}

		protected new CusStatementLine Parent
		{
			get { return (CusStatementLine)base.Parent; }
		}

		protected override void CheckB3_CustomsFeesTotal()
		{
			base.CheckB3_CustomsFeesTotal();
			if (Parent.Declaration != null && !Parent.IsStatusDeleted)
			{
				if (Parent.B3_CustomsFeesTotal != Parent.ARTotalAmount && Parent.StatementHeader.IsPaidByBroker)
				{
					Parent.B3_CustomsFeesTotalInfo.AddMessageError(CustomsFeeTotal);
				}
				else if (Parent.ARUnPostedAmount > 0)
				{
					Parent.B3_CustomsFeesTotalInfo.AddWarning(ARPostedArAmount);
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateReleaseStatus();
		}

		public void ValidateReleaseStatus()
		{
			ValidateCalculatedProperty(Parent.ReleaseStatusInfo);
		}

		protected void CheckReleaseStatus()
		{
			if (Parent.Declaration != null && !Parent.IsStatusDeleted && Parent.ReleaseStatus != CRLReleaseStatusList.Codes.NRT && Parent.ReleaseStatus != CRLReleaseStatusList.Codes.REL && !EntryTypeList.IsExWarehouseType(Parent.B3_EntryType) && !Parent.Declaration.US_ConsolACE)
			{
				Parent.ReleaseStatusInfo.AddMessageError(NotReleaseStatus);
			}
		}

		internal const string NotReleaseStatus = "This entry has not been released yet.";
		internal const string CustomsFeeTotal = "The AR amount of this entry does not match this amount.";
		internal const string ARPostedArAmount = "There is an unposted AR amount for this entry.";
	}
}
