using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public abstract class RatingModule : ZFilterGridModule
	{
		protected static MultilingualString BulkUpdateSubMenuText
		{
			get { return ResString.GetMultilingualString("8fb31c5a-5baf-4db6-9ec6-70a7432796b8", "Updates"); }
		}

		protected static MultilingualString BulkRateCostUpdateText
		{
			get { return ResString.GetMultilingualString("9ee23462-e6bd-4b9d-b2f8-7197adde5981", "Bulk Rate/Cost Update"); }
		}
	}
}

