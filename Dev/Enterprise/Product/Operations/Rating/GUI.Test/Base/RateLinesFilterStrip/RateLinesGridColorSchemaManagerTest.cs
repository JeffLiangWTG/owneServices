using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI.Testing;

sealed class RateLinesGridColorSchemaManagerTest : TestCaseWithFactory
{
	public void TestGetFilterBusinessObjectForGrid()
	{
		using (var grid = new ZGrid())
		{
			var gridColorSchemaManager = new RateLinesGridColorSchemaManager(grid);
			AssertType<RateLinesGridFilterStripBusinessObject>(gridColorSchemaManager.FilterBusinessObject);
		}
	}
}
