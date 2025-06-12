using CargoWise.eHub.Products.NZCustoms.Common;
using Common.Logging;

namespace CargoWise.eHub.Products.NZCustoms.Gateway
{
	public class LodgmentRequestBuilderForLegacy : LodgmentRequestBuilder
	{
		public LodgmentRequestBuilderForLegacy(ILog logger, string message)
			: base(logger, message)
		{
		}

		protected override string MimeTypeQualifierCode
		{
			get { return Constants.LegacyDeclarationMediaType; }
		}
	}
}