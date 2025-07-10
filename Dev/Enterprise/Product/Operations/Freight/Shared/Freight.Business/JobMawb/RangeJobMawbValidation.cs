using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class RangeJobMawbValidation : JobMawbValidation
	{
		readonly RangeJobMawb rangeJobMawb;

		public RangeJobMawbValidation(RangeJobMawb parent)
			: base(parent)
		{
			validationInternals = this;
			rangeJobMawb = parent;
		}

		readonly IValidationInternals validationInternals;

		#region Validate All

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateMawbCount();
			ValidateNumberRangeStart();
			ValidateNumberRangeEnd();
		}

		#endregion

		#region JM_MAWB

		protected override void CheckJM_MAWB()
		{
		}

		#endregion

		protected override void CheckJM_AirlinePrefixCompanyBranch()
		{
			if (rangeJobMawb.IsNeutralMAWB)
			{
				base.CheckJM_AirlinePrefixCompanyBranch();
			}
		}

		#region MawbCount

		public void ValidateMawbCount()
		{
			validationInternals.Validate(MawbParent.MawbCountInfo, CheckMawbCount);
		}

		void CheckMawbCount()
		{
			MandatoryValidation.CheckEntered(MawbParent.MawbCountInfo);

			if (MawbParent.MawbCount > 10000)
			{
				MawbParent.MawbCountInfo.AddError(Res.GetString("7c9d6569-2828-4be0-89e1-4ef6a6d02fc4", "You must add less than 10000 Master Bill Numbers."));
			}
			else if (MawbParent.MawbCount > 1000)
			{
				MawbParent.MawbCountInfo.AddWarning(Res.GetString("367f995a-eb74-4c43-8dc4-b7cb58347006", "You are attempting to add a large number of records to the database. This may take a long time."));
			}
		}

		#endregion

		#region NumberRangeStart

		public void ValidateNumberRangeStart()
		{
			validationInternals.Validate(MawbParent.NumberRangeStartInfo, CheckNumberRangeStart);
		}

		void CheckNumberRangeStart()
		{
			MandatoryValidation.CheckEntered(MawbParent.NumberRangeStartInfo);
			MAWBValidation.CheckMawbCheckDigit(MawbParent.NumberRangeStartInfo);

			if (MawbParent.IsEndMAWBOutOfRange)
			{
				MawbParent.NumberRangeStartInfo.AddError(Res.GetString("7882c272-7d35-4ffc-b7e1-eb57dcb8f585", "The From MAWB will generate a To MAWB that is out of range.  The MAWB generated can be no more than 8 digits."));
			}
		}

		#endregion

		#region NumberRangeEnd

		public void ValidateNumberRangeEnd()
		{
			validationInternals.Validate(MawbParent.NumberRangeEndInfo, CheckNumberRangeEnd);
		}

		void CheckNumberRangeEnd()
		{
			MandatoryValidation.CheckEntered(MawbParent.NumberRangeEndInfo);
			MAWBValidation.CheckMawbCheckDigit(MawbParent.NumberRangeEndInfo);
		}

		#endregion

		#region Implementation

		RangeJobMawb MawbParent
		{
			get
			{
				return (RangeJobMawb)Parent;
			}
		}

		#endregion
	}
}
