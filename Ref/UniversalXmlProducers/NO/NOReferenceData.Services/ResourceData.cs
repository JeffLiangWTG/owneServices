namespace CargoWise.RefDbRepo.NOReferenceData.Services
{
	public class ResourceData
	{
		public ResourceData(string url, string fileName, string fileContent, string lastModified)
		{
			Url = url;
			FileName = fileName;
			FileContents = fileContent;
			LastModified = lastModified;
		}

		public string Url { get; }
		public string FileName { get; }
		public string FileContents { get; }
		public string LastModified { get; }
	}
}
