using IntegrationShapeTypes = CargoWise.NetworkVisualisation.Integration.ShapeTypes;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding;

internal static class WinzorShapeTypes
{
	public const string Shape = IntegrationShapeTypes.Shape;
	public const string Annotation = IntegrationShapeTypes.Annotation;
	public const string Buffer = "BUF";

	public static IEnumerable<string> Values
	{
		get
		{
			yield return Shape;
			yield return Annotation;
			yield return Buffer;
		}
	}
}
