using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing;

[TestedType(typeof(RateLinesGridFilterStripBusinessObject))]
sealed class RateLinesGridFilterStripBusinessObjectTest : GridFilterStripBusinessObjectTest
{
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new RateLinesGridFilterStripBusinessObject(TestGrid);

	ZGrid TestGrid
	{
		get
		{
			if (testGrid == null)
			{
				testGrid = new ZGrid();
			}

			return testGrid;
		}
	}

	ZGrid testGrid;

	protected override void TearDown()
	{
		if (testGrid != null)
		{
			testGrid.Dispose();
			testGrid = null;
		}

		base.TearDown();
	}
}
