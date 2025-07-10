using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class RelatedContainersOfJobDeclarationFilter : ModuleGuidPivotFilter
	{
		public RelatedContainersOfJobDeclarationFilter(ZString description, GetList listDelegate)
			: base(description, ModuleIDs.Containers, CusContainerSchema.CO_JC, CusContainerSchema.CO_JE, listDelegate, typeof(BaseJobDeclaration), typeof(BaseCusContainer))
		{
			SetDefaultProperties();
		}

		void SetDefaultProperties()
		{
			MultilingualDescription = DefaultDescription;
		}

		static MultilingualString DefaultDescription => ResString.GetMultilingualString("0C91EDCE-F32A-45B5-825C-6E187E3F6E2E", "Related Containers");
		protected override FilterCategory DefaultCategory => FilterCategories.Other;
	}
}
