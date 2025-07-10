using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.LandedCosting.Business;

namespace Enterprise.LandedCosting.GUI
{
	public class LandedLineCostItemAmountPropertyDescriptor : KPropertyDescriptor
	{
		public LandedLineCostItemAmountPropertyDescriptor(string costType) : base(null, costType, Array.Empty<Attribute>())
		{
			this.costType = costType;
		}
		readonly string costType;

		public override Type PropertyType
		{
			get { return typeof(ZDecimal); }
		}

		protected override object GetValueCore(object component)
		{
			var landedCostHistory = (LandedCostHistory)component;
			return (landedCostHistory?.GetLineValue(costType)).GetValueOrDefault();
		}

		protected override void SetValueCore(object component, object value)
		{
			throw new NotSupportedException("LandedLineCostItemAmountPropertyDescriptor is read only");
		}

		public override bool IsReadOnly
		{
			get { return true; }
		}
	}
}
