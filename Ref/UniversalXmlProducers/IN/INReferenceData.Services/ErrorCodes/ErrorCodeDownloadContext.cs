namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public sealed class ErrorCodeDownloadContext
	{
		public ErrorCodeDownloadContext(ErrorCodeType codeType, string subUrl, string tableXpath)
		{
			CodeType = codeType;
			SubUrl = subUrl;
			TableXpath = tableXpath;
		}

		public ErrorCodeType CodeType { get; }
		public string SubUrl { get; }
		public string TableXpath { get; }
	}
}
