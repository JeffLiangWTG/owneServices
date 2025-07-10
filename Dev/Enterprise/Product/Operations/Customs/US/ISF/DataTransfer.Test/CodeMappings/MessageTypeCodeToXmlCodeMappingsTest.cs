using System;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.DataTransfer.Testing
{
	[TestedType(typeof(MessageTypeCodeToXmlCodeMappings))]
	sealed class MessageTypeCodeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(ISFMessageStatus.Codes) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => Array.Empty<string>();

		protected override bool EnterpriseAndExternalCodeShouldBeSame => false;
	}
}
