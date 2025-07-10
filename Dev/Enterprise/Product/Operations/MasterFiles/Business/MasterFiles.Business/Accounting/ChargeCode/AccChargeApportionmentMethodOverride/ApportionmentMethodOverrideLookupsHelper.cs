using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	public static class ApportionmentMethodOverrideLookupsHelper
	{
		public static CodeDescriptionPairList ConsolTypeList(ZString transportMode, ZString module)
		{
			var result = new CodeDescriptionPairList();
			switch (module)
			{
				case ApportionmentMethodModules.TransitWarehouse:
				case ApportionmentMethodModules.TransportBooking:
					break;

				default:
					result = ObjectFactory.Get<IFreightCodePairListProvider>().GetAgentTypeList(transportMode == Core.Constants.TransportModes.Air);
					break;
			}
			result.AddPair(ApportionmentMethod.AllCode, ApportionmentMethod.AllConsolTypeDescription);

			return result;
		}

		public static CodeDescriptionPairList ContainerModeList(ZString agentType, ZString transportMode, ZString module)
		{
			var result = new CodeDescriptionPairList();
			switch (module)
			{
				case ApportionmentMethodModules.TransitWarehouse:
					break;

				default:
					var type = agentType != ApportionmentMethod.AllCode ? agentType : ZString.Empty;
					var mode = transportMode != ApportionmentMethod.AllCode ? transportMode : ZString.Empty;
					result.AddRange(ObjectFactory.Get<IFreightCodePairListProvider>().GetConsolModeList(type, mode));
					break;
			}
			result.AddPair(ApportionmentMethod.AllCode, ApportionmentMethod.AllContainerModeDescription);

			return result;
		}

		public static CodeDescriptionPairList TransportModeList(ZString module)
		{
			var result = new CodeDescriptionPairList();
			switch (module)
			{
				case ApportionmentMethodModules.TransitWarehouse:
					break;

				default:
					result.AddRange(ObjectFactory.Get<IFreightCodePairListProvider>().GetConsolTransportModeList());
					break;
			}
			result.AddPair(ApportionmentMethod.AllCode, ApportionmentMethod.AllTransportModeDescription);

			return result;
		}

		public static CodeDescriptionPairList DirectionList(string module)
		{
			switch (module)
			{
				case ApportionmentMethodModules.TransitWarehouse:
				case ApportionmentMethodModules.TransportBooking:
					var result = new CodeDescriptionPairList();
					result.AddPair(Constants.FreightShipmentDirection.Code.All, Constants.FreightShipmentDirection.Description.All);
					return result;
				default:
					return MasterFiles.Business.JobConfigurationSelectorLookups.GetBaseDirectionList();
			}
		}

		public static CodeDescriptionPairList ModuleList
		{
			get
			{
				if (moduleList == null)
				{
					moduleList = new CodeDescriptionPairList();
					moduleList.AddPair(ApportionmentMethodModules.Forwarding, ApportionmentMethod.ModuleDescription.ForwardingDescription);
					moduleList.AddPair(ApportionmentMethodModules.TransportBooking, ApportionmentMethod.ModuleDescription.TransportDescription);
					moduleList.AddPair(ApportionmentMethodModules.TransitWarehouse, ApportionmentMethod.ModuleDescription.TransitWarehouseDescription);
					moduleList.AddPair(ApportionmentMethod.AllCode, ApportionmentMethod.ModuleDescription.AllModulesDescription);
				}
				return moduleList;
			}
		}

		[ThreadStatic]
		public static CodeDescriptionPairList moduleList;

		public static CodeDescriptionPairList ApportionmentList(ZString module)
		{
			var result = new CodeDescriptionPairList();
			var apportionmentList = new CodeDescriptionPairList(OLookUpEditType.AllocationMethod);

			switch (module)
			{
				case ApportionmentMethodModules.TransitWarehouse:
					var apportionmentListInTransitWarehouse = new[] {
						AllocationMethod.Manual,
						AllocationMethod.Shipment,
						AllocationMethod.GrossWeight,
						AllocationMethod.GrossVolume,
						AllocationMethod.OuterPackTotal,
					};
					foreach (var code in apportionmentListInTransitWarehouse)
					{
						result.AddPair(code, apportionmentList.GetDescriptionFromCode(code));
					}
					break;

				default:
					result = apportionmentList;
					break;
			}

			return result;
		}
	}
}
