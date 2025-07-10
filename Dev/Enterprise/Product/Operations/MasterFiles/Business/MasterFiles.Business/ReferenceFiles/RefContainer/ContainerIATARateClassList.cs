using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ContainerIATARateClassList : CodeDescriptionPairList
	{
		public ContainerIATARateClassList()
		{
			AddRange(GetListForAWB());
			AddPair("1S");
			AddPair("6B");

			Sort();
		}

		public static CodeDescriptionPairList GetListForAWB()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("1", FormatDescription("5540"));
			result.AddPair("1P", FormatDescription("4935"));
			result.AddPair("2", FormatDescription("2860"));
			result.AddPair("2A", FormatDescription("2610"));
			result.AddPair("2AA", FormatDescription("2390"));
			result.AddPair("2B", FormatDescription("2130"));
			result.AddPair("2BG", FormatDescription("1895"));
			result.AddPair("2C", FormatDescription("3220"));
			result.AddPair("2D", FormatDescription("2330"));
			result.AddPair("2H", FormatDescription("3525"));
			result.AddPair("2Q", FormatDescription("2750"));
			result.AddPair("2R", FormatDescription("2685"));
			result.AddPair("2W", FormatDescription("2265"));
			result.AddPair("2WA", FormatDescription("2500"));
			result.AddPair("3", FormatDescription("2100"));
			result.AddPair("3A", FormatDescription("2240"));
			result.AddPair("4", FormatDescription("1845"));
			result.AddPair("4A", FormatDescription("1700"));
			result.AddPair("5", FormatDescription("1630"));
			result.AddPair("5A", FormatDescription("1720"));
			result.AddPair("5W", FormatDescription("2170"));
			result.AddPair("5WA", FormatDescription("2125"));
			result.AddPair("6", FormatDescription("1155"));
			result.AddPair("6A", FormatDescription("1110"));
			result.AddPair("6W", FormatDescription("1460"));
			result.AddPair("7", FormatDescription("1015"));
			result.AddPair("7A", FormatDescription("950"));
			result.AddPair("8", FormatDescription("710"));
			result.AddPair("8A", FormatDescription("590"));
			result.AddPair("8B", FormatDescription("800"));
			result.AddPair("8C", FormatDescription("550"));
			result.AddPair("8D", FormatDescription("565"));
			result.AddPair("8F", FormatDescription("765"));
			result.AddPair("8G", FormatDescription("565"));
			result.AddPair("9", FormatDescription("885"));

			return result;
		}

		static string FormatDescription(string amount) => Res.GetString("483a8535-5bbb-484d-8b90-4fcca83de69e", "Minimum Chargeable {0} KG per ULD", amount);
	}
}
