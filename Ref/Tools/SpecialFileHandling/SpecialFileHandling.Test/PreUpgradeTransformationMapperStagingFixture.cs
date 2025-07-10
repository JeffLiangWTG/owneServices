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
	public class PreUpgradeTransformationMapperStagingFixture
	{
		[Test]
		public void TestShouldUnshelveForDATCheckin()
		{
			var preUpgradeDataTransformationMapper = new PreUpgradeTransformationMapperStaging(new SpecialFileHandlerContext(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/RefDataRepo/_git/RefDataRepo", "master", "/").WrapForExtensions()));

			Assert.False(preUpgradeDataTransformationMapper.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit,
				$"/Staging/Schema/DbUpgrader/{PreUpgradeTransformationMapperFile}").Object));

			Assert.False(
				preUpgradeDataTransformationMapper.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit,
					$"/Staging/Schema/DbUpgrader/{PreUpgradeTransformationVersionCsFile}").Object));

			Assert.True(
				preUpgradeDataTransformationMapper.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit,
					$"/Staging/Schema/DbUpgrader/{DummyFile}").Object));
		}

		[Test]
		public void TestUpdateForDATCheckin_ShouldUpdateVersionWhenMapperTxtFileIsIncluded()
		{
			SetupWorkspace();

			var mockPendingChange = new MockPendingChange(TfsChangeType.Edit, PreUpgradeTransformationMapperTxtFileServerPath, PreUpgradeTransformationMapperTxtFileLocalTempFilePath);
			var dataTransformationMapper = new PreUpgradeTransformationMapperStaging(new SpecialFileHandlerContext(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/RefDataRepo/_git/RefDataRepo", "master", "/").WrapForExtensions()));
			dataTransformationMapper.UpdateForDATCheckin(workspace.Object, mockPendingChange.Object);

			Assert.AreEqual(
				PreUpgradeTransformationVersionFileContents.Replace("Version = 11", "Version = 12"),
				File.ReadAllText(PreUpgradeTransformationVersionFileLocalTempFilePath));

			Assert.AreEqual(
				ExpectedTransformationMapperCsFileContentsAfterDatUpdate,
				File.ReadAllText(PreUpgradeTransformationMapperCsFileLocalTempFilePath)
				);
		}

		void SetupWorkspace()
		{
			workspace = new Mock<IWorkspaceAccess>();
			workspace.Setup(w => w.GetPendingChanges(PreUpgradeTransformationVersionFileServerPath, TfsRecursionType.None)).Returns(new IPendingChange[0]);
			workspace.Setup(w => w.GetLocalItemForServerItem(PreUpgradeTransformationVersionFileServerPath)).Returns(PreUpgradeTransformationVersionFileLocalTempFilePath);
			workspace.Setup(w => w.GetLatest(new[] { PreUpgradeTransformationVersionFileServerPath }, TfsRecursionType.None, TfsGetOptions.None));
			workspace.Setup(w => w.PendEdit(PreUpgradeTransformationVersionFileServerPath)).Returns(1);

			workspace.Setup(w => w.GetPendingChanges(PreUpgradeTransformationMapperFileServerPath, TfsRecursionType.None)).Returns(new IPendingChange[0]);
			workspace.Setup(w => w.GetLocalItemForServerItem(PreUpgradeTransformationMapperFileServerPath)).Returns(PreUpgradeTransformationMapperCsFileLocalTempFilePath);
			workspace.Setup(w => w.GetLatest(new[] { PreUpgradeTransformationMapperFileServerPath }, TfsRecursionType.None, TfsGetOptions.None));
			workspace.Setup(w => w.PendEdit(PreUpgradeTransformationMapperFileServerPath)).Returns(1);
		}

		Mock<IWorkspaceAccess> workspace;

		string PreUpgradeTransformationMapperFile => "ShelfCheckinPreUpgradeTransformationMapper.txt";

		string PreUpgradeTransformationVersionCsFile => "StagingSchemaVersion.cs";

		string DummyFile => "Dummy.cs";

		string PreUpgradeTransformationMapperCsFile => "PreUpgradeTransformationTasks.cs";

		string PreUpgradeTransformationMapperFileServerPath => $"/Staging/Schema/DbUpgrader/{PreUpgradeTransformationMapperCsFile}";

		string PreUpgradeTransformationVersionFileServerPath => $"/Staging/Schema/DbUpgrader/{PreUpgradeTransformationVersionCsFile}";

		string PreUpgradeTransformationMapperTxtFileServerPath => $"/Staging/Schema/DbUpgrader/{PreUpgradeTransformationMapperFile}";

		string PreUpgradeTransformationVersionFileLocalTempFilePath { get; set; }

		string PreUpgradeTransformationMapperCsFileLocalTempFilePath { get; set; }

		string PreUpgradeTransformationMapperTxtFileLocalTempFilePath { get; set; }

		string PreUpgradeTransformationVersionLocalTempFileName => "temp_local_file_transformation_version.cs";

		string PreUpgradeTransformationMapperCsLocalTempFileName => "temp_local_file_transformation_mapper.cs";

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

			PreUpgradeTransformationVersionFileLocalTempFilePath = CreateTempFile(executableLocation, PreUpgradeTransformationVersionLocalTempFileName, PreUpgradeTransformationVersionFileContents);
			PreUpgradeTransformationMapperCsFileLocalTempFilePath = CreateTempFile(executableLocation, PreUpgradeTransformationMapperCsLocalTempFileName, PreUpgradeTransformationMapperCsFileContents);
			PreUpgradeTransformationMapperTxtFileLocalTempFilePath = CreateTempFile(executableLocation, "Temp" + PreUpgradeTransformationMapperFile, PreUpgradeTransformationMapperTxtFileContents);
		}

		[TearDown]
		protected void TearDown()
		{
			File.Delete(PreUpgradeTransformationVersionFileLocalTempFilePath);
			File.Delete(PreUpgradeTransformationMapperCsFileLocalTempFilePath);
			File.Delete(PreUpgradeTransformationMapperTxtFileLocalTempFilePath);
		}

		string PreUpgradeTransformationVersionFileContents
		{
			get
			{
				return @"
//------------------------------------------------------------------------------
// <autogenerated>
//     Manual changes to this file will be lost when it is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public static class StagingSchemaVersion
	{
		// The variable names in this region SHOULD NOT be changed.
		// The are matched in order to increase the version number.
		#region AutoGenerated Variables

		public const int Version = 11;

		#endregion
	}
}
";
			}
		}

		string PreUpgradeTransformationMapperCsFileContents
		{
			get { return @"
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	class PreUpgradeTransformationTasks : TransformationTasks
	{
		/// <summary>
		///  _____                _   _______ _     _       ______ _          _   _
		/// |  __ \              | | |__   __| |   (_)     |  ____(_)        | | | |
		/// | |__) |___  __ _  __| |    | |  | |__  _ ___  | |__   _ _ __ ___| |_| |
		/// |  _  // _ \/ _` |/ _` |    | |  | '_ \| / __| |  __| | | '__/ __| __| |
		/// | | \ \  __/ (_| | (_| |    | |  | | | | \__ \ | |    | | |  \__ \ |_|_|
		/// |_|  \_\___|\__,_|\__,_|    |_|  |_| |_|_|___/ |_|    |_|_|  |___/\__(_)
		///
		/// *******************************************************************
		/// **********      DO NOT CHANGE THIS FILE MANUALLY!!!      **********
		/// *******************************************************************
		/// 
		/// See ShelfCheckinPreUpgradeTransformationMapper.txt for instructions on how to add and map
		/// new transformations according to the SHELF CHECK-IN process
		/// 
		/// </summary>
		///
		protected override IEnumerable<IDataTransformationTask> Tasks
		{
			get
			{
				//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW
				yield return new RenameToZZZ_NKDataGroupColumnTask(8);
				yield return new TariffTypeNKTransformationTask(14);
				yield return new DropUnnamedConstraintTask(17);
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
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	class PreUpgradeTransformationTasks : TransformationTasks
	{
		/// <summary>
		///  _____                _   _______ _     _       ______ _          _   _
		/// |  __ \              | | |__   __| |   (_)     |  ____(_)        | | | |
		/// | |__) |___  __ _  __| |    | |  | |__  _ ___  | |__   _ _ __ ___| |_| |
		/// |  _  // _ \/ _` |/ _` |    | |  | '_ \| / __| |  __| | | '__/ __| __| |
		/// | | \ \  __/ (_| | (_| |    | |  | | | | \__ \ | |    | | |  \__ \ |_|_|
		/// |_|  \_\___|\__,_|\__,_|    |_|  |_| |_|_|___/ |_|    |_|_|  |___/\__(_)
		///
		/// *******************************************************************
		/// **********      DO NOT CHANGE THIS FILE MANUALLY!!!      **********
		/// *******************************************************************
		/// 
		/// See ShelfCheckinPreUpgradeTransformationMapper.txt for instructions on how to add and map
		/// new transformations according to the SHELF CHECK-IN process
		/// 
		/// </summary>
		///
		protected override IEnumerable<IDataTransformationTask> Tasks
		{
			get
			{
				//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW
				yield return new MyOwnPreUpgradeTransformation(12);
				yield return new RenameToZZZ_NKDataGroupColumnTask(8);
				yield return new TariffTypeNKTransformationTask(14);
				yield return new DropUnnamedConstraintTask(17);
			}
		}
	}
}
"; }
		}

		string PreUpgradeTransformationMapperTxtFileContents
		{
			get
			{
				return @"
// Place your data transformation class below
// eg.RenameToZZZ_NKDataGroupColumnTask
MyOwnPreUpgradeTransformation
			";
			}
		}
	}
}
