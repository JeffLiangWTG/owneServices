using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobOrderLineDeliverContainerValidation : AutoJobOrderLineDeliverContainerValidation
	{
		public JobOrderLineDeliverContainerValidation(AutoJobOrderLineDeliverContainer parent)
			: base(parent)
		{
		}

		new OrderLineDeliverContainer Parent
		{
			get { return (OrderLineDeliverContainer)base.Parent; }
		}

		#region J5_ETA

		protected override void CheckJ5_ETA()
		{
			base.CheckJ5_ETA();
			if (Parent.J5_ETA.IsValid && Parent.J5_ETD.IsValid && Parent.J5_ETD > Parent.J5_ETA)
			{
				Parent.J5_ETAInfo.AddError(Res.GetString("b1a25bc7-32c3-4713-9350-17f0416649c2", "The Arrival Date must occur after the Departure Date."));
			}
		}

		#endregion

		#region J5_QuantityInvoiced

		protected override void CheckJ5_QuantityInvoiced()
		{
			base.CheckJ5_QuantityInvoiced();
			CustomLabelPropertyValidation.Validate(new OrderLineDeliverContainer.CustomLabelsProvider(Parent.Order), Parent.J5_QuantityInvoicedInfo);
		}

		#endregion

		#region J5_QuantityInStore

		protected override void CheckJ5_QuantityInStore()
		{
			base.CheckJ5_QuantityInStore();
			if (Parent.J5_QuantityInStore <= 0)
			{
				Parent.J5_QuantityInStoreInfo.AddWarning(Res.GetString("34f48cc8-048f-4cde-8cd0-dd957e65133e", "Quantity Delivered must be greater than zero"));
			}
			CompareValidation.CheckLessThanOrEqualTo(Parent.J5_QuantityInStoreInfo, Parent.J5_QuantityInvoiced);
			Parent.OrderLineDelivery.Validation.ValidateJ4_Allocated();
			CustomLabelPropertyValidation.Validate(new OrderLineDeliverContainer.CustomLabelsProvider(Parent.Order), Parent.J5_QuantityInStoreInfo);
		}

		#endregion

		#region J5_ContainerNum

		protected override void CheckJ5_ContainerNum()
		{
			base.CheckJ5_ContainerNum();
			MandatoryValidation.CheckEntered(Parent.J5_ContainerNumInfo);
			ContainerNumberValidation.WarnIfInvalid(Parent.J5_ContainerNumInfo);
		}

		#endregion

		#region J5_RC_NKContainerType

		protected override void CheckJ5_RC_NKContainerType()
		{
			base.CheckJ5_RC_NKContainerType();
			ListValidation.ErrorIfInvalidCode(Parent.J5_RC_NKContainerTypeInfo, Parent.J5_RC_List);
		}

		#endregion

		#region Custom Labels

		protected CustomLabelPropertyValidation CustomLabelPropertyValidation
		{
			get { return customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory)); }
		}
		CustomLabelPropertyValidation customLabelPropertyValidation;

		protected override void CheckJ5_CustomAttribute1()
		{
			base.CheckJ5_CustomAttribute1();
			CustomLabelPropertyValidation.Validate(new OrderLineDeliverContainer.CustomLabelsProvider(Parent.Order), Parent.J5_CustomAttribute1Info);
		}

		protected override void CheckJ5_CustomAttribute2()
		{
			base.CheckJ5_CustomAttribute2();
			CustomLabelPropertyValidation.Validate(new OrderLineDeliverContainer.CustomLabelsProvider(Parent.Order), Parent.J5_CustomAttribute2Info);
		}

		protected override void CheckJ5_CustomAttribute3()
		{
			base.CheckJ5_CustomAttribute3();
			CustomLabelPropertyValidation.Validate(new OrderLineDeliverContainer.CustomLabelsProvider(Parent.Order), Parent.J5_CustomAttribute3Info);
		}

		protected override void CheckJ5_CustomDecimal1()
		{
			base.CheckJ5_CustomDecimal1();
			CustomLabelPropertyValidation.Validate(new OrderLineDeliverContainer.CustomLabelsProvider(Parent.Order), Parent.J5_CustomDecimal1Info);
		}

		protected override void CheckJ5_CustomDecimal2()
		{
			base.CheckJ5_CustomDecimal2();
			CustomLabelPropertyValidation.Validate(new OrderLineDeliverContainer.CustomLabelsProvider(Parent.Order), Parent.J5_CustomDecimal2Info);
		}

		protected override void CheckJ5_CustomDecimal3()
		{
			base.CheckJ5_CustomDecimal3();
			CustomLabelPropertyValidation.Validate(new OrderLineDeliverContainer.CustomLabelsProvider(Parent.Order), Parent.J5_CustomDecimal3Info);
		}

		protected override void CheckJ5_CustomDate1()
		{
			base.CheckJ5_CustomDate1();
			CustomLabelPropertyValidation.Validate(new OrderLineDeliverContainer.CustomLabelsProvider(Parent.Order), Parent.J5_CustomDate1Info);
		}

		protected override void CheckJ5_CustomDate2()
		{
			base.CheckJ5_CustomDate2();
			CustomLabelPropertyValidation.Validate(new OrderLineDeliverContainer.CustomLabelsProvider(Parent.Order), Parent.J5_CustomDate2Info);
		}

		protected override void CheckJ5_CustomDate3()
		{
			base.CheckJ5_CustomDate3();
			CustomLabelPropertyValidation.Validate(new OrderLineDeliverContainer.CustomLabelsProvider(Parent.Order), Parent.J5_CustomDate3Info);
		}

		#endregion
	}
}
