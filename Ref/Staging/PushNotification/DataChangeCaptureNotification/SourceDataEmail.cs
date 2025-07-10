using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification
{
	class SourceDataEmail : Email<SourceData>
	{
		readonly SourceDataTemplate _htmlTemplate;
		readonly string _message;
		public SourceDataEmail(string message, string smtpServer, ICredentialsByHost credentials, int smtpPort) : base(smtpServer, credentials, smtpPort)
		{
			Argument.NotNullOrEmpty(smtpServer, nameof(smtpServer));
			Argument.InRangeWithBoundIncluded(smtpPort, 0, 65535, nameof(smtpPort));

			_htmlTemplate = new SourceDataTemplate();

			if (string.IsNullOrEmpty(message))
			{
				message = DefaultMessage;
			}

			_message = message;
		}

		public override IEnumerable<string> GetContacts(IEnumerable<SourceData> data)
		{
			var contacts = base.GetContacts(data).ToList();

			if (data.Any())
			{
				foreach (var sda in data.Where(d => !string.IsNullOrEmpty(d.SDA_Contacts)))
				{
					var splitEmail = sda.SDA_Contacts.Split(';');

					foreach (var email in splitEmail)
					{
						contacts.Add(email);
					}
				}
			}

			return contacts;
		}

		protected override string ApplyTemplate(IEnumerable<SourceData> data)
		{
			var result = _htmlTemplate.GetHeader();
			result += _htmlTemplate.GetTitle(_message);
			result += _htmlTemplate.GetBody(data);
			result += _htmlTemplate.GetFooter();
			return result;
		}
	}
}
