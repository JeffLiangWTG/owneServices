using System.ComponentModel.DataAnnotations;
using System.Linq;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class DpsRequestHeaderValidatorTest : TestCase
	{
		public void TestDpsRequestHeaderValidation_Success()
		{
			// Arrange
			var dpsNameCandidate = new DpsNameCandidate
			{
				NameType = "ORG",
				FullName = "Test"
			};
			var dpsRequestHeader = new DpsRequestHeader
			{
				DpsNameCandidates = new[] { dpsNameCandidate }
			};
			var dpsRequestHeaderValidator = new DpsRequestHeaderValidator();

			// Act and Assert
			AssertNoExceptionThrown(() => dpsRequestHeaderValidator.Validate(dpsRequestHeader));
		}

		public void TestDpsRequestHeaderValidation_Failure_WhenLargePayload()
		{
			// Arrange
			const string expectedExceptionMessage = "DpsRequestHeader validation failed: Payload size exceeds 3000000 Bytes";
			var requestHeader = new DpsRequestHeader
			{
				DpsNameCandidates = new[]
				{
					new DpsNameCandidate
					{
						NameType = "ORG",
						FullName = new string('a', 4_000_000)
					}
				}
			};

			var dpsRequestHeaderValidator = new DpsRequestHeaderValidator();

			// Act
			var exception = AssertExceptionThrown<ValidationException>(() => dpsRequestHeaderValidator.Validate(requestHeader));

			AssertEquals(exception?.Message, expectedExceptionMessage);
		}

		public void TestDpsRequestHeaderValidation_Failure_WhenTotalCandidatesExceedLimit()
		{
			// Arrange
			const string expectedExceptionMessage = "DpsRequestHeader validation failed: Total number of candidates in request header exceeds maximum allowed limit of 10000 candidates";
			var requestHeader = new DpsRequestHeader
			{
				DpsNameCandidates = Enumerable.Repeat(
					new DpsNameCandidate
					{
						NameType = "ORG",
						FullName = "Test Org",
					}, 10_001
				)
			};

			var dpsRequestHeaderValidator = new DpsRequestHeaderValidator();

			// Act
			var exception = AssertExceptionThrown<ValidationException>(() => dpsRequestHeaderValidator.Validate(requestHeader));

			AssertEquals(exception?.Message, expectedExceptionMessage);
		}
	}
}
