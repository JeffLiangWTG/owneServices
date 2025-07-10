using System.Linq;
using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitConsignmentOrderReferenceReader : DataObjectReader<OrderNumber, WhsItemConsignmentOrderReference>
	{
		public TransitConsignmentOrderReferenceReader(OrderNumber orderNumber, IWhsItemConsignmentOrderReferenceProvider parentBO, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(orderNumber, logger, factory)
		{
			this.ParentBO = Argument.NotNull(parentBO, "parent consignment");
		}

		readonly IWhsItemConsignmentOrderReferenceProvider ParentBO;

		protected override WhsItemConsignmentOrderReference GetExistingBusinessObject()
		{
			var consignmentOrderReferences = ParentBO.WhsItemConsignmentOrderReferences;
			return (WhsItemConsignmentOrderReference)consignmentOrderReferences.FirstOrDefault(c => c.ConsignmentOrderNumber.Equals(dataObject.OrderReference));
		}

		protected override void PopulateBusinessObject(WhsItemConsignmentOrderReference targetBO)
		{
			SetValue(targetBO, WhsItemConsignmentOrderReferenceSchema.WOR_ParentID, ParentBO.PK);
			SetValue(targetBO, WhsItemConsignmentOrderReferenceSchema.WOR_ParentTableCode, ParentBO.TablePrefix);
			SetValue(targetBO, WhsItemConsignmentOrderReferenceSchema.WOR_OrderReference, dataObject.OrderReference);
		}
	}
}
