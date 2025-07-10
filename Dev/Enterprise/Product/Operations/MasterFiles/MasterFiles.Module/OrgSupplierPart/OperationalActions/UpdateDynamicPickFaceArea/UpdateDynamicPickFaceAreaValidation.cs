using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module
{
	public class UpdateDynamicPickFaceAreaValidation : ZValidation
	{
		public UpdateDynamicPickFaceAreaValidation(BusinessObject parent)
			: base(parent)
		{
		}

		#region AutoValidationType

		public override Type AutoValidationType => typeof(UpdateDynamicPickFaceAreaValidation);

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

		#region ValidateDynamicPickAreaPK

		public void ValidateDynamicPickAreaPK()
		{
			ValidateCalculatedProperty(Applicator.DynamicPickAreaPKInfo);
		}

		protected void CheckDynamicPickAreaPK()
		{
			ListValidation.ErrorIfInvalidPK(Applicator.DynamicPickAreaPKInfo);
		}

		#endregion

		#region ValidateOverrideNonEmptyDynamicPickArea

		public void ValidateOverrideNonEmptyDynamicPickArea()
		{
			ValidateCalculatedProperty(Applicator.OverrideNonEmptyDynamicPickAreaInfo);
		}

		protected void CheckOverrideNonEmptyDynamicPickArea()
		{
			if (Applicator.DynamicPickAreaPK.IsEmpty && !Applicator.OverrideNonEmptyDynamicPickArea)
			{
				Applicator.OverrideNonEmptyDynamicPickAreaInfo.AddError(Res.GetString("8cfeacb7-a895-46de-8c5c-f592fab024e7", "Override Empty Dynamic Pick Area must be ticked when clearing Dynamic Pick Area."));
			}
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateClientPK();
			ValidateWarehousePK();
			ValidateDynamicPickAreaPK();
			ValidateOverrideNonEmptyDynamicPickArea();
		}

		#endregion

		#region Parent

		UpdateDynamicPickFaceAreaMethodApplicator Applicator => (UpdateDynamicPickFaceAreaMethodApplicator)base.ParentFilter;

		#endregion
	}
}
