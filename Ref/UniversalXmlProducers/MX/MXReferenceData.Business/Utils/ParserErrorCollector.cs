using System;
using System.Text;

namespace CargoWise.RefDbRepo.MXReferenceData.Business
{
	public class ParserErrorCollector
	{
		ParserErrorCollector() { }

		public static ParserErrorCollector Instance => instance;
		static readonly ParserErrorCollector instance = new ParserErrorCollector();

		readonly StringBuilder errorBuilder = new StringBuilder();

		public void AppendLine(string errorMessage) => errorBuilder.AppendLine(errorMessage);

		public void ReportErrors()
		{
			if (errorBuilder.Length > 0)
			{
				var errors = errorBuilder.ToString();
				errorBuilder.Clear();

				throw new InvalidOperationException(errors);
			}
		}
	}
}
