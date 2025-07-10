using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using WinzorFramework.JSInterop;

namespace Enterprise.MasterFiles.GUI
{
	internal class UpgradeDownloadObject : AbsDownloadObject
	{
		readonly UpgradeManager _upgradeMgr;
		readonly ZString _upgradeFileName;
		readonly Guid _upgradePackgePk;
		TempFile _tempFile;

		// ReSharper disable once ConvertToPrimaryConstructor
		public UpgradeDownloadObject(UpgradeManager mgr, ZString upgradeFilename, Guid packagePk)
		{
			_upgradeMgr = mgr;
			_upgradeFileName = upgradeFilename;
			_upgradePackgePk = packagePk;
		}

		public void Prepare()
		{
			// start to delete after 1s
			_tempFile = TempFileWithDelayedDelete.New();
			using var writer = File.OpenWrite(_tempFile.Filename);
			_upgradeMgr.DownloadUpgradePackageStream(_upgradePackgePk, writer);
		}
		public override string Name => _upgradeFileName;

		protected override async Task<Stream> OpenInStreamAsync()
		{
			await Task.CompletedTask;
			return File.OpenRead(_tempFile.Filename);
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				_tempFile?.Dispose();
				disposedValue = true;
			}
		}
	}
}
