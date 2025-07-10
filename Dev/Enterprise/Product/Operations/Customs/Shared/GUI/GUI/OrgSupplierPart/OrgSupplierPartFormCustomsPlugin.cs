using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GUI
{
	public class OrgSupplierPartFormCustomsPlugin : ZPlugIn
	{
		public OrgSupplierPartFormCustomsPlugin(OrgSupplierPart part)
			: base(part)
		{
			this.part = part;
		}

		public override string Name
		{
			get { return (NoResString)"Customs"; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Broker; }
		}

		protected override Control GetNewUserControl()
		{
			return new OrgSupplierPartFormCustomsControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return part;
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected virtual SecurityCheckpoint AuditSecurity
		{
			get { return Env.Security.CustomsSupplierPartAudit; }
		}

		public override void OnSaveCompletedOrAborted(bool saved)
		{
			base.OnSaveCompletedOrAborted(saved);
			if (saved && IsPromptAuditOnSavedEnabled && isPromptAuditOnSaved)
			{
				var userControl = UserControl as OrgSupplierPartFormCustomsControl;
				if (userControl != null && CusClassPartPivots.Length > 0)
				{
					userControl.ShowBatchAuditDialog(CusClassPartPivots);
				}
			}
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ContinueWithSave result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes && IsPromptAuditOnSavedEnabled && AuditSecurity.IsAllowed)
			{
				var message = Res.GetString("dbf85e65-e1e8-4cc4-a7a5-19356b72ca0b", "Auditing has not yet been run on this product. Do you want to audit now?");
				var userResponse = Globals.Message.Show(message, Res.GetString("86204e68-6792-4b79-8fdd-840bd16b40e3", "Auditing"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

				switch (userResponse)
				{
					case DialogResult.Yes:
						isPromptAuditOnSaved = true;
						result = ContinueWithSave.Yes;
						break;

					case DialogResult.No:
						isPromptAuditOnSaved = false;
						result = ContinueWithSave.Yes;
						break;

					default:
						isPromptAuditOnSaved = false;
						result = ContinueWithSave.No;
						break;
				}
			}
			return result;
		}
		ZBool isPromptAuditOnSaved = false;

		protected virtual ZBool IsPromptAuditOnSavedEnabled
		{
			get { return false; }
		}

		protected virtual Business.BaseCusClassPartPivot[] CusClassPartPivots => Array.Empty<Business.BaseCusClassPartPivot>();

		internal OrgSupplierPart part;

		public ZBool IsPromptAuditOnSaved_Exposed
		{
			get { return isPromptAuditOnSaved; }
		}
	}
}
