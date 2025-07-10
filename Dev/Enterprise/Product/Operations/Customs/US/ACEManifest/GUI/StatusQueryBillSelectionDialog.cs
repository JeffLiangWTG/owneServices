using System;
using Enterprise.Customs.US.ACEManifest.Business;

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public partial class StatusQueryBillSelectionDialog : AIMBillsSelectionDialog
	{
		[Obsolete("This constructor is just for the designer")]
		public StatusQueryBillSelectionDialog()
		{
			InitializeComponent();
		}

		public StatusQueryBillSelectionDialog(AIMMessageChooser messageChooser, string itemsType)
			: base(messageChooser, itemsType)
		{
			InitializeComponent();
			ReasonDropEdit.Visible = false;
		}
	}
}
