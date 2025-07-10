using System.IO;
using System.Text.Json;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public class ProcessingDataManager<TData> where TData : class, new()
	{
		public ProcessingDataManager(string filename)
			: this (ApplicationConfig.OutputDirectory, filename)
		{
		}

		public ProcessingDataManager(string outputDirectory, string filename)
		{
			this.outputDirectory = outputDirectory;
			this.filename = filename;
		}

		string outputDirectory;
		string filename;

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

		/// <summary>
		/// Serializes ProcessingData content to the supplied filename and folder.
		/// Note: Will overwrite all previous data in the file.
		///   Each Parser should use a different filename to avoid data loss.
		/// </summary>
		public void SaveData()
		{
			Directory.CreateDirectory(ProcessingDataDirectory);
			var options = new JsonSerializerOptions { WriteIndented = true };
			File.WriteAllText(ProcessingDataJsonFilePath, JsonSerializer.Serialize(ProcessingData, options));
		}

		string ProcessingDataDirectory => Path.Combine(outputDirectory, "AUCustomsProcessingData");
		string ProcessingDataJsonFilePath => Path.Combine(ProcessingDataDirectory, filename);
	}
}
