namespace Enterprise.Rating.Module;

public class UniversalChargeCodeFilterBusinessObject : MappedChargeCodeFilterBusinessObject
{
	protected override string[] VisibleFilters => [Descriptions.Code, Descriptions.Description];
}
