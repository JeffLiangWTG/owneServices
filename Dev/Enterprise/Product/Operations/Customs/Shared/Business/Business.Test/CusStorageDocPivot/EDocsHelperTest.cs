using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class EDocsHelperTest : TestCaseWithFactory
	{
		public void TestGetEDocCollections()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");

			var res = EDocsHelper.GetEDocCollections(declaration).ToArray();
			AssertEquals(1, res.Length);
			AssertEquals(1, res[0].Count);
			AssertEquals(eDoc1, res[0].GetFromUniqueKey(eDoc1.UniqueKey.ToGuid()));
		}
	}
}
