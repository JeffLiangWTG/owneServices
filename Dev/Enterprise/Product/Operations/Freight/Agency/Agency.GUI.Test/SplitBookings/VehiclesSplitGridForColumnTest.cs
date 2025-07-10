using Enterprise.Core.Forms;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class VehiclesSplitGridForColumnTest : VehiclesSplitGrid, ISplitGridForColumnTest
	{
		public ZGridColumnInfo GetColumnControlForTest(string columnName)
		{
			return GetColumnControl(columnName);
		}
	}
}
