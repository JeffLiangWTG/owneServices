using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AutoRateDateValidation : JobConfigurationSelectorValidation
	{
		public AutoRateDateValidation(AutoRateDate parent)
			: base(parent)
		{
		}

		protected new AutoRateDate Parent => (AutoRateDate)base.Parent;

		#region Implementation

		protected override string DuplicateJobParametersError => Res.GetString("b3a791c1-fee0-45f2-a6fa-652ab24d59ef", "At least one more record already sets Auto Rate behavior for the same parameters.");

		protected override bool IsDuplicateJobParameter(IJobConfigurationSelector item) =>
			IsDuplicateAutoRateDate(Parent, (IAutoRateDate)item);

		public static bool IsDuplicateAutoRateDate(IAutoRateDate autoRateDate1, IAutoRateDate autoRateDate2) =>
			autoRateDate1.Location == autoRateDate2.Location
			&& autoRateDate1.RateType == autoRateDate2.RateType
			&& autoRateDate1.ContainerMode == autoRateDate2.ContainerMode
			&& autoRateDate1.Mode == autoRateDate2.Mode
			&& autoRateDate1.JobType == autoRateDate2.JobType
			&& autoRateDate1.DirectionCode == autoRateDate2.DirectionCode;

		#endregion

		#region Date

		public void ValidateDateType()
		{
			MandatoryValidation.CheckEntered(Parent.DateTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DateTypeInfo, Parent.Lookups.DateTypeList);
		}

		#endregion

		#region Location

		public void ValidateLocation()
		{
			ListValidation.ErrorIfInvalidCode(Parent.LocationInfo, Parent.Lookups.AutoRatingLocationCollection);
		}

		#endregion

		#region Rate Type

		public void ValidateRateType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.RateTypeInfo, Parent.Lookups.RateTypeList);
		}

		#endregion

		#region Container Mode

		public void ValidateContainerMode()
		{
			if (!Parent.ContainerModeInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ContainerModeInfo, Parent.Lookups.ContainerModeList);
			}
		}

		#endregion
	}
}
