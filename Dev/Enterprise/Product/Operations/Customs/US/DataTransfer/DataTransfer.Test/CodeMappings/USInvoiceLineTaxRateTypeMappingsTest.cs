using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(USInvoiceLineTaxRateTypeMappings))]
	sealed class USInvoiceLineTaxRateTypeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(RateTypeList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			RateTypeList.Codes.Primary,
			RateTypeList.Codes.Secondary
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
