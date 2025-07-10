using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using Enterprise.MailManager;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruiter.ServiceTasks
{
	public class HRServiceEmailReader : Disposable
	{
		protected IMailProtocol protocol;
		bool isOpen;

		public string MailHost { get; private set; }

		public string MailUserName { get; private set; }

		public HRServiceEmailReader(string mailProtocol, string mailServer, int mailPort, string mailUsername, string mailPassword, string securityType)
		{
			MailHost = mailServer;
			MailUserName = mailUsername;

			var mailServerConfiguration = new MailServerConnectionConfiguration(mailServer, mailPort, securityType);
			var factory = new MailProtocolFactory();
			if (RecruiterDataRegistry.Instance.HREUseOAuth2.Value)
			{
				var cachedToken = RecruiterDataRegistry.Instance.HREMs365OAuth2Token.Value;
				var oAuth2Configuration = new Ms365OAuth2Configuration(
					RecruiterDataRegistry.Instance.HREMs365OAuth2TenantId.Value,
					RecruiterDataRegistry.Instance.HREMs365ApplicationId.Value,
					Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
					cachedToken.Token,
					TokenSaveAction,
					cachedToken.Identifier);

				protocol = factory.GetMailProtocol(mailProtocol, mailServerConfiguration, oAuth2Configuration);
			}
			else
			{
				var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(mailUsername, mailPassword);

				protocol = factory.GetMailProtocol(mailProtocol, mailServerConfiguration, userPasswordAuthConfiguration);
			}

			void TokenSaveAction(byte[] bytes)
			{
				if (bytes != null)
				{
					var token = RecruiterDataRegistry.Instance.HREMs365OAuth2Token.Value;
					token.Token = bytes;
					RecruiterDataRegistry.Instance.HREMs365OAuth2Token.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, token);
				}
			}
		}

		IMailProtocol MailProtocol
		{
			get
			{
				if (!isOpen)
				{
					protocol.Open();
					isOpen = true;
				}
				return protocol;
			}
		}

		public virtual long GetCount()
		{
			return MailProtocol.MessageCount;
		}

		public virtual string GetStringEmailFromPosition(long index)
		{
			var email = MailProtocol.GetMessageByMessageNumber(index);
			return Encoding.UTF8.GetString(email);
		}

		public virtual HRServiceEmail GetRawEmailFromString(string eml)
		{
			return new HRServiceEmail(Encoding.UTF8.GetBytes(eml));
		}

		public virtual void DeleteProcessedMail(List<long> processedList)
		{
			MailProtocol.DeleteMessageByNumber(processedList);
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (protocol != null)
				{
					protocol.Close();
					protocol = null;
				}
			}
		}
	}
}
