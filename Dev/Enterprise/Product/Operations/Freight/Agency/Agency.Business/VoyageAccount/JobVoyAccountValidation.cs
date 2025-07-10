//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyAccountValidation
//
//    This class should be used for overriding validation in AutoJobVoyAccountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class JobVoyAccountValidation : AutoJobVoyAccountValidation
	{
		public JobVoyAccountValidation(AutoJobVoyAccount parent)
			: base(parent) { }

		protected override void CheckNA_OH()
		{
			base.CheckNA_OH();
			CheckIsDistinct(Parent.NA_OHInfo);
		}

		protected override void CheckNA_JVIsNotEmpty()
		{
			// NA_JV is not bound directly so the validation should be on NA_Calc_Vessel and NA_Calc_Voyage
			// via the CheckSchedule method instead.
		}
		protected override void CheckNA_JVIsValidZGuid()
		{
			// NA_JV is not bound directly so the validation should be on NA_Calc_Vessel and NA_Calc_Voyage
			// via the CheckSchedule method instead.
		}

		public void ValidateNA_Calc_Vessel()
		{
			ValidateCalculatedProperty(Parent.NA_Calc_VesselInfo);
		}
		protected virtual void CheckNA_Calc_Vessel()
		{
			CheckSchedule(Parent.NA_Calc_VesselInfo);
			CheckIsDistinct(Parent.NA_Calc_VesselInfo);
		}

		public void ValidateNA_Calc_Voyage()
		{
			ValidateCalculatedProperty(Parent.NA_Calc_VoyageInfo);
		}
		protected virtual void CheckNA_Calc_Voyage()
		{
			CheckSchedule(Parent.NA_Calc_VoyageInfo);
			CheckIsDistinct(Parent.NA_Calc_VoyageInfo);
		}

		#region Implementation

		void CheckSchedule(ZPropertyInfo info)
		{
			if (Parent.NA_JV.IsEmpty || !Parent.NA_JV.IsValid)
			{
				info.AddError(Res.GetString("8b4da558-8a9c-4d52-a5b0-c67db95686fc", "Select a valid sailing schedule."));
			}
		}

		void CheckIsDistinct(ZPropertyInfo info)
		{
			if (!Parent.IsDistinct)
			{
				info.AddError(Parent.IsNotDistinctErrorMessage);
			}
		}

		protected new VoyageAccount Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (VoyageAccount)base.Parent; }
		}

		#endregion
	}
}


