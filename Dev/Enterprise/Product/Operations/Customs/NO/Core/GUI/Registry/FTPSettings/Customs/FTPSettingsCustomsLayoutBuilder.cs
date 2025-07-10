using Enterprise.Customs.NO.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

class FTPSettingsCustomsLayoutBuilder<T> : ColumnLayoutBuilder<T, FTPSettingsControlBag> where T : FTPSettingsCustomsRegistry
{
	public override FTPSettingsControlBag CommonBag => FTPSettingsControlBag.Instance;

	protected override int MaxColumns => 1;
}
