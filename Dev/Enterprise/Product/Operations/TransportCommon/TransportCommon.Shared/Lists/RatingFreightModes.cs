using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportCommon.Shared
{
	public class RatingFreightModes
	{
		#region Codes

		public static class Codes
		{
			public const string Containerised = "CNT";
			public const string Loose = "LSE";
			public const string Both = "BTH";
		}

		#endregion

		#region Descriptions

		public static class Descriptions
		{
			public static string Containerised
			{
				get { return Res.GetString("47b597dc-9668-42b9-a662-e068a1ffb599", "Rate Containers Only"); }
			}
			public static string Loose
			{
				get { return Res.GetString("45c1b893-c008-45b6-8bd2-90a002144471", "Rate Loose Only"); }
			}
			public static string Both
			{
				get { return Res.GetString("f46bcce4-ff8f-4e72-a005-59c037340a65", "Rate Both Containers and Loose"); }
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

					list.AddPair(Codes.Containerised, Descriptions.Containerised);
					list.AddPair(Codes.Loose, Descriptions.Loose);
					list.AddPair(Codes.Both, Descriptions.Both);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;

		#endregion
	}
}
