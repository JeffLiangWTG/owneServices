using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PermitNumbersToSelectFromForPrintingCollection))]
	sealed class PermitNumbersToSelectFromForPrintingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PermitNumbersToSelectFromForPrintingCollection>
	{
		public void TestAllowNewCore()
		{
			var collection = GetCollectionToTest();
			Assert(!collection.AllowNew);
		}

		public void TestSelectedPermitNumbersArray()
		{
			var collection = GetCollectionToTest();
			var permitNumber1 = new PermitNumberToSelectFromForPrinting("1");
			var permitNumber2 = new PermitNumberToSelectFromForPrinting("2");
			var permitNumber3 = new PermitNumberToSelectFromForPrinting("3");
			permitNumber1.NeedPrint = true;
			permitNumber2.NeedPrint = false;
			permitNumber3.NeedPrint = true;
			collection.Add(permitNumber1);
			collection.Add(permitNumber2);
			collection.Add(permitNumber3);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1", "3" }, collection.SelectedPermitNumbers);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PermitNumberToSelectFromForPrinting("1");
		}
		protected override PermitNumbersToSelectFromForPrintingCollection GetCollectionToTest()
		{
			return new PermitNumbersToSelectFromForPrintingCollection();
		}
	}
}
