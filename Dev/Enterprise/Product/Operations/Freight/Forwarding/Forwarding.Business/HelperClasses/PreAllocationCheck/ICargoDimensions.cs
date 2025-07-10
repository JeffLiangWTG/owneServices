using CargoWise.Types;
namespace Enterprise.Freight.Forwarding.Business
{
	public interface ICargoDimensions
	{
		ZString DimensionsUnits { get; }
		ZDecimal Length { get; }
		ZDecimal Width { get; }
		ZDecimal Height { get; }
	}
}
