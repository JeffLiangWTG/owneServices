using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.GUI
{
	public partial class PortCallRequestForm : ZChildForm, IFindBoxPopup
	{
		public PortCallRequestForm(PortCallManager manager)
			: base(manager)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				CreateFilterControl();
			}
		}

		IFindBox findBox;

		void CreateFilterControl()
		{
			FilterStripBusinessObject = new PortCallRequestFilterStripBusinessObject(Manager);
			FilterControl = new PortCallRequestFilterControl(Manager.Responses, FilterStripBusinessObject);

			filterPanel.Controls.Add(FilterControl);
			FilterControl.Dock = DockStyle.Fill;
			var caption = Manager.Request.RequestType == PortCallRequestType.Load
				? Res.GetString("a13eb408-b287-4aa9-940e-42a5bc9c24ef", "ETD")
				: Res.GetString("a4b30d45-1310-4bae-a5b7-8155e13015af", "ETA");

			FilterControl.FilteredGrid.SetColumnCaption(PortCallResponse.Schema.EstimatedTime, caption);

			FilterControl.PerformSearch += delegate
			{
				using (new ZWaitCursorChanger())
				{
					var notifications = new NotificationBuffer();
					FilterStripBusinessObject.ReBuildRequest();
					Manager.LoadResponses(notifications);

					if (notifications.HasErrors)
					{
						Globals.Message.ShowError(notifications.AsString.TrimEnd('\r', '\n'));
					}
					else
					{
						if (notifications.HasWarnings)
						{
							Globals.Message.ShowWarning(notifications.AsString.TrimEnd('\r', '\n'));
						}

						if (!Manager.Responses.Any())
						{
							Globals.Message.Show(Res.GetString("316fa3a4-21f7-4b24-b7b4-9655cc2d6d24", "There are no result found by provided criteria."));
						}
					}
				}
			};
		}

		public PortCallRequestFilterControl FilterControl { get; set; }

		PortCallManager Manager => (PortCallManager)BusinessEntity;
		PortCallRequestFilterStripBusinessObject FilterStripBusinessObject { get; set; }

		#region OK / Cancel button

		void OkBtn_Click(object sender, EventArgs e)
		{
			if (FilterControl.FilteredGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(Res.GetString("14ffcfdd-7f2e-4b21-a902-fcb6f3fb7d14", "Please select an item from the grid."));
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		void CancelBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		#region IFindBoxPopup

		public void ShowModal(IFindBox displayingFindBox, Form parentForm)
		{
			findBox = displayingFindBox;
			ZFormModaliser.Show(this, parentForm);
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox displayingFindBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		public void SelectRowByPK(ZGuid pK) { }

		#endregion

		#region Implement

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				var chosenResponse = FilterControl.FilteredGrid.SelectedElements.Length > 0 ? (PortCallResponse)FilterControl.FilteredGrid.SelectedElements[0] : null;
				if (findBox != null && chosenResponse != null)
				{
					findBox.Code = chosenResponse.ReferenceNumber;
				}
			}
			base.OnClosing(e);
		}

		public override string FormVerb => string.Empty;

		#endregion
	}
}
