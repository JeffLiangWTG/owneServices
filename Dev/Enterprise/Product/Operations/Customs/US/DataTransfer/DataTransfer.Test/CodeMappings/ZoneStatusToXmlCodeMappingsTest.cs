using System;
using Enterprise.Customs.Common.US;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(ZoneStatusToXmlCodeMappings))]
	sealed class ZoneStatusToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(ZoneStatusList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			ZoneStatusList.Codes.Domestic,
			ZoneStatusList.Codes.NonPrivilegedForeign,
			ZoneStatusList.Codes.PrivilegedForeign,
			ZoneStatusList.Codes.ZoneRestricted
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
