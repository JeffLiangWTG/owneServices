using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReleaseAdviceCollection))]
	public class CYDReleaseAdviceCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDReleaseAdviceCollection>
	{
		#region Implementation

		protected override CYDReleaseAdviceCollection GetCollectionToTest()
		{
			return new CYDReleaseAdviceCollection(Factory);
		}

		#endregion
	}
}
