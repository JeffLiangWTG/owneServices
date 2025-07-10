using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ModuleTariffFilter))]
	class ModuleTariffFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFormatedText()
		{
			var filter = new ModuleTariffFilter();
			filter.Property = "3.1..";
			AssertEquals("31", filter.Property);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ModuleTariffFilter();
		}
	}
}
