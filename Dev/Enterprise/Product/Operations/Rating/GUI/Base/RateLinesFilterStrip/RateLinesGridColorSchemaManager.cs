using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.GUI;

public class RateLinesGridColorSchemaManager : GridColourSchemeManager
{
	public RateLinesGridColorSchemaManager(ZGrid grid) : base(grid)
	{
		this.grid = grid;
	}

	readonly ZGrid grid;

	protected override FilterStripBusinessObject GetFilterBusinessObjectForGrid()
	{
		return new RateLinesGridFilterStripBusinessObject(grid);
	}
}
