using CargoWise.NetworkVisualisation.GUI.Utils;
using Point = Blazor.Diagrams.Core.Geometry.Point;

namespace NetworkVisualisation.GUI.Test.Winzor;

class NetworkLinkPathGeneratorTest
{
	[TestCase(0, 0, 2, 4, "M 0 0 C 0 2 2 2 2 4")]
	[TestCase(0, 0, 2, -4, "M 0 0 C 0 -2 2 -2 2 -4")]
	[TestCase(0, 0, 4, 2, "M 0 0 C 2 0 2 2 4 2")]
	[TestCase(0, 0, -4, 2, "M 0 0 C -2 0 -2 2 -4 2")]
	public void TestGeneratePath(double x0, double y0, double x1, double y1, string expected)
	{
		var generator = new NetworkLinkPathGenerator();
		var path = generator.GetResult(null, null, null, new Point(x0, y0), new Point(x1, y1));
		Assert.That(path.FullPath.ToString(), Is.EqualTo(expected));
	}
}
