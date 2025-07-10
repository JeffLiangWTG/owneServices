using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public class GoodsLocationFormLayoutProvider : EU.GUI.IGoodsLocationFormLayoutProvider
{
	public IPanelLayoutProvider GetGoodsLocationLayout() => new CusGoodsLocationLayout();
}
