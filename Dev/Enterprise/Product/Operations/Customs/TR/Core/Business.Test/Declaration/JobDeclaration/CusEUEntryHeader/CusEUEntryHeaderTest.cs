using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEUEntryHeader))]
	class CusEUEntryHeaderTest : EU.Business.Declaration.Testing.CusEUEntryHeaderAbstractTest<CusEUEntryHeader>
	{
		public void TestICusEUEntryHeaderIsCorrectlySetup()
		{
			var bizObj = (BusinessObject)Factory.New<Integration.Customs.TR.ICusEUEntryHeader>();
			AssertType<CusEUEntryHeader>(bizObj);
			AssertType<CusEUEntryHeader>(Factory.Load(bizObj.TablePrefix, bizObj.PK));
		}

		public void TestTypeSafe()
		{
			var entry = Factory.New<CusEntryHeader>();

			CombineAssertions("TR.CusEntryHeader AddInfo Types", () =>
			{
				AssertType<CusEUEntryHeader>("TR CusEUEntryHeader", entry.AddInfoChild);
				AssertType<EU.Business.Declaration.CusEUEntryHeaderLookups>("EU CusEUEntryHeaderLookups (currently no TR-specific)", entry.AddInfoChildLookups);
				AssertType<EU.Business.Declaration.CusEUEntryHeaderValidation>("EU CusEUEntryHeaderValidation (currently no TR-specific)", entry.AddInfoChildValidation);
			});
		}
	}
}
