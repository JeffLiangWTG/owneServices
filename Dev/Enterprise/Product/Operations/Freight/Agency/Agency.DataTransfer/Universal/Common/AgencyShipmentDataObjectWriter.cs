using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	abstract class AgencyShipmentDataObjectWriter<T> : CommonShipmentDataObjectWriter<T> where T : AgencyShipment
	{
		protected AgencyShipmentDataObjectWriter(IDataWritingManager manager, AgencyShipmentContainer container)
			: base(manager)
		{
			this.container = container;
		}

		readonly AgencyShipmentContainer container;
		readonly Dictionary<ZGuid, int> links = new Dictionary<ZGuid, int>();

		#region Implementation

		protected override void PopulateShipment(T sourceBO, UniversalShipment dataObject)
		{
			base.PopulateShipment(sourceBO, dataObject);

			dataObject.IsShipping = true;
			dataObject.ShipmentStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.JS_ShipmentStatus, sourceBO.Lookups.JS_ShipmentStatus_List);
			dataObject.BookingConfirmationReference = sourceBO.JS_CFSReference;
			dataObject.AgentsReference = sourceBO.JS_BookingReference;
			dataObject.CFSReference = null;

			dataObject.GoodsValue = sourceBO.JS_GoodsValue;
			dataObject.GoodsValueCurrency = Currency.New(sourceBO.GoodsValueCurr);

			dataObject.PlaceOfIssue = UNLOCO.New(sourceBO.HouseBillIssuePlace);
			dataObject.PlaceOfReceipt = UNLOCO.New(sourceBO.PlaceOfReceipt);
			dataObject.PlaceOfDelivery = UNLOCO.New(sourceBO.PlaceOfDischarge);

			dataObject.PaymentMethod = ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.JS_INCO, sourceBO.Lookups.JS_INCO_List);
			dataObject.ShipmentIncoTerm = null;
		}

		protected override void WriteDates(T shipmentBizObj, UniversalShipment dataObject)
		{
			base.WriteDates(shipmentBizObj, dataObject);

			dataObject.DateCollection.Add(Date.New(DateType.ShippedOnBoard, false, shipmentBizObj.JS_ShippedOnBoardDate));
			dataObject.DateCollection.Add(Date.New(DateType.BillIssued, false, shipmentBizObj.JS_HouseBillIssueDate));
		}

		protected override void WriteAddresses(T shipmentBizObj, UniversalShipment dataObject)
		{
			base.WriteAddresses(shipmentBizObj, dataObject);

			dataObject.AddOrgAddress(writeManager, shipmentBizObj.BookedShippingLine, DocAddressType.ShippingLineAddress);
			dataObject.AddOrgAddress(writeManager, shipmentBizObj.Principal, DocAddressType.Principal);
		}

		IEnumerable<AgencyShipmentContainer> GetContainersToWrite(T sourceBO)
		{
			return container != null
				? new[] { container }
				: sourceBO.ShippingContainers.Cast<AgencyShipmentContainer>();
		}

		protected override void WriteContainers(T sourceBO, UniversalShipment dataObject)
		{
			dataObject.SetContainerCollection(() =>
			{
				var containers = new DataObjectList<Container>();
				containers.Content = CollectionContent.Complete;
				var containerWriter = new ContainerDataObjectWriter(ListCache, writeManager);
				foreach (var containerSource in GetContainersToWrite(sourceBO))
				{
					var containerDataObject = containerWriter.GetDataObject(containerSource);
					containers.Add(containerDataObject);

					var link = containers.Count;
					containerDataObject.Link = link;
					links.Add(containerSource.PK, link);
				}
				return containers;
			});
		}

		IEnumerable<PackLine> GetPackLinesToWrite(T sourceBO)
		{
			return container != null
				? container.PackLines.Cast<PackLine>()
				: sourceBO.OuterPackLines.Cast<PackLine>();
		}

		protected override void WritePackLines(T sourceBO, UniversalShipment dataObject)
		{
			dataObject.SetPackingLineCollection(() =>
			{
				var packingLines = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };

				ProcessTopLevelPackingLines(sourceBO, packingLines);
				ProcessOtherPackingLines(sourceBO, packingLines);
				return packingLines;
			});
		}

		void ProcessTopLevelPackingLines(T sourceBO, DataObjectList<PackingLine> packingLines)
		{
			var packingLineWriter = new TopLevelPackPackingLineDataObjectWriter(writeManager);
			foreach (var containerSource in GetContainersToWrite(sourceBO).Where(c => c.IsTopLevelPack))
			{
				if (links.ContainsKey(containerSource.PK))
				{
					var packingLineDataObject = packingLineWriter.GetDataObject(containerSource);
					packingLineDataObject.ContainerLink = links[containerSource.PK];
					packingLines.Add(packingLineDataObject);
				}
			}
		}

		void ProcessOtherPackingLines(T sourceBO, DataObjectList<PackingLine> packingLines)
		{
			var packingLineWriter = new PackingLineDataObjectWriter<PackLine>(ListCache, writeManager);
			foreach (var packLineSource in GetPackLinesToWrite(sourceBO))
			{
				var packingLineDataObject = packingLineWriter.GetDataObject(packLineSource);
				if (packLineSource.JL_JC.IsValid && links.ContainsKey(packLineSource.JL_JC))
				{
					packingLineDataObject.ContainerLink = links[packLineSource.JL_JC];
				}

				packingLines.Add(packingLineDataObject);
			}
		}

		protected override void PopulateWayBillType(T sourceBO, UniversalShipment dataObject)
		{
			if (!dataObject.WayBillNumber.Value.IsEmpty)
			{
				var billTypeList = sourceBO.Factory.GetCachedValue<WayBillTypeList>();
				dataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, billTypeList);
			}
		}

		#endregion
	}
}
