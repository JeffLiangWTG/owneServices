using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class RelatedOrderLineDeliverContainerCollection : BusinessObjectCollection<OrderLineDeliverContainer>
	{
		public RelatedOrderLineDeliverContainerCollection(OrderLineDeliverContainer relatedContainer) : base(relatedContainer.Factory)
		{
			this.RelatedContainer = relatedContainer;
		}

		public readonly OrderLineDeliverContainer RelatedContainer;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#region Filter

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = new ZQuery();

			if (RelatedContainer.J5_ContainerNum.Length > 0 &&
				RelatedContainer.J5_RV_NKArrivalVessel.Length > 0 &&
				RelatedContainer.J5_Voyage.Length > 0)
			{
				result.AddToFilter(JobOrderLineDeliverContainerSchema.J5_ContainerNum, SQLComparisonOperator.Equal, RelatedContainer.J5_ContainerNum);
				result.AddToFilter(JobOrderLineDeliverContainerSchema.J5_RV_NKArrivalVessel, SQLComparisonOperator.Equal, RelatedContainer.J5_RV_NKArrivalVessel);
				result.AddToFilter(JobOrderLineDeliverContainerSchema.J5_Voyage, SQLComparisonOperator.Equal, RelatedContainer.J5_Voyage);
				result.AddToFilter(JobOrderLineDeliverContainerSchema.PK, SQLComparisonOperator.NotEqual, RelatedContainer.PK);
			}
			else
			{
				result.IsNoResultQuery = true;
			}

			return result;
		}

		#endregion
	}
}
