using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PermitNumberToSelectFromForPrinting))]
	sealed class PermitNumberToSelectFromForPrintingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPermitNumber()
		{
			var permitNumber = new PermitNumberToSelectFromForPrinting("1");
			AssertEquals("1", permitNumber.PermitNumber);
			Assert(permitNumber.NeedPrint);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PermitNumberToSelectFromForPrinting("1");
		}
	}
}
