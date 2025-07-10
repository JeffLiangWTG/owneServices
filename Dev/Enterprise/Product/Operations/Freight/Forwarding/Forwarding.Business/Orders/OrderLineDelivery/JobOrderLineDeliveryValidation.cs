using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobOrderLineDeliveryValidation : AutoJobOrderLineDeliveryValidation
	{
		public JobOrderLineDeliveryValidation(AutoJobOrderLineDelivery parent) : base(parent)
		{
		}

		new OrderLineDelivery Parent
		{
			get { return (OrderLineDelivery)base.Parent; }
		}

		protected override void CheckJ4_RL_NKDestinationPort()
		{
			base.CheckJ4_RL_NKDestinationPort();
			MandatoryValidation.WarnIfNotEntered(Parent.J4_RL_NKDestinationPortInfo);
		}

		protected override void CheckJ4_OA_NKDeliveryPoint()
		{
			base.CheckJ4_OA_NKDeliveryPoint();
			if (ErrorIfDeliveryPointNotEntered)
			{
				MandatoryValidation.CheckEntered(Parent.J4_OA_NKDeliveryPointInfo);
			}
			else
			{
				if (!Parent.J4_RL_NKDestinationPort.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.J4_OA_NKDeliveryPointInfo);
				}
				else
				{
					MandatoryValidation.WarnIfNotEntered(Parent.J4_OA_NKDeliveryPointInfo);
				}
			}
		}

		protected virtual bool ErrorIfDeliveryPointNotEntered
		{
			get { return false; }
		}

		protected override void CheckJ4_Allocated()
		{
			base.CheckJ4_Allocated();
			if (Parent.Containers.Count > 0 && Parent.J4_Allocated != Parent.J4_Calc_TotalQuantityAllocated)
			{
				Parent.J4_AllocatedInfo.AddWarning(Res.GetString("6ba67a24-eb1d-47b0-b2f6-72f00fb379fb", "Allocated Quantity doesn't add up to quantity invoiced on all containers (should be {0})", Parent.J4_Calc_TotalQuantityAllocated));
			}

			if (Parent.OrderLine != null)
			{
				Parent.OrderLine.Validation.ValidateJO_QtyReceived();
			}
		}

		#region Custom Label Mandatory Validation

		protected CustomLabelPropertyValidation CustomLabelPropertyValidation
		{
			get { return customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory)); }
		}
		CustomLabelPropertyValidation customLabelPropertyValidation;

		protected override void CheckJ4_CustomAttribute1()
		{
			base.CheckJ4_CustomAttribute1();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomAttribute1Info);
		}

		protected override void CheckJ4_CustomAttribute2()
		{
			base.CheckJ4_CustomAttribute2();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomAttribute2Info);
		}

		protected override void CheckJ4_CustomAttribute3()
		{
			base.CheckJ4_CustomAttribute3();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomAttribute3Info);
		}

		protected override void CheckJ4_CustomAttribute4()
		{
			base.CheckJ4_CustomAttribute4();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomAttribute4Info);
		}

		protected override void CheckJ4_CustomAttribute5()
		{
			base.CheckJ4_CustomAttribute5();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomAttribute5Info);
		}

		protected override void CheckJ4_CustomDecimal1()
		{
			base.CheckJ4_CustomDecimal1();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomDecimal1Info);
		}

		protected override void CheckJ4_CustomDecimal2()
		{
			base.CheckJ4_CustomDecimal2();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomDecimal2Info);
		}

		protected override void CheckJ4_CustomDecimal3()
		{
			base.CheckJ4_CustomDecimal3();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomDecimal3Info);
		}

		protected override void CheckJ4_CustomDecimal4()
		{
			base.CheckJ4_CustomDecimal4();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomDecimal4Info);
		}

		protected override void CheckJ4_CustomDecimal5()
		{
			base.CheckJ4_CustomDecimal5();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomDecimal5Info);
		}

		protected override void CheckJ4_CustomDate1()
		{
			base.CheckJ4_CustomDate1();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomDate1Info);
		}

		protected override void CheckJ4_CustomDate2()
		{
			base.CheckJ4_CustomDate2();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomDate2Info);
		}

		protected override void CheckJ4_CustomDate3()
		{
			base.CheckJ4_CustomDate3();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomDate3Info);
		}

		protected override void CheckJ4_CustomDate4()
		{
			base.CheckJ4_CustomDate4();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomDate4Info);
		}

		protected override void CheckJ4_CustomDate5()
		{
			base.CheckJ4_CustomDate5();
			CustomLabelPropertyValidation.Validate(new OrderLineDelivery.CustomLabelsProvider(Parent.Order), Parent.J4_CustomDate5Info);
		}

		#endregion
	}
}
