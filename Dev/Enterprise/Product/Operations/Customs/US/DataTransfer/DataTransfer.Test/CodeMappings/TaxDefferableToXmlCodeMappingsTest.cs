using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(TaxDefferableToXmlCodeMappings))]
	sealed class TaxDefferableToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(TaxDeferIndicatorList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax,
			TaxDeferIndicatorList.Codes.DeferredTax,
			TaxDeferIndicatorList.Codes.DeferredTaxWithEFT,
			TaxDeferIndicatorList.Codes.BulkLiquorDeferred
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
