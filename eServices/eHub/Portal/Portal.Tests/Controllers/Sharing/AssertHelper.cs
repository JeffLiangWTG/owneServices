using System.Text;

namespace CargoWise.eHub.Portal.Tests.Controllers.Sharing
{
	public static class AssertHelper
	{
		public static string AssertMessageBuilder(string booleanVariableName, string operationType = "", string exceptionMessage = "")
		{
			var sb = new StringBuilder($"{booleanVariableName} = false");

			if (operationType != "")
			{
				sb.AppendLine($"Operation = {operationType}");
			}

			if (exceptionMessage != "")
			{
				sb.AppendLine($"Exception Message: {exceptionMessage}");
			}

			return sb.ToString();
		}
	}
}
