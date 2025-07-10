using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveASNDataObjectWriter : TopLevelDataObjectWriter<WhsItemReceiveASN, UniversalShipment>
	{
		public WhsTransitReceiveASNDataObjectWriter(IDataWritingManager manager, bool shouldPopulateConsignments = true)
			: base(manager)
		{
			this.shouldPopulateConsignments = shouldPopulateConsignments;
		}
		readonly bool shouldPopulateConsignments;

		protected override void PopulateDataObject(WhsItemReceiveASN shippingNotice, UniversalShipment dataObject)
		{
			if (!shippingNotice.WRP_ETA.IsEmpty)
			{
				if (dataObject.SetDateCollection(() => new List<Date>()))
				{
					dataObject.DateCollection.Add(DateType.Arrival, true, shippingNotice.WRP_ETA);
				}
			}

			dataObject.PopulateTransportMode(shippingNotice.WRP_TransportMode);
			PopulateConsignments(shippingNotice, dataObject);
			PopulateAddresses(shippingNotice, dataObject);
			PopulateNotes(shippingNotice, dataObject);
			PopulateTransportRoutings(shippingNotice, dataObject);
			PopulateReferences(shippingNotice, dataObject);
			PopulateContainerCollection(shippingNotice, dataObject);
			PopulateForAirCargo(shippingNotice, dataObject);
		}

		void PopulateForAirCargo(WhsItemReceiveASN asn, UniversalShipment dataObject)
		{
			dataObject.WayBillNumber = asn.MasterBillNumber;
			dataObject.WayBillType = new WayBillType()
			{
				Code = WayBillTypeList.Codes.Master,
				Description = WayBillTypeList.Descriptions.Master,
			};
			dataObject.VoyageFlightNo = asn.VoyageNumber;

			var writer = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ArrivalCFSAddress));
			var warehouseOrganisationAddress = writer.GetDataObject(asn.IntendedWarehouse.WarehouseAddress);

			if (dataObject.OrganizationAddressCollection != null || dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { }))
			{
				dataObject.OrganizationAddressCollection.Add(warehouseOrganisationAddress);
			}
		}

		static void PopulateContainerCollection(WhsItemReceiveASN shippingNotice, UniversalShipment dataObject)
		{
			var containerisedRTUs = shippingNotice.ReceiveTransportationUnits.Where(r => r.HasContainerEquipmentDetails);
			if (containerisedRTUs.Any())
			{
				var containers = new DataObjectList<Container>();
				foreach (var relatedRTU in containerisedRTUs)
				{
					var container = new Container()
					{
						ContainerNumber = relatedRTU.WRH_VehicleReference,
						IsSealOk = relatedRTU.WRH_IsVehicleSecure,
						LCLUnpack = relatedRTU.WRH_UnloadCompleteTime.ToZDateTime()
					};
					containers.Add(container);
				}

				dataObject.SetContainerCollection(() => containers);
			}
		}

		void PopulateAddresses(WhsItemReceiveASN shippingNotice, UniversalShipment dataObject)
		{
			var writer = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.LocalCartageCFS));
			var warehouseOrganisationAddress = writer.GetDataObject(shippingNotice.IntendedWarehouse.WarehouseAddress);

			dataObject.SetOrganizationAddressCollection(() => ProcessCollection(shippingNotice.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));
			if (dataObject.OrganizationAddressCollection != null || dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { }))
			{
				dataObject.OrganizationAddressCollection.Add(warehouseOrganisationAddress);
			}
		}

		void PopulateNotes(WhsItemReceiveASN shippingNotice, UniversalShipment dataObject)
		{
			dataObject.SetNoteCollection(() =>
			{
				var notes = shippingNotice.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		void PopulateTransportRoutings(WhsItemReceiveASN shippingNotice, UniversalShipment dataObject)
		{
			var writer = ObjectFactory.Get<ITransportLegDataObjectWriter>("ITransportLegDataObjectWriter", writeManager) as DataObjectWriter<BusinessObject, TransportLeg>;
			var collection = shippingNotice.Factory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, shippingNotice.PK));
			// TODO: Every instance of a transport should have its ParentType set per comment in Transport.get_Parent.
			// Some other operational actions may refer to its property and ParentType check will throw exception with reason "ParentType not set". Test included.
			dataObject.SetTransportLegCollection(() => ProcessCollection(collection, writer, CollectionContent.Complete, true));
		}

		void PopulateReferences(WhsItemReceiveASN shippingNotice, UniversalShipment dataObject)
		{
			dataObject.SetAdditionalReferenceCollection(() => ProcessCollection(shippingNotice.AdditionalReferenceNumbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Partial));
		}

		void PopulateConsignments(WhsItemReceiveASN shippingNotice, UniversalShipment dataObject)
		{
			if (shouldPopulateConsignments)
			{
				var receiveConsignmentPKs = shippingNotice.PackageStates
				.Where(ps => !ps.WPS_WRC_TransitReceiveConsignment.IsEmpty)
				.Select(ps => ps.WPS_WRC_TransitReceiveConsignment)
				.Distinct();
				var consignmentWriter = new WhsTransitReceiveConsignmentDataObjectWriter(writeManager, shippingNotice);

				if (receiveConsignmentPKs.Any())
				{
					var receiveConsignments = shippingNotice.Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.PK, receiveConsignmentPKs));
					dataObject.SetSubShipmentCollection(() =>
					{
						var result = new DataObjectList<UniversalShipment>();
						foreach (var receiveConsignment in receiveConsignments)
						{
							var consignmentDataObject = consignmentWriter.GetDataObject(receiveConsignment);
							result.Add(consignmentDataObject);
						}
						return result;
					});
				}
			}
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransitReceiveASN;
		}
	}
}
