using CargoWise.EntityFramework;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(YardUnitMovement))]
	sealed class YardUnitMovementTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsSavedByFactory()
		{
			var yardUnitMovement = GetNewBusinessObject();
			AssertEquals(false, yardUnitMovement.IsSavedByFactory);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(GetExpectedBusinessObjectType());
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("GateBooking is not supposed to be saved by factory by default", true);
		}

		#endregion
	}
}
