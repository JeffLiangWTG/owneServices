namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class TopLevelPacksSplitGridColumnTest : SplitGridColumnTest
	{
		protected override ISplitGridForColumnTest GetSplitGridForTest()
		{
			return new TopLevelPacksSplitGridForColumnTest();
		}
	}
}
