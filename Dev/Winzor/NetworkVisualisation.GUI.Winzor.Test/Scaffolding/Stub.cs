using CargoWise.NetworkVisualisation.Integration;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
public static class Stub
{
	public const string RealControlMessage = "Prefer a real control";
	public static EntityForTest Entity() => new ();
	public static EntityForTest Entity(Action<EntityForTest> config)
	{
		var entity = Entity();
		config.Invoke(entity);
		return entity;
	}

	// Helper/explainer for specifying common diagram-level settings
	public static EntityForTest DiagramEntity(bool supportDiagramVisualStyles = false, bool isDiagramScaled = false, bool showNonscheduledSection = false)
		=> new ()
		{
			SupportedActions = supportDiagramVisualStyles ? NetworkActions.StyleDiagram : NetworkActions.None,
			IsDiagramScaled = isDiagramScaled,
			ShouldShowNonScheduledSection = showNonscheduledSection // NOTE: this defaults to true in the base EntityForTest
		};
}
