using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Yard.Testing
{
	[TestedType(typeof(CYDYardStorageFreeDaysCollection))]
	public class CYDYardStorageFreeDaysCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDYardStorageFreeDaysCollection>
	{
		#region Implementation

		protected override CYDYardStorageFreeDaysCollection GetCollectionToTest()
		{
			return new CYDYardStorageFreeDaysCollection(Factory.New<OrgHeader>());
		}

		#endregion
	}
}
