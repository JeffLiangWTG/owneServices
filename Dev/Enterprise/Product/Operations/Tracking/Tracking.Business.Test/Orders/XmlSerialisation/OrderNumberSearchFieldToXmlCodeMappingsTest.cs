using System;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(OrderNumberSearchFieldToXmlCodeMappings))]
	sealed class OrderNumberSearchFieldToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(OrdersConstants.NumberFilterTypes) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return false; }
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[]
					{
							OrdersConstants.NumberFilterTypes.None
					};
		}
	}
}
