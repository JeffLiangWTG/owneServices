using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Internal
{
	[SuppressFormDesignerAnalysis]
	public class ZCodeFindBoxFixedPreBoundMaxLength : ZCodeFindBox
	{
		protected override void SetControlSize(int charLength)
		{
			base.SetControlSize(PreBoundMaxLength);
		}
	}
}