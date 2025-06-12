
namespace CargoWise.eHub.Portal.Models.View.OceanCarrierMessaging
{
	public class Row
	{
		public string id;
		public string client;
		public string carrier;
		public string provider;
		public string port;
		public string carrierAgent;
		public string docName;
		public string shpType;
		public string purpose;
		public string splitBy;
		public string shipNamespace;
		public string partyToCopy;
		public string eventBranch;

		public static class RowNames
		{
			public static string id = "id";
			public static string client = "client";
			public static string carrier = "carrier";
			public static string provider = "provider";
			public static string port = "port";
			public static string carrierAgent = "carrierAgent";
			public static string docName = "docName";
			public static string shpType = "shpType";
			public static string purpose = "purpose";
			public static string splitBy = "splitBy";
			public static string shipNamespace = "shipNamespace";
			public static string partyToCopy = "partyToCopy";
			public static string eventBranch = "eventBranch";
		}
	}
}
