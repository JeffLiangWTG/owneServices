using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CargoWise.RefDbRepo.VNReferenceData.Business
{
	public class CodeListMetadata
	{
		[JsonPropertyName("d")]
		public IEnumerable<CodeListMetadataItem> DataItems { get; set; }
	}

	public class CodeListMetadataItem
	{
		[JsonPropertyName("stt")]
		public int Order { get; set; }

		[JsonPropertyName("Id")]
		public int Id { get; set; }

		[JsonPropertyName("NgonNgu")]
		public string Language { get; set; }

		[JsonPropertyName("Code")]
		public string Code { get; set; }

		[JsonPropertyName("Ten")]
		public string Name { get; set; }

		[JsonPropertyName("Mota")]
		public string Description { get; set; }

		[JsonPropertyName("DanhmucId")]
		public int CategoryId { get; set; }

		[JsonPropertyName("Files")]
		public string FileDownloadUrl { get; set; }

		[JsonPropertyName("TrangThai")]
		public int Status { get; set; }

		[JsonPropertyName("NgayCapNhat")]
		public string UpdatedDate { get; set; }

		[JsonPropertyName("NguoiCapNhat")]
		public string UpdatedBy { get; set; }

		[JsonPropertyName("TenNhom")]
		public string GroupName { get; set; }
	}
}
