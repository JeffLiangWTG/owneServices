using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class OfficeOfReceiptListDescriptionPairProvider : Integration.Customs.TW.ITWOfficeOfReceiptListDescriptionPairProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var factory = new BusinessObjectFactory();
			return factory.GetCachedValue("TWOfficeOfReceiptList", () => TWRefCusCodeListTypes.GetCustomsOfficeList(factory));
		}
	}
}
