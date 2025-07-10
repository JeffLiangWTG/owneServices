using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	class USACEFDAAddInfoProductValidation : USACEFDAAddInfoValidation
	{
		public USACEFDAAddInfoProductValidation(AutoUSACEFDAAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_Qty2()
		{
			base.CheckUS_Qty2();
			CheckQuantity(Parent.US_Qty2Info);
		}

		protected override void CheckUS_Qty3()
		{
			base.CheckUS_Qty3();
			CheckQuantity(Parent.US_Qty3Info);
		}

		protected override void CheckUS_Qty4()
		{
			base.CheckUS_Qty4();
			CheckQuantity(Parent.US_Qty4Info);
		}

		protected override void CheckUS_Qty5()
		{
			base.CheckUS_Qty5();
			CheckQuantity(Parent.US_Qty5Info);
		}

		protected override void CheckUS_Qty6()
		{
			base.CheckUS_Qty6();
			CheckQuantity(Parent.US_Qty6Info);
		}

		void CheckQuantity(ZPropertyInfo qtyInfo)
		{
			var partPivot = FDA.Parent as CusClassPartPivot;
			if (partPivot != null && !qtyInfo.Value.IsEmpty && partPivot.ACEFDAs.Count == 1)
			{
				var lastFDAQtyHasNotQty = FDA.GetLastFDAQty(false);
				var lastFDAQtyHasQty = FDA.GetLastFDAQty(true);

				if (lastFDAQtyHasNotQty.LastFDAQtyInfo == null && lastFDAQtyHasQty.LastFDAQtyInfo != null)
				{
					var lastQtyInfo = lastFDAQtyHasQty.LastFDAQtyInfo;
					if (lastQtyInfo != null && lastQtyInfo.Name == qtyInfo.Name)
					{
						qtyInfo.AddWarning("Quantity should not be entered here as it will be calculated when the invoice line is entered.");
					}
				}
			}
		}
	}
}
