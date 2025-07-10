using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradeDetailClonerForm : ZChildForm
	{
		[Obsolete("This just for designer.")]
		public TradeDetailClonerForm()
		{
			InitializeComponent();
		}

		public TradeDetailClonerForm(TradeDetailCloner cloner)
			: base(cloner)
		{
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;

		public TradeDetailCloner Cloner => BusinessEntity as TradeDetailCloner;

		void ConfirmButton_Click(object sender, EventArgs e)
		{
			if (Cloner != null)
			{
				Cloner.RunPreSaveValidation();
				if (Cloner.HasErrors)
				{
					ShowErrorsDialog();
				}
				else
				{
					Cloner.CopySelectionToTarget();
					Close();
				}
			}
		}

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			Cloner?.SelectAll();
		}

		void SelectUnsuccessfulButton_Click(object sender, EventArgs e)
		{
			Cloner?.SelectUnsuccessful();
		}

		void UnselectAllButton_Click(object sender, EventArgs e)
		{
			Cloner?.UnselectAll();
		}
	}
}
