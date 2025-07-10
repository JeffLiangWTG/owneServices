using System;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(OrderLocationSearchFieldToXmlCodeMappings))]
	sealed class OrderLocationSearchFieldToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(OrdersConstants.PortFilterTypes) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return false; }
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[]
					{
						OrdersConstants.PortFilterTypes.AvailableAtDeliveredToDesc,
						OrdersConstants.PortFilterTypes.LoadDischargeDesc,
						OrdersConstants.PortFilterTypes.None
					};
		}
	}
}
