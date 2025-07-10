using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(BondTypeToXmlCodeMappings))]
	sealed class BondTypeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(BondTypeList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[] { BondTypeList.Codes.ContinuousBond, BondTypeList.Codes.NoBondRequired, BondTypeList.Codes.SingleTransactionBond };

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
