using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDAdHocServiceCollection))]
	public class CYDAdHocServiceCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDAdHocServiceCollection>
	{
		#region Implementation

		protected override CYDAdHocServiceCollection GetCollectionToTest()
		{
			return new CYDAdHocServiceCollection(Factory);
		}

		#endregion
	}
}
