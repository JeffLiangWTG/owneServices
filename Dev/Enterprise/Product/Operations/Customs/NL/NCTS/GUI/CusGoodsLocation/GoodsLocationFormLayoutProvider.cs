using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI;

public class GoodsLocationFormLayoutProvider : EU.GUI.IGoodsLocationFormLayoutProvider
{
	public IPanelLayoutProvider GetGoodsLocationLayout() => new CusGoodsLocationLayout();
}
