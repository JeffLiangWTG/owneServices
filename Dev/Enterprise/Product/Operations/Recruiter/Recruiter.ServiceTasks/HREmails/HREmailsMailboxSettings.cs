using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruiter.ServiceTasks
{
	public sealed class HREmailsMailboxSettings
	{
		public HREmailsMailboxSettings()
		{
		}

		#region RegistryItem Properties

		IRegistryItem MailRetrievalProtocolRegistry => RecruiterDataRegistry.Instance.HREmailMailRetrievalProtocol;

		IRegistryItem MailServerRegistry => RecruiterDataRegistry.Instance.HREmailMailServer;

		IRegistryItem MailServerPortRegistry => RecruiterDataRegistry.Instance.HREmailMailServerPort;

		IRegistryItem MailboxUserNameRegistry => RecruiterDataRegistry.Instance.HREmailMailboxUserName;

		IRegistryItem MailboxPasswordRegistry => RecruiterDataRegistry.Instance.HREmailMailboxPassword;

		IRegistryItem IMAPSecureConnectionTypeRegistry => RecruiterDataRegistry.Instance.HREmailIMAPSecureConnectionType;

		IRegistryItem POP3SecureConnectionTypeRegistry => RecruiterDataRegistry.Instance.HREmailPOP3SecureConnectionType;

		#endregion

		#region Properties

		public string MailRetrievalProtocol
		{
			get { return MailRetrievalProtocolRegistry.Value.ToString(); }
#if DEBUG
			set { MailRetrievalProtocolRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string MailServer
		{
			get { return MailServerRegistry.Value.ToString(); }
#if DEBUG
			set { MailServerRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int ServerPort
		{
			get { return (int)MailServerPortRegistry.Value; }
		}

		public string UserName
		{
			get { return MailboxUserNameRegistry.Value.ToString(); }
#if DEBUG
			set { MailboxUserNameRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string Password
		{
			get { return MailboxPasswordRegistry.Value.ToString(); }
#if DEBUG
			set { MailboxPasswordRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string IMAPSecureConnectionType
		{
			get { return IMAPSecureConnectionTypeRegistry.Value.ToString(); }
		}

		public string POP3SecureConnectionType
		{
			get { return POP3SecureConnectionTypeRegistry.Value.ToString(); }
		}

		#endregion

		public IEnumerable<IRegistryItem> GetAllItems()
		{
			return new IRegistryItem[]
			{
				IMAPSecureConnectionTypeRegistry,
				MailboxPasswordRegistry,
				MailboxUserNameRegistry,
				MailServerRegistry,
				MailServerPortRegistry,
				POP3SecureConnectionTypeRegistry,
				MailRetrievalProtocolRegistry
			};
		}
	}
}
