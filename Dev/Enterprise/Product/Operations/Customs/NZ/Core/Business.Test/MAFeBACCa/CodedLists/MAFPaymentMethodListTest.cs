using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.Testing
{
	class MAFPaymentMethodListTest : CodeDescriptionPairListTestCase
	{
		protected override CodeDescriptionPairList GetNewList()
		{
			return new MAFPaymentMethodList();
		}
	}
}
