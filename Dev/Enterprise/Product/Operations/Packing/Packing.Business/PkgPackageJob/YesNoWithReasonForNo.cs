using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public struct YesNoWithReasonForNo
	{
		YesNoWithReasonForNo(ZString reasonForNo)
		{
			ReasonForNotAllowed = reasonForNo;
		}

		public readonly ZString ReasonForNotAllowed;

		public static YesNoWithReasonForNo Yes
		{
			get { return new YesNoWithReasonForNo(""); }
		}

		public static YesNoWithReasonForNo No(ZString reasonForNotAllowed)
		{
			return new YesNoWithReasonForNo(Argument.NotNullOrEmpty(reasonForNotAllowed, "reasonForNotAllowed"));
		}

		#region Equals Override

		public static implicit operator bool(YesNoWithReasonForNo yesNo)
		{
			return yesNo.IsAllowed;
		}

		public static bool operator ==(YesNoWithReasonForNo lhs, YesNoWithReasonForNo rhs)
		{
			return lhs.ReasonForNotAllowed == rhs.ReasonForNotAllowed;
		}
		public static bool operator !=(YesNoWithReasonForNo lhs, YesNoWithReasonForNo rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			return obj is YesNoWithReasonForNo && ((YesNoWithReasonForNo)obj) == this;
		}

		public override int GetHashCode()
		{
			return ReasonForNotAllowed.GetHashCode();
		}

		bool IsAllowed
		{
			get { return ReasonForNotAllowed.IsEmpty; }
		}

		#endregion
	}
}
