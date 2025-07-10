using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class PossibleOneOffQuoteMatchesForm : ZChildForm
	{
		public PossibleOneOffQuoteMatchesForm(SimpleOneOffQuoteCollectionWrapper quotes)
			: base(quotes)
		{
			InitializeComponent();
		}

		#region Buttons

		void UpdateButton_Click(object sender, System.EventArgs e)
		{
			if (zGrid1.SelectedElements.Length > 0)
			{
				((SimpleOneOffQuoteCollectionWrapper)BusinessEntity).SelectedQuote = (Quote)zGrid1.SelectedElements[0];
				Close();
				Dispose();
			}
		}

		void CancelButtonX_Click(object sender, System.EventArgs e)
		{
			Close();
			Dispose();
		}

		#endregion

		#region Implementation

		public override string FormHeading
		{
			get { return FormCaption; }
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

