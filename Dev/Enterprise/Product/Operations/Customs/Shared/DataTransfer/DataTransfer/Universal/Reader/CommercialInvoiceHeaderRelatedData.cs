using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderRelatedData
	{
		public CommercialInvoiceHeaderRelatedData(Shipment shipmentDataObject)
		{
			this.shipmentDataObject = Argument.NotNull(shipmentDataObject, "shipmentDataObject");
		}

		public ZGuid GetBranchPK(BusinessObjectFactory factory)
		{
			return shipmentDataObject.GetBranchPK(factory);
		}

		public CodeDescriptionPair MessageType
		{
			get { return shipmentDataObject.MessageType; }
		}

		public DataObjectList<TransportLeg> TransportLegCollection
		{
			get { return shipmentDataObject.TransportLegCollection; }
		}

		public IEnumerable<Note> NoteCollection
		{
			get { return shipmentDataObject.NoteCollection; }
		}

		public IEnumerable<AdditionalBill> BillCollection
		{
			get { return shipmentDataObject.AdditionalBillCollection; }
		}

		public IEnumerable<Container> ContainerCollection
		{
			get { return shipmentDataObject.ContainerCollection; }
		}

		public IEnumerable<AdditionalReference> AdditionalReferenceCollection
		{
			get { return shipmentDataObject.AdditionalReferenceCollection; }
		}

		public IEnumerable<UniversalCustoms.CommercialCharge> GroupChargeCollection
		{
			get { return shipmentDataObject.CommercialInfo == null ? null : shipmentDataObject.CommercialInfo.CommercialChargeCollection; }
		}

		public bool HasBillData
		{
			get { return IsSingleInvoiceData && shipmentDataObject.AdditionalBillCollection != null; }
		}

		public bool HasContainerData
		{
			get { return IsSingleInvoiceData && shipmentDataObject.ContainerCollection != null; }
		}

		public bool HasAdditionalReferenceData
		{
			get { return IsSingleInvoiceData && shipmentDataObject.AdditionalReferenceCollection != null; }
		}

		public bool HasGroupChargeData
		{
			get { return IsSingleInvoiceData && shipmentDataObject.CommercialInfo.CommercialChargeCollection != null; }
		}

		public bool IsSingleInvoiceData
		{
			get
			{
				if (!isSingleInvoiceData.HasValue)
				{
					var commercialInfo = shipmentDataObject.CommercialInfo;
					isSingleInvoiceData = commercialInfo != null
						&& commercialInfo.SubGroupCollection == null
						&& commercialInfo.CommercialInvoiceCollection != null
						&& commercialInfo.CommercialInvoiceCollection.Count == 1;
				}
				return isSingleInvoiceData.Value;
			}
		}
		bool? isSingleInvoiceData;

		readonly Shipment shipmentDataObject;
	}
}
