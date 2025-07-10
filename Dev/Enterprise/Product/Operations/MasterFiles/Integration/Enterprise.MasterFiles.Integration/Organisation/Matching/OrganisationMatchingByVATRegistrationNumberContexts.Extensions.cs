using System;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public partial class OrganisationMatchingByVATRegistrationNumberContexts
	{
		public static OrganisationMatcherContexts GetContextByCode(string code)
		{
			switch (code)
			{
				case Codes.Payables:
					return OrganisationMatcherContexts.Payables;
				case Codes.Receivables:
					return OrganisationMatcherContexts.Receivables;
				default:
					throw new NotSupportedException($"Unknown organization matcher context code. Invalid code: {code}");
			}
		}

		public static OrganisationMatcherContexts[] GetContexts(CodeDescriptionPairList pairs)
			=> pairs.GetAllCodes().Select(code => GetContextByCode(code)).ToArray();
	}
}
