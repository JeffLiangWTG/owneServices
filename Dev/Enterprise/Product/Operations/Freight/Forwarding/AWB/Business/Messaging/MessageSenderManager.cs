using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	class MessageSenderManager
	{
		internal MessageSenderManager(CIMEDIMessage message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			this.message = message;
		}

		readonly CIMEDIMessage message;

		internal void LogSenderIfRequired(OrgContact contactSending)
		{
			if (contactSending != null && EmailAddressValidation.IsEmailAddressValidAndNotEmpty(contactSending.OC_Email))
			{
				string logReference = contactSending.OC_Email + " (" + contactSending.OrganisationCode + ")";
				message.Logs.AddNew(Events.MessageResponseAddress, logReference);
			}
		}

		internal ZString SendersEmailAddress
		{
			get
			{
				StmALog sendersAddressLog = StmALogEntryLocator.Instance.GetLastPostEventOfType(message, AutoEvents.MessageResponseAddress);
				if (sendersAddressLog != null)
				{
					ZString reference = sendersAddressLog.SL_Reference;
					if (!reference.IsEmpty)
					{
						var splittingRegex = new Regex(@"^(?<Email>[^ @]+@[^ @]+) \(.+\)$");
						var match = splittingRegex.Match(reference);
						if (match.Success)
						{
							string emailAddress = match.Groups["Email"].Value;
							if (EmailAddressValidation.IsEmailAddressValidAndNotEmpty(emailAddress))
							{
								return emailAddress;
							}
						}
					}
				}

				var messageSender = message.Staff;
				return messageSender != null ? messageSender.GS_EmailAddress : ZString.Empty;
			}
		}
	}
}
