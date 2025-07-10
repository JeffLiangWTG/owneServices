using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AddAssistanceTaskForStaffMenuItemInfo
	{
		public AddAssistanceTaskForStaffMenuItemInfo(string userCode, MultilingualString menuItemDescription)
		{
			UserCode = userCode;
			MenuItemDescription = menuItemDescription;
		}

		public string UserCode { get; }
		public MultilingualString MenuItemDescription { get; }
	}
}
