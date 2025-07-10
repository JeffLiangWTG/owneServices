using System.Collections.Generic;

namespace Enterprise.Customs.PL.Business.Testing;

public readonly record struct ExpectedUnpackResult
{
	ExpectedUnpackResult(bool expectedSuccess,
		string expectedErrorReason,
		ExpectedMessage[] expectedMessages,
		ExpectedLog[] expectedLogs = null)
	{
		ExpectedSuccess = expectedSuccess;
		ExpectedErrorReason = expectedErrorReason;
		ExpectedMessages = new(expectedMessages ?? []);
		ExpectedLogs = new(expectedLogs ?? []);
	}

	public bool ExpectedSuccess { get; init; }
	public string ExpectedErrorReason { get; init; }
	public List<ExpectedMessage> ExpectedMessages { get; init; }
	public List<ExpectedLog> ExpectedLogs { get; init; }

	public static ExpectedUnpackResult Success(ExpectedMessage[] expectedMessages, ExpectedLog[] expectedLogs = null)
		=> new(expectedSuccess: true, expectedErrorReason: null, expectedMessages, expectedLogs);

	public static ExpectedUnpackResult Success(ExpectedLog[] expectedLogs)
		=> new(expectedSuccess: true, expectedErrorReason: null, expectedMessages: null, expectedLogs);

	public static ExpectedUnpackResult Failure(string errorText, ExpectedLog[] expectedLogs = null)
		=> new(expectedSuccess: false, expectedErrorReason: errorText, expectedMessages: null, expectedLogs: expectedLogs);
}
