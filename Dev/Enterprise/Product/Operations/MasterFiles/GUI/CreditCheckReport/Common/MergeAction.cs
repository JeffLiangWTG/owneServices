using System.Collections.Generic;

namespace Enterprise.MasterFiles.GUI
{
	public static class MergeAction
	{
		public static Dictionary<string, string> GetAddActions() => new Dictionary<string, string>()
		{
			{ Codes.Add, Descriptions.Add },
			{ Codes.Ignore, Descriptions.Ignore }
		};

		public static Dictionary<string, string> GetUpdateActions() => new Dictionary<string, string>()
		{
			{ Codes.Update, Descriptions.Update },
			{ Codes.Ignore, Descriptions.Ignore }
		};

		public static Dictionary<string, string> GetAddAndUpdateActions() => new Dictionary<string, string>()
		{
			{ Codes.Add, Descriptions.Add },
			{ Codes.Update, Descriptions.Update },
			{ Codes.Ignore, Descriptions.Ignore }
		};

		public static Dictionary<string, string> GetIgnoreActions() => new Dictionary<string, string>()
		{
			{ Codes.Ignore, Descriptions.Ignore }
		};

		public static class Codes
		{
			public const string Add = "ADD";
			public const string Update = "UPD";
			public const string Ignore = "IGN";
		}

		class Descriptions
		{
			public static string Add { get { return ResString.GetMultilingualString("EEBC4C7A-9347-447B-B566-6ED6412934BB", "Add"); } }
			public static string Update { get { return ResString.GetMultilingualString("67D05AA9-71B4-7CDF-A226-DB9251AFE3C4", "Update"); } }
			public static string Ignore { get { return ResString.GetMultilingualString("99B829C7-9004-493E-9923-970A3161C745", "Ignore"); } }
		}
	}
}
