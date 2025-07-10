using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.GUI
{
	public class CheckBoxItem
	{
		public CheckBoxItem(MultilingualString label, ZBool isSelected = default)
		{
			Label = label;
			Code = label.GetUnresolvedString();
			IsSelected = isSelected;
		}

		public ZBool IsSelected { get; set; }
		public string Code { get; set; }
		public MultilingualString Label { get; set; }
	}
}
