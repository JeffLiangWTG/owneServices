using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(EntryTypeModeToXmlCodeMappings))]
	sealed class EntryTypeModeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(EntryModeList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[] { EntryModeList.Codes.RLF, EntryModeList.Codes.Paired, };

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
