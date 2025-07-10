using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public partial class BaseClassificationForm : ZTemplateForm
	{
		public BaseClassificationForm()
		{
		}

		public BaseClassificationForm(BaseCusClassification businessEntity)
			: base(businessEntity)
		{
			AuditClassificationMenuItem.AddTo(ActionsMenuItem);
			this.Saved += BaseClassificationForm_Saved;
		}

		protected virtual BaseClassificationUserControl GetUserControl()
		{
			return new GeneralCountryClassificationUserControl();
		}

		public override string FormCaption
		{
			get { return Res.GetString("00e27fe1-a56f-4934-b228-832d414366fd", "Classification Lookup"); }
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			if (!this.IsDesignMode())
			{
				BaseClassificationUserControl control = GetUserControl();
				if (control != null)
				{
					control.Dock = DockStyle.Fill;
					control.Visible = true;
					MainTabPage.Controls.Add(control);
				}
			}
			InitializeComponent();
		}

		protected override bool ShowAuditTab => true;

		protected virtual SecurityCheckpoint AuditSecurity
		{
			get { return Env.Security.CusClassificationAudit; }
		}

		protected new BaseCusClassification BusinessEntity
		{
			get { return (BaseCusClassification)base.BusinessEntity; }
		}

		protected WriteToLogMenuItem AuditClassificationMenuItem
		{
			get
			{
				if (auditClassificationMenuItem == null)
				{
					auditClassificationMenuItem = new WriteToLogMenuItem(BusinessEntity, AuditSecurity, Res.GetString("Customs|BaseClassificationForm|AuditMenuItem", "Audit Classification"));
				}
				return auditClassificationMenuItem;
			}
		}
		WriteToLogMenuItem auditClassificationMenuItem;

		void BaseClassificationForm_Saved(object sender, EventArgs e)
		{
			if (isPromptAuditOnSaved)
			{
				AuditClassificationMenuItem.PerformClick();
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && IsPromptAuditOnSavedEnabled && AuditSecurity.IsAllowed)
			{
				var message = Res.GetString("ee253831-29b4-4b86-9176-dc8c9b0b7220", "Auditing has not yet been run on this classification. Do you want to audit now?");
				var userResponse = Globals.Message.Show(message, Res.GetString("0e92408c-83f4-4c7f-a422-c4a1e7bccf38", "Auditing"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

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
		internal ZBool isPromptAuditOnSaved = false;

		protected virtual ZBool IsPromptAuditOnSavedEnabled
		{
			get { return false; }
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			BaseJobComInvoiceLine invoiceLine = BusinessEntity.Factory.LoadTop1<BaseJobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_CC, BusinessEntity.PK));
			ContinueWithDelete result = ContinueWithDelete.Yes;
			if (invoiceLine != null)
			{
				Globals.Message.ShowInformation(Res.GetString("0bf94d94-1bc0-41a5-9033-9f2564efef74", "This classification lookup cannot be deleted. It has been entered on a declaration with lines referenced. Instead, if the classification is no longer valid, please untick the Active box within the Import Classification Lookup screen for the same effect.")
												, Res.GetString("5aa90749-1a92-4993-8e5e-30d6fe0d24b3", "Cannot delete this classification lookup"));
				result = ContinueWithDelete.No;
			}
			return result;
		}
	}
}
