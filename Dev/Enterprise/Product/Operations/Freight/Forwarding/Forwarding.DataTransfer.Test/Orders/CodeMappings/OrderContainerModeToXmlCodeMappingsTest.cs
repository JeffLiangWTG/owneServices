using System;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(OrderContainerModeToXmlCodeMappings))]
	public class OrderContainerModeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(Core.Constants.ContainerModes) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[]
			{
				Core.Constants.ContainerModes.All,
				Core.Constants.ContainerModes.BuyersConsol,
				Core.Constants.ContainerModes.ShippersConsol,
				Core.Constants.ContainerModes.Groupage,
				Core.Constants.ContainerModes.FreightAllKind,
				Core.Constants.ContainerModes.FCLMixedShipper,
				Core.Constants.ContainerModes.Empty,
				Core.Constants.ContainerModes.Combination,
				Core.Constants.ContainerModes.Containerised,
				Core.Constants.ContainerModes.NonContainerised,
			};
		}
	}
}
