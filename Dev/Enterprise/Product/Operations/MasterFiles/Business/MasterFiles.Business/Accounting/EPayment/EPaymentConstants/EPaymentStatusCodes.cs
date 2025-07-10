using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class EPaymentStatusCodes
	{
		public static class Quote
		{
			public const string Queued = "QUE";
			public const string Requested = "REQ";
			public const string Failed = "RQF";
			public const string Error = "ERR";
			public const string Received = "RCV";
			public const string Accepted = "ACP";
			public const string Discarded = "DCD";
			public const string Expired = "EXP";

			public static string[] ActiveStatusCodes => new string[4] { Queued, Requested, Received, Accepted };

			public static CodeDescriptionPairList CodesList
			{
				get
				{
					var codes = new CodeDescriptionPairList();
					codes.AddPair(Queued, ResString.GetMultilingualString("E70EC5FD-BCF5-4B5E-AFDA-FAFF88659A69", "Quote Request has been queued for sending"));
					codes.AddPair(Requested, ResString.GetMultilingualString("924DDDD0-32F5-4521-8119-0C4BAD0CC343", "Quote Request has been sent to provider"));
					codes.AddPair(Failed, ResString.GetMultilingualString("FFF39F7A-2935-47DD-AA5C-2E5802ED8F4B", "Quote Request has failed to send"));
					codes.AddPair(Error, ResString.GetMultilingualString("D3CE0FBC-DE85-4236-8E21-6250F847AF31", "Quote Request has encountered an error"));
					codes.AddPair(Received, ResString.GetMultilingualString("961CB715-7F99-44EA-80B4-4B62DE1D5C56", "Quote has been received from the provider"));
					codes.AddPair(Accepted, ResString.GetMultilingualString("D87DFED7-CCFA-47D7-B5D7-EAC5A9C8F392", "Quote has been accepted"));
					codes.AddPair(Discarded, ResString.GetMultilingualString("4AD9E9F0-6C09-45EE-81A1-C64FAED9AC02", "Quote has been discarded"));
					codes.AddPair(Expired, ResString.GetMultilingualString("F4B400A3-EAA3-4517-BEA8-87DB44F5ED85", "Quote has expired"));

					return codes;
				}
			}

			public static CodeDescriptionPairList CodeListWithShortDescription
			{
				get
				{
					var codes = new CodeDescriptionPairList();
					codes.AddPair(Queued, ResString.GetMultilingualString("D7BB09D8-3466-4B01-8769-CD4140AFB275", "Queued"));
					codes.AddPair(Requested, ResString.GetMultilingualString("59FF6337-2A71-4892-80AC-D617C0A16544", "Requested"));
					codes.AddPair(Failed, ResString.GetMultilingualString("2F402B2E-32CA-4DFB-AAA0-49E209E34FFD", "Request Failed"));
					codes.AddPair(Error, ResString.GetMultilingualString("5CC895B2-6DD3-4157-93ED-E0D394C92742", "Error"));
					codes.AddPair(Received, ResString.GetMultilingualString("C0FF53DD-3BA8-4136-9926-42FDFF83D7C7", "Received"));
					codes.AddPair(Accepted, ResString.GetMultilingualString("0D705A6C-BC28-44EB-8B8A-DB6F2D41390E", "Accepted"));
					codes.AddPair(Discarded, ResString.GetMultilingualString("29037D8F-1BD3-4A53-997B-28E511C141AE", "Discarded"));
					codes.AddPair(Expired, ResString.GetMultilingualString("AD89562E-1C2C-427C-97AE-14C4EEE1E404", "Expired"));

					return codes;
				}
			}
		}

		public static class Deal
		{
			public const string Queued = "QUE";
			public const string Pending = "PEN";
			public const string ReadyToSend = "RDY";
			public const string Requested = "REQ";
			public const string Accepted = "ACP";
			public const string InProgress = "INP";
			public const string Paid = "PAI";
			public const string SubmissionFailed = "SMF";
			public const string Cancelled = "CAN";
			public const string Declined = "DEC";
			public const string Failed = "FAL";

			public static string[] InactiveStatusCodes => new[] { Cancelled, SubmissionFailed, Declined, Failed };
			public static string[] ActiveStatusCodes => new[] { Queued, Pending, ReadyToSend, Requested, Accepted, InProgress, Paid };
			public static string[] ProviderConfirmedStatusCodes => new[] { Accepted, InProgress, Paid };
			public static string[] StatusCodesAllowedToCancel => new[] { Accepted, InProgress };

			public static CodeDescriptionPairList CodesList
			{
				get
				{
					var codes = new CodeDescriptionPairList();
					codes.AddPair(Queued, ResString.GetMultilingualString("73070346-be62-4f4b-a35f-3ce7d302335d", "Payment has been Queued for sending"));
					codes.AddPair(Pending, ResString.GetMultilingualString("4aaef71f-5f36-455c-a1e9-76fabed62d11", "Request to check quote status has been sent to provider"));
					codes.AddPair(ReadyToSend, ResString.GetMultilingualString("122f501f-5768-44cc-8678-60c2e073eb0f", "Quote is valid and Payment is ready to be submitted to provider"));
					codes.AddPair(Requested, ResString.GetMultilingualString("f59ed02d-154d-4c17-ab5a-673ef582eff7", "Request to book the payment has been sent to provider"));
					codes.AddPair(Accepted, ResString.GetMultilingualString("74954fc6-ca88-4e72-963d-67d4e050de82", "Payment has been booked"));
					codes.AddPair(InProgress, ResString.GetMultilingualString("a4bd6d06-77e4-4b59-8439-9693caa9e08e", "Provider have received funds"));
					codes.AddPair(Paid, ResString.GetMultilingualString("1e8d7d84-6f0f-436f-a056-d40415edac7d", "Payment is processed"));
					codes.AddPair(SubmissionFailed, ResString.GetMultilingualString("c47e6b25-3f47-45be-8085-f7c1871f3dad", "Failed to send Payment request to provider"));
					codes.AddPair(Cancelled, ResString.GetMultilingualString("08d58346-6884-404a-875e-06a5766d0b47", "Payment canceled by user"));
					codes.AddPair(Declined, ResString.GetMultilingualString("ffca1e28-773f-4f50-a7c2-eea8f09e8f97", "Provider declined to process payment"));
					codes.AddPair(Failed, ResString.GetMultilingualString("5ba46a53-4344-4772-8e61-e23a885697fe", "Payment processing failed on provider side"));

					return codes;
				}
			}
		}

		public static class BeneficiaryRequest
		{
			public const string Queued = "QUE";
			public const string Requested = "REQ";
			public const string Received = "RCV";
			public const string Partial = "PAR";
			public const string Error = "ERR";
			public static CodeDescriptionPairList CodesList
			{
				get
				{
					var codes = new CodeDescriptionPairList();
					codes.AddPair(BeneficiaryRequest.Queued, ResString.GetMultilingualString("7303DD98-01F2-44BD-ABC9-E5AC1FE9ED34", "New request created"));
					codes.AddPair(BeneficiaryRequest.Requested, ResString.GetMultilingualString("E2F06AEE-BEB7-4674-963A-654DE2A345E8", "Outgoing EDI message has been generated"));
					codes.AddPair(BeneficiaryRequest.Received, ResString.GetMultilingualString("FE64E9CC-0128-4167-97CF-9DFB8BC96644", "Recipient list has been received in full"));
					codes.AddPair(BeneficiaryRequest.Partial, ResString.GetMultilingualString("7EC1431B-5835-4F26-B642-526665E36C7E", "Recipient list has been received partially"));
					codes.AddPair(BeneficiaryRequest.Error, ResString.GetMultilingualString("7EF6FF04-24A4-467F-90BD-91E62ED165EB", "Error"));
					return codes;
				}
			}
		}
	}
}
