using System;
using System.Drawing;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI.CMDMessaging
{
	public partial class CMDShipmentUserControl : ZUserControl
	{
		public CMDShipmentUserControl()
		{
			InitializeComponent();
			messageGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(MessageGrid_ColourDeciding);
		}

		void MessageGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			CMDEDIMessage message = e.ObjectAtRow as CMDEDIMessage;
			if (message != null && message.IsActiveAndHasFailureReply)
			{
				e.Colour = Color.FromArgb(235, 155, 155);
			}
		}

		void SetCustomEntryNumbersButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new CMDShipmentCusEntryNumberCollectionForm(CurrentDataItem));
		}

		protected new CMDShipmentWrapper CurrentDataItem
		{
			get { return (CMDShipmentWrapper)base.CurrentDataItem; }
		}
	}
}
