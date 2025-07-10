using System;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public sealed class FileDetails : IFileDetails
	{
		public string Filename { get; set; }
		public long FileSize { get; set; }
		public string DownloadURL { get; set; }
		public ContentDetails Content { get; set; }

		DateTime IFileDetails.ExecutionDate => Content?.ExecutionDate ?? DateTime.MinValue;
	}
}
