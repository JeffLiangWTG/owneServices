using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.Business.Testing;

public readonly record struct ExpectedLog(
	Event EventCode,
	LogType? LogType,
	string Text,
	LogExpectedOn? ExpectedOn = null)
{
	public ExpectedLog(Event eventCode, string text, LogExpectedOn? expectedOn = null)
		: this(eventCode, LogType: null, text, expectedOn)
	{
	}

	public ExpectedLog(LogType logType, string text, LogExpectedOn? expectedOn = null)
		: this(EventCode: null, logType, text, expectedOn)
	{
	}

	public void AssertLog(string assertionDescription, IStmALogProvider logsProvider)
	{
		if (EventCode != null)
		{
			logsProvider.AssertHasLogMessagePart(assertionDescription, EventCode, Text);
		}
	}

	public void AssertLog(string assertionDescription, ISimpleLogResult logsProvider)
	{
		if (LogType.HasValue)
		{
			logsProvider.AssertHasLogMessagePart(assertionDescription, LogType.Value, Text);
		}
	}

	public static implicit operator ExpectedLog((Event EventCode, string Text, LogExpectedOn ExpectedOn) tuple)
		=> new(tuple.EventCode, tuple.Text, tuple.ExpectedOn);

	public static implicit operator ExpectedLog((LogType LogType, string Text, LogExpectedOn ExpectedOn) tuple)
		=> new(tuple.LogType, tuple.Text, tuple.ExpectedOn);

	public static implicit operator ExpectedLog((Event EventCode, LogType LogType, string Text, LogExpectedOn ExpectedOn) tuple)
		=> new(tuple.EventCode, tuple.LogType, tuple.Text, tuple.ExpectedOn);

	public static implicit operator ExpectedLog((Event EventCode, string Text) tuple)
		=> new(tuple.EventCode, tuple.Text, expectedOn: null);

	public static implicit operator ExpectedLog((LogType LogType, string Text) tuple)
		=> new(tuple.LogType, tuple.Text, expectedOn: null);

	public static implicit operator ExpectedLog((Event EventCode, LogType LogType, string Text) tuple)
		=> new(tuple.EventCode, tuple.LogType, tuple.Text, ExpectedOn: null);

	public override string ToString() => Text;
}
