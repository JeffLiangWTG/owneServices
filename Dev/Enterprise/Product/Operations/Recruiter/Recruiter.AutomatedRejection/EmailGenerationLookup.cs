using System;
using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.AutomatedRejection
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not needed")]
	public class EmailGenerationLookup : NonPersistentBusinessObject, DocumentWrappers.IRecruitmentRejectionEmailDataSource
	{
		public string CompanyName { get; set; }
		public string FirstName { get; set; }
		public Uri LinkedInURL { get; set; }
		public string CompanySignatureLogoHtml { get; set; }
	}
}
