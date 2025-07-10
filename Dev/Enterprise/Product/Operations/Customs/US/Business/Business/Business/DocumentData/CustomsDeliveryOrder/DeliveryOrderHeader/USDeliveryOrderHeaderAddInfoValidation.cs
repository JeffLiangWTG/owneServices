namespace Enterprise.Customs.US.Business
{
	public class USDeliveryOrderHeaderAddInfoValidation : AutoUSDeliveryOrderHeaderAddInfoValidation
	{
		public USDeliveryOrderHeaderAddInfoValidation(AutoUSDeliveryOrderHeaderAddInfo parent)
			: base(parent)
		{
		}

		protected DeliveryOrderHeader Header
		{
			get { return Parent.Parent; }
		}

		protected new USDeliveryOrderHeaderAddInfo Parent
		{
			get { return (USDeliveryOrderHeaderAddInfo)base.Parent; }
		}

		protected override void CheckUS_OH_Shipper()
		{
			base.CheckUS_OH_Shipper();
			if (!Parent.US_OH_Shipper.IsValid)
			{
			}
		}

		protected override void CheckUS_OH_BillToParty()
		{
			base.CheckUS_OH_BillToParty();
			if (Parent.US_PrepaidCollect == DeliveryOrderPrepaidCollectTypeList.Codes.ThirdParty && Header.BillToParty == null)
			{
				Parent.US_OH_BillToPartyInfo.AddWarning(BillToPartyIsRequired);
			}
		}
		internal const string BillToPartyIsRequired = "A Bill To Party is required when the freight charged to a third party.";
	}
}
