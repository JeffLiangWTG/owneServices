using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public sealed class AuthorityToLeaveOptions : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string DEF = "DEF";
			public const string NO = "NO";
			public const string YES = "YES";
		}

		public static class Descriptions
		{
			public static MultilingualString DEF
			{
				get
				{
					ZBool registryItemValue = ObjectFactory.Get<TransportCommon.Integration.ITransportRegistry>().AuthorityToLeave.Value;

					if (registryItemValue)
					{
						return ResString.GetMultilingualString("AuthorityToLeaveCodeList|DEFY", "Default from Registry (Currently authority to leave is granted).");
					}
					else
					{
						return ResString.GetMultilingualString("AuthorityToLeaveCodeList|DEFN", "Default from Registry (Currently authority to leave is denied).");
					}
				}
			}
			public static MultilingualString NO { get { return ResString.GetMultilingualString("AuthorityToLeaveCodeList|NO", "Authority to leave is denied."); } }
			public static MultilingualString YES { get { return ResString.GetMultilingualString("AuthorityToLeaveCodeList|YES", "Authority to leave is granted."); } }
		}

		public AuthorityToLeaveOptions()
		{
			AddPair(Codes.DEF, Descriptions.DEF);
			AddPair(Codes.NO, Descriptions.NO);
			AddPair(Codes.YES, Descriptions.YES);
		}
	}
}

