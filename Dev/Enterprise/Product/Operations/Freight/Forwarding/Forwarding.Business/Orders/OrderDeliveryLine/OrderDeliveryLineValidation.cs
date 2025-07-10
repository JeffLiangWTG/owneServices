using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderDeliveryLineValidation : ZValidation
	{
		public OrderDeliveryLineValidation(OrderDeliveryLine line)
			: base(line)
		{
			this.Parent = line;
		}

		#region Implementation

		public readonly OrderDeliveryLine Parent;

		public override Type AutoValidationType
		{
			get { return typeof(OrderDeliveryLineValidation); }
		}

		public override void ValidateAll()
		{
			ValidateOrderNumber();
			ValidateOrderLineNumber();
		}

		#endregion

		#region Order Number

		public void ValidateOrderNumber()
		{
			ValidateCalculatedProperty(Parent.OrderNumberInfo);
		}

		protected void CheckOrderNumber()
		{
			MandatoryValidation.CheckEntered(Parent.OrderNumberInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OrderNumberInfo, Parent.Lookups.OrderNumbers);
			CheckDuplicateOrderAndLineNumber(Parent.OrderNumberInfo);
		}

		#endregion

		#region Order Number Split

		public void ValidateOrderNumberSplit()
		{
			ValidateCalculatedProperty(Parent.OrderNumberSplitInfo);
		}

		protected void CheckOrderNumberSplit()
		{
			CheckDuplicateOrderAndLineNumber(Parent.OrderNumberSplitInfo);
		}

		#endregion

		#region Order Number And Split

		public void ValidateOrderNumberAndSplit()
		{
			ValidateCalculatedProperty(Parent.OrderNumberAndSplitInfo);
		}

		protected void CheckOrderNumberAndSplit()
		{
			ListValidation.ErrorIfInvalidCode(Parent.OrderNumberAndSplitInfo, Parent.Lookups.OrderNumberAndSplits);
		}

		#endregion

		#region Order Line Number

		public void ValidateOrderLineNumber()
		{
			ValidateCalculatedProperty(Parent.OrderLineNumberInfo);
		}

		protected void CheckOrderLineNumber()
		{
			MandatoryValidation.CheckEntered(Parent.OrderLineNumberInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OrderLineNumberInfo, Parent.Lookups.OrderLineNumbers);
			CheckDuplicateOrderAndLineNumber(Parent.OrderLineNumberInfo);
		}

		#endregion

		#region Duplicate

		void CheckDuplicateOrderAndLineNumber(ZPropertyInfo info)
		{
			foreach (OrderDeliveryLine line in Parent.PreAdvice.OrderLines)
			{
				if (line != Parent &&
					line.OrderNumber == Parent.OrderNumber &&
					line.OrderNumberSplit == Parent.OrderNumberSplit &&
					line.OrderLineNumber == Parent.OrderLineNumber &&
					!Parent.OrderNumber.IsEmpty &&
					!Parent.OrderLineNumber.IsEmpty)
				{
					info.AddError(Res.GetString("1daf16cc-2932-4a32-9b7d-d465cc0e218f", "This combination of order and order line number has already been entered. You can only deliver this line once."));
					break;
				}
			}
		}

		#endregion
	}
}
