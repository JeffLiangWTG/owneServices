using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using FreightTransport = Enterprise.Freight.Business.Transport;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	class CresaBuilderForTransitDispatchConsignment : CresaBuilderForTransitWarehouse<WhsItemDispatchConsignment>
	{
		public CresaBuilderForTransitDispatchConsignment(WhsItemDispatchConsignment consignment) : base(consignment)
		{
		}

		protected override Cresa CreateCresaMessage() => new Cresa(nameof(DataContextType.TransitDispatch), consignment.WDC_JobID);

		protected override JobDocAddress ConsigneeDocAddress => consignment.ConsigneeDocAddress;

		protected override JobDocAddress ConsignorDocAddress => consignment.ConsignorDocAddress;

		protected override ZDateTime GetETA()
		{
			var warehousePort = consignment?.Warehouse?.RelatedCompanyBranch.GB_RL_NKHomePort;
			var routings = consignment?.PackageStates.Where(p => p.DispatchLoadList != null).SelectMany(p => p.DispatchLoadList.Transports).Where(routing => routing != null).ToList();

			var outboundRouting = routings.OfType<FreightTransport>().FirstOrDefault(t => t.JW_RL_NKLoadPort == warehousePort.GetValueOrDefault());
			return outboundRouting == null || outboundRouting.JW_ETA.IsEmpty ? ZDateTime.Now : outboundRouting.JW_ETA;
		}

		protected override IRefUNLOCO NextDischargePort => consignment.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, consignment.WDC_RL_NKDestination);

		protected override WhsItemPackageStateCollection PackageStates => consignment.PackageStates;

		protected override IEnumerable<WhsItemPackageState> OuterPackageStatesForBuildingPackingLines => consignment.OuterPackageStates
			.Where(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Arrived ||
				p.WPS_Status == TransitWarehouseStatuses.Codes.Putaway ||
				p.WPS_Status == TransitWarehouseStatuses.Codes.Committed ||
				p.WPS_Status == TransitWarehouseStatuses.Codes.Picked ||
				p.WPS_Status == TransitWarehouseStatuses.Codes.Staged ||
				p.WPS_Status == TransitWarehouseStatuses.Codes.FreightLoaded ||
				p.WPS_Status == TransitWarehouseStatuses.Codes.Departed ||
				p.WPS_Status == TransitWarehouseStatuses.Codes.Finalized);

		protected override Enterprise.Integration.Customs.IPortReferenceCollection PortReferences => consignment.PortReferences;

		protected override void PopulateConsignmentDetails(Cresa cresa)
		{
			cresa.ShipmentType = new CodeDescription(consignment.Lookups.ServiceLevels)
			{
				Code = consignment.WDC_RS_NKServiceLevel
			};
		}

		protected override void PopulateReferences(Cresa cresa)
		{
			cresa.EntryNumber = consignment.WDC_JobID;
			cresa.CommodityReference = consignment.WDC_JobID;
			cresa.CarrierBookingReference = consignment.WDC_JobID;
			cresa.ShipmentNumber = consignment.WDC_JobID;
		}

		protected override bool ConsignmentIsRCN => false;
		protected override string ExpectedPortReferenceType => TransitWarehousePortReferenceTypes.Codes.PortExport;
	}
}
