using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(AESOriginIndicatorToXmlCodeMappings))]
	sealed class AESOriginIndicatorToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(AESOriginIndicatorList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[] { AESOriginIndicatorList.Codes.Domestic, AESOriginIndicatorList.Codes.Foreign };

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
