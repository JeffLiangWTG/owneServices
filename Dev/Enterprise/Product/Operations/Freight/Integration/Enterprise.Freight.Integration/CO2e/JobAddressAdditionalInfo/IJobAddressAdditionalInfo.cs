using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IJobAddressAdditionalInfo
	{
		public ZString TransportMode { get; set; }
		public ZString AddressType { get; set; }
		public void Delete();
	}
}
