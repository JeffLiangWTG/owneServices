using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.GUI
{
	public partial class ApportionmentProgressForm : ProgressForm
	{
		private readonly System.ComponentModel.Container components;

		public ApportionmentProgressForm(IInvoicesProvider jobDeclaration)
		{
			this.JobDeclaration = jobDeclaration;
			this.JobDeclaration.OnApportionmentProgressChanged += new BaseJobDeclaration.ApportionmentProgressEventHandler(JobDeclaration_OnApportionmentProgressChanged);
			ShowProgressBar = true;
			ShowCancelButton = false;
			Status = Res.GetString("d4e6bb96-8880-4c08-a14b-2270bcc6d84f", "Apportioning charges...");
			InitializeComponent();
		}

		protected readonly IInvoicesProvider JobDeclaration;

		protected virtual void UpdateProgress(int percentage)
		{
			PercentComplete = percentage;
			Refresh();
		}

		private void JobDeclaration_OnApportionmentProgressChanged(int percentage)
		{
			UpdateProgress(percentage);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				JobDeclaration.OnApportionmentProgressChanged -= new BaseJobDeclaration.ApportionmentProgressEventHandler(JobDeclaration_OnApportionmentProgressChanged);
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
