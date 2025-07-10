using CargoWise.Types;

namespace Enterprise.eTail.Integration
{
	public interface IEManifestLine
	{
		ZGuid PK { get; }
		ZString Reference { get; }
		ZInt PackCount { get; }
		ZString CountryOfDestination { get; }
		ZString GoodsOwner { get; }
		ZString GoodsDescription { get; }
	}
}
