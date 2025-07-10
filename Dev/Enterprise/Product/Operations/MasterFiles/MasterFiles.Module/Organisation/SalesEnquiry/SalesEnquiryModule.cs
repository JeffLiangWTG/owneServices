using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class SalesEnquiryModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public SalesEnquiryModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem(Res.GetString("3fdc6446-2149-4c51-9f77-cabcef7f685b", "From CSV"), OnImportFromCsv, false);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.SalesEnquiry; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.SalesEnquiry }; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.SalesEnquiry);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new SalesEnquiryFilterControl(GridCollection, (SalesEnquiryFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new SalesEnquiryCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new SalesEnquiryFilterBusinessObject();
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode; }
		}

		#region Menu / Toolbar

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());

			result.Add(new ZMenuItem("-"));
			result.Add(CloseMenuItem);
			result.Add(ReopenMenuItem);

			return result.ToArray();
		}

		MenuItem ReopenMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.SalesEnquiry.Reopen", "&Re-open"), new EventHandler(OnReopen)); }
		}

		MenuItem CloseMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.SalesEnquiry.Close", "&Close"), new EventHandler(OnClose)); }
		}

		protected void OnReopen(Object sender, EventArgs e)
		{
			if (Env.Security.InquiryManagerReopen.IsAllowed)
			{
				Globals.Message.Show(SalesEnquiry.Reopen(new BusinessObjectFactory(), GridSelectedElements));
			}
			else
			{
				Env.Security.InquiryManagerReopen.ShowError();
			}
		}

		protected void OnClose(Object sender, EventArgs e)
		{
			if (Env.Security.InquiryManagerClose.IsAllowed)
			{
				var closer = new SalesEnquiryCloseAction(GridSelectedElements);

				if (ZFormModaliser.ShowDialogAndDispose(new SalesEnquiryCloseForm(closer)) == DialogResult.OK)
				{
					Globals.Message.Show(Res.GetString("A58F509E-7D84-4150-AC0B-ABF0BDFD2894", "Selected open Inquiries are closed."));
				}
			}
			else
			{
				Env.Security.InquiryManagerClose.ShowError();
			}
		}

		#endregion

		#region Actions

		protected virtual ZGuid[] GridSelectedElements
		{
			get { return Array.ConvertAll(Grid.SelectedElements, x => x.PK); }
		}

		#endregion

		#region Security Checkpoint

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.InquiryManager; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.InquiryManager; }
		}

		#endregion

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new SalesEnquiryActionSupporter(); }
		}

		#endregion

		#region Import From CSV

		void OnImportFromCsv(Object sender, EventArgs e)
		{
			new ImportInquiryFromCSVForm().Show();
		}

		#endregion
	}
}
