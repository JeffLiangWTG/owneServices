using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(LandedCostingCustomsFee))]
	sealed class LandedCostingCustomsFeeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new LandedCostingCustomsFee(Factory);
	}
}
