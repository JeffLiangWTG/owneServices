using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateChartFormatLookups : AutoAccAlternateChartFormatLookups
	{
		public AccAlternateChartFormatLookups(AutoAccAlternateChartFormat parent) : base(parent)
		{
		}

		public static class SeparatorCode
		{
			public const string DOT = ".";
			public const string LINE = "-";
		}

		public CodeDescriptionPairList SeparatorList
		{
			get
			{
				var separatorList = new CodeDescriptionPairList();
				separatorList.AddPair(SeparatorCode.DOT, SeparatorCode.DOT);
				separatorList.AddPair(SeparatorCode.LINE, SeparatorCode.LINE);

				return separatorList;
			}
		}
	}
}
