using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public static class JobMessageTypeList
	{
		public class Codes : Common.US.USJobMessageTypeList.Codes { }
		public class MoreCodes : Common.US.USJobMessageTypeList.MoreCodes { }

		public static bool IsImport(string code)
		{
			return code == Codes.FTZ
				|| code == Codes.Import
				|| code == Codes.ImportByExternalBroker
				|| code == Codes.Miscellaneous;
		}

		public static CodeDescriptionPairList GetListWithAdvanceShippingNotice(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("USInvoiceHeaderMessageTypes", delegate
			{
				var result = new Common.US.USJobMessageTypeList();
				result.RemoveCode(Codes.Recon);
				result.RemoveCode(Codes.Drawback);
				result.AddAdvanceShippingNotice();
				return result;
			});
		}
	}
}
