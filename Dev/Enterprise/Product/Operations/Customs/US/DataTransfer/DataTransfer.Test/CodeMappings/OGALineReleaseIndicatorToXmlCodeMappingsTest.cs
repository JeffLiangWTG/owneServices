using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(OGALineReleaseIndicatorToXmlCodeMappings))]
	sealed class OGALineReleaseIndicatorToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(YesNoDefaultList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			YesNoDefaultList.Codes.Default
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
