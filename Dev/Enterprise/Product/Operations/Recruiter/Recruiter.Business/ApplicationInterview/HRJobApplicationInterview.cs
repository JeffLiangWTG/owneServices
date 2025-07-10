using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business
{
	[DependentBusinessObject(typeof(HRJobApplication), "Interviews")]
	public class HRJobApplicationInterview : AutoHRJobApplicationInterview
	{
		public HRJobApplicationInterview(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoHRJobApplicationInterview.Schema
		{
			public const string InterviewerName = "InterviewerName";
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString commentText = Res.GetString("a9a5f3ae-2e37-4f8a-b7dc-da051026b540", "Interview for {0} with {1}",
					Application != null && Application.Applicant != null ? Application.Applicant.HA_FullName : ZString.Empty,
					Interviewer != null ? Interviewer.GS_FullName : ZString.Empty);

				return commentText;
			}
		}

		#endregion

		#region Properties

		#region HI_HP

		[RelatedBusinessObject("Application")]
		public override ZGuid HI_HP
		{
			get { return base.HI_HP; }
			set { base.HI_HP = value; }
		}

		#endregion

		#region InterviewerName

		public ZString InterviewerName
		{
			get { return Interviewer != null ? Interviewer.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo InterviewerNameInfo
		{
			get { return GetZPropertyInfo(Schema.InterviewerName); }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public HRJobApplication Application
		{
			get { return Factory.Load<HRJobApplication>(HI_HP); }
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			HRJobApplication application = Factory.NewWithValidTestData<HRJobApplication>();
			HI_HP = application.PK;
		}

#endif
		#endregion

	}
}
