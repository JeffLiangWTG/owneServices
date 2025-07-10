using System;
using System.ComponentModel;
using System.Linq;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class AMSUserControl : ZUserControl
	{
		public AMSUserControl()
		{
			InitializeComponent();
			mO4.IdentityCode = AMSProgramList.Codes.MO4;
			mO6.IdentityCode = AMSProgramList.Codes.MO6;
			new ZGridPGADataCorrectionSupporter(AMSGrid).AddPGALineEditMenu();
		}

		internal void RemoveUnavailableComlumnsForProduct()
		{
			AMSGrid.RemoveFromAvailableColumns(new[]
			{
				AMS.Schema.US_TrackingStatusDesc,
				"Status",	// This is a column name to be removed
				"StatusDesc",// This is a column name to be removed
				"StatusDate"// This is a column name to be removed
			});
			mO1.MO1Grid.RemoveFromAvailableColumns(ColumnsWithAMSTypeHelper.GetElementsToBeHide(AMSProgramList.Codes.MO1).Cast<string>().ToArray());
			mO2.MO2Grid.RemoveFromAvailableColumns(ColumnsWithAMSTypeHelper.GetElementsToBeHide(AMSProgramList.Codes.MO2).Cast<string>().ToArray());
			mO4.MO4Grid.RemoveFromAvailableColumns(ColumnsWithAMSTypeHelper.GetElementsToBeHide(AMSProgramList.Codes.MO4).Cast<string>().ToArray());
			mO5.MO5Grid.RemoveFromAvailableColumns(ColumnsWithAMSTypeHelper.GetElementsToBeHide(AMSProgramList.Codes.MO5).Cast<string>().ToArray());
			mO6.MO4Grid.RemoveFromAvailableColumns(ColumnsWithAMSTypeHelper.GetElementsToBeHide(AMSProgramList.Codes.MO6).Cast<string>().ToArray());
			eg1.EG1Grid.RemoveFromAvailableColumns(ColumnsWithAMSTypeHelper.GetElementsToBeHide(AMSProgramList.Codes.EG1).Cast<string>().ToArray());
			eg2.EG2Grid.RemoveFromAvailableColumns(ColumnsWithAMSTypeHelper.GetElementsToBeHide(AMSProgramList.Codes.EG2).Cast<string>().ToArray());
			pn1.PN1Grid.RemoveFromAvailableColumns(ColumnsWithAMSTypeHelper.GetElementsToBeHide(AMSProgramList.Codes.PN1).Cast<string>().ToArray());
			pn1.SetInvisibleLotGrid();
		}

		void AMSGrid_AfterBind(object sender, EventArgs e)
		{
			AMSGrid.ListManager.PositionChanged += ListManagerOnPositionChanged;
			AMSGrid.ListManager.ListChanged += ListManager_ListChanged;
			UpdateCurrentAMS();
			SetAMSDetailsVisible();
			SetViewEditButtonVisible();
		}

		void ListManager_ListChanged(object sender, ListChangedEventArgs e)
		{
			SetAMSDetailsVisible();
			SetViewEditButtonVisible();
		}

		void ListManagerOnPositionChanged(object sender, EventArgs eventArgs)
		{
			UpdateCurrentAMS();
			SetAMSDetailsVisible();
			SetViewEditButtonVisible();
		}

		void SetAMSDetailsVisible()
		{
			var currentAMS = GetCurrentAMS();
			if (currentAMS != null && !currentAMS.US_Program.IsEmpty)
			{
				var programCode = currentAMS.US_Program;
				foreach (ZUserControl control in this.splitContainer1.Panel2.Controls)
				{
					var amsControl = (IAMSControlIdentity)control;
					if (amsControl != null)
					{
						control.Visible = programCode.IsEmpty || amsControl.IdentityCode == programCode;
					}
				}
			}
		}

		void SetViewEditButtonVisible()
		{
			ViewEditButton.Visible = false;

			var currentAMS = GetCurrentAMS();
			if (currentAMS != null && !currentAMS.US_Program.IsEmpty)
			{
				ViewEditButton.Visible = currentAMS.IsOR1Program;
			}
		}

		void UpdateCurrentAMS()
		{
			var currentAMS = GetCurrentAMS();
			if (previousAMS != currentAMS && currentAMS != null)
			{
				currentAMS.US_ProgramInfo.ValueChanged -= US_ProgramInfo_ValueChanged;
				currentAMS.US_ProgramInfo.ValueChanged += US_ProgramInfo_ValueChanged;
				previousAMS = currentAMS;
			}
		}
		AMS previousAMS;

		void US_ProgramInfo_ValueChanged(object sender, EventArgs e)
		{
			SetAMSDetailsVisible();
			SetViewEditButtonVisible();
		}

		AMS GetCurrentAMS()
		{
			var listManager = AMSGrid.ListManager;
			if (listManager != null)
			{
				return (AMS)listManager.GetCurrent();
			}
			return null;
		}

		void ViewEditButton_Click(object sender, EventArgs e)
		{
			var header = this.CurrentDataItem as AMS;
			if (header != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new AMSEditForm(header));
				var grid = AMSGrid;
				if (grid != null)
				{
					grid.ListManager.EndCurrentEdit();
				}
			}
			else
			{
				Globals.Message.ShowInformation(NotificationMessage, NotificationCaption);
			}
		}
		internal const string NotificationMessage = "Please select (highlight) a AMS line.";
		internal const string NotificationCaption = "Edit AMS Detail";
	}

	internal interface IAMSControlIdentity
	{
		string IdentityCode { get; }
	}
}
