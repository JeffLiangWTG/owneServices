using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgHandlingUnitCollection))]
	public class PkgHandlingUnitCollectionTest : ActiveBusinessObjectCollectionTestCase<PkgHandlingUnitCollection>
	{
		#region Implementation

		protected override PkgHandlingUnitCollection GetCollectionToTest()
		{
			return new PkgHandlingUnitCollection(Factory);
		}

		#endregion
	}
}
