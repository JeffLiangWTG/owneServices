using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestClasses
{
	internal class LoaderConfig
	{
		public LoaderConfig(string fileName, string resource, DateTime fileDate)
		{
			FileName = fileName;
			Resource = resource;
			FileDate = fileDate;
		}

		public string FileName { get; set; }
		public string Resource { get; set; }
		public DateTime FileDate { get; set; }
	}

	internal class LoaderHelper
	{
		readonly List<LoaderConfig> configs;
		public LoaderHelper(List<LoaderConfig> configs)
		{
			this.configs = configs;
		}

		public IFileDownloader CreateDownloader(string fileName)
		{
			IFileDownloader result = null;

			var mapping = configs.FirstOrDefault(x => x.FileName == fileName);
			if (mapping != null && !string.IsNullOrEmpty(mapping.FileName))
			{
				result = MockFileDownloader.Create(mapping.Resource, mapping.FileDate);
			}

			return result;
		}
	}
}
