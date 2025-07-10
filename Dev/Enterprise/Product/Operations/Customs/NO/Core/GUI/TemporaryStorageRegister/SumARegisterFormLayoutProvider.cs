using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

public sealed class SumARegisterFormLayoutProvider : EFTA.TemporaryStorageRegister.GUI.SumARegisterFormLayoutProvider
{
	protected override IPanelLayoutProvider GetDetailsHeaderLayoutCore() => new SumARegisterDetailsHeaderLayout();
}
