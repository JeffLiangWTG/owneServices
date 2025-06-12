namespace CargoWise.eHub.Products.NZCustoms.Common
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Lodgement")]
	public class LodgementDocument
	{
		public string FileName { get; set; }
		public string DocumentType { get; set; }
		public string DocumentMediaType { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public byte[] Content { get; set; }
	}
}