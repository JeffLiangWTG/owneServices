using System.Collections.Generic;
using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	public class DataTransformationManagerFixture
	{
		[Test]
		public void VersionBump()
		{
			var versionMgr = new Mock<ISchemaVersionManager>();
			versionMgr.Setup(x => x.GetVersion(It.IsAny<IDbTransaction>())).Returns(4);
			var connection = new Mock<IDbConnection>();
			var trans = new Mock<IDbTransaction>();
			connection.Setup(x => x.BeginTransaction()).Returns(trans.Object);
			var task = new Mock<IDataTransformationTask>();
			task.Setup(x => x.Version).Returns(5);
			var tasks = new DummyTransformationTasks(new[] { task.Object });

			var mgr = new DataTransformationManager(tasks, versionMgr.Object, connection.Object, 4);
			mgr.Execute();
			task.Verify(x => x.Run(It.IsAny<IDbTransaction>()), Times.Never);

			mgr = new DataTransformationManager(tasks, versionMgr.Object, connection.Object, 5);
			mgr.Execute();
			task.Verify(x => x.Run(trans.Object));
			versionMgr.Verify(x => x.UpdateVersion(5, trans.Object));
		}

		class DummyTransformationTasks : TransformationTasks
		{
			public DummyTransformationTasks(IEnumerable<IDataTransformationTask> tasks)
			{
				this.tasks = tasks;
			}
			readonly IEnumerable<IDataTransformationTask> tasks;

			protected override IEnumerable<IDataTransformationTask> Tasks => tasks;
		}
	}
}
