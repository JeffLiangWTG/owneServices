
using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderXMLValueObjectSerializer : XmlValueObjectSerializer
	{
		public OrderXMLValueObjectSerializer()
			: this(typeof(Xsd.Order))
		{
		}

		public OrderXMLValueObjectSerializer(Type valueObjectType)
			: base(valueObjectType)
		{
		}

		protected override void ReadCollectionFromXml(XmlReader reader, IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObjectImportContext context)
		{
			List<ImportedOrder> importedOrders = new List<ImportedOrder>();

			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			((IFactory)tempFactory).CanSave = false;
			ValueObjectImportContext tempContext = new ValueObjectImportContext(tempFactory, (Xsd.XmlInterchange)context.Interchange, context.OrganisationMatching, new NotificationBuffer());

			reader.ReadStartElement(dataAdapter.RootCollectionElementName);
			while (CanDeserialize(reader))
			{
				try
				{
					reader.MoveToContent();

					IValueObject value = CreateValueObject(reader, dataAdapter, context);

					//Import Orders WITHOUT a Shipment/Declaration
					Order order = (Order)CreateOrUpdateFromValueObject(dataAdapter, collection, value, context);
					if (order != null)
					{
						importedOrders.Add(new ImportedOrder(order));
					}
					else
					{
						//May not have imported because a Shipment/Declaration may have been attached.
						//We still need to fake the import and add it to the Report
						order = (Order)dataAdapter.FindBusinessObject(value, tempContext);
						if (order != null && !order.CanBeUpdatedByImport)
						{
							dataAdapter.ImportFromValueObject(order, value, tempContext);
							importedOrders.Add(new ImportedOrder(order));
						}
					}
				}
				catch (XmlException ex)
				{
					context.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, ex.Message));
				}
			}
			ImportedOrderChangesDocumentManager.DeliverAllImportedOrderChanges(importedOrders);

#if DEBUG
			importedOrdersForTest = importedOrders;
#endif
		}

#if DEBUG
		public List<ImportedOrder> importedOrdersForTest;

#endif
	}
}
