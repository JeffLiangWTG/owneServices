using System;
using Enterprise.Core.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DialogDefaultEditForm : ZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public DialogDefaultEditForm()
		{
			InitializeComponent();
		}

		public DialogDefaultEditForm(StmDialogDefault bo)
			: base(bo)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, saveButton, exitButton);
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
