using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderLineTopLevelDataObjectWriter : TopLevelDataObjectWriter<OrderLine, UniversalShipment>
	{
		public OrderLineTopLevelDataObjectWriter(IDataWritingManager manager)
			: this(manager, null)
		{
		}

		public OrderLineTopLevelDataObjectWriter(IDataWritingManager writeManager, IOrderLineLinkManager orderLineLinkManager)
			: base(writeManager)
		{
			this.orderLineLinkManager = orderLineLinkManager;
		}
		readonly IOrderLineLinkManager orderLineLinkManager;

		#region Context

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.OrderManagerOrderLine;
		}

		#endregion

		#region PopulateDataObject

		protected override void PopulateDataObject(OrderLine orderLineBO, UniversalShipment dataObject)
		{
			if (orderLineBO != null)
			{
				var orderBO = orderLineBO.Order;
				dataObject.Order = new UniversalDataBuss.DataObjects.Universal.Order(writeManager.WriterStrategy);
				dataObject.Order.SetOrderLineCollection(() => ProcessCollection(new[] { orderLineBO }, new OrderLineDataObjectWriter(writeManager, orderLineLinkManager, new LineRelatedDataWriterHelper(orderBO)), CollectionContent.Partial));

				if (orderBO != null)
				{
					CustomLabelsCustomizedFieldDataObjectWriter.Write(JobOrderLineSchema.Instance, orderLineBO, dataObject, OrderLine.NewCustomLabelsProvider(orderBO));
				}
			}
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(OrderLine orderLineBO)
		{
			return orderLineBO.GetUserDefinedValues();
		}

		#endregion
	}
}

