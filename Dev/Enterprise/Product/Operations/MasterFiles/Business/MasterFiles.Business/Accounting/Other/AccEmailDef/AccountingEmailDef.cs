using System;
using CargoWise.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public enum EmailSendResult
	{
		Successful,
		Unsuccessful
	}

	public abstract class AccountingEmailDef : EmailDef, IEmailCreator
	{
		public EmailSendResult Send()
		{
			return CreateOrSend();
		}

		public EmailSendResult Create(ITransactionParticipant factory)
		{
			return CreateOrSend(false, factory);
		}

		/// <summary>
		/// Render the Subject and Body properties without creating or sending the email.
		/// </summary>
		public AccountingEmailDef Render()
		{
			Subject = GetSubject();
			Body = GetBody();
			return this;
		}

		EmailSendResult CreateOrSend(bool isSend = true, ITransactionParticipant factory = null)
		{
			EmailSendResult result = EmailSendResult.Unsuccessful;
			if (string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(Body))
			{
				Render();
			}

			try
			{
				if (isSend)
				{
					SendCore();
				}
				else
				{
					CreateCore(factory);
				}
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

		protected virtual Guid GetRecipient()
		{
			return Recipient.Value;
		}

		protected virtual void CreateCore(ITransactionParticipant factory)
		{
			Env.OutgoingMailManager.Create(factory, this, GetRecipient(), GroupSourceLocator.GetFromRegistryItem(Recipient));
		}

		protected virtual void SendCore()
		{
			Env.OutgoingMailManager.CreateAndSave(this, GetRecipient(), GroupSourceLocator.GetFromRegistryItem(Recipient));
		}

		protected abstract string GetBody();
		protected abstract string GetSubject();
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
