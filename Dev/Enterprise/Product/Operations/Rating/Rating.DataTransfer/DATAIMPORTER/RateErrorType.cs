using System;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.DataTransfer
{
	[Serializable]
	public class RateErrorType : ErrorType
	{
		internal RateErrorType(string name, string message)
			: base(name, message)
		{
		}

		internal RateErrorType(string message)
			: this(message, message)
		{
		}

		public static RateErrorType UnmatchOrgAddress { get { return new RateErrorType("UnmatchOrgAddress", ""); } }
		public static RateErrorType InvalidOrgAddressType { get { return new RateErrorType("InvalidOrgAddressType", ""); } }
		public static RateErrorType InvalidChargeCode { get { return new RateErrorType("InvalidChargeCode", Res.GetString("829019e2-6015-4f0b-b0ca-3d42adff7953", "Invalid Charge Code")); } }

		public static RateErrorType UnknownCartageZone { get { return new RateErrorType("UnknownCartageZone", Res.GetString("51b4a356-0961-43da-8489-222b31862c29", "Transport Zone Set Does Not Exist")); } }
	}
}
