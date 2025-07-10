using System.ComponentModel;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.OrderManager.Orders
{
	public partial class OrderLineToleranceControl : ZUserControl
	{
		public OrderLineToleranceControl()
		{
			InitializeComponent();

			ToleranceModifierLabel.CaptionResourceString = GetToleranceModifier();
			ToleranceTypeLabel.CaptionResourceString = GetToleranceType();
		}

		[DefaultValue(ToleranceModifiers.None)]
		public ToleranceModifiers ToleranceModifier
		{
			get => toleranceModifier;
			set
			{
				toleranceModifier = value;
				ToleranceModifierLabel.CaptionResourceString = GetToleranceModifier();

				UpdateToleranceBounds();
			}
		}

		[DefaultValue(ToleranceTypes.Percent)]
		public ToleranceTypes ToleranceType
		{
			get => toleranceType;
			set
			{
				toleranceType = value;
				ToleranceTypeLabel.CaptionResourceString = GetToleranceType();

				UpdateToleranceBounds();
			}
		}

		void UpdateToleranceBounds()
		{
			if (toleranceType == ToleranceTypes.Days)
			{
				ToleranceCalcEdit.Decimals = 0;
				ToleranceCalcEdit.MaxValue = 99.999m;
			}
			else
			{
				ToleranceCalcEdit.Decimals = 3;

				if (toleranceModifier == ToleranceModifiers.Negative)
				{
					ToleranceCalcEdit.MaxValue = 99.999m;
				}
				else
				{
					ToleranceCalcEdit.MaxValue = 999.999m;
				}
			}
		}

		ResourceStringData GetToleranceModifier()
		{
			switch (ToleranceModifier)
			{
				case ToleranceModifiers.Positive:
					return Res.GetData("OrdersUserControl|18870ab4-c2f8-4a85-b8e4-5cd9e21bf081", "+");
				case ToleranceModifiers.Negative:
					return Res.GetData("OrdersUserControl|d162c343-70c4-4e68-8cb2-75195cbe4966", "-");
				case ToleranceModifiers.None:
				default:
					return null;
			}
		}

		ResourceStringData GetToleranceType()
			=> ToleranceType == ToleranceTypes.Percent
				? Res.GetData("OrdersUserControl|03aa363d-4beb-454a-8540-a06bd7ea143a", "%")
				: Res.GetData("OrdersUserControl|081af827-498d-4e79-8d63-12606d124c38", "Days");

		ToleranceModifiers toleranceModifier;
		ToleranceTypes toleranceType;
	}

	public enum ToleranceModifiers
	{
		None,
		Negative,
		Positive
	}

	public enum ToleranceTypes
	{
		Percent,
		Days
	}
}
