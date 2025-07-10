using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common.Test
{
	[TestFixture]
	public class ProcessMonitorFixture
	{
		[Test]
		public void Instance_ReturnsSingleton()
		{
			var instance1 = ProcessMonitor.Instance;
			var instance2 = ProcessMonitor.Instance;
			Assert.That(instance2, Is.SameAs(instance1));
		}

		[Test]
		public async Task DoesNotLogMemoryUsage_ForExitedProcess()
		{
			var process = new Mock<IProcessWrapper>();
			process.Setup(x => x.Id).Returns(1);
			process.Setup(x => x.HasExited).Returns(true);
			process.Setup(x => x.WorkingSet64).Returns(10120323L);
			var mockLogHelper = new Mock<ILogHelper>();

			using var processMonitor = new ProcessMonitor(new Mock<Job>().Object);
			processMonitor.AddProcessToMemoryMonitor(process.Object, mockLogHelper.Object);
			await Task.Delay(1500);
			processMonitor.KillAll(mockLogHelper.Object);
			mockLogHelper.Verify(x => x.LogMemoryInfo(It.IsAny<double>()), Times.Never);
		}

		[Test]
		public async Task LogsMemoryUsage_ForRunningProcess()
		{
			var process = new Mock<IProcessWrapper>();
			process.Setup(x => x.Id).Returns(1);
			process.SetupSequence(x => x.HasExited)
				.Returns(false).Returns(true);
			process.SetupSequence(x => x.WorkingSet64)
				.Returns(120323L).Returns(10120323L);
			var mockLogHelper = new Mock<ILogHelper>();

			using var processMonitor = new ProcessMonitor(new Mock<Job>().Object);
			processMonitor.AddProcessToMemoryMonitor(process.Object, mockLogHelper.Object);
			await Task.Delay(6000);
			processMonitor.KillAll(mockLogHelper.Object);
			process.Verify(x => x.Refresh(), Times.Once);
			mockLogHelper.Verify(x => x.LogMemoryInfo(It.IsAny<double>()), Times.Once);
		}

		[Test]
		public void KillAll()
		{
			var jobManager = new Mock<Job>();
			int err = 0;
			jobManager.Setup(x => x.Close(out err)).Verifiable();
			using var monitor = new ProcessMonitor(jobManager.Object);
			monitor.KillAll(new Mock<ILogHelper>().Object);
			jobManager.Verify(x => x.Close(out err), Times.Once);
		}
	}
}
