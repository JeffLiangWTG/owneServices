using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsPickUniversalShipmentDataObjectWriter : DataObjectWriter<WhsPick, UniversalShipment>
	{
		public WhsPickUniversalShipmentDataObjectWriter(IDataWritingManager manager, IEnumerable<WhsOrderToPackageItemNumbers> packageItemNumbers = null)
			: base(manager)
		{
			OrderToPackageItemNumbers = packageItemNumbers;
		}

		readonly IEnumerable<WhsOrderToPackageItemNumbers> OrderToPackageItemNumbers;

		protected override UniversalShipment PopulateDataObject(WhsPick sourceBO)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.Instance);
			shipment.DataContext = DataContextFactory.New();

			PopulateOrderPackingData(shipment, sourceBO);
			return shipment;
		}

		void PopulateOrderPackingData(UniversalShipment shipment, WhsPick pick)
		{
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			foreach (WhsOrder order in pick.Orders)
			{
				shipment.DataContext.AddDataSource(DataContextType.WarehouseOrder, order.WD_DocketID);
				shipment.SubShipmentCollection?.Add(GetOrderPackingShipmentDataObject(order));
			}
		}

		UniversalShipment GetOrderPackingShipmentDataObject(WhsOrder order)
		{
			var orderDataObject = WhsOrderPackageDataObjectHelper.GetWhsOrderPackageDataObject(writeManager, order, ExcludeElement.None);
			new DataContextDataObjectWriter().PopulateDataObject(writeManager.Action, order, orderDataObject.DataContext);
			if (OrderToPackageItemNumbers is not null)
			{
				PopulatePackingLineItemNumbers(orderDataObject, order);
			}

			return orderDataObject;
		}

		void PopulatePackingLineItemNumbers(UniversalShipment orderPackingShipmentDataObject, WhsOrder order)
		{
			var orderToPackageItemNumbers = OrderToPackageItemNumbers.FirstOrDefault(orderToPackage => orderToPackage.Order.JobNo == order.WD_ExternalReference);
			if (orderToPackageItemNumbers is not null)
			{
				foreach (var packLine in orderPackingShipmentDataObject.PackingLineCollection)
				{
					var packageItemNumbers = orderToPackageItemNumbers.PackageItemNumbers.Where(pkgItemNumber => pkgItemNumber.Key.KP_PackageID == packLine.ReferenceNumber.Value);
					if (packageItemNumbers.Any())
					{
						packLine.ItemNo = (ZShort)packageItemNumbers.Single().Value;
					}
				}
			}
		}
	}
}
