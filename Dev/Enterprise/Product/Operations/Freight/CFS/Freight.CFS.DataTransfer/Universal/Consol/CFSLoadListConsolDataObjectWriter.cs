using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.CFS.DataTransfer.Universal
{
	class CFSLoadListConsolDataObjectWriter : TopLevelDataObjectWriter<CFSLoadListConsol, Shipment>
	{
		public CFSLoadListConsolDataObjectWriter(IDataWritingManager manager, bool includeSubShipments, ContainerLinkManager<CFSLoadListConsol> linkManager = null)
			: base(manager)
		{
			this.includeSubShipments = includeSubShipments;
			this.linkManager = linkManager;
		}

		readonly bool includeSubShipments;
		ContainerLinkManager<CFSLoadListConsol> linkManager;

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.CFSLoadListConsol;
		}

		protected override void PopulateDataObject(CFSLoadListConsol consolBO, Shipment dataObject)
		{
			linkManager = linkManager ?? new ContainerLinkManager<CFSLoadListConsol>(consolBO);
			var listCache = BindToLists.GetCachedLists(consolBO.Factory);

			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_TransportMode, consolBO.JK_TransportMode_List);
			dataObject.ContainerMode = ListHelper.GetWithDescription<ContainerMode>(consolBO.JK_ConsolMode, consolBO.JK_ConsolMode_List);
			dataObject.PortOfLoading = ListHelper.GetWithName(consolBO.JK_RL_NKLoadPort, consolBO.RefUNLOCO_List);
			dataObject.PortOfDischarge = ListHelper.GetWithName(consolBO.JK_RL_NKDischargePort, consolBO.RefUNLOCO_List);
			dataObject.AgentsReference = consolBO.JK_AgentsReference;
			dataObject.BookingConfirmationReference = consolBO.JK_BookingReference;
			dataObject.WayBillNumber = consolBO.JK_MasterBillNum;
			dataObject.CFSReference = consolBO.JK_CustomsReference;

			PopulateOrganizations(consolBO, dataObject);

			dataObject.SetTransportLegCollection(() => ProcessCollection(consolBO.Transports, new TransportLegDataObjectWriter(writeManager), CollectionContent.Complete, true));
			dataObject.SetContainerCollection(() => ProcessCollection(consolBO.Containers, new ContainerWithPackLinesDataObjectWriter<CFSLoadListConsol>(linkManager, listCache, writeManager), CollectionContent.Complete, true));

			if (includeSubShipments)
			{
				dataObject.SetSubShipmentCollection(() => PopulateSubShipmentCollection(consolBO));
			}
		}

		DataObjectList<Shipment> PopulateSubShipmentCollection(CFSLoadListConsol consolBizO)
		{
			var shipments = consolBizO.TopLevelShipments;
			var data = ProcessCollection(shipments, writeManager.Schema == UniversalXmlSchema.Version_2012_11_DO_NOT_USE
				? new CFSShipmentDataObjectWriter(writeManager, true, true, linkManager, consolBizO)
				: new CFSShipmentDataObjectWriter(writeManager, true, false, linkManager));
			return data != null ? new DataObjectList<Shipment>(data) : null;
		}

		void PopulateOrganizations(CFSLoadListConsol consolBO, Shipment dataObject)
		{
			dataObject.AddOrgAddress(writeManager, consolBO.Forwarder, AddressTypes.Forwarder);
			dataObject.AddOrgAddress(writeManager, consolBO.ShippingLineAddress, DocAddressType.ShippingLineAddress);
			dataObject.AddOrgAddress(writeManager, consolBO.EmptyContainerYardAddress, AddressTypes.ContainerYardAddress);
			dataObject.AddOrgAddress(writeManager, consolBO.CTOAddress, AddressTypes.CTOAddress);
			dataObject.AddOrgAddress(writeManager, consolBO.CartageCoAddress, DocAddressType.LocalCartageAddress1);
			dataObject.AddOrgAddress(writeManager, consolBO.DepotAddress, AddressTypes.DepotAddress);
		}
	}
}
