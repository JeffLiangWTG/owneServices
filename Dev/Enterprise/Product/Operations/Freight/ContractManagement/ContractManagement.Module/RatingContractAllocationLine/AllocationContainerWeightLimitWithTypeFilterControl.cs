using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ContractManagement.Module.Common
{
	public partial class AllocationContainerWeightLimitWithTypeFilterControl : ZUserControl
	{
		public Control[] GetFilterControl(ZBindingSource bindingSource)
		{
			InitializeComponent(bindingSource);
			return [UnitControl, FromCalcEdit, ToCalcEdit, TypeEdit];
		}
	}
}
