using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class OrderNumberCollectionReader : DataObjectCollectionReader<OrderNumber, OrderItem>
	{
		public OrderNumberCollectionReader(DataObjectList<OrderNumber> orderNumbers, JobDocsAndCartage parentBO, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(orderNumbers)
		{
			this.parentBO = Argument.NotNull(parentBO, "JobDocsAndCartage parent");
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
		}

		readonly JobDocsAndCartage parentBO;
		protected readonly IXmlImportLogger logger;
		protected readonly UniversalObjectFactory factory;

		protected override OrderItem[] BusinessObjects
		{
			get
			{
				if (bizOs == null)
				{
					bizOs = parentBO.OrderItems.Cast<OrderItem>().ToArray();
				}
				return bizOs;
			}
		}
		OrderItem[] bizOs;

		protected override void AddToCollection(OrderItem businessObject)
		{
			parentBO.OrderItems.Add(businessObject);
		}

		protected override void RemoveFromCollection(OrderItem businessObject)
		{
			businessObject.Delete();
		}

		protected override OrderItem FindMatchingBusinessObject(OrderNumber dataObject)
		{
			var orderItems = parentBO.OrderItems.Cast<OrderItem>();

			var orderItem = orderItems.FirstOrDefault(c => c.JT_OrderReference.Equals(dataObject.OrderReference) && c.JT_Sequence == dataObject.Sequence);

			return orderItem;
		}

		protected override OrderItem ReadIntoBusinessObject(OrderNumber dataObject, OrderItem businessObject)
		{
			var reader = new OrderNumberDataObjectReader(dataObject, logger, factory, parentBO, true);
			return reader.ReadIntoBusinessObject();
		}

		protected override bool SkipEntity(OrderNumber dataObject)
		{
			if (ContentType == CollectionContent.Complete)
			{
				return false;
			}

			var orderItems = parentBO.OrderItems.Cast<OrderItem>();

			return orderItems.Any(c => c.JT_OrderReference.Equals(dataObject.OrderReference));
		}
	}
}
