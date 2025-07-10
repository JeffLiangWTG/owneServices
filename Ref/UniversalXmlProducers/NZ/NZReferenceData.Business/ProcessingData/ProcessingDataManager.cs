using System.IO;
using System.Text.Json;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public class ProcessingDataManager<TData> where TData : new()
	{
		public ProcessingDataManager(string processingDataDirectory, string filename)
		{
			this.ProcessingDataDirectory = processingDataDirectory;
			this.Filename = filename;
		}

		protected string Filename { get; }

		protected string ProcessingDataDirectory { get; }

		protected string ProcessingDataJsonFilePath => Path.Combine(ProcessingDataDirectory, Filename);

		public TData ProcessingData
		{
			get
			{
				if (processingData == null)
				{
					var filePath = ProcessingDataJsonFilePath;
					if (File.Exists(filePath))
					{
						processingData = JsonSerializer.Deserialize<TData>(File.ReadAllText(filePath));
					}
					if (processingData == null)
					{
						processingData = new TData();
					}
				}
				return processingData;
			}
		}
		TData processingData;

		public void SaveData()
		{
			Directory.CreateDirectory(ProcessingDataDirectory);
			var options = new JsonSerializerOptions { WriteIndented = true };
			File.WriteAllText(ProcessingDataJsonFilePath, JsonSerializer.Serialize(ProcessingData, options));
		}
	}
}
