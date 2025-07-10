using System.Text;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common
{
	internal class TestLogger : ILogger
	{
		public StringBuilder Combined = new StringBuilder();
		public StringBuilder Errors = new StringBuilder();
		public StringBuilder Information = new StringBuilder();

		public void LogError(string message)
		{
			Combined.AppendLine(message);
			Errors.AppendLine(message);
		}

		public void LogInfo(string message)
		{
			Combined.AppendLine(message);
			Information.AppendLine(message);
		}

		public string CombinedString => Combined.ToString();
		public string ErrorString => Errors.ToString();
		public string InfoString => Information.ToString();
	}
}
