using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(ConsolidatedInformalToXmlCodeMappings))]
	sealed class ConsolidatedInformalToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(ConsolidatedInformalList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[] { ConsolidatedInformalList.Codes.Consolidated, ConsolidatedInformalList.Codes.Personal, ConsolidatedInformalList.Codes.Samples };

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
