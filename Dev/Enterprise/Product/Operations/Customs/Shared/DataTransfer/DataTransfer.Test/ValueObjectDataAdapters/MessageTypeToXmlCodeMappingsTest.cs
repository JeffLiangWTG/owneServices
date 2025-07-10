using System;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(MessageTypeToXmlCodeMappings))]
	sealed class MessageTypeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;

		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(Common.Shared.SharedJobMessageTypeList.Codes) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			JobMessageTypeList.Codes.Drawback,
			JobMessageTypeList.Codes.Refund,
			JobMessageTypeList.Codes.WarehousedByExternalAgent
		};
	}
}
