using System;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business
{
	public class JobInvoicingSecurityHelper
	{
		public JobInvoicingSecurityHelper(Func<SecurityCheckpoint> pluginSecurityRetriever, bool defaultIsAllowedValue = true)
		{
			this.pluginSecurityRetriever = pluginSecurityRetriever;
			this.defaultIsAllowedValue = defaultIsAllowedValue;
		}

		public JobInvoicingSecurityHelper(SecurityCheckpoint pluginSecurity, bool defaultIsAllowedValue = true)
		{
			this.pluginSecurity = pluginSecurity;
			this.pluginSecurityRetriever = () => pluginSecurity;
			this.defaultIsAllowedValue = defaultIsAllowedValue;
		}

		public SecurityCheckpoint GetInvSecurity(string name)
		{
			return Env.Security.GetInvoicingSecurityCheckPoint(PlugInSecurity, name);
		}

		public string GetErrorTextForSecurityCheckPoint(string name)
		{
			string errorText = defaultIsAllowedValue ? "" : securityNotFoundErrorMessage;
			SecurityCheckpoint checkPoint = GetInvSecurity(name);
			if (checkPoint != null)
			{
				errorText = checkPoint.ErrorMessageForNotAllowed;
			}
			return errorText;
		}

		public bool CheckIsAllowedForSecurityCheckPoint(string name)
		{
			bool allowed = defaultIsAllowedValue;
			SecurityCheckpoint checkPoint = GetInvSecurity(name);
			if (checkPoint != null)
			{
				allowed = checkPoint.IsAllowed;
			}
			return allowed;
		}

		public void ShowError(string name)
		{
			SecurityCheckpoint checkPoint = GetInvSecurity(name);
			if (checkPoint != null)
			{
				checkPoint.ShowError();
			}
			else if (!defaultIsAllowedValue)
			{
				throw new InvalidOperationException(securityNotFoundErrorMessage);
			}
		}

		internal string securityNotFoundErrorMessage
		{
			get { return Res.GetString("2a918a99-a7ea-4558-a325-7c494b04e045", "Security checkpoint is not found."); }
		}

		public SecurityCheckpoint PlugInSecurity
		{
			get { return pluginSecurity ?? pluginSecurityRetriever(); }
		}

		readonly SecurityCheckpoint pluginSecurity;
		readonly bool defaultIsAllowedValue;
		readonly Func<SecurityCheckpoint> pluginSecurityRetriever;
	}
}
