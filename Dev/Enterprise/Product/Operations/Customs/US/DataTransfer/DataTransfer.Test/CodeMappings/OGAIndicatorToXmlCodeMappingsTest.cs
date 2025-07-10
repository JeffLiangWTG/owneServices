using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(OGAIndicatorToXmlCodeMappings))]
	sealed class OGAIndicatorToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(OGAIndicatorList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			OGAIndicatorList.Codes.Declared,
			OGAIndicatorList.Codes.Disclaimed
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => false;
	}
}
