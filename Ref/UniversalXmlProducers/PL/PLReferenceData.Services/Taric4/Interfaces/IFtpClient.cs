namespace CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;

interface IFtpClient
{
	void CreateDirectory(string directoryName);
	string[] GetDirectoryContent(string directoryName = null);
	void DownloadFile(string fileName, string directoryName, string localDownloadDirectory);
	void DeleteFile(string fileName, string directoryName);
}
