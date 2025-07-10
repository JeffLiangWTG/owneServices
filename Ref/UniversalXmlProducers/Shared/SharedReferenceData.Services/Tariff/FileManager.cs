using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff
{
	public abstract class FileManager : IFileManager
	{
		protected FileManager()
		{
			xmlHelper = new XmlHelper();
		}
		XmlHelper xmlHelper;

		public IReadOnlyCollection<IFileDetails> GetFiles()
		{
			var contentFolder = GetContentFolder();
			DownloadFiles(contentFolder);

			return GetProcessingList(contentFolder);
		}

		public List<FileDetails> GetProcessingList(string contentFolder)
		{
			try
			{
				var fileList = GetFileProcessingList(contentFolder);

				fileList.ForEach(file =>
				{
					xmlHelper.ValidateFile(file.Filename);
					file.Content = GetContentDetails(file.Filename);
				});

				return SortAndFilter(fileList);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to get file processing list from {contentFolder}", ex);
			}
		}

		public ContentDetails GetContentDetails(string filePath)
		{
			using (var xmlStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				xmlStream.Seek(0, SeekOrigin.Begin);
				var result = xmlHelper.ReadNext<ContentDetails>(xmlStream, "ResultsInfo");

				if (result.EndDate == DateTime.MinValue && result.DatabaseDate > DateTime.MinValue)
				{
					result.EndDate = result.DatabaseDate;
				}

				return result;
			}
		}

		public static List<FileDetails> SortAndFilter(List<FileDetails> fileDetails)
		{
			fileDetails.RemoveAll(x => x.Content == null || x.Content.TotalRecords < 1);
			fileDetails.RemoveAll(x => fileDetails.Any(y => y.Content.StartDate <= x.Content.StartDate && y.Content.EndDate >= x.Content.EndDate && y.Content.ExecutionDate > x.Content.ExecutionDate));

			return fileDetails
					.OrderBy(x => x.Content.StartDate)
					.ThenBy(x => x.Content.EndDate)
					.ThenBy(x => x.Content.ExecutionDate)
					.ToList();
		}

		public static List<FileDetails> GetFileProcessingList(string contentPath)
		{
			try
			{
				var files = Directory.EnumerateFiles(contentPath, "*.xml", SearchOption.AllDirectories);

				return files.Select(x => new FileDetails { Filename = x }).ToList();
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to retrieve file list from {contentPath}", ex);
			}
		}

		protected abstract void DownloadFiles(string contentFolder);
		protected abstract string GetContentFolder();
	}
}
