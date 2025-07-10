using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	sealed class USAPHISProcessingCodeDescriptionPairProvider : CodeDescriptionPairList,
		Integration.Customs.US.IUSAPHISProcessingCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return (CodeDescriptionPairList)APHISGovernmentAgencyProcessingCodeList.GetListForProgram(new CargoWise.EntityFramework.BusinessObjectFactory(), string.Empty);
		}

		public ReadOnlyCodeDescriptionPairList GetDependenceCodeDescriptionPairList(string programType)
		{
			return (CodeDescriptionPairList)APHISGovernmentAgencyProcessingCodeList.GetListForProgram(new CargoWise.EntityFramework.BusinessObjectFactory(), programType);
		}
	}
}
