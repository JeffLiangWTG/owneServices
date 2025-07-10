using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(InBondTypeToXmlCodeMappings))]
	sealed class InBondTypeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(InbondTypeList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			InbondTypeList.Codes.IEForeignTradeZoneWithdrawal,
			InbondTypeList.Codes.IEWarehouseWithdrawal,
			InbondTypeList.Codes.MerchandiseNOTShippedInbond,
			InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal,
			InbondTypeList.Codes.TAndEWarehouseWithdrawal
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
