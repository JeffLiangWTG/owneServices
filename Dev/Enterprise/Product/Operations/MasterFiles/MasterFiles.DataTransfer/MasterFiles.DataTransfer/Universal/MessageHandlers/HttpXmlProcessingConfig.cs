using Enterprise.Integration;

namespace Enterprise.MasterFiles.DataTransfer
{
	public class DefaultProcessingConfig : IHttpXmlProcessingConfig
	{
		public bool ThrowOnParsingError => false;
	}
}
