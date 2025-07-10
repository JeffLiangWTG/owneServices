using System;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Moq;
using NUnit.Framework;

namespace WinzorTestAdapter.Test
{
	class TestDiscovererTests
	{
		[Test]
		public void DiscoverTests()
		{
			var mockSink = new Mock<ITestCaseDiscoverySink>();
			new TestDiscoverer().DiscoverTests(new[] { TestClassesInfo.TestClassesSource }, null, null, mockSink.Object);
			mockSink.Verify(s => s.SendTestCase(It.Is<TestCase>(t => t.FullyQualifiedName == TestClassesInfo.PassingTest)), Times.Once());
		}

		[Test]
		public void DiscoveredTestsExecutorUri()
		{
			var mockSink = new Mock<ITestCaseDiscoverySink>();
			new TestDiscoverer().DiscoverTests(new[] { TestClassesInfo.TestClassesSource }, null, null, mockSink.Object);
			mockSink.Verify(s => s.SendTestCase(It.Is<TestCase>(t => t.ExecutorUri != new Uri(TestExecutor.ExecutorUriString))), Times.Never());
		}

		[Test]
		public void ExplicitTrait()
		{
			var mockSink = new Mock<ITestCaseDiscoverySink>();
			new TestDiscoverer().DiscoverTests(new[] { TestClassesInfo.TestClassesSource }, null, null, mockSink.Object);
			mockSink.Verify(s => s.SendTestCase(It.Is<TestCase>(t => t.FullyQualifiedName == TestClassesInfo.ExplicitTest && t.Traits.Any(p => p.Name == "Explicit"))), Times.Once());
			mockSink.Verify(s => s.SendTestCase(It.Is<TestCase>(t => t.FullyQualifiedName == TestClassesInfo.ExplicitWithWhiteSpaceTest && t.Traits.Any(p => p.Name == "Explicit"))), Times.Once());
		}

		[Test]
		public void ExplicitProperty()
		{
			var mockSink = new Mock<ITestCaseDiscoverySink>();
			new TestDiscoverer().DiscoverTests(new[] { TestClassesInfo.TestClassesSource }, null, null, mockSink.Object);
			mockSink.Verify(s => s.SendTestCase(It.Is<TestCase>(t => t.FullyQualifiedName == TestClassesInfo.ExplicitTest && t.Properties.Any(p => p.Label == "Explicit"))), Times.Once());
			mockSink.Verify(s => s.SendTestCase(It.Is<TestCase>(t => t.FullyQualifiedName == TestClassesInfo.ExplicitWithWhiteSpaceTest && t.Properties.Any(p => p.Label == "Explicit"))), Times.Once());
		}

		[Test]
		public void DisplayName()
		{
			var mockSink = new Mock<ITestCaseDiscoverySink>();
			new TestDiscoverer().DiscoverTests(new[] { TestClassesInfo.TestClassesSource }, null, null, mockSink.Object);
			mockSink.Verify(s => s.SendTestCase(It.Is<TestCase>(t => t.FullyQualifiedName == TestClassesInfo.PassingTest && t.DisplayName == "TestPass")), Times.Once());
		}

		[Test]
		public void SourceCodeDatCapabilityRequirement()
		{
			var mockSink = new Mock<ITestCaseDiscoverySink>();
			new TestDiscoverer().DiscoverTests(new[] { TestClassesInfo.TestClassesSource }, null, null, mockSink.Object);
			mockSink.Verify(s => s.SendTestCase(It.Is<TestCase>(t => t.FullyQualifiedName == TestClassesInfo.RequiresSourceCodeTest && t.Traits.Any(p => p.Name == "DAT:CapabilityRequirements" && p.Value.Contains("SOURCE_CODE", StringComparison.OrdinalIgnoreCase)))), Times.Once());
		}

		[Test]
		public void GuiCapabilityRequirementRemoved()
		{
			var mockSink = new Mock<ITestCaseDiscoverySink>();
			new TestDiscoverer().DiscoverTests(new[] { TestClassesInfo.TestClassesSource }, null, null, mockSink.Object);
			mockSink.Verify(s => s.SendTestCase(It.Is<TestCase>(t => t.FullyQualifiedName == TestClassesInfo.GuiTest && t.Traits.Any(p => p.Name == "DAT:CapabilityRequirements" && !p.Value.Contains("GUI", StringComparison.OrdinalIgnoreCase)))), Times.Once());
		}

		[Test]
		public void Net48CapabilityRequirementRemoved()
		{
			var mockSink = new Mock<ITestCaseDiscoverySink>();
			new TestDiscoverer().DiscoverTests(new[] { TestClassesInfo.TestClassesSource }, null, null, mockSink.Object);
			mockSink.Verify(s => s.SendTestCase(It.Is<TestCase>(t => t.FullyQualifiedName == TestClassesInfo.GuiTest && t.Traits.Any(p => p.Name == "DAT:CapabilityRequirements" && !p.Value.Contains("NET48", StringComparison.OrdinalIgnoreCase)))), Times.Once());
		}
	}
}
