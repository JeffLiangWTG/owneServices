using System;
using CargoWise.Common;
using Enterprise.HRM.Common;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.HRM.GUI
{
	public partial class ReviewProcessForm : ZTemplateForm
	{
		protected override bool SupportsEDocs => false;

		[Obsolete("For designer only. Use constructor that accepts a bizo")]
		public ReviewProcessForm()
		{
			InitializeComponent();
		}

		public ReviewProcessForm(ReviewProcess bizo)
			: base(bizo)
		{
			ControllerID = ControllerIDs.ReviewProcess;

			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveButtonUserControl);

			if (!this.IsDesignMode())
			{
				WorkflowTabPage.Initialize(bizo);
			}
		}
	}
}
