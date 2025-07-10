using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Business
{
	public interface IPackLineInfo
	{
		ZString PackType { get; }
		ZInt NumberOfPackages { get; }
		ZWeight Weight { get; }
		ZVolume Volume { get; }

		ZDecimal Length { get; }
		ZDecimal Width { get; }
		ZDecimal Height { get; }
		ZString UnitOfDimension { get; }

		ZString GoodsDescription { get; }
		ZString ContainerNumber { get; }

		ZString MarksAndNumbers { get; }
	}
}
