using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using FreightTransport = Enterprise.Freight.Business.Transport;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	class CresaBuilderForTransitReceiveConsignment : CresaBuilderForTransitWarehouse<WhsItemReceiveConsignment>
	{
		public CresaBuilderForTransitReceiveConsignment(WhsItemReceiveConsignment consignment) : base(consignment)
		{
		}

		protected override Cresa CreateCresaMessage() => new Cresa(nameof(DataContextType.TransitReceive), consignment.WRC_JobID);

		protected override ZDateTime GetETA()
		{
			var warehousePort = consignment?.Warehouse?.RelatedCompanyBranch.GB_RL_NKHomePort;

			var outboundRouting = consignment.Transports.OfType<FreightTransport>().FirstOrDefault(t => t.JW_RL_NKLoadPort == warehousePort.GetValueOrDefault());
			return outboundRouting == null || outboundRouting.JW_ETA.IsEmpty ? ZDateTime.Now : outboundRouting.JW_ETA;
		}

		protected override JobDocAddress ConsigneeDocAddress => consignment.ConsigneeDocAddress;

		protected override JobDocAddress ConsignorDocAddress => consignment.ConsignorDocAddress;

		protected override IRefUNLOCO NextDischargePort => consignment.NextDischargePort;

		protected override IEnumerable<WhsItemPackageState> OuterPackageStatesForBuildingPackingLines => consignment.OuterPackages
			.Where(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Arrived ||
				p.WPS_Status == TransitWarehouseStatuses.Codes.Putaway ||
				p.WPS_Status == TransitWarehouseStatuses.Codes.Committed ||
				p.WPS_Status == TransitWarehouseStatuses.Codes.Picked ||
				p.WPS_Status == TransitWarehouseStatuses.Codes.Staged);

		protected override WhsItemPackageStateCollection PackageStates => consignment.PackageStates;

		protected override Enterprise.Integration.Customs.IPortReferenceCollection PortReferences => consignment.PortReferences;

		protected override void PopulateConsignmentDetails(Cresa cresa)
		{
			cresa.ShipmentType = new CodeDescription(consignment.Lookups.ServiceLevels)
			{
				Code = consignment.WRC_RS_NKServiceLevel
			};
		}

		protected override void PopulateReferences(Cresa cresa)
		{
			cresa.EntryNumber = consignment.WRC_JobID;
			cresa.CommodityReference = consignment.WRC_JobID;
			cresa.CarrierBookingReference = consignment.WRC_JobID;
			cresa.ShipmentNumber = consignment.WRC_JobID;
		}

		protected override bool ConsignmentIsRCN => true;
		protected override string ExpectedPortReferenceType => TransitWarehousePortReferenceTypes.Codes.PortAuthority;
	}
}
