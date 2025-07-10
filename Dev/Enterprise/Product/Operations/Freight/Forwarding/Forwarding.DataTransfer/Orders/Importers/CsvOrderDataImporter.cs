using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class CsvOrderDataImporter : FlatFileDataImporter
	{
		protected override IValueObject CreateXsd()
		{
			return new Xsd.Orders();
		}

		protected override bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
		{
			Xsd.Orders ordersValue = (Xsd.Orders)xsd;

			List<ImportedOrder> importedOrders = new List<ImportedOrder>();

			foreach (Xsd.Order orderValue in ordersValue.Order)
			{
				NotificationBuffer currentOrderNotifications = new NotificationBuffer(notifications);
				ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider.Current, new Xsd.XmlInterchange(), currentOrderNotifications);
				ImportedOrder order = new ImportedOrder(Adapter.CreateOrUpdateFromValueObject(orderValue, importContext));

				if (importContext.NotificationsHasErrors)
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("99235caa-cd47-49da-bae3-476e16a8e152", "Order {0} could not be imported due to invalid data.", order.OrderNumberAndSplit)));
					FactoryProvider.CreateNewAndReclaimMemoryWithoutSave();
				}
				else
				{
					importedOrders.Add(order);
					FactoryProvider.SaveCurrentAndCreateNew();
				}
			}

			ImportedOrderChangesDocumentManager.DeliverAllImportedOrderChanges(importedOrders);

#if DEBUG
			importedOrdersForTest = importedOrders;
#endif
			return true;
		}

#if DEBUG
		public List<ImportedOrder> importedOrdersForTest;
#endif

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CsvFlatFileFormat(); }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new OrderConverter(notifications, FactoryProvider.Current);
		}

		OrderValueObjectDataAdapter Adapter
		{
			get { return adapter ?? (adapter = new OrderValueObjectDataAdapter()); }
		}
		OrderValueObjectDataAdapter adapter;
	}
}
