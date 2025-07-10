using System;
using System.Drawing;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class BulkCommunicationEntryUserControl : ZUserControl
	{
		public BulkCommunicationEntryUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				BulkCommunication communication = CurrentDataItem as BulkCommunication;
				if (communication != null)
				{
					communication.StatusChanged += communication_StatusChanged;
				}
			}
		}

		void communication_StatusChanged(object sender, EventArgs e)
		{
			SetOverallDispositionLabelBackColor();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetOverallDispositionLabelBackColor();
		}

#if DEBUG
		public
#endif
		void SetOverallDispositionLabelBackColor()
		{
			BulkCommunication communication = CurrentDataItem as BulkCommunication;
			if (communication != null)
			{
				OverallDispositionLabel.BackColor = communication.IsClosed ? Color.Red : Color.LimeGreen;
				communication.OverallDispositionInfo.RefreshBinding();
			}
		}
	}
}
