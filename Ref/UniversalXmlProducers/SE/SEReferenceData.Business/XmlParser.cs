using System.Text;

namespace CargoWise.RefDbRepo.SEReferenceData.Business
{
	public abstract class XmlParser<TXmlObject> where TXmlObject : class
	{
		public abstract string ConvertToXMLFile(TXmlObject[] items, string lastModified, string outputFileWithPath);

		protected StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		protected string MissingDescriptionError { get; set; } = string.Empty;

		StringBuilder errorBuilder;
	}
}
