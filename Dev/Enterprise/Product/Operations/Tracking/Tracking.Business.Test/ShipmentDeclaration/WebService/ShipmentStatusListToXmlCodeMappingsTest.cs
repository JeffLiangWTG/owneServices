using System;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(ShipmentStatusListToXmlCodeMappings))]
	sealed class ShipmentStatusListToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(Shipments.ShipmentStatus.Codes) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return false; }
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return Array.Empty<string>();
		}
	}
}
