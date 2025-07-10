using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class BuyerAndOrderNumAndSplitIsUniqueValidation : ValidationProvider
	{
		public BuyerAndOrderNumAndSplitIsUniqueValidation(Order order) : base(order)
		{
			this.Order = order;
		}

		public void CheckBuyerAndOrderNumAndSplitIsUnique(ZPropertyInfo propertyToSetErrorOn)
		{
			if (Order.JD_OA_BuyerAddress.IsValid && !Order.JD_OrderNumber.IsEmpty)
			{
				if (!CheckBuyerAndOrderNumAndSplitIsUnique(Order))
				{
					var error = Res.GetString("d32f48ae-5b8d-49dc-9a85-25d93c9b5445", "An order already exists with the same Buyer, {0} and {1}.",
						Order.JD_OrderNumberInfo.Description,
						Order.JD_OrderNumberSplitInfo.Description);

					propertyToSetErrorOn.AddError(error);
				}
			}
		}

		public static bool CheckBuyerAndOrderNumAndSplitIsUnique(Order currentOrder)
		{
			if (currentOrder.JD_OA_BuyerAddress.IsValid && !currentOrder.JD_OrderNumber.IsEmpty)
			{
				var sqlFilter = new ZQuery();
				sqlFilter.AddToFilter(JobOrderHeaderSchema.PK, SQLComparisonOperator.NotEqual, currentOrder.PK);
				sqlFilter.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, currentOrder.Buyer == null ? ZGuid.Empty : currentOrder.Buyer.Addresses.Select(x => x.PK));
				sqlFilter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, currentOrder.JD_OrderNumber);
				sqlFilter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, currentOrder.JD_OrderNumberSplit);
				sqlFilter.AddToFilter(JobOrderHeaderSchema.JD_IsCancelled, 0);
				sqlFilter.IgnoreActiveFilter = true;

				return !currentOrder.Factory.ExistsInDatabase(JobOrderHeaderSchema.Constants.TableName, sqlFilter);
			}

			return true;
		}

		#region Implementation

		protected Order Order;

		#endregion
	}
}
