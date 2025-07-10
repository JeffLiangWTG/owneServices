using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingShipmentToWarehouseOrderSender : ModuleToModuleSender<ForwardingShipment>
	{
		ForwardingShipmentToWarehouseOrderSender()
		{
		}

		public static PublishToUniversalResult CreateWarehouseOrderFromForwardingShipment(ForwardingShipment shipment)
		{
			return new ForwardingShipmentToWarehouseOrderSender().CreateNewEntityFromParent(shipment);
		}

		protected override ZString ErrorPrefix
		{
			get { return Res.GetString("9053a43e-3ee0-4d32-a864-b4131d779a9e", "Failed to create Warehouse Order:"); }
		}

		protected override UniversalEvent[] GetUniversalEvents(ForwardingShipment shipment)
		{
			var factory = new BusinessObjectFactory();
			UniversalEvent[] events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, GlbCompany.CurrentCompany.OrgProxy, new RecipientRoleType[1] { RecipientRoleType.WAR }, shipment).ToArray();
				factory.Save();
			}
			return events;
		}

		protected override DataContextType EntityTypeToLoad
		{
			get { return DataContextType.WarehouseOrder; }
		}

		public static void UpdatePackLinesFromWarehouseOrder(ForwardingShipment shipment, BusinessObject warehouseOrder)
		{
			var orderContextManager = (IShipmentDataContextManager)warehouseOrder.GetUniversalDataContextManager();
			if (orderContextManager.DataContextType != DataContextType.WarehouseOrder)
			{
				throw new InvalidOperationException("Only a Warehouse Order can be passed in.");
			}

			var writer = orderContextManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, warehouseOrder)));
			var orderDataObject = (UniversalShipment)writer.GetDataObject(warehouseOrder);
			if (orderDataObject.PackingLineCollection != null)
			{
				foreach (var packingLineDataObject in orderDataObject.PackingLineCollection)
				{
					var packLine = new PackingLineDataObjectReader<ForwardingPackLine, ForwardingShipment>(packingLineDataObject, new DummyLogger(), shipment.Factory, shipment).ReadIntoBusinessObject();
					shipment.OuterPackLines.Add(packLine);
				}
			}
			else if (orderDataObject.CommercialInfo != null
				&& orderDataObject.CommercialInfo.CommercialInvoiceCollection != null
				&& orderDataObject.CommercialInfo.CommercialInvoiceCollection.Count == 1
				&& orderDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection != null)
			{
				foreach (var commercialInvoiceLine in orderDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection)
				{
					var reader = new CommercialInvoiceLineDataObjectReader(commercialInvoiceLine, shipment, new DummyLogger(), shipment.Factory);
					var packLine = reader.ReadIntoBusinessObject();

					shipment.OuterPackLines.Add(packLine);
				}
			}
		}
	}
}
