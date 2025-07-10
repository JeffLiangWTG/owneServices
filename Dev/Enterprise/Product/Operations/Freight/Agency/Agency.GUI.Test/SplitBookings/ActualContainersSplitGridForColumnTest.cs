using Enterprise.Core.Forms;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class ActualContainersSplitGridForColumnTest : ActualContainersSplitGrid, ISplitGridForColumnTest
	{
		public ZGridColumnInfo GetColumnControlForTest(string columnName)
		{
			return GetColumnControl(columnName);
		}
	}
}
