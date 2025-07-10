using System;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Integration.Testing
{
	public class OrganisationMatchingByVATRegistrationNumberContextsTest : TestCase
	{
		public void TestGetContextByCode()
		{
			var context = OrganisationMatchingByVATRegistrationNumberContexts.GetContextByCode(OrganisationMatchingByVATRegistrationNumberContexts.Codes.Receivables);
			AssertEquals(OrganisationMatcherContexts.Receivables, context);

			AssertExceptionThrown<NotSupportedException>("Unknown organization matcher context code.", () => OrganisationMatchingByVATRegistrationNumberContexts.GetContextByCode("Unknown"));
		}

		public void TestGetContexts()
		{
			var contextsPairList = new CodeDescriptionPairList();
			contextsPairList.AddRange(new OrganisationMatchingByVATRegistrationNumberContexts());
			var contexts = OrganisationMatchingByVATRegistrationNumberContexts.GetContexts(contextsPairList);
			var expectedContexts = new OrganisationMatcherContexts[] { OrganisationMatcherContexts.Payables, OrganisationMatcherContexts.Receivables };
			AssertContainsExactElementsInExactOrder(expectedContexts, contexts);

			contextsPairList.AddPair("UnknownCode", "Unknown Description");
			AssertExceptionThrown<NotSupportedException>("Unknown organization matcher context code. Invalid code: UnknownCode", () => OrganisationMatchingByVATRegistrationNumberContexts.GetContexts(contextsPairList));
		}
	}
}
