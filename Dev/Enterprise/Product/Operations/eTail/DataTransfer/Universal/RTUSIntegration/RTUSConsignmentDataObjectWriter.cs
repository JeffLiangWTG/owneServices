using System.Linq;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using WTG.RTUS.Interface;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class RTUSConsignmentDataObjectWriter : HVLVConsignmentDataObjectWriter
	{
		public RTUSConsignmentDataObjectWriter(IDataWritingManager manager, ZGuid currentItemPK, RequestType requestType)
			: base(manager, HVLVConsignmentToRTUSDataExportStrategy.Instance)
		{
			this.currentItemPK = currentItemPK;
			this.requestType = requestType;
		}

		readonly ZGuid currentItemPK;
		readonly RequestType requestType;

		protected override void PopulateDataObject(HVLVConsignment consignmentBO, Shipment dataObject)
		{
			base.PopulateDataObject(consignmentBO, dataObject);
			PopulatePackingLinesForRTUS(consignmentBO, dataObject);

			dataObject.DataContext = DataContextFactory.New(writeManager.Schema.Namespace);
			new DataContextDataObjectWriter().PopulateDataObject(writeManager.Action, dataObject.DataContext);
		}

		protected override void InsertParents(HVLVConsignment sourceBO, ref Shipment dataObject)
		{
			if (requestType == RequestType.Booking || requestType == RequestType.Cancellation)
			{
				var dataContext = dataObject?.DataContext;
				if (dataContext != null)
				{
					var firstDataSource = dataContext.DataSourceCollection?.FirstOrDefault();
					if (firstDataSource != null)
					{
						var currentItem = sourceBO.Items.Cast<HVLVItem>().FirstOrDefault(item => item.PK == currentItemPK);
						if (currentItem != null)
						{
							firstDataSource.Type = nameof(DataContextType.HVLVItem);
							firstDataSource.Key = currentItem.HVI_ItemId;

							if (currentItem.Shipment != null)
							{
								dataContext.AddDataSource(DataContextType.ForwardingShipment, currentItem.Shipment.JS_UniqueConsignRef);
							}

							if (currentItem.Consignment != null)
							{
								dataContext.AddDataSource(DataContextType.HVLVConsignment, currentItem.Consignment.HVC_ConsignmentId);
							}
						}
					}
				}
			}
		}

		#region Implementation

		void PopulatePackingLinesForRTUS(HVLVConsignment consignmentBO, Shipment dataObject)
		{
			DataObjectList<PackingLine> packingLines = null;

			if (requestType == RequestType.Booking)
			{
				packingLines = GetPackingLinesForBooking(consignmentBO, dataObject);
			}

			if (requestType == RequestType.Cancellation)
			{
				packingLines = GetPackingLinesForCancellation();
			}

			dataObject.SetPackingLineCollection(() => packingLines);
		}

		DataObjectList<PackingLine> GetPackingLinesForBooking(HVLVConsignment consignmentBO, Shipment dataObject)
		{
			var packingLines = new DataObjectList<PackingLine> { Content = CollectionContent.Partial };
			var currentItem = consignmentBO.Items.Cast<HVLVItem>().FirstOrDefault(item => item.PK == currentItemPK);

			if (currentItem != null)
			{
				var packingLine = new HVLVItemDataObjectWriter(writeManager, dataObject).GetDataObject(currentItem);
				if (packingLine != null && shouldForceWriteBarcodeToReferenceNumberBecauseOfRTUSImplementation)
				{
					packingLine.ReferenceNumber = currentItem.HVI_CurrentBarcode;
					packingLines.Add(packingLine);
				}
			}

			return packingLines;
		}

		DataObjectList<PackingLine> GetPackingLinesForCancellation() => new DataObjectList<PackingLine> { Content = CollectionContent.Partial };

		readonly bool shouldForceWriteBarcodeToReferenceNumberBecauseOfRTUSImplementation = true;

		#endregion
	}
}
