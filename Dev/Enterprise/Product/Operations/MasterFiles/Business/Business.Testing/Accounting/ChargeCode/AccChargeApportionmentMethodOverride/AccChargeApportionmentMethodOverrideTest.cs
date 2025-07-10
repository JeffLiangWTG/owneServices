using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeApportionmentMethodOverride))]
	sealed class AccChargeApportionmentMethodOverrideTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = (AccChargeApportionmentMethodOverride)base.GetNewBusinessObject();
			result.AAM_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery()).PK;
			result.AAM_ApportionmentMethod = AllocationMethod.Manual;
			return result;
		}

		public void TestIsDuplicateOf()
		{
			TestIsDuplicateOf((b, v) => b.AAM_AC = v, ZGuid.NewZGuid());
			TestIsDuplicateOf((b, v) => b.AAM_Module = v, "TES");
			TestIsDuplicateOf((b, v) => b.AAM_ConsolType = v, "TES");
			TestIsDuplicateOf((b, v) => b.AAM_Direction = v, "TES");
			TestIsDuplicateOf((b, v) => b.AAM_TransportMode = v, "TES");
			TestIsDuplicateOf((b, v) => b.AAM_ContainerMode = v, "TES");
		}

		void TestIsDuplicateOf<PropType>(Action<AccChargeApportionmentMethodOverride, PropType> setter, PropType randomValue)
		{
			var apportionmentMethodOverride1 = GetNewBusinessObject() as AccChargeApportionmentMethodOverride;
			var apportionmentMethodOverride2 = GetNewBusinessObject() as AccChargeApportionmentMethodOverride;

			AssertEquals(true, apportionmentMethodOverride1.IsDuplicateOf(apportionmentMethodOverride2));
			AssertEquals(true, apportionmentMethodOverride2.IsDuplicateOf(apportionmentMethodOverride1));

			setter(apportionmentMethodOverride1, randomValue);

			AssertEquals(false, apportionmentMethodOverride1.IsDuplicateOf(apportionmentMethodOverride2));
			AssertEquals(false, apportionmentMethodOverride2.IsDuplicateOf(apportionmentMethodOverride1));

			setter(apportionmentMethodOverride2, randomValue);

			AssertEquals(true, apportionmentMethodOverride1.IsDuplicateOf(apportionmentMethodOverride2));
			AssertEquals(true, apportionmentMethodOverride2.IsDuplicateOf(apportionmentMethodOverride1));
		}

		public void TestIConsolCostApportionmentMethod()
		{
			var method = GetNewBusinessObject() as AccChargeApportionmentMethodOverride;
			method.AAM_TransportMode = Constants.TransportModes.Air;
			method.AAM_ConsolType = Constants.AgentType.Direct;
			method.AAM_ContainerMode = Constants.ContainerModes.Loose;
			method.AAM_Module = ApportionmentMethodModules.Forwarding;
			method.AAM_ApportionmentMethod = AllocationMethod.CapacityPerContainer;
			method.AAM_Direction = Constants.CartageDirection.Import;

			IApportionmentMethodOverride consolCostApportionmentMethod = method;
			AssertEquals("PreCondition", ApportionmentMethodModules.Forwarding, consolCostApportionmentMethod.Module);
			AssertEquals("PreCondition", Constants.TransportModes.Air, consolCostApportionmentMethod.TransportMode);
			AssertEquals("PreCondition", Constants.AgentType.Direct, consolCostApportionmentMethod.ConsolType);
			AssertEquals("PreCondition", Constants.ContainerModes.Loose, consolCostApportionmentMethod.ContainerMode);
			AssertEquals("PreCondition", Constants.CartageDirection.Import, consolCostApportionmentMethod.Direction);
			AssertEquals("PreCondition", AllocationMethod.CapacityPerContainer, consolCostApportionmentMethod.ApportionmentMethod);

			method.AAM_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(nameof(consolCostApportionmentMethod.TransportMode), Constants.TransportModes.Sea, consolCostApportionmentMethod.TransportMode);

			method.AAM_ConsolType = Constants.AgentType.Agent;
			AssertEquals(nameof(consolCostApportionmentMethod.ConsolType), Constants.AgentType.Agent, consolCostApportionmentMethod.ConsolType);

			method.AAM_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals(nameof(consolCostApportionmentMethod.ContainerMode), Constants.ContainerModes.FCL, consolCostApportionmentMethod.ContainerMode);

			method.AAM_ApportionmentMethod = AllocationMethod.GrossVolume;
			AssertEquals(nameof(consolCostApportionmentMethod.ApportionmentMethod), AllocationMethod.GrossVolume, consolCostApportionmentMethod.ApportionmentMethod);

			method.AAM_Direction = Constants.CartageDirection.Export;
			AssertEquals(nameof(consolCostApportionmentMethod.Direction), Constants.CartageDirection.Export, consolCostApportionmentMethod.Direction);

			method.AAM_Module = ApportionmentMethod.AllCode;
			AssertEquals(nameof(consolCostApportionmentMethod.Module), ApportionmentMethod.AllCode, consolCostApportionmentMethod.Module);
		}

		public void TestModuleSetToAllOnTRWSelection()
		{
			var apportionmentMethodOverride = GetNewBusinessObject() as AccChargeApportionmentMethodOverride;
			apportionmentMethodOverride.AAM_ConsolType = "OTH";
			AssertEquals("Precondition", "OTH", apportionmentMethodOverride.AAM_ConsolType);
			apportionmentMethodOverride.AAM_Module = "TRW";
			AssertEquals("Consol Type should be set to all on module change to TRW", ApportionmentMethod.AllCode, apportionmentMethodOverride.AAM_ConsolType);
			AssertEquals("Transport Mode should be set to all on module change to TRW", ApportionmentMethod.AllCode, apportionmentMethodOverride.AAM_TransportMode);
			AssertEquals("Container Mode should be set to all on module change to TRW", ApportionmentMethod.AllCode, apportionmentMethodOverride.AAM_ContainerMode);
			AssertEquals("Direction should be set to all on module change to TRW", Enterprise.Core.Constants.FreightShipmentDirection.Code.All, apportionmentMethodOverride.AAM_Direction);
			AssertContainsExactElementsInAnyOrder("ApportionmentList should be set to specific methods on module change to TRW",
				new[] {
					AllocationMethod.Manual,
					AllocationMethod.Shipment,
					AllocationMethod.GrossWeight,
					AllocationMethod.GrossVolume,
					AllocationMethod.OuterPackTotal,
				},
				apportionmentMethodOverride.Lookups.ApportionmentList.GetAllCodes());
		}
	}
}
