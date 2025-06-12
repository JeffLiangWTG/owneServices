using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace CargoWise.eServices.Billing.Collector.WindowsService.Common
{
	public class Impersonator : IDisposable
	{
		public Impersonator(ILogger log)
		{
			if (log == null) throw new ArgumentNullException("log");
			this.log = log;
		}

		public virtual void Impersonate(string userName, string password, string domainName)
		{
			if (string.IsNullOrEmpty(userName)) return;

			if (RevertToSelf()
				&& LogonUser(userName, domainName, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_WINNT50, ref primaryToken) != 0
				&& DuplicateToken(primaryToken, (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, ref impersonationToken) != 0)
			{
				impersonatedUser = new WindowsIdentity(impersonationToken).Impersonate();
			}
			else
			{
				throw new OperationCanceledException($"Failed to impersonate User (userName: {userName}, password: {password}, domainName: {domainName}).");
			}
		}

		public void Dispose()
		{
			UnImpersonate(impersonatedUser, primaryToken, impersonationToken);
		}

		void UnImpersonate(WindowsImpersonationContext impersonatedUser, params IntPtr[] tokens)
		{
			if (impersonatedUser != null) impersonatedUser.Undo();
			foreach (var token in tokens) if (token != IntPtr.Zero) CloseHandle(token);
		}

		WindowsImpersonationContext impersonatedUser = null;
		IntPtr primaryToken = IntPtr.Zero;
		IntPtr impersonationToken = IntPtr.Zero;
		readonly ILogger log;

		const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
		const int LOGON32_PROVIDER_WINNT50 = 3;

		[DllImport("advapi32.DLL", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool RevertToSelf();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool CloseHandle(IntPtr handle);
	}

	enum SECURITY_IMPERSONATION_LEVEL
	{
		SecurityAnonymous,
		SecurityIdentification,
		SecurityImpersonation,
		SecurityDelegation
	}
}
