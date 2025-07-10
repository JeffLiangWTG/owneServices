using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.NL.GUI;

sealed class GoodsLocationGridFindBox : ZGridFindBox
{
	public GoodsLocationGridFindBox()
	{
		CodeBox.ReadOnly = true;
		PopupButtonReadonlyCanBeDifferent = true;
		PopupButton.ReadOnly = false;
	}

	protected override IFindBoxPopup GetNewPopupForm() => new CusGoodsLocationForm(GoodsLocationProvider);

	public ICusGoodsLocationProvider GoodsLocationProvider { get; internal set; }

	protected override void OnPopupFormClosed(IFindBoxPopup popupForm)
	{
		base.OnPopupFormClosed(popupForm);
		if (!GoodsLocationProvider.GoodsLocation.DisplayText.IsEmpty && ColumnStyle != null)
		{
			((GoodsLocationColumnStyle)ColumnStyle).CommitEditingRow();
		}
	}
}
