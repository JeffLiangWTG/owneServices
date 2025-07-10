using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.Testing
{
	class DocumentTypeListTest : CodeDescriptionEnumListTestCase
	{
		protected override CodeDescriptionPairList GetNewList()
		{
			return new DocumentTypeList();
		}
	}
}
