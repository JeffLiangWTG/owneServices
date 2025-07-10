using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class StaffStmNumsTypeList : CodeDescriptionPairList
	{
		public StaffStmNumsTypeList()
		{
			AddPair(Codes.PatentNumber, Descriptions.PatentNumber);
			//Sort(); when add more items please uncomment this
		}

		public static class Codes
		{
			public const string PatentNumber = OrgConstants.NumberFountains.Code.PatentNumber;
		}

		public static class Descriptions
		{
			public static MultilingualString PatentNumber => OrgConstants.NumberFountains.Description.PatentNumber;
		}
	}
}
