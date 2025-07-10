using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class StmUpgradeFormForAUTest : StmUpgradeForm
	{
		public StmUpgradeFormForAUTest(StmUpgradeCollectionContainer dataSource)
			: base(dataSource)
		{
		}

		public void UpdateCMRFilesInvalidCompressedTesting()
		{
			UpdateCMRFiles(CMRUpdateMethod.WebMainFile);
		}

		protected override ICMRReferenceFileUpgrader GetNewReferenceFileUpdater()
		{
			var mock = new Mock<ICMRReferenceFileUpgrader>();
			mock
				.Setup(x => x.ImportData(It.IsAny<ZBlob>()))
				.Throws(new FackInvalidCompressedFileException("Unable to decompress file."));
			return mock.Object;
		}
	}
}
