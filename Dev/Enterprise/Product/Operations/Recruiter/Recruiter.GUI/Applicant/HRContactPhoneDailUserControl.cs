using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.Business.Applicant;

namespace Enterprise.Recruiter.GUI.Applicant
{
	public partial class HRContactPhoneDialUserControl : MultiPhoneDiallerUserControl
	{
		public HRContactPhoneDialUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupGridEvents();
		}

		#region PhoneDialInfo

		void SetupGridEvents()
		{
			var application = Application;
			if (application != null)
			{
				HRJobApplicantContactPhoneDialBuilder.GetDefaultDialInfo(application.Applicant);
			}
		}

		internal HRJobApplication Application
			=> CurrentDataItem as HRJobApplication;

		public void RefreshDialInfo(EventArgs e)
		{
			var application = Application;
			if (application != null && application.Applicant != null)
			{
				OnCurrentDataItemChanged(e);
			}
		}

		protected override PhoneDialInfo GetDefaultDialInfo()
		{
			var application = Application;
			return application != null ? HRJobApplicantContactPhoneDialBuilder.GetDefaultDialInfo(application.Applicant) : null;
		}
		protected override IEnumerable<PhoneDialInfo> AlternativePhoneDialInfoList
		{
			get
			{
				var application = Application;
				return application != null ? HRJobApplicantContactPhoneDialBuilder.GetAlternativePhoneDialInfos(application.Applicant) : Enumerable.Empty<PhoneDialInfo>();
			}
		}

		protected override IEnumerable<PhoneDialInfo> OrderedList => AlternativePhoneDialInfoList;

		#endregion
	}
}
