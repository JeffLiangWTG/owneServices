using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Module
{
	public partial class ExpiryDateConfirmationForm : ZChildForm
	{
		public ExpiryDateConfirmationForm(NonPersistentExpiryDateObject business) : base(business)
		{
			InitializeComponent();
		}
	}
}
