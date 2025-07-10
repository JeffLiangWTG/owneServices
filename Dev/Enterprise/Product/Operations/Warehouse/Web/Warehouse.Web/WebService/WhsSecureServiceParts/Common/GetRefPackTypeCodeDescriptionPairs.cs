using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetRefPackTypeCodeDescriptionPairs

		[WebMethod(Description = "Get RefPackType code description pairs")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public CodeDescriptionPairWebServiceResponse GetRefPackTypeCodeDescriptionPairs()
		{
			return HandleWebServiceRequest<CodeDescriptionPairWebServiceResponse>(result => result.CodeDescriptionPairs = GetRefPackTypeCodeDescriptionPairsCore());
		}

		CodeDescriptionPairInfoCollection GetRefPackTypeCodeDescriptionPairsCore()
		{
			var collection = new CodeDescriptionPairInfoCollection();
			collection.AddRange(GetRefPackTypeCodeDescriptionPairsHelper.GetAsCodeDescriptionPairWithStandardUnits(SecurityHeader.IsAndroidDevice, Factory));
			return collection;
		}

		#endregion
	}
}
