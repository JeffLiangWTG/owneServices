using System;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(CostDistributionMechanismListToXmlCodeMappings))]
	sealed class CostDistributionMechanismListToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;

		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(CostDistributionMechanismList.Codes) };
	}
}
