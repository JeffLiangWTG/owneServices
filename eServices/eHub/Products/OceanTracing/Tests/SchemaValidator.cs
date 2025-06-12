using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
	public class SchemaValidator
	{
		public void ValidateSchema<T>(string expectedOutput, List<string> errorWhiteList) where T : SchemaBase, new()
		{
			var schemaLogs = XmlValidator.Validate<T>(expectedOutput).Split(Environment.NewLine.ToCharArray()).ToList();

			List<string> result = new List<string>();
			foreach (var log in schemaLogs)
			{
				if (!ContainsAny(log, errorWhiteList))
				{
					result.Add(log);
				}
			}

			var actualLogs = result.Where(x => !string.IsNullOrEmpty(x)).ToList();
			var logs = string.Join("\r\n", actualLogs);
			Assert.AreEqual(0, actualLogs.Count, expectedOutput + "\r\n" + typeof(T).Name + " is not valid: \r\n " + logs);
		}

		internal bool ContainsAny(string input, List<string> containsKeywords)
		{
			return containsKeywords.Any(keyword => input.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0);
		}
	}
}
