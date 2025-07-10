using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static System.FormattableString;

namespace Enterprise.TransportConsignment.Business
{
	[Immutable]
	public struct PackTypeCount
	{
		public PackTypeCount(ZString packType, ZInt quantity)
		{
			this.packType = packType;
			this.quantity = quantity;
		}

		readonly ZString packType;
		readonly ZInt quantity;

		#region Properties

		public ZString PackType
		{
			get { return packType; }
		}

		public ZInt Quantity
		{
			get { return quantity; }
		}

		#endregion

		#region Equals

		public override bool Equals(object obj)
		{
			return obj is PackTypeCount ? (PackTypeCount)obj == this : base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return PackType.GetHashCode() ^ Quantity.GetHashCode();
		}

		#endregion

		#region Operators

		public static bool operator ==(PackTypeCount x, PackTypeCount y)
		{
			return x.PackType == y.PackType && x.Quantity == y.Quantity;
		}

		public static bool operator !=(PackTypeCount x, PackTypeCount y)
		{
			return !(x == y);
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			return Invariant($"{Quantity}x {PackType}"); // String is empty or contains only symbols.
		}

		#endregion
	}
}
