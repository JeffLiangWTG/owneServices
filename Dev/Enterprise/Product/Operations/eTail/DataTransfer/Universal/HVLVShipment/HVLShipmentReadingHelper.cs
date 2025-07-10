using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLShipmentReadingHelper : IHVLShipmentReadingHelper
	{
		public HVLShipmentReadingHelper(UniversalShipment shipmentDataObject, ForwardingShipment shipmentBO, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			this.shipmentDataObject = Argument.NotNull(shipmentDataObject, nameof(shipmentDataObject));
			this.shipmentBO = Argument.NotNull(shipmentBO, nameof(shipmentBO));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly UniversalShipment shipmentDataObject;
		readonly ForwardingShipment shipmentBO;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;

		void IHVLShipmentReadingHelper.ReadConsignmentsFromSubShipments()
		{
			var consignmentHeader = shipmentBO.GetOrCreateHVLVConsignmentHeader();
			PopulateShipmentDestinationBeforehandForItemUsageCalculation();
			PopulateConsignments(consignmentHeader);
			PopulateOrganizations();
		}

		void PopulateConsignments(HVLVConsignmentHeader consignmentHeader)
		{
			if (shipmentDataObject.SubShipmentCollection != null)
			{
				var collectionReader = new HVLVConsignmentCollectionDataObjectReader(consignmentHeader, logger, factory, shipmentDataObject.SubShipmentCollection.ToArray(), shipmentBO);
				collectionReader.ReadIntoCollectionRetainingUnmatchedElements();
				HVLVConsignmentTransactionParticipant.UnRegister(consignmentHeader.Consignments);
			}
		}

		OrgAddress GetMatchingOrgAddress(string addressType)
		{
			var addressDataObject = shipmentDataObject.OrganizationAddressCollection?.FirstOrDefault(addressType);
			return addressDataObject != null ? new OrganisationDataObjectReader(addressDataObject, logger, factory).GetMatched() : null;
		}

		void PopulateOrganizations()
		{
			var billToParty = GetMatchingOrgAddress(AddressTypes.SendersLocalClient);

			if (billToParty == null)
			{
				if (shipmentBO != null)
				{
					logger.Log(Enterprise.Integration.LogType.Information, Res.GetString("4a2d2d12-8170-4e19-916d-5bd18d758f6d", "Cannot find matching Local Client Organization, using Shipment's Consignor as Bill To Party."));
				}
			}
		}

		void PopulateShipmentDestinationBeforehandForItemUsageCalculation()
		{
			if (shipmentDataObject.PortOfDestination != null && shipmentDataObject.PortOfDestination.Code.HasValue)
			{
				shipmentBO.JS_RL_NKDestination = (ZString)shipmentDataObject.PortOfDestination.Code;
			}
		}
	}
}
