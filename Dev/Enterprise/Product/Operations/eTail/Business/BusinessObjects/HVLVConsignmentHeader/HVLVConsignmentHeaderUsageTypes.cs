using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentHeaderUsageTypes : CodeDescriptionPairList
	{
		public HVLVConsignmentHeaderUsageTypes()
		{
			AddPair(HVLVConstants.HVLVConsignmentHeaderUsageTypesCodes.Plus, Descriptions.Plus);
			AddPair(HVLVConstants.HVLVConsignmentHeaderUsageTypesCodes.Standard, Descriptions.Standard);
		}

		public static class Descriptions
		{
			public static string Plus => Res.GetString("eb49aebc-d2f4-4c92-896b-b0aa5450d33d", "Ecommerce Plus");
			public static string Standard => Res.GetString("9d20e395-e868-4c45-a5c0-17de8464d4b2", "Ecommerce Standard");
		}
	}
}

