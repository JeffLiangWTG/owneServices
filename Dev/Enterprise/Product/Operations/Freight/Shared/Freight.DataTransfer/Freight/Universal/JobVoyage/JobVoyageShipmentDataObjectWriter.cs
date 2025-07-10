using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class JobVoyageShipmentDataObjectWriter : TopLevelDataObjectWriter<JobVoyage, Shipment>
	{
		public JobVoyageShipmentDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObject(JobVoyage voyageBO, Shipment shipmentData)
		{
			PopulateJobVoyage(voyageBO, shipmentData);
			PopulateJobSailings(voyageBO, shipmentData);
		}

		void PopulateJobVoyage(JobVoyage voyageBO, Shipment shipmentData)
		{
			if (voyageBO.Line != null)
			{
				shipmentData.AddOrgAddress(writeManager, voyageBO.Line.MainAddress, nameof(DocAddressType.Carrier));
			}

			var origin = (VoyageOrigin)voyageBO.Origins.FirstOrDefault();
			if (origin != null)
			{
				shipmentData.PortOfLoading = UNLOCO.New(origin.PortOfLoading);
			}
			var destination = (VoyageDestination)voyageBO.Destinations.FirstOrDefault();
			if (destination != null)
			{
				shipmentData.PortOfDischarge = UNLOCO.New(destination.PortOfDischarge);
			}
			shipmentData.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(voyageBO.JV_AirSeaRoad, new CodeDescriptionPairList(OLookUpEditType.TransportType));
			shipmentData.VesselName = voyageBO.JV_RV_NKVessel;
			shipmentData.VoyageFlightNo = voyageBO.JV_VoyageFlight;
			shipmentData.LloydsIMO = voyageBO.Vessel.GetLloydsIMO();
		}

		void PopulateJobSailings(JobVoyage voyageBO, Shipment shipmentData)
		{
			shipmentData.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { Content = CollectionContent.Complete });
			shipmentData.SetSubShipmentCollection(() => new DataObjectList<Shipment>());

			var sailingWriter = new ScheduleTransportLegDataObjectWriter(writeManager);
			var billOfLadingType = ObjectFactory.GetType<Agency.IBillOfLading>();
			var dataContextManager = billOfLadingType.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var billOfLadingWrite = dataContextManager == null ? null : dataContextManager.GetShipmentDataObjectWriter(writeManager);

			foreach (JobSailing sailing in voyageBO.Sailings)
			{
				if (shipmentData.TransportLegCollection != null)
				{
					var legDataObject = sailingWriter.GetDataObject(sailing);
					shipmentData.TransportLegCollection.Add(legDataObject);
				}

				if (shipmentData.SubShipmentCollection != null)
				{
					foreach (var billOfLading in LoadBillOfLadings(sailing, billOfLadingType))
					{
						var subShipmentData = (Shipment)billOfLadingWrite.GetDataObject(billOfLading);
						shipmentData.SubShipmentCollection.Add(subShipmentData);
					}
				}
			}
		}

		BusinessObject[] LoadBillOfLadings(JobSailing sailing, Type billOfLadingType)
		{
			ZQuery statusQuery = new ZQuery(JobShipmentSchema.JS_ShipmentStatus, new ZString[]
			{
				ShipmentStatusList.Codes.Confirmed,
				ShipmentStatusList.Codes.WebFwdInstruction,
			});
			ZQuery query = new ZQuery();
			query.AddToFilter(JobShipmentSchema.JS_JX, sailing.PK);
			query.AddToFilter(JoinCondition.And, JobShipmentSchema.JS_IsShipping, true);
			query.AddToFilter(statusQuery, JoinCondition.And);

			return sailing.Factory.Load(billOfLadingType, query);
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.SailingSchedule;
		}
	}
}
