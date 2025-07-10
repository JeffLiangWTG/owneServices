using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitConsignmentOrderReferenceCollectionReader : DataObjectCollectionReader<OrderNumber, WhsItemConsignmentOrderReference>
	{
		public TransitConsignmentOrderReferenceCollectionReader(OrderNumber[] orderNumbers, IWhsItemConsignmentOrderReferenceProvider parentBO, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(orderNumbers)
		{
			this.parentTransitConsignment = Argument.NotNull(parentBO, "parent consignment");
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
		}

		readonly IWhsItemConsignmentOrderReferenceProvider parentTransitConsignment;
		protected readonly IXmlImportLogger logger;
		protected readonly UniversalObjectFactory factory;

		protected override WhsItemConsignmentOrderReference[] BusinessObjects
		{
			get
			{
				if (bizOs == null)
				{
					bizOs = (WhsItemConsignmentOrderReference[])parentTransitConsignment.WhsItemConsignmentOrderReferences.ToArray();
				}
				return bizOs;
			}
		}
		WhsItemConsignmentOrderReference[] bizOs;

		protected override void AddToCollection(WhsItemConsignmentOrderReference businessObject)
		{
			parentTransitConsignment.WhsItemConsignmentOrderReferences.Add(businessObject);
		}

		protected override void RemoveFromCollection(WhsItemConsignmentOrderReference businessObject)
		{
			businessObject.Delete();
		}

		protected override WhsItemConsignmentOrderReference FindMatchingBusinessObject(OrderNumber dataObject)
		{
			var consignmentOrderReferences = parentTransitConsignment.WhsItemConsignmentOrderReferences;
			return (WhsItemConsignmentOrderReference)consignmentOrderReferences.FirstOrDefault(c => c.ConsignmentOrderNumber.Equals(dataObject.OrderReference));
		}

		protected override WhsItemConsignmentOrderReference ReadIntoBusinessObject(OrderNumber dataObject, WhsItemConsignmentOrderReference businessObject)
		{
			var reader = new TransitConsignmentOrderReferenceReader(dataObject, parentTransitConsignment, logger, factory);
			return reader.ReadIntoBusinessObject();
		}
	}
}
