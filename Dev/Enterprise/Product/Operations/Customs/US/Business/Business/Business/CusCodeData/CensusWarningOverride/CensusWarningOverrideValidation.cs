namespace Enterprise.Customs.US.Business
{
	public class CensusWarningOverrideValidation : Customs.Business.CusCodeDataValidation
	{
		public CensusWarningOverrideValidation(CensusWarningOverride parent)
			: base(parent)
		{
		}

		public new CensusWarningOverride Parent
		{
			get { return (CensusWarningOverride)base.Parent; }
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();
			ValidateCY_Data();
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (!Parent.CY_Code.IsEmpty && Parent.CY_Data.IsEmpty)
			{
				Parent.CY_DataInfo.AddMessageError(CensusWarningOverrideValidation.OvercodeRequired);
			}
		}

		internal const string OvercodeRequired = "Override Code is required if Census Condition Code is entered.";
	}
}
