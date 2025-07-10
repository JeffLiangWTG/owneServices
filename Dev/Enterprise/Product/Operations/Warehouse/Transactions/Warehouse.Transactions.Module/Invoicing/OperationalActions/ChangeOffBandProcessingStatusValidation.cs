using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ChangeOffBandProcessingStatusValidation : ZValidation
	{
		public ChangeOffBandProcessingStatusValidation(BusinessObject parent)
			: base(parent)
		{
		}

		#region AutoValidationType

		public override Type AutoValidationType => typeof(ChangeOffBandProcessingStatusValidation);

		#endregion

		#region ValidateSelectedOffBandProcessingStatus

		public void ValidateSelectedOffBandProcessingStatus()
		{
			ValidateCalculatedProperty(Applicator.SelectedOffBandProcessingStatusInfo);
		}

		protected void CheckSelectedOffBandProcessingStatus()
		{
			MandatoryValidation.CheckEntered(Applicator.SelectedOffBandProcessingStatusInfo);
			ListValidation.ErrorIfInvalidCode(Applicator.SelectedOffBandProcessingStatusInfo, Applicator.StorageOffBandProcessingStatuses);
		}

		#endregion

		#region Validate All

		public override void ValidateAll() => ValidateSelectedOffBandProcessingStatus();

		#endregion

		#region Parent

		ChangeOffBandProcessingStatusActionMethodApplicator Applicator => (ChangeOffBandProcessingStatusActionMethodApplicator)ParentFilter;

		#endregion
	}
}
