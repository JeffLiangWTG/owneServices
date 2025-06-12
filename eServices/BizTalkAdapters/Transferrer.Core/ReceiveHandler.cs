using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Common.Logging;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Core
{
	public class ReceiveHandler : IReceiveHandler
	{
		ITransferrer transferrer;
		readonly string folder;
		readonly string fileMask;
		readonly string flagFile;
		readonly string sortOrder;
		readonly string emptyFileOption;
		readonly string renameBeforeDownload;
		readonly string moveBeforeDownload;
		readonly string renameAfterDownload;
		readonly string moveAfterDownload;
		bool disposed;

		public ReceiveHandler(Func<ITransferrer> transferrerFactory, XmlDocument configXml)
		{
			this.transferrer = transferrerFactory();
			folder = TransferrerHelpers.GetValue<string>(configXml, "/Config/Folder");
			fileMask = TransferrerHelpers.GetValueOrDefault(configXml, "/Config/FileMask", "*");
			flagFile = TransferrerHelpers.GetValueOrDefault<string>(configXml, "/Config/FlagFile");
			sortOrder = TransferrerHelpers.GetValueOrDefault<string>(configXml, "/Config/SortOrder");
			emptyFileOption = TransferrerHelpers.GetValueOrDefault<string>(configXml, "/Config/EmptyFileOption", "Ignore");
			renameBeforeDownload = TransferrerHelpers.GetValueOrDefault<string>(configXml, "/Config/RenameBeforeDownload");
			moveBeforeDownload = TransferrerHelpers.GetValueOrDefault<string>(configXml, "/Config/MoveBeforeDownload");
			renameAfterDownload = TransferrerHelpers.GetValueOrDefault<string>(configXml, "/Config/RenameAfterDownload");
			moveAfterDownload = TransferrerHelpers.GetValueOrDefault<string>(configXml, "/Config/MoveAfterDownload");
		}

		public async Task OpenAsync(XmlDocument configXml, ILog log, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();
			await transferrer.OpenAsync(configXml, log, cancelToken);
		}

		public async Task<List<TransferrerFileInfo>> ListServerFilesAsync(ILog log, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();

			var serverFiles = await GetServerFiles(log, cancelToken);

			cancelToken.ThrowIfCancellationRequested();

			log.DebugFormat("Found {0} matching file(s).", serverFiles.Count);

			if (serverFiles.Count == 0)
				return serverFiles;

			if (serverFiles.Any(f => f.Size == 0))
			{
				switch (emptyFileOption)
				{
					case "Ignore":
						log.Debug("Skipping empty files.");
						serverFiles.RemoveAll(f => f.Size == 0);
						break;
					case "Discard":
						log.Debug("Deleting empty files.");
						foreach (var file in serverFiles.Where(f => f.Size == 0).ToArray())
						{
							cancelToken.ThrowIfCancellationRequested();
							try
							{
								await transferrer.DeleteFileAsync(TransferrerHelpers.AltPathCombine(file.Folder, file.Name), log, cancelToken);
							}
							catch (Exception) { }
							serverFiles.Remove(file);
						}
						break;
				}
			}

			cancelToken.ThrowIfCancellationRequested();

			switch (sortOrder)
			{
				case "Name":
					log.Debug("Sorting by file name.");
					serverFiles.Sort((x, y) => x.Name.CompareTo(y.Name));
					break;
				case "Timestamp":
					log.Debug("Sorting by file timestamp.");
					serverFiles.Sort((x, y) => x.Timestamp.CompareTo(y.Timestamp));
					break;
			}

			cancelToken.ThrowIfCancellationRequested();

			log.DebugFormat("{0} file(s) available to download.", serverFiles.Count);

			return serverFiles;
		}

		public async Task<List<TransferrerFileInfo>> GetServerFiles(ILog log, CancellationToken cancelToken)
		{
			log.DebugFormat("Listing server files. Folder: '{0}' FileMask: '{1}'", folder, fileMask);
			var serverFiles = await transferrer.ListFilesAsync(folder, fileMask, log, cancelToken);
			cancelToken.ThrowIfCancellationRequested();

			if (!String.IsNullOrWhiteSpace(flagFile))
			{
				string flagFileMask = TransferrerHelpers.ConvertMarkupToFileMask(flagFile);
				var flagFileRegex = new Regex(TransferrerHelpers.ConvertFileMaskToRegexPattern(flagFileMask), RegexOptions.IgnoreCase);
				serverFiles.RemoveAll(f => flagFileRegex.IsMatch(f.Name));
				cancelToken.ThrowIfCancellationRequested();

				log.DebugFormat("Listing flag files. Folder: '{0}' FlagFileMask: '{1}'", folder, flagFileMask);
				var flagFiles = await transferrer.ListFilesAsync(folder, flagFileMask, log, cancelToken);
				cancelToken.ThrowIfCancellationRequested();
				return serverFiles.GroupJoin(flagFiles, 
					d => new { d.Folder, Name = TransferrerHelpers.ResolveMarkup(d.Name, flagFile) }, 
					f => new { f.Folder, f.Name }, 
					(d, f) => new { Download = d, FlagFile = f.Any() })
					.Where(f => f.FlagFile).Select(d => d.Download).ToList();
			}
			else if (!String.IsNullOrWhiteSpace(renameBeforeDownload) && String.IsNullOrWhiteSpace(moveBeforeDownload))
			{
				string renamedMask = TransferrerHelpers.ConvertMarkupToFileMask(renameBeforeDownload);
				var renamedRegex = new Regex(TransferrerHelpers.ConvertFileMaskToRegexPattern(renamedMask), RegexOptions.IgnoreCase);
				serverFiles.RemoveAll(f => renamedRegex.IsMatch(f.Name));
				cancelToken.ThrowIfCancellationRequested();

				log.DebugFormat("Renaming server files. RenameMarkup: '{0}'", renameBeforeDownload);
				foreach (var file in serverFiles)
				{
					cancelToken.ThrowIfCancellationRequested();
					await transferrer.RenameFileAsync(TransferrerHelpers.AltPathCombine(file.Folder, file.Name), TransferrerHelpers.AltPathCombine(file.Folder, TransferrerHelpers.ResolveMarkup(file.Name, renameBeforeDownload)), log, cancelToken);
				}

				cancelToken.ThrowIfCancellationRequested();
				log.DebugFormat("Listing renamed server files. RenamedMask: '{0}'", renamedMask);
				return await transferrer.ListFilesAsync(folder, renamedMask, log, cancelToken);
			}
			else if (!String.IsNullOrWhiteSpace(moveBeforeDownload))
			{
				log.DebugFormat("Moving server files. DestinationFolder: '{0}' RenameMarkup: '{0}'", moveBeforeDownload, renameBeforeDownload);
				foreach (var file in serverFiles)
				{
					cancelToken.ThrowIfCancellationRequested();
					await transferrer.RenameFileAsync(TransferrerHelpers.AltPathCombine(file.Folder, file.Name), TransferrerHelpers.AltPathCombine(moveBeforeDownload, TransferrerHelpers.ResolveMarkup(file.Name, renameBeforeDownload)), log, cancelToken);
				}

				cancelToken.ThrowIfCancellationRequested();
				string renamedMask = TransferrerHelpers.ConvertMarkupToFileMask(renameBeforeDownload);
				log.DebugFormat("Listing moved server files. Folder: '{0}' FileMask: '{1}'", moveBeforeDownload, renamedMask);
				return await transferrer.ListFilesAsync(moveBeforeDownload, renamedMask, log, cancelToken);
			}
			else
			{
				return serverFiles;
			}
		}

		public async Task<Stream> DownloadAsync(TransferrerFileInfo file, ILog log, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();
			return await transferrer.GetFileAsync(TransferrerHelpers.AltPathCombine(file.Folder, file.Name), log, cancelToken);
		}

		public async Task PostDownloadProcessingAsync(TransferrerFileInfo file, ILog log)
		{
			if (!String.IsNullOrWhiteSpace(flagFile))
			{
				await transferrer.DeleteFileAsync(TransferrerHelpers.AltPathCombine(file.Folder, TransferrerHelpers.ResolveMarkup(file.Name, flagFile)), log, CancellationToken.None);
				await transferrer.DeleteFileAsync(TransferrerHelpers.AltPathCombine(file.Folder, file.Name), log, CancellationToken.None);
			}
			else if (!String.IsNullOrWhiteSpace(renameAfterDownload + moveAfterDownload))
			{
				var afterFolder = String.IsNullOrWhiteSpace(moveAfterDownload) ? file.Folder : moveAfterDownload;
				var afterFileName = String.IsNullOrWhiteSpace(renameAfterDownload) ? file.Name : TransferrerHelpers.ResolveMarkup(file.Name, renameAfterDownload);
				await transferrer.RenameFileAsync(TransferrerHelpers.AltPathCombine(file.Folder, file.Name), TransferrerHelpers.AltPathCombine(afterFolder, afterFileName), log, CancellationToken.None);
			}
			else
			{
				await transferrer.DeleteFileAsync(TransferrerHelpers.AltPathCombine(file.Folder, file.Name), log, CancellationToken.None);
			}
		}

		public async Task CloseAsync(ILog log, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();
			await transferrer.CloseAsync(log, cancelToken);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (transferrer != null)
					{
						transferrer.Dispose();
					}
					transferrer = null;
					disposed = true;
				}
			}
		}
	}
}
