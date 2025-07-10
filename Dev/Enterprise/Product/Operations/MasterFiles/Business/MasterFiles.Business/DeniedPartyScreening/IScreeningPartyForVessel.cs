using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IScreeningPartyForVessel
	{
		public ZString Code { get; }

		public ZString CurrentScreeningStatus { get; }
	}
}
