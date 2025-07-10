namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class BookedContainersSplitGridColumnTest : SplitGridColumnTest
	{
		protected override ISplitGridForColumnTest GetSplitGridForTest()
		{
			return new BookedContainersSplitGridForColumnTest();
		}
	}
}
