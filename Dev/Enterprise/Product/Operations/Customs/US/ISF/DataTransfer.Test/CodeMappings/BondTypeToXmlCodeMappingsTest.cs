using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.DataTransfer.Testing
{
	[TestedType(typeof(BondTypeToXmlCodeMappings))]
	sealed class BondTypeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(ImporterBondTypeList.Codes) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => Array.Empty<string>();

		protected override bool EnterpriseAndExternalCodeShouldBeSame => false;
	}
}
