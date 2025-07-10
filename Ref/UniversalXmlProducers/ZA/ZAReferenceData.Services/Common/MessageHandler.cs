using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Models;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Common
{
	public enum SupportedMessageTypes
	{
		Prodat,
		Gesmes
	}
	public class MessageHandler : IMessageHandler
	{
		readonly IStagingRepository StagingRepo;
		readonly ILogger Logger;
		readonly string inputFolder;
		readonly string inputFile;
		readonly SupportedMessageTypes messageType;
		bool disposedValue;

		public MessageHandler(IStagingRepository stagingRepo, string inputFolder, string inputFile, ILogger logger, SupportedMessageTypes messageType)
		{
			StagingRepo = stagingRepo;
			this.inputFolder = inputFolder;
			this.inputFile = inputFile;
			Logger = logger;
			this.messageType = messageType;
		}

		public IEnumerable<SourceDataMessage> GetMessages()
		{
			var results = new List<SourceDataMessage>();

			results = GetMessagesFromSourceData();
			results.AddRange(GetMessagesFromFiles());

			results = results.OrderBy(x => x.PublishDate)
								.ThenBy(x => x.CreatedDate)
								.ToList();

			RecalculateCreatedDate(results);

			return results;
		}

		static void RecalculateCreatedDate(List<SourceDataMessage> msgs)
		{
			if (msgs.Any())
			{
				var cd = NextSecond(msgs[0].CreatedDate);
				msgs[0].CreatedDate = cd;

				for (int i = 1; i < msgs.Count; i++)
				{
					if (msgs[i].CreatedDate <= cd)
					{
						cd = cd.AddSeconds(1);
					}
					else
					{
						cd = NextSecond(msgs[i].CreatedDate);
					}
					msgs[i].CreatedDate = cd;
				}
			}
		}

		static DateTime NextSecond(DateTime dt) => dt.Millisecond == 0 ? dt : dt.AddSeconds(1).AddMilliseconds(-dt.Millisecond);

		List<SourceDataMessage> GetMessagesFromSourceData()
		{
			var results = new List<SourceDataMessage>();

			try
			{
				Logger.LogInfo("Loading messages from SourceData");

				var contentType = GetContentTypeFromMessageType();

				results = StagingRepo.Get<SourceData>()
								.Where(x => x.SDA_Source == DataSourceConstants.Source.eHubZACustomsRepositoryQueue &&
											x.SDA_ContentType == contentType &&
											x.SDA_Status == StatusProvider.GetQUEStatus()).ToList()
								.Select(x => Convert(x)).ToList();
			}
#pragma warning disable CA1031
			catch (Exception ex)
			{
				Logger.LogError($"Failed to retrieve messages from SourceData: {ex.Message}");
			}
#pragma warning restore CA1031

			return results;
		}

		internal string GetContentTypeFromMessageType()
		{
			switch (messageType)
			{
				case SupportedMessageTypes.Gesmes:
					return DataSourceConstants.ContentType.ZA_Gesmes;
				case SupportedMessageTypes.Prodat:
					return DataSourceConstants.ContentType.ZA_ProDat;
				default:
					throw new NotSupportedException($"Message type '{messageType} not supported");
			}
		}

		List<SourceDataMessage> GetMessagesFromFiles()
		{
			var results = new List<SourceDataMessage>();

			var folder = inputFolder;
			try
			{
				Logger.LogInfo($"Checking input file: '{inputFile}'");
				if (File.Exists(inputFile) && IsValidFile(inputFile))
				{
					results.Add(ConvertFile(inputFile));
					Logger.LogInfo($"Valid input file found: '{inputFile}'");
				}

				if (IsValidInputFolder(folder))
				{
					Logger.LogInfo($"Loading messages from Input Folder: '{folder}'");

					foreach (var f in Directory.GetFiles(folder).Except(new string[] { inputFile } ))
					{
						if (IsValidFile(f))
						{
							results.Add(ConvertFile(f));
						}
					}
				}
			}
#pragma warning disable CA1031
			catch (Exception ex)
			{
				Logger.LogError($"Failed to retrieve messages from Input Folder {folder}: {ex.Message}");
			}
#pragma warning restore CA1031

			return results;
		}

		static SourceDataMessage ConvertFile(string filename)
		{
			var fi = new FileInfo(filename);

			return new SourceDataMessage
			{
				ID = Guid.Empty,
				Content = File.ReadAllText(filename),
				Status = "QUE",
				CreatedDate = fi.CreationTimeUtc,
				PublishDate = fi.LastWriteTimeUtc,
				Filename = fi.Name,
				FullPath = fi.FullName
			};
		}

		void IMessageHandler.UpdateStatus(SourceDataMessage prodatMsg, bool success) => UpdateStatus(prodatMsg, GetNewStatus(success));

		protected virtual void UpdateStatus(SourceDataMessage prodatMsg, string newStatus)
		{
			if (prodatMsg.ID != Guid.Empty)
			{
				var msg = StagingRepo.Get<SourceData>().Where(x => x.SDA_PK == prodatMsg.ID && x.SDA_Status == StatusProvider.GetQUEStatus()).FirstOrDefault();

				if (msg != null)
				{
					msg.SDA_Status = newStatus;
					StagingRepo.SaveChanges();
				}
				else
				{
					throw new ReferenceDataException($"Could not find message in SourceData with SDA_PK = '{prodatMsg.ID}' to update the status to '{newStatus}'.");
				}
			}
			else
			{
				MoveFileBasedOnStatus(prodatMsg, newStatus);
			}
		}

		void MoveFileBasedOnStatus(SourceDataMessage prodatMsg, string newStatus)
		{
			var subFolder = newStatus == StatusProvider.GetMERStatus() ? ProcessedFolder : FailedFolder;
			var newFile = GetDestinationFilename(Path.Combine(inputFolder, subFolder), prodatMsg.Filename);

			Logger.LogInfo($"Moving {prodatMsg.FullPath} to {newFile}");
			File.Copy(prodatMsg.FullPath, newFile, true);
			File.Delete(prodatMsg.FullPath);
		}

		static SourceDataMessage Convert(SourceData source)
		{
			return new SourceDataMessage
			{
				ID = source.SDA_PK,
				CreatedDate = source.SDA_CreatedTime,
				PublishDate = source.SDA_SourceTime ?? source.SDA_CreatedTime,
				Status = source.SDA_Status,
				Content = source.SDA_ContentText,
				Filename = string.Empty
			};
		}

		protected bool IsValidInputFolder(string folder)
		{
			var result = false;

			if (Directory.Exists(folder))
			{
				try
				{
					Directory.CreateDirectory(Path.Combine(folder, ProcessedFolder));
					Directory.CreateDirectory(Path.Combine(folder, FailedFolder));

					var tmp = Path.Combine(folder, Path.GetRandomFileName());
					File.WriteAllText(tmp, "");
					File.Delete(tmp);

					result = true;
				}
				catch (IOException ex)
				{
					Logger.LogError($"Could not read/write to {folder}: {ex.Message}");
				}
			}

			return result;
		}

		protected bool IsValidFile(string filename)
		{
			var content = File.ReadAllText(filename);

			var result = content.StartsWith("UNB+UNOB:", StringComparison.Ordinal);

			if (messageType == SupportedMessageTypes.Prodat)
			{
				result &= content.Contains("UNH+1+PRODAT:D:96B");
			}
			else if (messageType == SupportedMessageTypes.Gesmes)
			{
				result &= content.Contains("GESMES:D:96B");
			}

			return result;
		}

		protected static string GetDestinationFilename(string folder, string filename)
		{
			var newfilename = Path.Combine(folder, filename);

			var i = 0;

			var fi = new FileInfo(newfilename);

			while (fi.Exists)
			{
				i++;
				newfilename = Path.Combine(folder, $"{fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length)}[{i}]{fi.Extension}");
				fi = new FileInfo(newfilename);
			}

			return newfilename;
		}

		protected static string GetNewStatus(bool success) => success ? StatusProvider.GetMERStatus() : StatusProvider.GetERRStatus();

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					StagingRepo?.Dispose();
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		const string ProcessedFolder = "Processed";
		const string FailedFolder = "Failed";
	}
}
