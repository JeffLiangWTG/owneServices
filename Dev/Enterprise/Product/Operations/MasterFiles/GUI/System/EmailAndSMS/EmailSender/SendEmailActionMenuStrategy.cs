using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class SendEmailActionMenuStrategy : ISendEmailActionMenuStrategy
	{
		public void AddSendEmailActionMenuIfApplicable(Form form)
		{
			var zForm = form as ZForm;
			if (zForm != null)
			{
				Type type = typeof(SendEmailActionMenuStrategy);
				type = TypeDecider.GetTypeForBinding(type);
				var sendEmailActionMenuStrategy = (SendEmailActionMenuStrategy)Activator.CreateInstance(type);
				sendEmailActionMenuStrategy.AddSendEmailActionMenuIfApplicableCore(zForm);
			}
		}

		protected virtual void AddSendEmailActionMenuIfApplicableCore(ZForm zForm)
		{
			var sendEmailSource = GetSendEmailSource(zForm);
			if (sendEmailSource != null)
			{
				var menuItem = EmailSender.GetSendEmailMenuItem();
				menuItem.Enabled = !(zForm.DisplayMode == ODisplayMode.ReadOnly || zForm.DisplayMode == ODisplayMode.Delete) && IsAllowSendEmail(zForm);
				menuItem.Click += new EventHandler((o, e) => EmailSender.HandleEventHandler(zForm, sendEmailSource, null));
				ZFormMenuStrategy.AddActionsMenuItem(zForm, menuItem);
			}
		}

		bool IsAllowSendEmail(ZForm zForm)
		{
			var module = zForm.GetModule();
			if (module != null)
			{
				var moduleSecurityCheckpoint = module.SecurityCheckpoint;
				var sendEmailSecurityCheckpoint = Env.Security.FindOrCreateSendEmailCheckpoint(moduleSecurityCheckpoint);
				return sendEmailSecurityCheckpoint.IsAllowed;
			}
			return false;
		}

		protected ISendEmailSource GetSendEmailSource(ZForm zForm)
		{
			ISendEmailSource sendEmailSource = null;

			if (zForm != null)
			{
				sendEmailSource = zForm.BusinessEntity as ISendEmailSource;
			}

			return sendEmailSource;
		}
	}
}
