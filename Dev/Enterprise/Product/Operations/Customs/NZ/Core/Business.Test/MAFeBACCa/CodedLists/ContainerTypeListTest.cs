using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.Testing
{
	class ContainerTypeListTest : CodeDescriptionEnumListTestCase
	{
		protected override CodeDescriptionPairList GetNewList()
		{
			return new ContainerTypeList();
		}
	}
}
