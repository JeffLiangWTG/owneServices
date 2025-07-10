using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusEntryHeaderChargesValidationTest : BusinessObjectValidationTestCase
	{
		[ExpectNoExceptions]
		public void TestEntryHeaderCharges()
		{
			CusEntryHeaderCharges parent = Factory.New<CusEntryHeaderCharges>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Validation.EntryHeaderCharges));
		}
	}
}
