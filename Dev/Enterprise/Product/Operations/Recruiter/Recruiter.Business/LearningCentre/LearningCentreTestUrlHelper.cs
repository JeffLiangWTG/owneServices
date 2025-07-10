using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public static class LearningCentreTestUrlHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string GetTestUrl(HRJobApplicant applicant, string countryCode, string language = "")
		{
			return "";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string GetTestUrl(string examId, IExamUrlRecipient recipient, string countryCode, string jobSkillCode = "", string language = "", string examSettingsCode = "", BusinessObjectFactory factory = null)
		{
			return string.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string GetTestUrlForContact(string examId, Guid contactPK, string countryCode, string jobSkillCode = "", string language = "", string examSettingsCode = "")
		{
			var factory = new BusinessObjectFactory();
			var contact = factory.Load<OrgContact>(contactPK);
			return GetTestUrl(examId, contact, countryCode, jobSkillCode, language, examSettingsCode, factory);
		}
	}
}
