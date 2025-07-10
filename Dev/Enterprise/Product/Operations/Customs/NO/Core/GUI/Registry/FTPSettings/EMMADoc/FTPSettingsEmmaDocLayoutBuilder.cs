using Enterprise.Customs.NO.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

class FTPSettingsEmmaDocLayoutBuilder<T> : ColumnLayoutBuilder<T, FTPSettingsControlBag> where T : FTPSettingsRegistry
{
	public override FTPSettingsControlBag CommonBag => FTPSettingsControlBag.Instance;

	protected override int MaxColumns => 1;
}
