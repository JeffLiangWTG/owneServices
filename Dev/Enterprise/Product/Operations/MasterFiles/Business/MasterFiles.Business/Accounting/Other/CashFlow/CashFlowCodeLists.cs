using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class CashFlowCodeLists
	{
		public static class Codes
		{
			public const string CSH = "CSH";
			public const string EXX = "EXX";
			public const string F01 = "F01";
			public const string F02 = "F02";
			public const string F03 = "F03";
			public const string F04 = "F04";
			public const string F05 = "F05";
			public const string F06 = "F06";
			public const string F07 = "F07";
			public const string F08 = "F08";
			public const string I01 = "I01";
			public const string I02 = "I02";
			public const string I03 = "I03";
			public const string I04 = "I04";
			public const string I05 = "I05";
			public const string NON = "NON";
			public const string O01 = "O01";
			public const string O02 = "O02";
			public const string O03 = "O03";
			public const string O04 = "O04";
			public const string O05 = "O05";
			public const string O06 = "O06";
			public const string XXX = "XXX";
		}

		public static class ReservedReportCodes
		{
			public const string ZZZ = "ZZZ";
		}

		public static CodeDescriptionPairList CashFlowTypeList
		{
			get
			{
				if (fCashFlowTypeList == null)
				{
					fCashFlowTypeList = new CodeDescriptionPairList();
					fCashFlowTypeList.AddPair(Codes.CSH, ResString.GetMultilingualString("3E7B03E0-BDC0-4CDA-B1EA-21AC988F3FD5", "Cash or Cash Equivalent"));
					fCashFlowTypeList.AddPair(Codes.EXX, ResString.GetMultilingualString("6FEA9BBA-1CE8-4AAF-8425-CDCC3CC12046", "Effects of Exchange Rate Change"));
					fCashFlowTypeList.AddPair(Codes.F01, ResString.GetMultilingualString("5BC582EF-5A6C-4101-837F-5FAAB0C109C3", "Interests Paid"));
					fCashFlowTypeList.AddPair(Codes.F02, ResString.GetMultilingualString("A57AAA21-99E0-4C44-891B-E5365FB484BF", "Dividends Paid"));
					fCashFlowTypeList.AddPair(Codes.F03, ResString.GetMultilingualString("55490B89-D716-4BF0-BB41-0B868A3D3FE5", "Proceeds from Bank Borrowings"));
					fCashFlowTypeList.AddPair(Codes.F04, ResString.GetMultilingualString("8C74B863-BEE8-4972-87C8-FE2A37FAE334", "Repayments of Bank Borrowings"));
					fCashFlowTypeList.AddPair(Codes.F05, ResString.GetMultilingualString("69753E3B-5BE0-4809-B1B9-B115B755F0B8", "Proceeds from Issuance of New Shares"));
					fCashFlowTypeList.AddPair(Codes.F06, ResString.GetMultilingualString("55669F81-0BC8-4987-AD2A-3975DB123946", "Loans from Intercompany"));
					fCashFlowTypeList.AddPair(Codes.F07, ResString.GetMultilingualString("0D4FF6BD-B3E7-4511-814A-359A11FDA3A3", "Repayments of Intercompany Loans"));
					fCashFlowTypeList.AddPair(Codes.F08, ResString.GetMultilingualString("226FBD6B-3779-457E-A3DD-AC484BF39E26", "Repayments of Finance Leases"));
					fCashFlowTypeList.AddPair(Codes.I01, ResString.GetMultilingualString("CF1EE1E6-AD23-4E04-9EC1-09182A777FF2", "Dividends Received"));
					fCashFlowTypeList.AddPair(Codes.I02, ResString.GetMultilingualString("8010243B-B678-4EF9-AE4B-CF616E3DD1C2", "Proceeds from Disposal of Non-Current Assets"));
					fCashFlowTypeList.AddPair(Codes.I03, ResString.GetMultilingualString("47150980-AAC5-4654-9FBA-701DB2DE8B74", "Purchases of Non-Current Assets"));
					fCashFlowTypeList.AddPair(Codes.I04, ResString.GetMultilingualString("D62F4801-1805-4ED4-9628-C67E80C337EF", "Proceeds from Disposal of Financial Assets"));
					fCashFlowTypeList.AddPair(Codes.I05, ResString.GetMultilingualString("782955A7-7B43-4EBB-9D40-680EA6F25DB4", "Acquisitions of Financial Assets"));
					fCashFlowTypeList.AddPair(Codes.NON, ResString.GetMultilingualString("0CD6EB93-6784-4F94-B2DA-58A573B4FB52", "Non Cash"));
					fCashFlowTypeList.AddPair(Codes.O01, ResString.GetMultilingualString("01CD7833-1BC7-4F2D-9E09-D62A1993EFF7", "Receipts From Customers"));
					fCashFlowTypeList.AddPair(Codes.O02, ResString.GetMultilingualString("28705F47-6F14-4BC5-A842-EB51A6A91FEE", "Receipts From Other Operating Activities"));
					fCashFlowTypeList.AddPair(Codes.O03, ResString.GetMultilingualString("6563C53B-E08D-43AD-919C-C34FB12C71B4", "Payments to Suppliers"));
					fCashFlowTypeList.AddPair(Codes.O04, ResString.GetMultilingualString("99197530-5F72-405E-B34C-36A000B8270B", "Payments to Employees"));
					fCashFlowTypeList.AddPair(Codes.O05, ResString.GetMultilingualString("2CADA08B-D476-400E-97F1-09845367F892", "Payment of Taxes"));
					fCashFlowTypeList.AddPair(Codes.O06, ResString.GetMultilingualString("D7EE7F2F-6702-4CCB-9F2E-FA754A387EC8", "Payments for Other Operating Activities"));
					fCashFlowTypeList.AddPair(Codes.XXX, ResString.GetMultilingualString("69D5F779-0586-41DD-84D2-23E8B0EF9DD0", "Undefined"));
				}
				return fCashFlowTypeList;
			}
		}

		static CodeDescriptionPairList fCashFlowTypeList;
	}
}
