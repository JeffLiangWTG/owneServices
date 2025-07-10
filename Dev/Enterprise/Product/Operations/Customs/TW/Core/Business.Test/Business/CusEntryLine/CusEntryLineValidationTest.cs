using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		[ExpectNoExceptions]
		public void TestEntryLine()
		{
			CusEntryLine parent = Factory.New<CusEntryLine>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Validation.EntryLine));
		}
	}
}
