
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	sealed class USAPHISProgramTypeDescriptionPairProvider : CodeDescriptionPairList,
		Integration.Customs.US.IUSAPHISProgramTypeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return (CodeDescriptionPairList)US.Business.APHISProgramCodeList.GetActiveList(new CargoWise.EntityFramework.BusinessObjectFactory());
		}
	}
}
