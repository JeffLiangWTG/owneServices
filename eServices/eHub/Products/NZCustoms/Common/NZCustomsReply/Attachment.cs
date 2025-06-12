using System.Xml.Serialization;

namespace CargoWise.eHub.Products.NZCustoms.Common
{
	public class Attachment
	{
		private string _filename;

		[XmlAttribute]
		public string Filename {
			get
			{
				return _filename?.TrimStart('<').TrimEnd('>');
			}
			set
			{
				_filename = value;
			}
		}
		[XmlAttribute]
		public string ContentType { get; set; }
		[XmlText]
		public string Content { get; set; }
	}
}
