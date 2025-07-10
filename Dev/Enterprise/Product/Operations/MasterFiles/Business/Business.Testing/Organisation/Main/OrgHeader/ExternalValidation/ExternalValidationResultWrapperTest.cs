using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExternalValidationResultWrapper))]
	sealed class ExternalValidationResultWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var testResult = new ExternalValidationResult { Value = "Invalid", Errors = new[] { "Error" }, Warnings = new[] { "Warning" } };
			return new ExternalValidationResultWrapper(testResult);
		}

		public void TestMessageIsCorrectForInvalidResult()
		{
			var testResult = new ExternalValidationResult { Value = "Invalid", Errors = new[] { "Error1", "Error2" }, Warnings = new[] { "Warning1", "Warning2" } };
			var testResultWrapper = new ExternalValidationResultWrapper(testResult);
			var entries = testResultWrapper.Messages.Cast<ExternalValidationResultMessage>();
			Assert(entries.Any(m => m.MessageType == ExternalValidationResultWrapper.ErrorStatus.ToString() && m.MessageContent == "Error1"));
			Assert(entries.Any(m => m.MessageType == ExternalValidationResultWrapper.ErrorStatus.ToString() && m.MessageContent == "Error2"));
			Assert(entries.Any(m => m.MessageType == ExternalValidationResultWrapper.WarningStatus.ToString() && m.MessageContent == "Warning1"));
			Assert(entries.Any(m => m.MessageType == ExternalValidationResultWrapper.WarningStatus.ToString() && m.MessageContent == "Warning2"));
		}

		public void TestMessageIsEmptyForValidResult()
		{
			var testResult = new ExternalValidationResult { Value = "Valid" };
			var testResultWrapper = new ExternalValidationResultWrapper(testResult);
			AssertEquals(0, testResultWrapper.Messages.Count);
		}
	}
}
