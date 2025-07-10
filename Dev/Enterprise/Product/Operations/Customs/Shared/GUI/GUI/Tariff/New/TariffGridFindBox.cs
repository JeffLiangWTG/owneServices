using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.GUI
{
	public partial class TariffGridFindBox : ZGridFindBox, ITariffFindBoxPopupSupport
	{
		public TariffGridFindBox()
		{
			TariffInfo = new TariffPropertyInfo();
		}

		#region Overrides

		protected internal object CurrentItemInternal => CurrentItem;

		protected internal IFindBoxPopup GetNewPopupFormInternal() => GetNewPopupForm();
		protected override IFindBoxPopup GetNewPopupForm()
		{
			return TariffFindBoxPopup.GetNewPopup() ?? base.GetNewPopupForm();
		}

		#endregion

		#region Implementation of ITariffFindBoxPopupSupport

		public TariffPropertyInfo TariffInfo { get; private set; }
		public string BindToTariffPropertyInfo { get; set; }
		public object BoundItem
		{
			get { return CurrentItem; }
		}

		#endregion
	}
}
