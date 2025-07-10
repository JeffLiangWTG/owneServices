using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Tracking.Business
{
	[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public static class TransportHelper
	{
		public static class ColumnHeaders
		{
			public static string Leg { get { return Res.GetString("e72b6a2a-3a6f-449d-9725-4feddc9a914b", "Leg"); } }
			public static string Mode { get { return Res.GetString("b3020d6e-3174-4257-9477-9d42bece5313", "Mode"); } }
			public static string Type { get { return Res.GetString("6923718e-9017-411b-a20d-76210e5ebdc8", "Type"); } }
			public static string Parent { get { return Res.GetString("6f4be8c4-33d4-4f89-b103-a0d342d0b51d", "Parent"); } }
			public static string Bill { get { return Res.GetString("b5ec52bd-7f36-4233-a181-52846852487a", "Bill"); } }
			public static string Vessel { get { return Res.GetString("e6ce64b0-15dd-4eb1-9a37-f1ca1450de8d", "Vessel"); } }
			public static string VoyageFlight { get { return "Voyage/Flight"; } }
			public static string Load { get { return Res.GetString("dd181ea8-99be-4e09-b65e-95e27a359af4", "Load"); } }
			public static string Discharge { get { return Res.GetString("749087c4-94d7-4668-aa3d-5080eb6b6a57", "Discharge"); } }
			public static string Departure { get { return Res.GetString("cb4ac63e-279b-4d55-892e-cfc410e60af9", "Departure"); } }
			public static string Arrival { get { return Res.GetString("edcce794-1c9b-4ffe-9119-ad368760f29c", "Arrival"); } }
			public static string Status { get { return Res.GetString("a1b59d7a-feae-4945-b4cd-791c5ab80f41", "Status"); } }
			public static string Carrier { get { return Res.GetString("9e838af6-0288-441f-9060-23ecaeda0f56", "Carrier"); } }
		}
	}
}
