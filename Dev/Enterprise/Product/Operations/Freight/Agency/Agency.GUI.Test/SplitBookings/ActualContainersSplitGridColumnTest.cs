namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class ActualContainersSplitGridColumnTest : SplitGridColumnTest
	{
		protected override ISplitGridForColumnTest GetSplitGridForTest()
		{
			return new ActualContainersSplitGridForColumnTest();
		}
	}
}
