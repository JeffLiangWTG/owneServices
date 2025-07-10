using System.IO;
using System.Reflection;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;
using Moq;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace CargoWise.RefDbRepo.SpecialFileHandling.Test
{
	[TestFixture]
	public abstract class SchemaVersionChangeRequestFixture
	{
		[Test]
		public void ShouldUnshelveForDatCheckin()
		{
			Assert.True(SchemaVersionRequest.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Add,
				$"{SchemaVersionTableSQLPath}/SomeDatabaseTableFile.sql").Object));
		}

		[Test]
		public void UpdateForDATCheckin_ShouldUpdateVersionNumber_WhenSQLFileIsModifiedInApplicableFolder()
		{
			SetupWorkspace();

			var updatedFiles = SchemaVersionRequest.UpdateForDATCheckin(workspace.Object,
				new MockPendingChange(TfsChangeType.Edit,
					$"{SchemaVersionTableSQLPath}/SomeDatabaseTableFile.sql").Object);

			var expectedFile = SchemaVersionFileServerPath;
			var expectedFileContents = SchemaVersionFileContents.Replace("Version = 10", "Version = 11");

			Assert.Contains(expectedFile, updatedFiles);
			Assert.AreEqual(expectedFileContents, File.ReadAllText(schemaVersionLocalTempFilePath));
		}

		[Test]
		public void UpdateForDATCheckin_ShouldNotUpdateVersionNumber_WhenNonSQLFileIsModifiedInApplicableFolder()
		{
			SetupWorkspace();

			var updatedFiles = SchemaVersionRequest.UpdateForDATCheckin(workspace.Object,
				new MockPendingChange(TfsChangeType.Edit,
					$"{SchemaVersionTableSQLPath}/SomeCSFile.cs").Object);

			Assert.IsNull(updatedFiles);
			Assert.AreEqual(SchemaVersionFileContents, File.ReadAllText(schemaVersionLocalTempFilePath));
		}

		[Test]
		public void UpdateForDATCheckin_ShouldNotUpdateVersionNumber_WhenSQLFileIsModifiedInNonApplicableFolder()
		{
			SetupWorkspace();

			var updatedFiles = SchemaVersionRequest.UpdateForDATCheckin(workspace.Object,
				new MockPendingChange(TfsChangeType.Edit, "/Service/SomeOtherDB/dbo/Tables/databaseTableFile.sql").Object);

			Assert.IsNull(updatedFiles);
			Assert.AreEqual(SchemaVersionFileContents, File.ReadAllText(schemaVersionLocalTempFilePath));
		}

		#region Implementation

		[SetUp]
		protected void SetUp()
		{
			var executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			schemaVersionLocalTempFilePath = Path.Combine(executableLocation, SchemaVersionLocalTempFileName);

			var localStream = File.Create(schemaVersionLocalTempFilePath);
			localStream?.Dispose();

			File.WriteAllText(schemaVersionLocalTempFilePath, SchemaVersionFileContents);
		}

		[TearDown]
		protected void TearDown()
		{
			File.Delete(schemaVersionLocalTempFilePath);
		}

		void SetupWorkspace()
		{
			workspace = new Mock<IWorkspaceAccess>();
			workspace.Setup(w => w.GetPendingChanges(SchemaVersionFileServerPath, TfsRecursionType.None)).Returns([]);
			workspace.Setup(w => w.GetLocalItemForServerItem(SchemaVersionFileServerPath)).Returns(schemaVersionLocalTempFilePath);
			workspace.Setup(w => w.GetLatest(new[] { SchemaVersionFileServerPath }, TfsRecursionType.None, TfsGetOptions.None));
			workspace.Setup(w => w.PendEdit(SchemaVersionFileServerPath)).Returns(1);
		}

		Mock<IWorkspaceAccess> workspace;
		string schemaVersionLocalTempFilePath;

		public abstract string SchemaVersionTableSQLPath { get; }
		public abstract string SchemaVersionLocalTempFileName { get; }
		public abstract string SchemaVersionFileServerPath { get; }

		public abstract string SchemaVersionFileContents { get; }
		public abstract SchemaVersionChangeRequest SchemaVersionRequest { get; }

		#endregion
	}
}
