using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class QuoteSignaturesControl : ZUserControl
	{
		public QuoteSignaturesControl()
		{
			InitializeComponent();
		}

		void OverallRepButton_Click(object sender, System.EventArgs e)
		{
			if (CurrentQuote != null && CurrentQuote.Header != null)
			{
				if (CurrentQuote.HeaderStaffAssignments.OverallSalesRepStaff != null)
				{
					if (sender == OverallRepButton1)
					{
						CurrentQuote.TH_GS_NKFirstSignatory = CurrentQuote.HeaderStaffAssignments.OverallSalesRepStaff.GS_Code;
					}
					else if (sender == OverallRepButton2)
					{
						CurrentQuote.TH_GS_NKSecondSignatory = CurrentQuote.HeaderStaffAssignments.OverallSalesRepStaff.GS_Code;
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("68bffe06-fe1a-4a8f-81d6-1e2a1dfffd2c", "There is no Overall Rep for this client."));
				}
			}
		}

		void CurrentUserButton_Click(object sender, System.EventArgs e)
		{
			if (sender == CurrentUserButton1)
			{
				CurrentQuote.TH_GS_NKFirstSignatory = GlbStaff.CurrentUser.GS_Code;
			}
			else if (sender == CurrentUserButton2)
			{
				CurrentQuote.TH_GS_NKSecondSignatory = GlbStaff.CurrentUser.GS_Code;
			}
		}

		#region CurrentQuote

		Quote CurrentQuote
		{
			get
			{
				if (fCurrentQuote == null)
				{
					ZForm currentForm = FindForm() as ZForm;
					fCurrentQuote = (Quote)currentForm.BusinessEntity;
				}

				return fCurrentQuote;
			}
		}

		Quote fCurrentQuote;

		#endregion

		#region IDisposable Members

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
