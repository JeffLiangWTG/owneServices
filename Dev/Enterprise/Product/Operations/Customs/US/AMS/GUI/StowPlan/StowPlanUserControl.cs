using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.GUI
{
	public partial class StowPlanUserControl : ZUserControl
	{
		public StowPlanUserControl()
		{
			InitializeComponent();
		}

		StowPlanSailingData SailingData
		{
			get { return (StowPlanSailingData)this.DataSource; }
		}

		internal void SendButton_Click(object sender, EventArgs e)
		{
			ValidateAll();
			if (SailingData.HasErrors)
			{
				Globals.Message.Show(Res.GetString("607BC114-2512-4CC2-81A1-23C2884F408F", "Please fix the errors before sending to US Customs."), Res.GetString("82209DD0-B2FA-4161-AF89-E470A9CE11DB", "Errors"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else if (!SailingData.HasMessageErrorIssues || Globals.Message.Show(
				Res.GetString("D8AD6481-76B2-46DE-87D1-A4D0B1871A64", "There are message errors. Do you still want to send a message?"),
				Res.GetString("4736AABB-60AB-4CB7-8BD9-2BBD8E07A5A4", "Message Sending"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				SailingData.CreateStowPlanMessage();
				try
				{
					SailingData.Factory.Save();
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(exception);
				}
				Globals.Message.ShowInformation(Res.GetString("1FED8EC4-D557-4ED7-A9EA-AE79E9B9A0A1", "The message has been sent."));
				ParentForm.Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
		}

		void issuesGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var row = this.issuesGrid.HitTest(e.X, e.Y).Row;
			if (row > -1)
			{
				var issue = (PortMessageIssue)issuesGrid.ListManager.List[row];
				OpenIssue(issue);
			}
		}

		void ValidateAll()
		{
			((StowPlanForm)ParentForm).ValidationMenuItem.PerformClick();
		}

		internal IZForm OpenIssue(PortMessageIssue issue)
		{
			if (!issue.TargetPK.IsEmpty)
			{
				var controller = GetController(issue.TargetCode);
				if (controller != null)
				{
					var factory = controller.Factory;
					controller.SetFormsModalTo(ParentForm);
					var bizObj = factory.Load(controller.TypeOfTopLevelBusinessObject, issue.TargetPK);
					if (bizObj == null || bizObj.IsDeleted)
					{
						ShowDeletedBizObjMessage(issue.HumanReadableName);
					}
					else
					{
						return OpenForm(controller, bizObj);
					}
				}
			}
			else
			{
				var module = GetModule(issue.TargetCode);
				return module.ShowPopup();
			}
			return null;
		}

		IZForm OpenForm(ZController controller, BusinessObject bizObj)
		{
			var form = controller.ShowEditForm(bizObj);
			form.Closed += OnIssueFormClosed;
			ParentForm.Enabled = false;
			return form;
		}

		void ShowDeletedBizObjMessage(ZString bizObjName)
		{
			Globals.Message.ShowInformation(Res.GetString("A38E2E09-43A7-42B0-93A4-BC599BD862D7", "The selected {0} has been deleted.", bizObjName), Res.GetString("DC9164B4-BFDF-45E4-B83D-FA635B54B974", "Deleted {0}", bizObjName));
		}

		void OnIssueFormClosed(object sender, EventArgs e)
		{
			var form = (IZForm)sender;
			form.Closed -= OnIssueFormClosed;
			SailingData.OnEventsThatIssuesNeedRebuilding(sender, e);
			ParentForm.Enabled = true;
		}

		ZController GetController(ZString targetCode)
		{
			switch (targetCode)
			{
				case JobShipmentSchema.Constants.Prefix:
					return ZControllerFactory.Create(ControllerIDs.AgencyBillOfLading);
				case JobContainerSchema.Constants.Prefix:
					return ZControllerFactory.Create(ControllerIDs.AgencyBillContainers);
				case RefVesselSchema.Constants.Prefix:
					return ZControllerFactory.Create(ControllerIDs.RefVessel);
				case RefContainerStockSchema.Constants.Prefix:
					return ZControllerFactory.Create(ControllerIDs.AgencyContainerManager);
				default:
					return null;
			}
		}

		ZFilterModule GetModule(ZString targetCode)
		{
			switch (targetCode)
			{
				case JobShipmentSchema.Constants.Prefix:
					return (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.AgencyBillOfLading);
				case JobContainerSchema.Constants.Prefix:
					return (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.AgencyBillContainers);
				case RefContainerStockSchema.Constants.Prefix:
					return (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.AgencyContainerManager);
				default:
					return null;
			}
		}

		void GoToErrorButton_Click(object sender, EventArgs e)
		{
			var issue = this.issuesGrid.GetFirstSelectedRow() as PortMessageIssue;
			if (issue != null)
			{
				OpenIssue(issue);
			}
		}

		internal void issuesGrid_MouseClick(object sender, MouseEventArgs e)
		{
			var row = this.issuesGrid.CurrentRowIndex;
			if (row > -1)
			{
				var issue = (StowPlanMessageIssue)this.issuesGrid.ListManager.List[row];
				if (issue != null)
				{
					((ZForm)ParentForm).MessageStatusBarPanel.Text = issue.MoreDetail;
				}
			}
		}

		void BillsGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var row = this.BillsGrid.HitTest(e.X, e.Y).Row;
			if (row > -1)
			{
				var shipmentData = (IStowPlanNotificationProvider)BillsGrid.ListManager.List[row];
				var controller = GetController(JobShipmentSchema.Constants.Prefix);
				controller.SetFormsModalTo(ParentForm);
				var sailingShipment = controller.Factory.Load(controller.TypeOfTopLevelBusinessObject, shipmentData.TargetPK);
				if (sailingShipment != null)
				{
					OpenForm(controller, sailingShipment);
				}
				else
				{
					ShowDeletedBizObjMessage(shipmentData.TargetSubject);
				}
			}
		}
	}
}
