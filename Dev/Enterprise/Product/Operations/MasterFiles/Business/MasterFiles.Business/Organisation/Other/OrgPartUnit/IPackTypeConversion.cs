using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IPackTypeConversion
	{
		ZString UOMType { get; }
		decimal PackQty { get; }
		decimal QtySKU { get; }
	}
}
