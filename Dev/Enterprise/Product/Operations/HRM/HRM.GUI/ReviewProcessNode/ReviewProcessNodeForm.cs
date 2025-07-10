using System;
using CargoWise.Common;
using Enterprise.HRM.Common;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.HRM.GUI
{
	public partial class ReviewProcessNodeForm : ZTemplateForm
	{
		protected override bool SupportsEDocs => false;

		[Obsolete("For designer only. Use constructor that accepts a bizo")]
		public ReviewProcessNodeForm()
		{
			InitializeComponent();
		}

		public ReviewProcessNodeForm(ReviewProcessNode bizo)
			: base(bizo)
		{
			ControllerID = ControllerIDs.ReviewProcessNode;

			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveButtonUserControl);

			if (!this.IsDesignMode())
			{
				WorkflowTabPage.Initialize(bizo);
			}
		}
	}
}
