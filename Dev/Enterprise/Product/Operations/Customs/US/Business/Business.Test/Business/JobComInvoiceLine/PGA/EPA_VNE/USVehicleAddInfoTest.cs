using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USVehicleAddInfo))]
	public class USVehicleAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var vehicle = Factory.New<Vehicle>();
			var addInfo = new USVehicleAddInfo(vehicle.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
