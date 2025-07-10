using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class AircraftPartsUserControl : ZUserControl
	{
		public AircraftPartsUserControl()
		{
			InitializeComponent();
			this.BindingSource.SetBindingMember(this.EnglishDescriptionLongTextControl, "FilteredInvoiceLines.JI_Description");
			this.BindingSource.SetBindingMember(this.ChineseDescriptionLongTextControl, "FilteredInvoiceLines.JI_NDescription");
		}
	}
}
