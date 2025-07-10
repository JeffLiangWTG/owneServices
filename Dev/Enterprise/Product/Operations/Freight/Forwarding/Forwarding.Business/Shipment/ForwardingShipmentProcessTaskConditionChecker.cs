using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business;

public sealed class ForwardingShipmentProcessTaskConditionChecker
{
	BusinessObjectFactory Factory { get; }
	ForwardingShipment Parent { get; }

	public ForwardingShipmentProcessTaskConditionChecker(BusinessObjectFactory factory, ForwardingShipment parent)
	{
		Factory = Argument.NotNull(factory, nameof(factory));
		Parent = Argument.NotNull(parent, nameof(parent));
	}

	public bool IsCondition1Met(ZString conditionCode)
	{
		switch (conditionCode)
		{
			case JobShipmentWorkflowCondition1CodeList.Codes.Import:
				return Parent.IsImport();
			case JobShipmentWorkflowCondition1CodeList.Codes.Export:
				return Parent.IsExport();
			case JobShipmentWorkflowCondition1CodeList.Codes.Domestic:
				return Parent.IsDomestic();

			case JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached:
				return Parent.Declarations.Length > 0;

			case JobShipmentWorkflowCondition1CodeList.Codes.AirCargoAttached:
				return Factory.Load<Enterprise.Integration.Customs.Shared.ICusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, Parent.PK))
					.Any(hawb => hawb is Enterprise.Integration.Customs.AU.ICusHAWB);

			case JobShipmentWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad:
				CommonConsol firstConsol = GetFirstConsol();
				return firstConsol != null && !firstConsol.IsDeleted && Parent.JS_RL_NKOrigin != firstConsol.JK_RL_NKLoadPort;

			case JobShipmentWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge:
				CommonConsol finalConsol = GetFinalConsol();
				return finalConsol != null && !finalConsol.IsDeleted && Parent.JS_RL_NKDestination != finalConsol.JK_RL_NKDischargePort;

			case JobShipmentWorkflowCondition1CodeList.Codes.ConsolDischargeLeg:
				return Parent.ArrivalConsol != null && Parent.ArrivalConsol.Transports.ArrivalTransport != null;

			case JobShipmentWorkflowCondition1CodeList.Codes.ConsolPreCarriageLeg:
				return Parent.Consols.Cast<ForwardingConsol>().SelectMany(consol => consol.Transports.Cast<Transport>()).Any(transport => transport.JW_TransportType == Constants.TransportPlanningType.PreCarriage);

			case JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesPickupCartage:
				JobDocsAndCartage docsAndCartage = JobDocsAndCartage.Load(Parent);
				return docsAndCartage != null && docsAndCartage.PickupCartageCoPK == GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			case JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesDeliveryCartage:
				docsAndCartage = JobDocsAndCartage.Load(Parent);
				return docsAndCartage != null && docsAndCartage.DeliveryCartageCoPK == GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			default:
				return false;
		}
	}

	public bool IsCondition2Met(ZString conditionCode, ZString value) => (string)conditionCode switch
	{
		JobShipmentWorkflowCondition2CodeList.Codes.Import => Parent.IsImport(),
		JobShipmentWorkflowCondition2CodeList.Codes.Export => Parent.IsExport(),
		JobShipmentWorkflowCondition2CodeList.Codes.Domestic => Parent.IsDomestic(),
		JobShipmentWorkflowCondition2CodeList.Codes.ColoadMaster => Parent.IsCoLoadMaster || Parent.IsBlindCoLoadMaster,
		JobShipmentWorkflowCondition2CodeList.Codes.ColoadSub => Parent.JS_JS_ColoadMasterShipment.IsValid && (Parent.CoLoadMasterShipment.IsCoLoadMaster || Parent.CoLoadMasterShipment.IsBlindCoLoadMaster),
		JobShipmentWorkflowCondition2CodeList.Codes.AssemblyMaster => Parent.IsAssemblyMaster,
		JobShipmentWorkflowCondition2CodeList.Codes.AssemblySub => Parent.CoLoadShipments.Count == 0 && Parent.HasMaster(Constants.ShipmentTypes.AssemblyMaster),
		JobShipmentWorkflowCondition2CodeList.Codes.LCL => Parent.JS_PackingMode == Core.Constants.ContainerModes.LCL,
		JobShipmentWorkflowCondition2CodeList.Codes.FCL => Parent.JS_PackingMode == Core.Constants.ContainerModes.FCL,
		JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolMaster => Parent.JS_PackingMode == Core.Constants.ContainerModes.BuyersConsol && Parent.IsBuyersConsolLead,
		JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolSub => Parent.JS_PackingMode == Core.Constants.ContainerModes.BuyersConsol && !Parent.IsBuyersConsolLead,
		JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType => Parent.JS_ReleaseType == value,
		_ => conditionCode == Parent.JS_PackingMode
	};

	CommonConsol GetFirstConsol()
	{
		CommonConsol[] consolsInTransportOrder = GetConsolsInTransportOrder();
		return consolsInTransportOrder.Length == 0 ? null : consolsInTransportOrder[0];
	}

	CommonConsol GetFinalConsol()
	{
		CommonConsol[] consolsInTransportOrder = GetConsolsInTransportOrder();
		return consolsInTransportOrder.Length == 0 ? null : consolsInTransportOrder[consolsInTransportOrder.Length - 1];
	}

	CommonConsol[] GetConsolsInTransportOrder()
	{
		CommonConsol[] result = (CommonConsol[])Parent.Consols.ToArray(typeof(CommonConsol));
		MovementLegComparer.SortMovementLegsByPorts(result);
		return result;
	}
}
