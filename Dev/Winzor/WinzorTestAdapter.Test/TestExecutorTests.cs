using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Moq;
using NUnit.Framework;

namespace WinzorTestAdapter.Test
{
	class TestExecutorTests
	{
		[Test]
		public void RunPassingTest()
		{
			var mockRunContext = new Mock<IRunContext>();
			var mockFramworkHandle = new Mock<IFrameworkHandle>();
			var testCase = new TestCase(TestClassesInfo.PassingTest, new Uri(TestExecutor.ExecutorUriString), TestClassesInfo.TestClassesSource);
			new TestExecutor().RunTests(new[] { testCase }, mockRunContext.Object, mockFramworkHandle.Object);
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.Outcome == TestOutcome.Passed)), Times.Once());
		}

		[Test]
		public void RunFailngTest()
		{
			var mockRunContext = new Mock<IRunContext>();
			var mockFramworkHandle = new Mock<IFrameworkHandle>();
			var testCase = new TestCase(TestClassesInfo.FailingTest, new Uri(TestExecutor.ExecutorUriString), TestClassesInfo.TestClassesSource);
			new TestExecutor().RunTests(new[] { testCase }, mockRunContext.Object, mockFramworkHandle.Object);
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.Outcome == TestOutcome.Failed)), Times.Once());
		}

		[Test]
		public void RunTestsBySource()
		{
			var mockRunContext = new Mock<IRunContext>();
			var mockFramworkHandle = new Mock<IFrameworkHandle>();
			new TestExecutor().RunTests(new[] { TestClassesInfo.TestClassesSource }, mockRunContext.Object, mockFramworkHandle.Object);
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.TestCase.FullyQualifiedName == TestClassesInfo.PassingTest)), Times.Once());
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.TestCase.FullyQualifiedName == TestClassesInfo.FailingTest)), Times.Once());
		}

		[Test]
		public void Cancel()
		{
			var mockRunContext = new Mock<IRunContext>();
			var mockFramworkHandle = new Mock<IFrameworkHandle>();
			var testExecutor = new TestExecutor();
			mockFramworkHandle.Setup(o => o.RecordResult(It.IsAny<TestResult>())).Callback(() => testExecutor.Cancel());
			var testCases = new[]
			{
				new TestCase(TestClassesInfo.PassingTest, new Uri(TestExecutor.ExecutorUriString), TestClassesInfo.TestClassesSource),
				new TestCase(TestClassesInfo.FailingTest, new Uri(TestExecutor.ExecutorUriString), TestClassesInfo.TestClassesSource),
			};
			testExecutor.RunTests(testCases, mockRunContext.Object, mockFramworkHandle.Object);
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.TestCase.FullyQualifiedName == TestClassesInfo.PassingTest && r.Outcome == TestOutcome.Passed)), Times.Once());
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.TestCase.FullyQualifiedName == TestClassesInfo.FailingTest && r.Outcome == TestOutcome.Skipped)), Times.Once());
		}

		[Test]
		public void SkipExplicit()
		{
			var mockRunContext = new Mock<IRunContext>();
			var mockFramworkHandle = new Mock<IFrameworkHandle>();
			new TestExecutor().RunTests(new[] { TestClassesInfo.TestClassesSource }, mockRunContext.Object, mockFramworkHandle.Object);
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.TestCase.FullyQualifiedName == TestClassesInfo.PassingTest && r.Outcome == TestOutcome.Passed)), Times.Once());
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.TestCase.FullyQualifiedName == TestClassesInfo.FailingTest && r.Outcome == TestOutcome.Failed)), Times.Once());
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.TestCase.FullyQualifiedName == TestClassesInfo.ExplicitTest && r.Outcome == TestOutcome.Skipped)), Times.Once());
		}

		[Test]
		public void RunExplicitWhenOnlyExplicit()
		{
			var mockRunContext = new Mock<IRunContext>();
			var mockFramworkHandle = new Mock<IFrameworkHandle>();
			var testCase = new TestCase(TestClassesInfo.ExplicitTest, new Uri(TestExecutor.ExecutorUriString), TestClassesInfo.TestClassesSource);
			testCase.Traits.Add("Explicit", "");
			var testCases = new[] { testCase };
			new TestExecutor().RunTests(testCases, mockRunContext.Object, mockFramworkHandle.Object);
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.TestCase.FullyQualifiedName == TestClassesInfo.ExplicitTest && r.Outcome == TestOutcome.Failed)), Times.Once());
		}

		[Test]
		public void DurationRecorded()
		{
			var mockRunContext = new Mock<IRunContext>();
			var mockFramworkHandle = new Mock<IFrameworkHandle>();
			var testCase = new TestCase(TestClassesInfo.SlowTest, new Uri(TestExecutor.ExecutorUriString), TestClassesInfo.TestClassesSource);
			new TestExecutor().RunTests(new[] { testCase }, mockRunContext.Object, mockFramworkHandle.Object);
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.Duration >= TimeSpan.FromMilliseconds(10))), Times.Once());
		}

		[Test]
		public void ExceptionInRunner()
		{
			var mockRunContext = new Mock<IRunContext>();
			var mockFramworkHandle = new Mock<IFrameworkHandle>();
			var testCase = new TestCase("NoSuchTest", new Uri(TestExecutor.ExecutorUriString), TestClassesInfo.TestClassesSource);
			new TestExecutor().RunTests(new[] { testCase }, mockRunContext.Object, mockFramworkHandle.Object);
			mockFramworkHandle.Verify(f => f.RecordResult(It.Is<TestResult>(r => r.Outcome == TestOutcome.Failed)), Times.Once());
		}
	}
}
