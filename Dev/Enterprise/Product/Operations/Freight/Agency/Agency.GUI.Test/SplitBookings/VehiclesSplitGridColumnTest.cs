namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class VehiclesSplitGridColumnTest : SplitGridColumnTest
	{
		protected override ISplitGridForColumnTest GetSplitGridForTest()
		{
			return new VehiclesSplitGridForColumnTest();
		}
	}
}
