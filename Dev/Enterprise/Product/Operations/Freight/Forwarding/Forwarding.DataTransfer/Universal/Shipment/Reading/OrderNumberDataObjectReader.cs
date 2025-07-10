using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderNumberDataObjectReader : DataObjectReader<OrderNumber, OrderItem>
	{
		public OrderNumberDataObjectReader(OrderNumber orderNumberDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, JobDocsAndCartage parent, bool strictChecking = false)
			: base(orderNumberDataObject, logger, factory)
		{
			this.parent = Argument.NotNull(parent, "JobDocsAndCartage parent");
			this.strictChecking = strictChecking;
		}

		readonly bool strictChecking;
		readonly JobDocsAndCartage parent;

		protected override OrderItem GetExistingBusinessObject()
		{
			if (!dataObject.OrderReference.HasValue)
			{
				return null;
			}

			if (strictChecking)
			{
				var orderItems = parent.OrderItems.Cast<OrderItem>();
				return orderItems.FirstOrDefault(c => c.JT_OrderReference.Equals(dataObject.OrderReference) && c.JT_Sequence == dataObject.Sequence);
			}

			var foundOrders = parent.OrderItems.Find(new ZQuery(JobOrderItemSchema.JT_OrderReference, dataObject.OrderReference));
			return foundOrders.Length == 0 ? null : (OrderItem)foundOrders[0];
		}

		protected override void PopulateBusinessObject(OrderItem orderItemBO)
		{
			orderItemBO.JT_JP = parent.PK;
			SetValue(orderItemBO, JobOrderItemSchema.JT_Sequence, dataObject.Sequence);
			SetValue(orderItemBO, JobOrderItemSchema.JT_OrderReference, dataObject.OrderReference);
		}
	}
}

