namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyWrapperForOnlyICS2 : GlbCompanyWrapper
	{
		public GlbCompanyWrapperForOnlyICS2(GlbCompany company) : base(company)
		{
		}

		public override bool IsValidWrapper => true;
	}
}
