using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module
{
	public class AssignWhsPutawayGroupValidation : ZValidation
	{
		public AssignWhsPutawayGroupValidation(BusinessObject parent)
			: base(parent)
		{
		}

		#region AutoValidationType

		public override Type AutoValidationType
		{
			get { return typeof(AssignWhsPutawayGroupValidation); }
		}

		#endregion

		#region ValidateWhsPutawayGroupPK

		public void ValidateWhsPutawayGroupPK()
		{
			ValidateCalculatedProperty(Applicator.WhsPutawayGroupPKInfo);
		}

		protected void CheckWhsPutawayGroupPK()
		{
			ListValidation.ErrorIfInvalidPK(Applicator.WhsPutawayGroupPKInfo);
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

		#region Validate All

		public override void ValidateAll()
		{
			ValidateWhsPutawayGroupPK();
			ValidateWarehousePK();
		}

		#endregion

		#region Parent

		AssignWhsPutawayGroupMethodApplicator Applicator
		{
			get { return (AssignWhsPutawayGroupMethodApplicator)base.ParentFilter; }
		}

		#endregion
	}
}
