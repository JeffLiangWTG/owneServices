using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module
{
	public class UpdateExpiryNotificationPeriodValidation : ZValidation
	{
		public UpdateExpiryNotificationPeriodValidation(BusinessObject parent)
			: base(parent)
		{
		}

		#region AutoValidationType

		public override Type AutoValidationType
		{
			get { return typeof(UpdateExpiryNotificationPeriodValidation); }
		}

		#endregion

		#region ValidateClientPK

		public void ValidateClientPK()
		{
			ValidateCalculatedProperty(Applicator.ClientPKInfo);
		}

		protected void CheckClientPK()
		{
			MandatoryValidation.CheckEntered(Applicator.ClientPKInfo);
			ListValidation.ErrorIfInvalidPK(Applicator.ClientPKInfo);
		}

		#endregion

		#region ValidateWarehousePK

		public void ValidateWarehousePK()
		{
			ValidateCalculatedProperty(Applicator.WarehousePKInfo);
		}

		protected void CheckWarehousePK()
		{
			MandatoryValidation.CheckEntered(Applicator.WarehousePKInfo);
			ListValidation.ErrorIfInvalidPK(Applicator.WarehousePKInfo);
		}

		#endregion

		#region ValidateExpiryNotificationPeriod

		public void ValidateExpiryNotificationPeriod()
		{
			ValidateCalculatedProperty(Applicator.ExpiryNotificationPeriodInfo);
		}

		protected void CheckExpiryNotificationPeriod()
		{
			MandatoryValidation.CheckNotNegative(Applicator.ExpiryNotificationPeriodInfo);
		}

		#endregion

		#region ValidateOverrideExistingNonZeroValues

		public void ValidateOverrideExistingNonZeroValues()
		{
			ValidateCalculatedProperty(Applicator.OverrideNonZeroExpiryNotificationPeriodInfo);
		}

		protected void CheckOverrideNonZeroExpiryNotificationPeriod()
		{
			if (Applicator.ExpiryNotificationPeriod == 0 && !Applicator.OverrideNonZeroExpiryNotificationPeriod)
			{
				Applicator.OverrideNonZeroExpiryNotificationPeriodInfo.AddError(Res.GetString("c7a3aaa4-5fa8-4486-882c-fa3030f2f954", "Override Non-Zero Expiry Notification Period must be true when clearing Expiry Notification Periods."));
			}
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateClientPK();
			ValidateWarehousePK();
			ValidateExpiryNotificationPeriod();
			ValidateOverrideExistingNonZeroValues();
		}

		#endregion

		#region Parent

		UpdateExpiryNotificationPeriodMethodApplicator Applicator
		{
			get { return (UpdateExpiryNotificationPeriodMethodApplicator)base.ParentFilter; }
		}

		#endregion
	}
}
