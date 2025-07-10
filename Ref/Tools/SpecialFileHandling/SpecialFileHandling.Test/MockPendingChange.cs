using System;
using System.IO;
using Dat.Integration.VersionControl;
using Moq;

namespace CargoWise.RefDbRepo.SpecialFileHandling.Test
{
	internal class MockPendingChange : Mock<IPendingChange>
	{
		readonly string actualFile;

		public MockPendingChange(TfsChangeType changeType, string serverItem, string actualFile = null)
		{
			Setup(c => c.ChangeType).Returns(changeType);
			Setup(c => c.ServerItem).Returns(serverItem);
			this.actualFile = actualFile;
			Setup(c => c.DownloadShelvedFile(It.IsAny<string>())).Callback((string localFileName) => DownloadShelvedFile(localFileName));
		}

		public void DownloadShelvedFile(string localFileName)
		{
			if (!string.IsNullOrEmpty(actualFile))
			{
				File.Copy(actualFile, localFileName, true);
				new FileInfo(localFileName).IsReadOnly = false;
			}
			else
			{
				throw new InvalidOperationException("actualFile not given");
			}
		}
	}
}
