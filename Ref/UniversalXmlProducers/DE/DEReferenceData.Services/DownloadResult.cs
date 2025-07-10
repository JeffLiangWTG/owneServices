using System;

namespace CargoWise.RefDbRepo.DEReferenceData.Services
{
	public class DownloadResult
	{
		public DateTimeOffset LastModified { get; set; }
		public string Content { get; set; }
	}
}
