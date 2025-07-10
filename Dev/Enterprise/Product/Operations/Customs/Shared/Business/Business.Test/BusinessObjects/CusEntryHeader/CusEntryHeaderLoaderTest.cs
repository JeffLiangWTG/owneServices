using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryHeader.Loader))]
	sealed class CusEntryHeaderLoaderTest : LoaderTestCase
	{
		public void TestFindByEntryNumber()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = "XXX";
			entryHeader.EntryNumber = "E123";

			AssertNull(new CusEntryHeader.Loader(Factory).FindByEntryNumberAndCurrentCompany("E124"));
			AssertEquals(entryHeader, new CusEntryHeader.Loader(Factory).FindByEntryNumberAndCurrentCompany("E123"));

			GlbBranch branch = Factory.New<GlbBranch>();
			declaration.JE_GB = branch.PK;
			AssertNull(new CusEntryHeader.Loader(Factory).FindByEntryNumberAndCurrentCompany("E123"));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusEntryHeader.Loader(Factory);
		}
	}
}
