using System;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(OrderDateSearchFieldToXmlCodeMappings))]
	sealed class OrderDateSearchFieldToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(OrdersConstants.DateFilterTypes) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return false; }
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[]
					{
						OrdersConstants.DateFilterTypes.None,
						OrdersConstants.DateFilterTypes.FollowUpDate,
					};
		}
	}
}
