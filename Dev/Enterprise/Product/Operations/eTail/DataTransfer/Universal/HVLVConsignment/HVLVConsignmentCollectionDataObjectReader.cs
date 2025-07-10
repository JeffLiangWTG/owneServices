using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVConsignmentCollectionDataObjectReader : DataObjectCollectionReader<UniversalShipment, HVLVConsignment>
	{
		public HVLVConsignmentCollectionDataObjectReader(IHVLVConsignmentCollectionParent consignmentCollectionParent, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalShipment[] consignments, ForwardingShipment shipmentBO = null)
			: base(consignments)
		{
			consignmentBOCollectionParent = Argument.NotNull(consignmentCollectionParent, nameof(consignmentCollectionParent));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.shipmentBO = shipmentBO;
			businessObjects = consignmentBOCollectionParent.Consignments.Cast<HVLVConsignment>().ToArray();
			idMatchingDictionary = businessObjects.Where(consignment => consignment.HVC_IsValidatedForUniqueness)
				.ToKeyListDictionary(consignment => consignment.HVC_ConsignmentId);
			waybillNumberMatchingDictionary = businessObjects.ToKeyListDictionary(consignment => consignment.HVC_WaybillNumber);
		}

		readonly IHVLVConsignmentCollectionParent consignmentBOCollectionParent;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly ForwardingShipment shipmentBO;
		readonly HVLVConsignment[] businessObjects;
		readonly Dictionary<ZString, List<HVLVConsignment>> idMatchingDictionary;
		readonly Dictionary<ZString, List<HVLVConsignment>> waybillNumberMatchingDictionary;

		protected override HVLVConsignment[] BusinessObjects
		{
			get { return businessObjects; }
		}

		protected override void AddToCollection(HVLVConsignment consignment)
		{
			if (consignmentBOCollectionParent is HVLVConsignmentHeader)
			{
				if (consignment.HVC_HCH_Header.IsEmpty)
				{
					if (consignment.BookingHeader is HVLVBookingHeader header)
					{
						header.HVH_IsProcessedAtOriginDepot = true;
					}

					consignmentBOCollectionParent.Consignments.Add(consignment);
				}
				else
				{
					logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("a0607b34-c6ca-46a9-9d12-6569de0e00e2", "Cannot detach matched consignment {0} from original Shipment as it has already been submitted to Customs.", consignment.HVC_ConsignmentId));
				}
			}
			else if (consignmentBOCollectionParent is HVLVBookingHeader bookingHeader)
			{
				if (consignment.HVC_HVH_BookingHeader.IsEmpty)
				{
					consignmentBOCollectionParent.Consignments.Add(consignment);
				}
				else
				{
					logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("fe050e99-36b3-41bc-ba54-fe16e66e7ef7", "Consignment {0} already existed in Booking Header {1}, it can't be linked to the imported Booking Header {2}.", consignment.HVC_ConsignmentId, consignment.BookingHeader.HVH_BookingReference, bookingHeader.HVH_BookingReference));
				}
			}
		}

		protected override void RemoveFromCollection(HVLVConsignment consignment)
		{
			var errorMessage = Res.GetString("633b70d2-33d6-4d33-b2ce-61dd57884b14", "Attempted to delete {0}. HVLV consignments should not be deleted during import.",
				consignment.HumanReadableName);
			throw new DataObjectReadFailureException(errorMessage);
		}

		protected override HVLVConsignment FindMatchingBusinessObject(UniversalShipment dataObject)
		{
			if (BusinessObjects.Any())
			{
				var dataTarget = dataObject.DataContext?.DataTargetCollection?
				.FirstOrDefault(target => target.Type.GetValueOrDefault() == nameof(DataContextType.HVLVConsignment));
				var dataTargetKey = dataTarget?.Key.GetValueOrDefault() ?? ZString.Empty;

				if (!dataTargetKey.IsEmpty
					&& idMatchingDictionary.TryGetValue(dataTargetKey, out var matchedConsignments)
					&& matchedConsignments.Count == 1)
				{
					return matchedConsignments[0];
				}

				var waybillNumber = dataObject.WayBillNumber.GetValueOrDefault();
				if (!waybillNumber.IsEmpty
					&& waybillNumberMatchingDictionary.TryGetValue(waybillNumber, out matchedConsignments)
					&& matchedConsignments.Count == 1)
				{
					return matchedConsignments[0];
				}
			}

			return null;
		}

		protected override HVLVConsignment ReadIntoBusinessObject(UniversalShipment dataObject, HVLVConsignment consignmentBO)
		{
			var reader = new HVLVConsignmentDataObjectReader(dataObject, logger, factory, consignmentBO, shipmentBO);
			return reader.ReadIntoBusinessObject();
		}

		protected override bool SkipEntity(UniversalShipment dataObject)
		{
			return HasDataSource(dataObject) && dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment) == null;
		}

		bool HasDataSource(UniversalShipment shipmentDataObject)
		{
			var result = shipmentDataObject?.DataContext?.DataSourceCollection?.Any(source =>
				source != null
				&& source.Type.HasValue
				&& !source.Type.Value.IsEmpty);

			return result.GetValueOrDefault();
		}
	}
}
