using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeApportionmentMethodOverrideLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConsolTypeList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestConsolTypeList(
				() => Parent.Lookups.ModuleList,
				(module) =>
				{
					Parent.AAM_Module = module;
					return Parent.Lookups.TransportModeList;
				},
				(transport, module) =>
				{
					Parent.AAM_Module = module;
					Parent.AAM_TransportMode = transport;
					return Parent.Lookups.ConsolTypeList;
				}
			);
		}

		public void TestContainerModeList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestContainerModeList(
				() => Parent.Lookups.ModuleList,
				(module) =>
				{
					Parent.AAM_Module = module;
					return Parent.Lookups.TransportModeList;
				},
				(transport, module) =>
				{
					Parent.AAM_Module = module;
					Parent.AAM_TransportMode = transport;
					return Parent.Lookups.ConsolTypeList;
				},
				(consolType, transport, module) =>
				{
					Parent.AAM_Module = module;
					Parent.AAM_TransportMode = transport;
					Parent.AAM_ConsolType = consolType;
					return Parent.Lookups.ContainerModeList;
				}
			);
		}

		public void TestModuleList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestModuleList(
				() => Parent.Lookups.ModuleList
			);
		}

		public void TestTransportModeList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestTransportModeList(
				() => Parent.Lookups.ModuleList,
				(module) =>
				{
					Parent.AAM_Module = module;
					return Parent.Lookups.TransportModeList;
				}
			);
		}

		public void TestDirectionList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestDirectionList(
				() => Parent.Lookups.ModuleList,
				(module) =>
				{
					Parent.AAM_Module = module;
					return Parent.Lookups.DirectionList;
				}
			);
		}

		public void TestApportionmentMethodList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestApportionmentMethodList(
				() => Parent.Lookups.ModuleList,
				(module) =>
				{
					Parent.AAM_Module = module;
					return Parent.Lookups.ApportionmentList;
				}
			);
		}

		AccChargeApportionmentMethodOverride Parent => parent ?? (parent = Factory.NewWithValidTestData<AccChargeApportionmentMethodOverride>());

		AccChargeApportionmentMethodOverride parent;
	}
}
