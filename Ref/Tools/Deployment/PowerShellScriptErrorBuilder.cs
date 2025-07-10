using System;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace CargoWise.RefDbRepo.Deployment
{
	public class PowerShellScriptErrorBuilder : IErrorBuilder
	{
		readonly StringBuilder errorBuilder = new StringBuilder();

		public string Build()
		{
			return errorBuilder.ToString();
		}

		public void Append(ErrorRecord errorRecord)
		{
			var errorMessage = errorRecord.ToString();
			if (IsIgnoredMessage(errorMessage))
			{
				return;
			}
			errorBuilder.AppendLine(errorMessage);
			errorBuilder.AppendLine(errorRecord.ScriptStackTrace);
		}

		bool IsIgnoredMessage(string message)
		{
			return !string.IsNullOrEmpty(message) && ignoredErrorMessageList.Any(errorMessage => message.Contains(errorMessage, StringComparison.OrdinalIgnoreCase));
		}

		readonly string[] ignoredErrorMessageList = { "Could not detect any platforms from" };
	}
}
