using System;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class QuoteTabControl : BaseTabControl
	{
		public QuoteTabControl()
		{
			InitializeComponent();

			quoteSignaturesControl1.AllowOverlap(PrintInheritedDestinationCheckBox);
			PrintDestinationCheckBox.AllowOverlap(PrintInheritedDestinationCheckBox);
			PrintDestinationCheckBox.AllowOverlap(PrintInheritedOriginCheckBox);
			PrintInheritedDestinationCheckBox.AllowOverlap(PrintInheritedOriginCheckBox);
			PrintOriginCheckBox.AllowOverlap(PrintInheritedOriginCheckBox);
		}

		public override ZTabControl TopLevelTabControl
		{
			get { return TabControl; }
		}

		protected override string[] GetInapplicableCategories()
		{
			return new[]
			{
				RatingConstants.RateCategory.CYD,
				RatingConstants.RateCategory.CYU
			};
		}

		Quote CurrentQuote
		{
			get { return RatingHeader as Quote; }
		}

		internal override void HostTabIndexChanged(object sender, EventArgs e)
		{
			base.HostTabIndexChanged(sender, e);

			var currentTabPage = sender as ZTemplateTabControl;
			if (currentTabPage != null && currentTabPage.SelectedTab == QuoteFormatTabPage)
			{
				CurrentQuote.QuoteFormatEntries.LoadEntries();
			}
		}

		#region IDisposable Members

		System.ComponentModel.IContainer components;

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
