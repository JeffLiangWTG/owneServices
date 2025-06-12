using CargoWise.eHub.Portal.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System;
using Assert = NUnit.Framework.Assert;

namespace CargoWise.eHub.Portal.Tests.HelperTests
{
	[TestClass]
	public class HelperTests
	{
		[TestMethod]
		public void TestExceptionHelper()
		{
			var innerException2 = new Exception("Second inner exception message");
			var innerException1 = new Exception("First inner exception message", innerException2);
			var mainException = new Exception("Main exception message", innerException1);

			var exceptionMessages = ExceptionHelper.GetExceptionMessages(mainException);

			Assert.AreEqual(
@"Main exception message
First inner exception message
Second inner exception message"
				, exceptionMessages
				, "Exception messages not parsed correctly");
		}

		[TestCase("CX_CC_ID LIKE \"aaa%\" AND CX_Qualifier LIKE \"%bbb\" AND CodeX LIKE \"%ab%\"", "AND", new[] { "CX_CC_ID|bw|aaa", "CX_Qualifier|ew|bbb", "CodeX|cn|ab" })]
		[TestCase("CX_CC_ID LIKE \"aaa%\" OR CX_Qualifier LIKE \"%bbb\"", "OR", new[] { "CX_CC_ID|bw|aaa", "CX_Qualifier|ew|bbb" })]
		[TestCase("", "OR", new string[0])]
		[TestCase("CX_CC_ID LIKE \"%test%\"", "OR", new[] { "CX_CC_ID|cn|test" })]
		public void ConvertToFilterObject_ShouldParseSearchQueryCorrectly(string searchQuery, string expectedGroupOp, string[] expectedRules)
		{
			var result = DynamicQueryable.ConvertSQLQueryToMultipleFilterObject(searchQuery);

			Assert.AreEqual(expectedGroupOp, result.groupOp);
			Assert.AreEqual(expectedRules.Length, result.rules.Count);

			for (int i = 0; i < expectedRules.Length; i++)
			{
				var expectedRuleParts = expectedRules[i].Split('|');
				Assert.AreEqual(expectedRuleParts[0], result.rules[i].field);
				Assert.AreEqual(expectedRuleParts[1], result.rules[i].op);
				Assert.AreEqual(expectedRuleParts[2], result.rules[i].data);
			}
		}

		[TestCase("CX_CC_ID LIKE aaa", 0)] // No valid LIKE pattern
		[TestCase("CX_CC_ID aaa%", 0)]    // Invalid syntax
		public void ConvertToFilterObject_ShouldHandleInvalidQueries(string searchQuery, int expectedRuleCount)
		{
			var result = DynamicQueryable.ConvertSQLQueryToMultipleFilterObject(searchQuery);

			Assert.AreEqual(expectedRuleCount, result.rules.Count);
		}
	}
}
