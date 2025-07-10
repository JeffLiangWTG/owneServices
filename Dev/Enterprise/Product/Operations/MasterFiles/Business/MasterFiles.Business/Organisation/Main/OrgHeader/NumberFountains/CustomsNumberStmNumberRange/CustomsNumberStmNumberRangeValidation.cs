namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberStmNumberRangeValidation : StmNumberRangeValidation
	{
		public CustomsNumberStmNumberRangeValidation(CustomsNumberStmNumberRange parent)
			: base(parent)
		{
		}

		protected override void CheckSNR_NameIsWesternEuropean()
		{
		}

		protected override void CheckSNR_ThresholdRunOutWarning()
		{
			base.CheckSNR_ThresholdRunOutWarning();
			if (Parent.TotalAvailableNumbers < Parent.SNR_ThresholdRunOutWarning)
			{
				Parent.SNR_ThresholdRunOutWarningInfo.AddWarning(ThresholdRunOutWarningIsLessThanTotalAvailableNumbers);
			}
		}

		public static string ThresholdRunOutWarningIsLessThanTotalAvailableNumbers
		{
			get { return ResString.GetMultilingualString("{BE3F86CE-82F6-4A93-96D4-A15543A59324}", "Threshold Run Out Warning is greater than the total of available numbers."); }
		}

		protected new CustomsNumberStmNumberRange Parent
		{
			get { return (CustomsNumberStmNumberRange)base.Parent; }
		}
	}
}
