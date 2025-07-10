using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class ImportMessageUserControl : Customs.GUI.ImportMessageUserControl
	{
		public ImportMessageUserControl()
		{
			InitializeComponent();

			RequiresMergeLabel.AllowOverlap(MainHorizontalSplitContainer);
		}

		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);
	}
}
