namespace Enterprise.Rating.Module;

public class CarrierChargeCodeFilterBusinessObject : MappedChargeCodeFilterBusinessObject
{
	protected override string[] VisibleFilters => [Descriptions.ForeignCode, Descriptions.ForeignName];
}
