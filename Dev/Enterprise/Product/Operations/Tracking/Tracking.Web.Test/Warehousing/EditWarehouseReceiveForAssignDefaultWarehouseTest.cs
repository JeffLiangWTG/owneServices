using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class EditWarehouseReceiveForAssignDefaultWarehouseTest : EditWarehouseReceive
	{
		public BusinessObject GetNewDataSourceForTest()
		{
			return GetNewDataSource();
		}

		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}
	}
}
