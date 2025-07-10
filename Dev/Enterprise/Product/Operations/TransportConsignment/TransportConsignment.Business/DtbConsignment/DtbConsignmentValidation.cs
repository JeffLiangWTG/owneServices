
namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentValidation : AutoDtbConsignmentValidation
	{
		public DtbConsignmentValidation(AutoDtbConsignment parent) : base(parent)
		{
		}

		protected override void CheckLTC_JobType()
		{
			base.CheckLTC_JobType();
			if (Parent.LTC_JobType != "FCL" && Parent.LTC_JobType != "FTL" && Parent.LTC_JobType != "LTL")
			{
				Parent.LTC_JobTypeInfo.AddError(Res.GetString("F958196D-CA6E-4757-96EA-00BB1FDCAC26", "LTC Job Type should be set to 'FCL', 'FTL', or 'LTL'."));
			}
		}
	}
}
