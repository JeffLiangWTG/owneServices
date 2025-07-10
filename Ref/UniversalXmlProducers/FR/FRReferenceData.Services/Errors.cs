namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	/// <summary>
	/// Possible errors
	/// </summary>
	public enum Errors
	{
		No = 0,
		DownloadErr = -1,
		ExtractErr = -2,
		ReadXMLSErr = -3,
		ExportErr = -4,
		WebErr = -5,
		BadArgFileFormat = -6,
		IO = -7,
		Unknow = -100
	}
}
