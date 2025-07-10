using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccEPaymentStaffTokenLookups : AutoAccEPaymentStaffTokenLookups
	{
		public AccEPaymentStaffTokenLookups(AutoAccEPaymentStaffToken parent) : base(parent)
		{
		}

		public static class StatusCodes
		{
			public const string NotAuthorised = "NAT";
			public const string Pending = "PND";
			public const string Authorised = "ATH";
			public const string Error = "ERR";
		}

		public virtual CodeDescriptionPairList StatusCodeList
		{
			get
			{
				var codes = new CodeDescriptionPairList();
				codes.AddPair(StatusCodes.NotAuthorised, ResString.GetMultilingualString("932957B6-A2FA-4CD6-B8E1-2338E8326498", "Unauthorized"));
				codes.AddPair(StatusCodes.Pending, ResString.GetMultilingualString("4893A3E5-F12F-4730-BD7A-5B767F398895", "Pending"));
				codes.AddPair(StatusCodes.Authorised, ResString.GetMultilingualString("B750F51C-2DAE-42C0-B906-8723A3973BD4", "Authorized"));
				codes.AddPair(StatusCodes.Error, ResString.GetMultilingualString("6B88166B-5B49-43FB-9C42-09893E8A301D", "Error"));
				return codes;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		public static class ScopeCodes
		{
			public const string Ofxrates = "ofxrates";
			public const string Payments = "payments";
			public const string Users = "users";
		}

		public virtual CodeDescriptionPairList ScopeList
		{
			get
			{
				var codes = new CodeDescriptionPairList();
				codes.AddPair(ScopeCodes.Ofxrates, ResString.GetMultilingualString("6F2269A5-9F3F-4F26-860A-38EB3BFC26EC", "OFX Rates Scope"));
				codes.AddPair(ScopeCodes.Payments, ResString.GetMultilingualString("A23877F1-E944-49A2-BFEB-B18C5C6815CB", "Payments Scope"));
				codes.AddPair(ScopeCodes.Users, ResString.GetMultilingualString("3872A24B-B0BB-4C6B-9BC4-B2B8AFC903B0", "Users Scope"));
				return codes;
			}
		}
	}
}
