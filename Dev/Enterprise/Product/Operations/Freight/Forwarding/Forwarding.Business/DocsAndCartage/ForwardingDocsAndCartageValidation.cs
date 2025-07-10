using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingDocsAndCartageValidation : JobDocsAndCartageValidation
	{
		public ForwardingDocsAndCartageValidation(AutoJobDocsAndCartage parent)
			: base(parent)
		{
		}

		#region JP_OrderItemsAsString

		protected override void CheckJP_OrderItemsAsString()
		{
			base.CheckJP_OrderItemsAsString();

			List<string> allErrors = new List<string>();
			foreach (OrderItem item in Parent.OrderItems)
			{
				item.Validation.ValidateJT_OrderReference();
				allErrors.AddRange(item.NotificationsIncludingChildren.GetErrors().Select(error => error.Message));
			}

			foreach (string uniqueError in allErrors.Distinct())
			{
				Parent.JP_OrderItemsAsStringInfo.AddError(uniqueError);
			}

			if (Parent.Parent != null)
			{
				var docsAndCartage = Parent as ForwardingDocsAndCartage;

				if (docsAndCartage != null)
				{
					if (Parent.Parent.RequiresOrderTrackLink())
					{
						if (!docsAndCartage.HasOrders)
						{
							Parent.JP_OrderItemsAsStringInfo.AddError(Res.GetString("61c3f200-9e1e-424a-ac63-021140f51515", "You cannot save without attaching any orders."));
						}
					}
					else if (Parent.Parent.RequiresOrderNumbersOnDocs() && Parent.OrderItems.Count == 0 && !docsAndCartage.HasOrders)
					{
						Parent.JP_OrderItemsAsStringInfo.AddError(Res.GetString("6e8d5719-a5b6-4d5c-8b89-78f4f02ff798", "You cannot save without specifying any orders or order references."));
					}
				}
			}

			CheckDuplicateOrderNumbers();
		}

		void CheckDuplicateOrderNumbers()
		{
			var allOrderNumbers = !Parent.JP_OrderItemsAsString.IsEmpty ? Parent.JP_OrderItemsAsString.Split(',').Select(x => x.ToUpper().Trim()) : new List<ZString>();

			var genericOrderParent = (Parent.Parent as IAttachGenericOrders);
			if (genericOrderParent != null)
			{
				allOrderNumbers = allOrderNumbers.Concat(genericOrderParent.GenericOrders.OfType<IAttachedOrder>().Select(o => o.JobNo.ToUpper()));
			}
			else
			{
				var orderParent = (Parent.Parent as IAttachOrders);
				if (orderParent != null)
				{
					allOrderNumbers = allOrderNumbers.Concat(orderParent.AttachedOrders.OfType<IAttachedOrder>().Select(o => o.JobNo.ToUpper()));
				}
			}

			if (allOrderNumbers
				.GroupBy(o => o)
				.Any(g => g.Take(2).Count() > 1))
			{
				Parent.JP_OrderItemsAsStringInfo.AddError(Res.GetString("c1d1b84c-ea32-428d-8f29-29331096f85f", "There are duplicate Order Refs. You must either delete the duplicate number from the Order Refs field or unlink the linked Order before saving."));
			}
		}

		#endregion
	}
}
