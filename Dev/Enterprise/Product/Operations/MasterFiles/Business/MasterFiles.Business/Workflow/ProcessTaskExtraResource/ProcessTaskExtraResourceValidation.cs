using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskExtraResourceValidation : AutoProcessTaskExtraResourceValidation
	{
		public ProcessTaskExtraResourceValidation(AutoProcessTaskExtraResource parent) : base(parent)
		{
		}

		#region PE_GS_NKStaffOrResource

		protected override void CheckPE_GS_NKStaffOrResource()
		{
			base.CheckPE_GS_NKStaffOrResource();
			MandatoryValidation.CheckEntered(Parent.PE_GS_NKStaffOrResourceInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PE_GS_NKStaffOrResourceInfo);
		}

		#endregion
	}
}
