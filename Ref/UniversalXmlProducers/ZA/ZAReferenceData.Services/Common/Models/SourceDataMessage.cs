using System;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Models
{
	public class SourceDataMessage
	{
		public Guid ID { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime PublishDate { get; set; }
		public string Content { get; set; }
		public string Status { get; set; }
		public string Filename { get; set; }
		public string FullPath { get; set; }
	}
}
