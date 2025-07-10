using System.Collections.Generic;
using System.Net;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification
{
	class ProcessorStatusEmail : Email<ProcessorStatus>
	{
		readonly ProcessorStatusTemplate _htmlTemplate;
		readonly string _message;
		public ProcessorStatusEmail(string message, string smtpServer, ICredentialsByHost credentials, int smtpPort) : base(smtpServer, credentials, smtpPort)
		{
			Argument.NotNullOrEmpty(smtpServer, nameof(smtpServer));
			Argument.InRangeWithBoundIncluded(smtpPort, 0, 65535, nameof(smtpPort));

			_htmlTemplate = new ProcessorStatusTemplate();

			if (string.IsNullOrEmpty(message))
			{
				message = DefaultMessage;
			}

			_message = message;
		}

		protected override string ApplyTemplate(IEnumerable<ProcessorStatus> data)
		{
			var result = _htmlTemplate.GetHeader();
			result += _htmlTemplate.GetTitle(_message);
			result += _htmlTemplate.GetBody(data);
			result += _htmlTemplate.GetFooter();
			return result;
		}
	}
}
