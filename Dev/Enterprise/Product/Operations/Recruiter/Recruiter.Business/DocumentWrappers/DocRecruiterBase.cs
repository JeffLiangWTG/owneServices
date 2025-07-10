using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public abstract class DocRecruiterBase : DocumentWrapper
	{
		public static class Fields
		{
			public const string AllTestsToBeCompleted = "(*AllTestsToBeCompleted*)"; // Document Macro
			public const string CompulsoryTestsToBeCompleted = "(*CompulsoryTestsToBeCompleted*)"; // Document Macro
			public const string RoleSpecificTestsToBeCompleted = "(*RoleSpecificTestsToBeCompleted*)"; // Document Macro
		}

		protected DocRecruiterBase(BusinessObject objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		[DocumentField("Applicant's Name")]
		public ZString ApplicantName
		{
			get { return GetApplicantValue(a => a.HA_FullName); }
		}

		[DocumentField("Applicant's Email Address")]
		public ZString ApplicantEmailAddress
		{
			get { return GetApplicantValue(a => a.HA_EmailAddress); }
		}

		[DocumentField("Positions Previously Applied")]
		public ZString CompletedJobApplications
		{
			get { return GetApplications(true); }
		}

		[DocumentField("Positions Currently Applying")]
		public ZString InProgressJobApplications
		{
			get { return GetApplications(false); }
		}

		[DocumentField("Company Name")]
		public ZString CompanyName
		{
			get { return GlbCompany.CurrentCompany.GC_Name; }
		}

		T GetApplicantValue<T>(Func<HRJobApplicant, T> func)
		{
			return (Applicant != null) ? func(Applicant) : default(T);
		}

		ZString GetApplications(bool completed)
		{
			IEnumerable<HRJobApplication> applications = GetApplicantValue(
				applicant => applicant.Applications.Cast<HRJobApplication>().Where(
					application => completed == CompletedApplicationStatuses.Contains(application.HP_CurrentStatus.ToString())));

			StringBuilder builder = new StringBuilder();
			foreach (HRJobApplication application in applications)
			{
				builder.AppendFormat(Res.GetString("62de9713-cfb3-451d-ae02-2c92ef94d092", "<tr><td style='padding-right:20px'>{0}</td><td>{1}</td></tr>",
					application?.JobOpening?.HV_AdTitle ?? ZString.Empty,
					application.Logs.AddedLog.SL_EventTime.Date.ToString()));
			}

			return (builder.Length > 0) ? string.Format(ApplicationsHtmlTemplate, builder.ToString()) : "";
		}

		string[] CompletedApplicationStatuses
		{
			get { return new[] { "REJ", "ACC" }; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "preformatted html")]
		const string ApplicationsHtmlTemplate =
@"<table>
	<thead>
		<td style='padding-right:20px'><b>Ad Title</b></td>
		<td><b>Applied</b></td>
	</thead>
	{0}
</table>";

		protected abstract HRJobApplicant Applicant { get; }
	}
}
