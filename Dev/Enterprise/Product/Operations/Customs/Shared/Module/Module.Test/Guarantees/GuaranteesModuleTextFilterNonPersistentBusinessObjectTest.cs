using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(GuaranteesModuleTextFilter))]
	sealed class GuaranteesModuleTextFilterNonPersistentBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var moduleFilter = new GuaranteesModuleTextFilter("Reference", new GuaranteesFilterStripBusinessObject());
			AssertNotNull(moduleFilter);
		}

		protected override BusinessObject GetNewBusinessObject() => new GuaranteesModuleTextFilter("Reference", new GuaranteesFilterStripBusinessObject());
	}
}
