using System;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(USInvoiceLineTaxCodeMappings))]
	sealed class USInvoiceLineTaxCodeTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(Business.Testing.ExciseTaxListForTesting) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			Core.Constants.USCustoms.FeeCodes.DistilledSpirits,
			Core.Constants.USCustoms.FeeCodes.Wines,
			Core.Constants.USCustoms.FeeCodes.Tobacco,
			Core.Constants.USCustoms.FeeCodes.OtherExcise
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
