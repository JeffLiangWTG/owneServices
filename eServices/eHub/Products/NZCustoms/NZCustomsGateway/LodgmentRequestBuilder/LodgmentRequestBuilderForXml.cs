using CargoWise.eHub.Products.NZCustoms.Client.SubmitLodgement_v2;
using CargoWise.eHub.Products.NZCustoms.Common;
using Common.Logging;

namespace CargoWise.eHub.Products.NZCustoms.Gateway
{
	public class LodgmentRequestBuilderForXml : LodgmentRequestBuilder
	{
		public LodgmentRequestBuilderForXml(ILog logger, string message)
			: base(logger, message)
		{
		}

		protected override string MimeTypeQualifierCode
		{
			get { return Constants.XmlDeclarationMediaType; }
		}
	}
}