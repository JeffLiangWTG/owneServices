using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class ExitEntryTypeCodeList : CodeDescriptionPairList
	{
		public ExitEntryTypeCodeList()
		{
			AddPair("1", "Permanent");
			AddPair("2", "Temporary");
			AddPair("3", "In Transit");
			AddPair("4", "Re-Export");
			AddPair("5", "Re-Import");
		}
	}
}
