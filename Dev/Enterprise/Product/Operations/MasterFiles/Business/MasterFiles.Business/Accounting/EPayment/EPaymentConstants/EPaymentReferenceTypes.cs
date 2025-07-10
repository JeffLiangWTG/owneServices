using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class EPaymentReferenceTypes
	{
		public const string FreeText = "TXT";
		public const string InvoiceNumbers = "INV";
		public const string PaymentReferenceNum = "PRN";

		public static CodeDescriptionPairList CodesList
		{
			get
			{
				var codes = new CodeDescriptionPairList();

				codes.Add(new CodeDescriptionPair(FreeText, ResString.GetMultilingualString("29AE957B-B15A-4944-A9A3-18A6492BCEDA", "Free Text")));
				codes.Add(new CodeDescriptionPair(InvoiceNumbers, ResString.GetMultilingualString("6E837BDE-40D6-4DB7-8C31-0C144988E2D2", "Invoice Numbers")));
				codes.Add(new CodeDescriptionPair(PaymentReferenceNum, ResString.GetMultilingualString("F9755B3F-DEA9-49BB-AEC0-C985106F7FA2", "Payment Reference Number")));

				return codes;
			}
		}
	}
}
