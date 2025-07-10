using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public abstract class AccountingHtmlEmailDef : HtmlEmailDef
	{
		public EmailSendResult Send()
		{
			EmailSendResult result = EmailSendResult.Unsuccessful;
			Subject = GetSubject();
			LoadHtmlUsingTemplate(GetBody(), GetCss(), GetCompanyPK(), GetBranchPK(), GetDepartmentPK());
			try
			{
				SendCore();
				result = EmailSendResult.Successful;
			}
			catch (EmailHasNoRecipientsException ex)
			{
				DisplayEmailHasNoRecipientsError(ex.Message);
			}
			catch (EmailNotCompleteException ex)
			{
				DisplayEmailNotCompleteError(ex.Message);
			}
			catch (EmailSendFailedException ex)
			{
				DisplayEmailSendFailedError(ex.Message);
			}
			return result;
		}

		#region Implementation

		protected virtual void SendCore()
		{
			Env.OutgoingMailManager.CreateAndSave(this, Recipient.Value, GroupSourceLocator.GetFromRegistryItem(Recipient));
		}

		protected abstract string GetBody();
		protected abstract string GetSubject();
		protected abstract string GetCss();

		protected abstract Guid GetCompanyPK();
		protected abstract Guid GetBranchPK();
		protected abstract Guid GetDepartmentPK();

		public abstract ZString EmailDescription { get; }

		protected abstract GuidRegistryItem Recipient { get; }

		protected virtual void DisplayEmailSendFailedError(string exceptionMessage)
		{
		}

		protected virtual void DisplayEmailNotCompleteError(string exceptionMessage)
		{
		}

		protected virtual void DisplayEmailHasNoRecipientsError(string exceptionMessage)
		{
		}
		#endregion
	}
}
