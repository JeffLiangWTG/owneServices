using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(FDAProducerFirmTypeToXmlCodeMappings))]
	sealed class FDAProducerFirmTypeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(ProducerFirmTypeList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			ProducerFirmTypeList.Codes.C,
			ProducerFirmTypeList.Codes.G,
			ProducerFirmTypeList.Codes.M
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
