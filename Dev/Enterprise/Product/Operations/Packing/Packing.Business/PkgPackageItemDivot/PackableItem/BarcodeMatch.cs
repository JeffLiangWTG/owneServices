using CargoWise.Common;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Packing.Business
{
	[Immutable]
	public class BarcodeMatch
	{
		public static readonly BarcodeMatch Yes = new BarcodeMatch(true);
		public static readonly BarcodeMatch No = new BarcodeMatch(false);

		public BarcodeMatch(bool isMatch)
		{
			IsMatch = isMatch;
		}

		/// <summary>
		/// Only use this constructor if you want Packing to create a new Package of the specified type.
		/// </summary>
		public BarcodeMatch(bool isMatch, ZString packTypeToPackInto, ZDecimal qtyToPack)
			: this(isMatch)
		{
			Argument.NotNullOrEmpty(packTypeToPackInto, "packTypeToPackInto");

			PackTypeToPackInto = packTypeToPackInto;
			this.qtyToPack = qtyToPack > 0 ? qtyToPack : 1;
		}

		public ZDecimal QtyToPack
		{
			get { return PackTypeToPackInto.IsEmpty ? 1 : qtyToPack; }
		}

		public bool IsTUN
		{
			get { return !PackTypeToPackInto.IsEmpty; }
		}

		public readonly bool IsMatch;
		public readonly ZString PackTypeToPackInto;

		readonly ZDecimal qtyToPack;
	}
}
