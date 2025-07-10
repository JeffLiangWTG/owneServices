using Enterprise.Core.Forms;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class BookedContainersSplitGridForColumnTest : BookedContainersSplitGrid, ISplitGridForColumnTest
	{
		public ZGridColumnInfo GetColumnControlForTest(string columnName)
		{
			return GetColumnControl(columnName);
		}
	}
}
