using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.Testing
{
	class ConsignmentTypeListTest : CodeDescriptionEnumListTestCase
	{
		protected override CodeDescriptionPairList GetNewList()
		{
			return new ConsignmentTypeList();
		}
	}
}
