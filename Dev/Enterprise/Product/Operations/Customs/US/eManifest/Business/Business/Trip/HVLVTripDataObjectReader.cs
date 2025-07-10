using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class HVLVTripDataObjectReader : ShipmentDataObjectReader<Trip>
	{
		public HVLVTripDataObjectReader(UniversalXml.Shipment consolLevelDataObject, UniversalXml.Shipment shipmentLevelDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipmentLevelDataObject, logger, factory)
		{
			this.consolLevelDataObject = consolLevelDataObject;
		}

		readonly UniversalXml.Shipment consolLevelDataObject;

		public override DataContextType DataContextType => DataContextType.USeManifestTrip;

		protected override CharacterCase StringValueCharacterCase => CharacterCase.Upper;

		protected override IMatchingBusinessEntityFinder<Trip> GetCombinedReferenceMatcher() => null;

		protected override Trip GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => consolLevelDataObject.FindRelatedCustomsJobFromHVLVShipment<Trip>(factory.BOFactory);

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(Trip tripBO)
		{
			if (tripBO != null)
			{
				var shipmentQuery = new ZQuery(CusInBondBillSchema.B0_BH, tripBO.PK);
				var shipments = factory.Load<Shipment>(shipmentQuery);

				var masterBillNumberHashSet = shipments.GroupBy(b => b.B0_MasterBillNumber)
													.Where(p => p.Count() > 1 || p.Any(b => string.IsNullOrEmpty(b.B0_MasterBillNumber)))
													.Select(p => p.Key)
													.ToHashSet();

				if (masterBillNumberHashSet.Count > 0)
				{
					if (masterBillNumberHashSet.Contains(string.Empty))
					{
						return Res.GetString("071e9552-dfba-417b-a992-3d89c724f396", "Cannot sync eManifest as there are Shipment(s) that have an empty House Bill Number. Please fix before reattempting to sync.");
					}
					else
					{
						return Res.GetString("55b2758a-c28c-4d23-b7bb-99aa831845f9", "Cannot sync eManifest as the following House Bill Numbers are duplicated on the Shipments: {0}. Please fix before reattempting to sync.", string.Join(", ", masterBillNumberHashSet));
					}
				}
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(tripBO);
		}

		protected override void PopulateBusinessObject(Trip tripBO)
		{
			SetValue(tripBO, CusInBondHeaderSchema.BH_VoyageNumber, dataObject.VoyageFlightNo.GetValueOrDefault());
			SetValue(tripBO, CusInBondHeaderSchema.BH_PortUnladingDCode, dataObject.AddInfoCollection?.GetZStringValue(DataTransferConstants.AddInfoKeys.PortOfDischargeScheduleD) ?? ZString.Empty);
			SetValue(tripBO, CusInBondHeaderSchema.BH_RL_NKPortUnlading, dataObject.PortOfDischarge?.Code ?? ZString.Empty);

			PopulateParentInfo(tripBO);

			PopulateOrganizations(tripBO);

			var firstTransportLeg = GetFirstTransportLeg(dataObject);
			if (firstTransportLeg != null)
			{
				SetValue(tripBO,
					CusInBondHeaderSchema.BH_TransitDirection,
					IsImport(firstTransportLeg) ? TransitDirectionCodes.Codes.Importation
												: IsExport(firstTransportLeg) ? TransitDirectionCodes.Codes.Exportation
												: TransitDirectionCodes.Codes.Transit);
				SetValue(tripBO, CusInBondHeaderSchema.BH_ETA, firstTransportLeg.EstimatedArrival);
			}

			PopulateShipmentsFromConsignmentLevelSubShipments(tripBO);
		}

		void PopulateParentInfo(Trip tripBO)
		{
			var shipmentDataSource = dataObject.DataContext?.DataSourceCollection?.SingleOrDefault(s => s.Type.Equals(nameof(DataContextType.ForwardingShipment)));
			if (shipmentDataSource != null && shipmentDataSource.Key.HasValue)
			{
				var shipmentNumber = shipmentDataSource.Key.Value;
				var forwardingShipmentType = ObjectFactory.GetType(nameof(IForwardingShipment));
				var forwardingShipment = factory.LoadTop1(forwardingShipmentType, new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentNumber));
				if (forwardingShipment != null)
				{
					SetValue(tripBO, CusInBondHeaderSchema.BH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
					SetValue(tripBO, CusInBondHeaderSchema.BH_ParentID, forwardingShipment.PK);
				}
			}
		}

		void PopulateOrganizations(Trip tripBO)
		{
			var carrierAddress = GetMatchingOrgAddress(consolLevelDataObject, nameof(DocAddressType.ShippingLineAddress));
			if (carrierAddress != null)
			{
				var carrier = carrierAddress.Header;
				SetValue(tripBO, CusInBondHeaderSchema.BH_OH_Carrier, carrier.PK);
				SetValue(tripBO, CusInBondHeaderSchema.BH_CarrierSCAC, carrier.SCACCode);
			}

			var client = GetMatchingOrgAddress(dataObject, nameof(DocAddressType.ConsignorDocumentaryAddress));
			if (client != null)
			{
				SetValue(tripBO, CusInBondHeaderSchema.BH_OA_Importer, client.PK);
			}
		}

		UniversalXml.TransportLeg GetFirstTransportLeg(UniversalXml.Shipment dataObject)
		{
			return dataObject.TransportLegCollection?.FirstOrDefault();
		}

		bool IsImport(UniversalXml.TransportLeg leg) => !IsLocalPort(leg.PortOfLoading) && IsLocalPort(leg.PortOfDischarge);

		bool IsExport(UniversalXml.TransportLeg leg) => IsLocalPort(leg.PortOfLoading) && !IsLocalPort(leg.PortOfDischarge);

		bool IsLocalPort(UniversalXml.UNLOCO port) => port?.Code.GetValueOrDefault().StartsWith(CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase) ?? false;

		void PopulateShipmentsFromConsignmentLevelSubShipments(Trip tripBO)
		{
			var shipments = dataObject.SubShipmentCollection?.ToArray();
			if (shipments != null)
			{
				using (tripBO.CreateTemporaryMasterBillToShipmentLookup())
				{
					var collectionReader = new HVLVShipmentCollectionDataObjectReader(tripBO, consolLevelDataObject, dataObject, logger, factory, shipments);
					collectionReader.ReadIntoCollection();
				}
			}
		}

		OrgAddress GetMatchingOrgAddress(UniversalXml.Shipment shipment, string addressType)
		{
			var addressDataObject = shipment.OrganizationAddressCollection?.FirstOrDefault(addressType);
			return addressDataObject != null ? new OrganisationDataObjectReader(addressDataObject, logger, factory).GetMatched() : null;
		}
	}
}
