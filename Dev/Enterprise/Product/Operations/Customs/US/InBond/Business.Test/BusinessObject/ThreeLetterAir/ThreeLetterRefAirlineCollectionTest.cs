using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(ThreeLetterRefAirlineCollection))]
	sealed class ThreeLetterRefAirlineCollectionTest : ActiveBusinessObjectCollectionTestCase<ThreeLetterRefAirlineCollection>
	{
		public void TestCollectionHasCorrectModuleID()
		{
			var attrs = System.Attribute.GetCustomAttributes(typeof(ThreeLetterRefAirlineCollection));
			Assert("Should have ThreeLetterRefAirline module attribute", attrs.Any(attr => attr is ModuleIDAttribute moduleAttr && moduleAttr.ModuleId.Equals(ModuleId.ThreeLetterRefAirline)));
		}
	}
}
