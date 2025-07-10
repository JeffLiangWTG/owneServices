using System;

namespace CargoWise.RefDbRepo.CHReferenceData.Services
{
	public class DownloadResult
	{
		public byte[] Content { get; set; }
		public DateTimeOffset LastModified { get; set; }
	}
}
