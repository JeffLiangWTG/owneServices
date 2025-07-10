using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class CusBondDetailValidation : MasterFiles.Business.CusBondDetailValidation
	{
		public CusBondDetailValidation(CusBondDetail bondData)
			: base(bondData)
		{
		}

		new CusBondDetail Parent
		{
			get { return (CusBondDetail)base.Parent; }
		}

		#region Overrides

		protected override void CheckPW_ActivityCode()
		{
			base.CheckPW_ActivityCode();

			if (Parent.PW_ActivityCode.IsEmpty)
			{
				Parent.PW_ActivityCodeInfo.AddError(ActivityCodeShouldBeEntered);
			}

			ListValidation.ErrorIfInvalidCode(Parent.PW_ActivityCodeInfo, Parent.Lookups.ActivityCodeList);
		}

		internal const string ActivityCodeShouldBeEntered = "Activity Code is required. Bond details are defaulted to the Declaration from the Importer of Record Organization where Activity Code is '1' or '1a1'.";

		protected override void CheckPW_BondAmount()
		{
			base.CheckPW_BondAmount();

			MandatoryValidation.CheckNotNegative(Parent.PW_BondAmountInfo);
		}

		protected override void CheckPW_BondFiledPort()
		{
			base.CheckPW_BondFiledPort();
			ListValidation.ErrorIfInvalidCode(Parent.PW_BondFiledPortInfo, Parent.Lookups.RegionPorts);
		}

		protected override void CheckPW_BondType()
		{
			base.CheckPW_BondType();
			ListValidation.ErrorIfInvalidCode(Parent.PW_BondTypeInfo, Parent.Lookups.BondTypeList);
		}

		protected override void CheckPW_BondExpiryDate()
		{
			base.CheckPW_BondExpiryDate();

			if (Parent.PW_BondExpiryDate < Parent.PW_BondEffectiveDate)
			{
				Parent.PW_BondExpiryDateInfo.AddError(ExpiryDateLessThenEffectiveDate);
			}
		}
		internal const string ExpiryDateLessThenEffectiveDate = "Bond Expiry Date should be greater or equal to Effective Date.";

		protected override void CheckPW_BondEffectiveDate()
		{
			base.CheckPW_BondEffectiveDate();

			if (Parent.PW_BondType != ImporterBondTypeList.Codes.SingleTransactionBond)
			{
				MandatoryValidation.CheckEntered(Parent.PW_BondEffectiveDateInfo, "Bond Effective Date");
			}
			ValidatePW_BondExpiryDate();
		}

		protected override void CheckPW_BondEffectiveDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckPW_BondExpiryDateIsValidZDateTimeRange()
		{
		}

		#endregion
	}
}
