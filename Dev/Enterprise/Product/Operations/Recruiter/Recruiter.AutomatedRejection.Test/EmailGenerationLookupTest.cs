using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Recruiter.AutomatedRejection;
using NUnit.Framework;

namespace Enterprise.Recruiter.Testing.AutomatedRejection
{
	[TestedType(typeof(EmailGenerationLookup))]
	sealed class EmailGenerationLookupTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var lookup = new EmailGenerationLookup()
			{
				CompanyName = "WiseTech Global",
				FirstName = "Bob",
				LinkedInURL = new Uri("http://www.likedin.com/abc"),
				CompanySignatureLogoHtml = "<img/>"
			};

			AssertEquals("WiseTech Global", lookup.CompanyName);
			AssertEquals("Bob", lookup.FirstName);
			AssertEquals("http://www.likedin.com/abc", lookup.LinkedInURL);
			AssertEquals("<img/>", lookup.CompanySignatureLogoHtml);
		}

		protected override BusinessObject GetNewBusinessObject() => new EmailGenerationLookup();
	}
}
