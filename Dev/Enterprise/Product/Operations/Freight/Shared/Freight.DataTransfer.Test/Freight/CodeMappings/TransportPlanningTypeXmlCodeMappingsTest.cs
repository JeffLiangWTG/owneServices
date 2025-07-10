using System;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(TransportPlanningTypeXmlCodeMappings))]
	sealed class TransportPlanningTypeXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(Core.Constants.TransportPlanningType) }; }
		}
	}
}
