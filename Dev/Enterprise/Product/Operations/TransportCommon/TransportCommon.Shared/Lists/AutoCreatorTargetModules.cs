using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportCommon.Shared
{
	public class AutoCreatorTargetModules
	{
		#region Codes

		public static class Codes
		{
			public const string PortTransport = "PTR";
			public const string LandTransportConsignment = "GCN";
		}

		#endregion

		#region Descriptions

		public static class Descriptions
		{
			public static string PortTransport
			{
				get { return Res.GetString("AutoCreatorTargetModules|PortTransport", "Target the Port Transport Module"); }
			}
			public static string LandTransportConsignment
			{
				get { return Res.GetString("AutoCreatorTargetModules|LandTransportConsignment", "Target the Land Transport Module"); }
			}
		}

		#endregion

		#region List

		public CodeDescriptionPairList List
		{
			get
			{
				if (list == null)
				{
					list = new CodeDescriptionPairList();

					list.AddPair(Codes.PortTransport, Descriptions.PortTransport);
					list.AddPair(Codes.LandTransportConsignment, Descriptions.LandTransportConsignment);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;

		#endregion
	}
}
