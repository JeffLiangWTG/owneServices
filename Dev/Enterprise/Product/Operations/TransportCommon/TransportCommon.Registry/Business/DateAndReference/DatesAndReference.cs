using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.TransportCommon.Registry
{
	public struct DatesAndReference
	{
		public DatesAndReference(ZDateTime estimated, ZDateTime actual, ZDateTime reqFrom, ZDateTime reqTo, ZString reference, ZDateTime slotTime, ZString slotReference)
		{
			Estimated = estimated;
			Actual = actual;
			ReqFrom = reqFrom;
			ReqTo = reqTo;
			Reference = reference;
			SlotTime = slotTime;
			SlotReference = slotReference;
			isNotEmpty = !(Estimated.IsEmpty && Actual.IsEmpty && ReqFrom.IsEmpty && ReqTo.IsEmpty && Reference.IsEmpty);
		}

		#region Empty

		public static DatesAndReference Empty
		{
			get { return new DatesAndReference(); }
		}

		public ZBool IsEmpty
		{
			get { return !isNotEmpty; }
		}

		readonly bool isNotEmpty;

		#endregion

		#region Any

		public static ZString Any
		{
			get { return "ANY"; }
		}

		#endregion

		#region Equals

		public override bool Equals(object obj)
		{
			return (obj is DatesAndReference && (DatesAndReference)obj == this);
		}

		#endregion

		#region Operator Overloads

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(DatesAndReference lhs, DatesAndReference rhs)
		{
			return
				lhs.Estimated == rhs.Estimated &&
				lhs.Actual == rhs.Actual &&
				lhs.ReqFrom == rhs.ReqFrom &&
				lhs.ReqTo == rhs.ReqTo &&
				lhs.Reference == rhs.Reference;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(DatesAndReference lhs, DatesAndReference rhs)
		{
			return !(lhs == rhs);
		}

		public override int GetHashCode()
		{
			return Estimated.GetHashCode() ^ Actual.GetHashCode() ^ ReqFrom.GetHashCode() ^ ReqTo.GetHashCode() ^ Reference.GetHashCode();
		}

		#endregion

		public readonly ZDateTime Estimated;
		public readonly ZDateTime Actual;
		public readonly ZDateTime ReqFrom;
		public readonly ZDateTime ReqTo;
		public readonly ZString Reference;
		public readonly ZDateTime SlotTime;
		public readonly ZString SlotReference;
	}
}
