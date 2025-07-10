using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Organisation.Registry
{
	public static class CreditControlledDocumentsCheckConfigurationInvoiceTypes
	{
		public static CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				CodeDescriptionPairList invoiceTypeList = new CodeDescriptionPairList();
				invoiceTypeList.Insert(0, All);
				invoiceTypeList.Insert(0, NDB);
				invoiceTypeList.Insert(0, DSB);
				return invoiceTypeList;
			}
		}

		public static CodeDescriptionPair All
		{
			get { return new CodeDescriptionPair("ALL", Res.GetString("8a86c2a4-50bf-4e2b-a392-dd3c62091107", "Any Invoice Type")); }
		}

		public static CodeDescriptionPair DSB
		{
			get { return new CodeDescriptionPair("DSB", Res.GetString("a6a575fb-45ef-4f85-9c6b-f2a23a6d00c0", "Any Disbursement Type")); }
		}

		public static CodeDescriptionPair NDB
		{
			get { return new CodeDescriptionPair("NDB", Res.GetString("6e2156be-6e20-44fd-aa86-5b79890e971a", "Not a Disbursement Type")); }
		}
	}
}
