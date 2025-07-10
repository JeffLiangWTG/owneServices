using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RateCommodityFMCSelectorForm : ZChildForm
	{
		public RateCommodityFMCSelectorForm(RateCommodityFMCSelectorViewModel viewModel)
			: base(viewModel)
		{
			InitializeComponent();
		}

		#region Buttons

		void YesButton_Click(object sender, System.EventArgs e)
		{
			var current = zGrid1.ListManager.GetCurrent();
			if (current != null)
			{
				Model.SelectedPair = (RateCommodityFMCViewModel)current;
				Close();
				Dispose();
			}
		}

		void NoButton_Click(object sender, System.EventArgs e)
		{
			Close();
			Dispose();
		}

		public RateCommodityFMCSelectorViewModel Model
			=> (RateCommodityFMCSelectorViewModel)BusinessEntity;

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

