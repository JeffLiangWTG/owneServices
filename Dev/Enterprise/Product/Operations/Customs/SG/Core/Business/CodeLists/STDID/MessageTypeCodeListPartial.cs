using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class MessageTypeCodeList : Common.SG.SGJobMessageTypeList
	{
		public static CodeDescriptionPairList GetListWithAdvanceShippingNotice(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("SGInvoiceHeaderMessageTypes", delegate
			{
				var result = new MessageTypeCodeList();
				result.AddAdvanceShippingNotice();
				return result;
			});
		}
	}
}
