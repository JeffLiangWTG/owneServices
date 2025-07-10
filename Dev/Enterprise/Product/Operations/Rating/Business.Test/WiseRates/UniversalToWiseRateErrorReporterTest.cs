using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business.WiseRates;

namespace Enterprise.Rating.Business.Test.WiseRates
{
	public class UniversalToWiseRateErrorReporterTest : TestCaseWithFactory
	{
		public void TestReportMappingError_GivenSameFunction_ShouldReportErrorOnce()
		{
			// Arrange
			var reporter = new UniversalToWiseRateErrorReporter("Correlation1234");

			// Act
			reporter.ReportMappingError("message 1", "functionName", ("param1", "object1"), ("param2", "object2"));
			reporter.ReportMappingError("message 2", "functionName", ("param1", "object1"));

			// Assert
			var lastErrorMessage = ErrorReporter.LastMessageReported;

			const string expectedMessage = @"
Error mapping URS object during functionName: message 1

-- Additional Information --

CATEGORY: URS Mapping
CorrelationID:
Correlation1234
param1:
""object1""
param2:
""object2""
";

			AssertEquals(
				"Only the first error should be logged for the same function name",
				expectedMessage.Trim(),
				lastErrorMessage
			);

			ErrorReporter.Clear();
		}

		public void TestReportMappingError_GivenObjectAsParam_ShouldSerialiseAsJson()
		{
			// Arrange
			var reporter = new UniversalToWiseRateErrorReporter("Correlation1234");

			var testObj = new TestClass
			{
				Name = "TestName",
				Count = 123
			};

			// Act
			reporter.ReportMappingError("message 1", "functionName", ("param1", testObj));

			// Assert
			var lastErrorMessage = ErrorReporter.LastMessageReported;

			const string expectedMessage = @"
Error mapping URS object during functionName: message 1

-- Additional Information --

CATEGORY: URS Mapping
CorrelationID:
Correlation1234
param1:
{""Name"":""TestName"",""Count"":123}
";

			AssertEquals(
				"Object parameter values should be serialised to JSON",
				expectedMessage.Trim(),
				lastErrorMessage
			);

			ErrorReporter.Clear();
		}

		public void TestReportMappingError_GivenParameterThatDoesNotSerialise_ShouldContinue()
		{
			// Arrange
			var reporter = new UniversalToWiseRateErrorReporter("Correlation1234");

			var cannotSerialise = new CannotSerialise();

			// Act
			reporter.ReportMappingError("message 1", "functionName", ("param1", cannotSerialise));

			// Assert
			var lastErrorMessage = ErrorReporter.LastMessageReported;

			const string expectedMessage = @"
Error mapping URS object during functionName: message 1

-- Additional Information --

CATEGORY: URS Mapping
CorrelationID:
Correlation1234
param1:
Error during serialisation: Error getting value from 'Name' on 'Enterprise.Rating.Business.Test.WiseRates.CannotSerialise'.
";

			AssertEquals("Object parameter values should be serialised to JSON", expectedMessage.Trim(), lastErrorMessage);

			ErrorReporter.Clear();
		}

		public void TestReportMappingError_GivenDifferentFunctions_ShouldReportEachError()
		{
			// Arrange
			var reporter = new UniversalToWiseRateErrorReporter("Correlation1234");

			reporter.ReportMappingError("message 1", "firstFunction", ("name", "object1"));
			var lastErrorMessage = ErrorReporter.LastMessageReported;
			AssertNotNull("lastErrorMessage", lastErrorMessage);

			// Act
			reporter.ReportMappingError("message 2", "secondFunction", ("name", "object2"));

			// Assert
			lastErrorMessage = ErrorReporter.LastMessageReported;

			const string expectedMessage = @"
Error mapping URS object during secondFunction: message 2

-- Additional Information --

CATEGORY: URS Mapping
CorrelationID:
Correlation1234
name:
""object2""
";

			AssertEquals(
				"Separate exceptions should be logged for different function names",
				expectedMessage.Trim(),
				lastErrorMessage
			);

			ErrorReporter.Clear();
		}
	}

	class TestClass
	{
		public string Name { get; set; }
		public int Count { get; set; }
	}

	class CannotSerialise
	{
		public string Name => throw new InvalidOperationException("This should not serialize");
	}
}
