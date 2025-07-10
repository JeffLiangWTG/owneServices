using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class ZAProvisionalPaymentTypes :
		Integration.Customs.ZA.IProvisionalPaymentTypeProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var factory = new BusinessObjectFactory();
			return factory.GetFullProvisionalPaymentTypeList();
		}
	}
}
