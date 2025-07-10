using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public readonly struct DetailLine
	{
		public readonly ZString Value;
		public readonly ZBool IsText;

		public DetailLine(ZString value) : this(value, ZBool.False)
		{
		}

		public DetailLine(ZString value, ZBool isText)
		{
			Value = value;
			IsText = isText;
		}

		public override bool Equals(object obj)
		{
			return obj is DetailLine identifier
				&& EqualityComparer<ZString>.Default.Equals(Value, identifier.Value)
				&& EqualityComparer<ZBool>.Default.Equals(IsText, identifier.IsText);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = -210812945;
				hashCode = hashCode * -152113429 + Value.GetHashCode();
				hashCode = hashCode * -152113429 + IsText.GetHashCode();
				return hashCode;
			}
		}
	}
}
