using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class LoggerDecoratorTest : TestCase
	{
		public void TestGroupDuplicateLines()
		{
			var message = @"Rates Service: Could not create calculator from:
- &o0
  ChargeCode: FRT
  Unit: CN
  PerUnitRate: 455.0
  Currency: USD
- *o0
- *o0
- *o0
";

			AssertEquals(@"Rates Service: Could not create calculator from:
	- &o0
	  ChargeCode: FRT
	  Unit: CN
	  PerUnitRate: 455.0
	  Currency: USD
	- *o0 (x3)", LoggerDecorator.GroupDuplicateLines(message).ToStringWithDelimiterBetweenStrings(System.Environment.NewLine + '\t'));
		}

		public void TestCheckForDuplicateAndGroupLogs()
		{
			var testData = @"CHARGES CALCULATED:
WCAF: 1 Unit(s) @ AUD 0.00 / Unit
WCAF: 1 Unit(s) @ AUD 0.00 / Unit
WCAF1: 1 Unit(s) @ AUD 0.00 / Unit
WCAF1: 1 Unit(s) @ AUD 0.00 / Unit
WCAF10: 1 Unit(s) @ AUD 0.00 / Unit
WCAF10: 1 Unit(s) @ AUD 0.00 / Unit
WCAF2: 1 Unit(s) @ AUD 0.00 / Unit
WCAF2: 1 Unit(s) @ AUD 0.00 / Unit


WCAF3: 1 Unit(s) @ AUD 0.00 / Unit
WCAF3: 1 Unit(s) @ AUD 0.00 / Unit
WCAF4: 1 Unit(s) @ AUD 0.00 / Unit

WCAF4: 1 Unit(s) @ AUD 0.00 / Unit
WCAF5: 1 Unit(s) @ AUD 0.00 / Unit

WCAF5: 1 Unit(s) @ AUD 0.00 / Unit
WCAF6: 1 Unit(s) @ AUD 0.00 / Unit
WCAF6: 1 Unit(s) @ AUD 0.00 / Unit
WCAF7: 1 Unit(s) @ AUD 0.00 / Unit
WCAF7: 1 Unit(s) @ AUD 0.00 / Unit
WCAF8: 1 Unit(s) @ AUD 0.00 / Unit
WCAF8: 1 Unit(s) @ AUD 0.00 / Unit
WCAF9: 1 Unit(s) @ AUD 0.00 / Unit
WCAF9: 1 Unit(s) @ AUD 0.00 / Unit";

			var expectedMessage = @"CHARGES CALCULATED:
	WCAF: 1 Unit(s) @ AUD 0.00 / Unit (x2)
	WCAF1: 1 Unit(s) @ AUD 0.00 / Unit (x2)
	WCAF10: 1 Unit(s) @ AUD 0.00 / Unit (x2)
	WCAF2: 1 Unit(s) @ AUD 0.00 / Unit (x2)
	WCAF3: 1 Unit(s) @ AUD 0.00 / Unit (x2)
	WCAF4: 1 Unit(s) @ AUD 0.00 / Unit (x2)
	WCAF5: 1 Unit(s) @ AUD 0.00 / Unit (x2)
	WCAF6: 1 Unit(s) @ AUD 0.00 / Unit (x2)
	WCAF7: 1 Unit(s) @ AUD 0.00 / Unit (x2)
	WCAF8: 1 Unit(s) @ AUD 0.00 / Unit (x2)
	WCAF9: 1 Unit(s) @ AUD 0.00 / Unit (x2)";

			var actualMessage = LoggerDecorator.GroupDuplicateLines(testData).ToStringWithDelimiterBetweenStrings(System.Environment.NewLine + '\t');
			AssertEquals(expectedMessage, actualMessage);

			testData = @"CHARGES CALCULATED:
WCAF: 1 Unit(s) @ AUD 0.00 / Unit
WCAF1: 1 Unit(s) @ AUD 0.00 / Unit
WCAF10: 1 Unit(s) @ AUD 0.00 / Unit
WCAF2: 1 Unit(s) @ AUD 0.00 / Unit
WCAF3: 1 Unit(s) @ AUD 0.00 / Unit
WCAF4: 1 Unit(s) @ AUD 0.00 / Unit
WCAF5: 1 Unit(s) @ AUD 0.00 / Unit
WCAF6: 1 Unit(s) @ AUD 0.00 / Unit
WCAF7: 1 Unit(s) @ AUD 0.00 / Unit
WCAF8: 1 Unit(s) @ AUD 0.00 / Unit
WCAF9: 1 Unit(s) @ AUD 0.00 / Unit
WCAF: 1 Unit(s) @ AUD 0.00 / Unit
WCAF1: 1 Unit(s) @ AUD 0.00 / Unit
WCAF10: 1 Unit(s) @ AUD 0.00 / Unit
WCAF2: 1 Unit(s) @ AUD 0.00 / Unit
WCAF3: 1 Unit(s) @ AUD 0.00 / Unit
WCAF4: 1 Unit(s) @ AUD 0.00 / Unit
WCAF5: 1 Unit(s) @ AUD 0.00 / Unit
WCAF6: 1 Unit(s) @ AUD 0.00 / Unit
WCAF7: 1 Unit(s) @ AUD 0.00 / Unit
WCAF8: 1 Unit(s) @ AUD 0.00 / Unit
WCAF9: 1 Unit(s) @ AUD 0.00 / Unit";

			expectedMessage = @"CHARGES CALCULATED:
	WCAF: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF1: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF10: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF2: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF3: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF4: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF5: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF6: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF7: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF8: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF9: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF1: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF10: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF2: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF3: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF4: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF5: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF6: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF7: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF8: 1 Unit(s) @ AUD 0.00 / Unit
	WCAF9: 1 Unit(s) @ AUD 0.00 / Unit";

			actualMessage = LoggerDecorator.GroupDuplicateLines(testData).ToStringWithDelimiterBetweenStrings(System.Environment.NewLine + '\t');
			AssertEquals(expectedMessage, actualMessage);
		}

		public void TestYesNoWarningWithAnswersCache()
		{
			var testInteractor = new TestInteractor();
			var interactor = new LoggerDecorator(testInteractor);

			interactor.YesNoWarning("Some warning message 1");
			interactor.YesNoWarning("Some warning message 1");
			interactor.YesNoWarning("Some warning message 2");
			interactor.YesNoWarning("Some warning message 2");

			AssertEquals("Expect only 2 messages to be shown", 2, testInteractor.YesNoWarnings.Count);
			AssertEquals("Expect only 2 Information logs", 2, testInteractor.Information.Count);
			AssertEquals("Some warning message 1", testInteractor.YesNoWarnings[0]);
			AssertEquals("Some warning message 2", testInteractor.YesNoWarnings[1]);
		}
	}

	public class TestInteractor : IAutoRatingGUIInteractor
	{
		public void SuspendLayout() { }

		public void ResumeLayout() { }

		public void ShowPossibleMatchesDialog(AutoRater.AdditionalRatesNotifications notifications) { }

		public Quote SelectQuote(QuoteCollection quotes) { return null; }

		public ZDialogResult ShowNamedAccountMessageBox(string jobNamedAccount, string rateNamedAccount)
		{
			var message = $"During Autorating operation, Named Account '{rateNamedAccount}' is found on the rates to be applied, which is different from the Named Account '{jobNamedAccount}' input in the job. How would you like to proceed?";
			var result = UnitTestUserNotification.Instance.Show(message, "Test", ZMessageBoxButtons.OK, ZDialogResult.None);
			Log(LogType.Information, $"Selected {result}");
			return result;
		}

		public void SetJobInvoicingSecurityOverrideProvider(JobHeader job) { }

		public void ReportProgress(string currentProcessName, int totalItems, int done, TimeSpan eta, decimal speedPerTick)
		{
			var progressMessage = GetProgressMessage(currentProcessName, totalItems, done, eta, speedPerTick);
			ProgressReports.Add(progressMessage);
		}

		static string GetProgressMessage(string currentProcessName, int total, int done, TimeSpan eta, decimal speedPerTick)
		{
			var speedPerSecond = speedPerTick * 10000000;
			return string.Format("{0} {1}/{2} ETA: {3:hh\\:mm\\:ss} {4:f2}/s", currentProcessName, done, total, eta, speedPerSecond);
		}

		public IDisposable DeferErrorPopup()
		{
			return null;
		}

		public readonly List<string> ProgressReports = new List<string>();
		public readonly List<ZString> Errors = new List<ZString>();
		public readonly List<ZString> Information = new List<ZString>();
		public readonly List<ZString> Debugs = new List<ZString>();
		public readonly List<bool> Answers = new List<bool>();
		public readonly List<string> YesNoWarnings = new List<string>();

		public bool YesNoWarning(string message)
		{
			bool answer;
			if (Answers.Any())
			{
				answer = Answers[0];
				Answers.RemoveAt(0);
			}
			else
			{
				answer = true;
			}

			YesNoWarnings.Add(message);
			return answer;
		}

		public void ShowException(Exception ex)
		{
		}

		public virtual void ShowRatesAdditionResult(AutoRatesAdditionResult additionResult, string entityName, CostSell costOrSell)
		{
		}

		public IDisposable StartRatingSession()
		{
			return null;
		}

		public void Log(LogType type, string message)
		{
			Log(type, message, null);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			switch (type)
			{
				case LogType.Error:
					Errors.Add(message);
					break;
				case LogType.Warning:
					YesNoWarnings.Add(message);
					break;
				case LogType.Information:
					Information.Add("Information " + message);
					break;
				case LogType.Debug:
					Debugs.Add(message);
					break;
			}
		}

		public virtual IEnumerable<AutoRateInfo> SelectRate(IRatingContext ratingContext, RatingCriteria criteria)
		{
			return Array.Empty<AutoRateInfo>();
		}

		public PromptResponse<IRateEntry> SelectCompanyTariffRate(IRatingContext context, IEnumerable<IRateEntry> ratesToPickFrom)
		{
			return PromptResponse<IRateEntry>.Invalid;
		}
	}
}
