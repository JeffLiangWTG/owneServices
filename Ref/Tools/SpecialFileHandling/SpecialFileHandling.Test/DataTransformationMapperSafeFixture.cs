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
	class DataTransformationMapperSafeFixture
	{
		[Test]
		public void TestShouldUnshelveForDATCheckin()
		{
			var dataTransformationMapper = new DataTransformationMapperSafe(new SpecialFileHandlerContext(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/RefDataRepo/_git/RefDataRepo", "master", "/").WrapForExtensions()));

			Assert.False(dataTransformationMapper.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit,
				$"$/RefDataRepo/Service/SchemaManagement/UpgradeManagerRunner/{DataTransformationMapperFile}").Object));

			Assert.False(
				dataTransformationMapper.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit,
					$"$/RefDataRepo/Service/SchemaManagement/UpgradeManagerRunner/{TransformationVersionCsFile}").Object));

			Assert.True(
				dataTransformationMapper.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit,
					$"$/RefDataRepo/Service/SchemaManagement/UpgradeManagerRunner/{TransformationMapperCsFile}").Object));

			Assert.True(
				dataTransformationMapper.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit,
					$"$/RefDataRepo/Service/SchemaManagement/UpgradeManagerRunner/{DummyFile}").Object));
		}

		[Test]
		public void TestUpdateForDATCheckin_ShouldUpdateVersionWhenMapperTxtFileIsIncluded()
		{
			SetupWorkspace();

			var mockPendingChange = new MockPendingChange(TfsChangeType.Edit, DataTransformationMapperFileServerPath, TransformationMapperTxtFileLocalTempFilePath);
			var dataTransformationMapper = new DataTransformationMapperSafe(new SpecialFileHandlerContext(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/RefDataRepo/_git/RefDataRepo", "master", "/").WrapForExtensions()));
			dataTransformationMapper.UpdateForDATCheckin(workspace.Object, mockPendingChange.Object);

			Assert.AreEqual(
				TransformationVersionFileContents.Replace("Version = 0", "Version = 1"),
				File.ReadAllText(TransformationVersionFileLocalTempFilePath));

			Assert.AreEqual(
				ExpectedTransformationMapperCsFileContentsAfterDatUpdate,
				File.ReadAllText(TransformationMapperCsFileLocalTempFilePath)
				);

		}

		void SetupWorkspace()
		{
			workspace = new Mock<IWorkspaceAccess>();
			workspace.Setup(w => w.GetPendingChanges(TransformationVersionFileServerPath, TfsRecursionType.None)).Returns(new IPendingChange[0]);
			workspace.Setup(w => w.GetLocalItemForServerItem(TransformationVersionFileServerPath)).Returns(TransformationVersionFileLocalTempFilePath);
			workspace.Setup(w => w.GetLatest(new[] { TransformationVersionFileServerPath }, TfsRecursionType.None, TfsGetOptions.None));
			workspace.Setup(w => w.PendEdit(TransformationVersionFileServerPath)).Returns(1);

			workspace.Setup(w => w.GetPendingChanges(TransformationMapperFileServerPath, TfsRecursionType.None)).Returns(new IPendingChange[0]);
			workspace.Setup(w => w.GetLocalItemForServerItem(TransformationMapperFileServerPath)).Returns(TransformationMapperCsFileLocalTempFilePath);
			workspace.Setup(w => w.GetLatest(new[] { TransformationMapperFileServerPath }, TfsRecursionType.None, TfsGetOptions.None));
			workspace.Setup(w => w.PendEdit(TransformationMapperFileServerPath)).Returns(1);
		}

		Mock<IWorkspaceAccess> workspace;

		string DataTransformationMapperFile => "ShelfCheckinDataTransformationMapper.txt";

		string TransformationVersionCsFile => "TransformationVersion.cs";

		string DummyFile => "Dummy.cs";

		string TransformationMapperCsFile => "DataTransformationTasks.cs";

		string TransformationMapperFileServerPath => $"/Service/SchemaManagement/UpgradeManagerRunner/{TransformationMapperCsFile}";

		string TransformationVersionFileServerPath => $"/Service/SchemaManagement/UpgradeManagerRunner/{TransformationVersionCsFile}";

		string DataTransformationMapperFileServerPath => $"/Service/SchemaManagement/UpgradeManagerRunner/{DataTransformationMapperFile}";

		string TransformationVersionFileLocalTempFilePath { get; set; }

		string TransformationMapperCsFileLocalTempFilePath { get; set; }

		string TransformationMapperTxtFileLocalTempFilePath { get; set; }

		string TransformationVersionLocalTempFileName => "temp_local_file_transformation_version.cs";

		string TransformationMapperCsLocalTempFileName => "temp_local_file_transformation_mapper.cs";

		string CreateTempFile(string folderPath, string fileName, string fileContent)
		{
			var filePath = Path.Combine(folderPath, fileName);
			var localStream = File.Create(filePath);
			localStream?.Dispose();
			File.WriteAllText(filePath, fileContent);
			return filePath;
		}

		[SetUp]
		protected void SetUp()
		{
			var executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			TransformationVersionFileLocalTempFilePath = CreateTempFile(executableLocation, TransformationVersionLocalTempFileName, TransformationVersionFileContents);
			TransformationMapperCsFileLocalTempFilePath = CreateTempFile(executableLocation, TransformationMapperCsLocalTempFileName, TransformationMapperCsFileContents);
			TransformationMapperTxtFileLocalTempFilePath = CreateTempFile(executableLocation, "Temp" + DataTransformationMapperFile, DataTransformationMapperTxtFileContents);
		}

		[TearDown]
		protected void TearDown()
		{
			File.Delete(TransformationVersionFileLocalTempFilePath);
			File.Delete(TransformationMapperCsFileLocalTempFilePath);
			File.Delete(TransformationMapperTxtFileLocalTempFilePath);
		}

		string TransformationVersionFileContents
		{
			get
			{
				return @"
namespace CargoWise.RefDbRepo.Service.DataTransformation
{
	static class TransformationVersion
	{
		public const int Version = 0;
	}
}
";
			}
		}

		string TransformationMapperCsFileContents
		{
			get { return @"
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	class DataTransformationTasks : TransformationTasks
	{
		protected override IEnumerable<IDataTransformationTask> Tasks
		{
			get {
				//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW
				yield return new PRCCTransformation(1);
				yield return new IncorrectEndDateTransformation(2);
			}
		}
	}
}
"; }
		}

		string ExpectedTransformationMapperCsFileContentsAfterDatUpdate
		{
			get { return @"
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	class DataTransformationTasks : TransformationTasks
	{
		protected override IEnumerable<IDataTransformationTask> Tasks
		{
			get {
				//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW
				yield return new MyOwnDataTransformation(1);
				yield return new PRCCTransformation(1);
				yield return new IncorrectEndDateTransformation(2);
			}
		}
	}
}
"; }
		}

		string DataTransformationMapperTxtFileContents
		{
			get
			{
				return @"
// Place your data transformation class below
// eg.PRCCTransformation
MyOwnDataTransformation
			";
			}
		}
	}
}
