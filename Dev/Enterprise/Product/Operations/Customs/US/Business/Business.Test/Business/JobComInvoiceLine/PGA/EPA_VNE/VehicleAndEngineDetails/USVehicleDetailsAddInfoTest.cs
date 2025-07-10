using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USVehicleDetailsAddInfo))]
	public class USVehicleDetailsAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestVehicleDetails()
		{
			var vehicle = Factory.New<VehicleDetails>();
			var addInfo = new USVehicleDetailsAddInfo(vehicle.B7_AddInfoDataInfo);
			AssertEquals(vehicle.PK, addInfo.VehicleDetails.PK);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var vehicle = Factory.New<VehicleDetails>();
			var addInfo = new USVehicleDetailsAddInfo(vehicle.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
