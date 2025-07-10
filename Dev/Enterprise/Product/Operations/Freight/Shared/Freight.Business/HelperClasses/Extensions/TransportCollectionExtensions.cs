
namespace Enterprise.Freight.Business.Extensions
{
	using CargoWise.Types;

	public static class TransportCollectionExtensions
	{
		public static Transport New(this TransportCollection collection, ZString from, ZString to, string transportMode = "")
		{
			var transport = collection.AddNew();
			transport.JW_RL_NKLoadPort = from;
			transport.JW_RL_NKDiscPort = to;
			transport.JW_TransportMode = transportMode;

			return transport;
		}
	}
}
