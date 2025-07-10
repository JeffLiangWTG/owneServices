using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class RefCusPackListProvider : MasterFiles.Business.RefCusPackListProvider
	{
		public override CodeDescriptionPairList GetDeclarationPackTypeList(BusinessObjectFactory factory)
		{
			return factory.GetNull<BaseJobDeclaration>().Lookups.PackingUnitTypesList;
		}
	}
}
