using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class ProcessServiceTaskTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestRunTaks()
		{
			var processor = new Mock<IProcessor>();
			var logger = new Mock<ILogger>();
			var task = new Mock<ProcessServiceTask>(processor.Object) { Object = { ServiceLogger = logger.Object } };
			processor.Setup(m => m.Process(It.IsAny<INotifications>(), It.IsAny<CancellationToken>()));
			task.Object.RunTask();
		}
	}
}
