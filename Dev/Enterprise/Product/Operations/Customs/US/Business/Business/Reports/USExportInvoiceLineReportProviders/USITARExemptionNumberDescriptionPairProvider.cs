using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.Business.Reports
{
	sealed class USITARExemptionNumberDescriptionPairProvider : Integration.Customs.US.IUSITARExemptionNumberDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList() => CusITARENCodeConstants.GetITARExemptionNumberCodeList(Factory);
	}
}
