using CargoWise.NetworkVisualisation.GUI.Services.AnnotationNode;

namespace CargoWise.NetworkVisualisation.GUI.Models;

public class AnnotationNodeModel : NetworkNodeModel
{
	public AnnotationNodeModel(IAnnotationNodeService service) : base(service)
	{ }
}
