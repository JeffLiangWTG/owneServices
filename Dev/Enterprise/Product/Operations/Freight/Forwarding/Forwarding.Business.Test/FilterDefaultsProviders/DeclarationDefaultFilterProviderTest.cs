using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(DeclarationDefaultFilterProvider))]
	sealed class DeclarationDefaultFilterProviderTest : DefaultFilterProviderTest<DeclarationDefaultFilterProvider>
	{
		#region TestTransportMode

		public void TestTransportMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, DeclarationDefaultFilterProvider.Constants.TransportMode);

			Provider.TransportMode = Core.Constants.TransportModes.Sea;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, DeclarationDefaultFilterProvider.Constants.TransportMode, "Property", (ZString)Core.Constants.TransportModes.Sea);
		}

		#endregion

		#region TestContainerMode

		public void TestContainerMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, DeclarationDefaultFilterProvider.Constants.ContainerMode);

			Provider.ContainerMode = Core.Constants.ContainerModes.FCL;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, DeclarationDefaultFilterProvider.Constants.ContainerMode, "Property", (ZString)Core.Constants.ContainerModes.FCL);
		}

		#endregion

		#region TestLoadPort

		public void TestLoadPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, DeclarationDefaultFilterProvider.Constants.LoadDischarge);

			Provider.LoadPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, DeclarationDefaultFilterProvider.Constants.LoadDischarge, "Property1", HomePort);
		}

		#endregion

		#region TestDischargePort

		public void TestDischargePort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, DeclarationDefaultFilterProvider.Constants.LoadDischarge);

			Provider.DischargePort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, DeclarationDefaultFilterProvider.Constants.LoadDischarge, "Property2", HomePort);
		}

		#endregion

		#region TestOriginPort

		public void TestOriginPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, DeclarationDefaultFilterProvider.Constants.OriginDestination);

			Provider.OriginPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, DeclarationDefaultFilterProvider.Constants.OriginDestination, "Property1", HomePort);
		}

		#endregion

		#region TestDestinationPort

		public void TestDestinationPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, DeclarationDefaultFilterProvider.Constants.OriginDestination);

			Provider.DestinationPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, DeclarationDefaultFilterProvider.Constants.OriginDestination, "Property2", HomePort);
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.JobDeclaration; }
		}

		#endregion
	}
}
