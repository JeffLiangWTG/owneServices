using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.Recruiter.Business
{
	public class DocHRJobApplication : DocRecruiterBase
	{
		DocHRJobApplication(HRJobApplication application, BusinessObjectFactory factory)
			: base(application, factory)
		{
		}

		public static DocHRJobApplication New(HRJobApplication application, BusinessObjectFactory factory)
		{
			return new DocHRJobApplication(application, factory);
		}

		#region Document Fields

		[DocumentField("Ad Title")]
		public ZString AdTitle
		{
			get { return GetJobCampaignValue(c => c.HV_AdTitle); }
		}

		[DocumentField("Job Role")]
		public ZString JobRole
		{
			get { return GetJobCampaignValue(c => c.JobRole != null ? c.JobRole.HJ_JobTitle : ZString.Empty); }
		}

		[DocumentField("Ad Start Date")]
		public ZDate AdStartDate
		{
			get { return GetJobCampaignValue(c => c.HV_CampaignStartDate.Date); }
		}

		[DocumentField("Ad End Date")]
		public ZDate AdEndDate
		{
			get { return GetJobCampaignValue(c => c.HV_CampaignEndDate.Date); }
		}

		[DocumentField("Number of Positions Available")]
		public ZByte NoOfPositionsAvailable
		{
			get { return GetJobCampaignValue(c => c.HV_NumberOfPositionsAvailable); }
		}

		[DocumentField("Number of Positions Already Filled")]
		public ZByte NoOfPositionsFilled
		{
			get { return GetJobCampaignValue(c => c.HV_NumberOfPositionsFilled); }
		}

		[DocumentField("Recruiter's Contact Name")]
		public ZString RecruiterContactName
		{
			get { return GetJobCampaignValue(c => c.ControlledBy != null ? c.ControlledBy.GS_FullName : ZString.Empty); }
		}

		[DocumentField("Salary Range")]
		public ZString SalaryRange
		{
			get { return GetJobCampaignValue(c => c.SalaryRangeAsText); }
		}

		T GetJobCampaignValue<T>(Func<HRRecruitmentJobCampaign, T> func)
		{
			return (WrappedObject.JobOpening != null) ? func(WrappedObject.JobOpening) : default(T);
		}

		#endregion

		public new HRJobApplication WrappedObject
		{
			get { return (HRJobApplication)base.WrappedObject; }
		}

		protected override HRJobApplicant Applicant
		{
			get { return WrappedObject.Applicant; }
		}
	}
}
