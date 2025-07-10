//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobSundryChargesValidation
//
//    This class should be used for overriding validation in AutoJobSundryChargesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Agency.Business
{
	using CargoWise.EntityFramework;

	public class JobSundryChargesValidation : AutoJobSundryChargesValidation
	{
		public JobSundryChargesValidation(AutoJobSundryCharges parent)
			: base(parent) { }

		protected override void CheckD4_OH_BillToParty()
		{
			base.CheckD4_OH_BillToParty();
			MandatoryValidation.CheckEntered(Parent.D4_OH_BillToPartyInfo);
			ListValidation.ErrorIfInvalidPK(Parent.D4_OH_BillToPartyInfo, Parent.Lookups.BillToParties);
		}

		protected override void CheckD4_SundriesJobType()
		{
			base.CheckD4_SundriesJobType();

			MandatoryValidation.CheckEntered(Parent.D4_SundriesJobTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.D4_SundriesJobTypeInfo, Parent.Lookups.Types);
		}

		protected override void CheckD4_SundryJobMode()
		{
			base.CheckD4_SundryJobMode();

			MandatoryValidation.CheckEntered(Parent.D4_SundryJobModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.D4_SundryJobModeInfo, Parent.Lookups.Modes);
		}

		protected override void CheckD4_SundryJobActivity()
		{
			base.CheckD4_SundryJobActivity();

			MandatoryValidation.CheckEntered(Parent.D4_SundryJobActivityInfo);
			ListValidation.ErrorIfInvalidCode(Parent.D4_SundryJobActivityInfo, Parent.Lookups.Activities);
		}

		protected override void CheckD4_FromDate()
		{
			base.CheckD4_FromDate();
			MandatoryValidation.CheckEntered(Parent.D4_FromDateInfo);

			if (!Parent.D4_FromDate.IsEmpty && !Parent.D4_ToDate.IsEmpty && Parent.D4_FromDate > Parent.D4_ToDate)
			{
				Parent.D4_FromDateInfo.AddError(Res.GetString("a8e3b2c3-440d-4071-86b1-7d8a29a01b3d", "From Date cannot be after To Date."));
			}

			foreach (OverlappingJob job in Parent.OverlappingJobs)
			{
				Parent.D4_FromDateInfo.AddError(Res.GetString("c2925df3-96ea-4b73-b4e2-6d4719cc0cda", "This job overlaps {0} ({1:dd-MMM-yy} to {2:dd-MMM-yy}) for the same bill to party.", job.JobNumber, job.FromDate, job.ToDate));
			}
		}

		protected override void CheckD4_ToDate()
		{
			base.CheckD4_ToDate();
			MandatoryValidation.CheckEntered(Parent.D4_ToDateInfo);

			if (!Parent.D4_ToDate.IsEmpty && !Parent.D4_FromDate.IsEmpty && Parent.D4_FromDate > Parent.D4_ToDate)
			{
				Parent.D4_ToDateInfo.AddError(Res.GetString("874aac7e-112e-4e88-b078-ab3e17b1608a", "To Date cannot be before From Date."));
			}

			foreach (OverlappingJob job in Parent.OverlappingJobs)
			{
				Parent.D4_ToDateInfo.AddError(Res.GetString("c2925df3-96ea-4b73-b4e2-6d4719cc0cda", "This job overlaps {0} ({1:dd-MMM-yy} to {2:dd-MMM-yy}) for the same bill to party.", job.JobNumber, job.FromDate, job.ToDate));
			}
		}

		#region Implementation

		new SundryCharges Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (SundryCharges)base.Parent; }
		}

		#endregion
	}
}


