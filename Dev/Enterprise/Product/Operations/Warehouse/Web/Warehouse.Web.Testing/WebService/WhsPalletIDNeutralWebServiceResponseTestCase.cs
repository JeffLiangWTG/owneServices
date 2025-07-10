using System.Collections.Generic;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsPalletIDNeutralWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Properties

		protected override void TestPropertiesCore()
		{
			base.TestPropertiesCore();

			var response = new WhsPalletIDNeutralWebServiceResponse();
			AssertNull(response.NewPickLine);
			AssertNull(response.CompletePalletPickingPallets);

			var pickLineInfo = new WhsPickLineInfo();
			response.NewPickLine = pickLineInfo;
			AssertEquals(pickLineInfo, response.NewPickLine);

			var completePallets = new List<string>();
			response.CompletePalletPickingPallets = completePallets;
			AssertEquals(completePallets, response.CompletePalletPickingPallets);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsPalletIDNeutralWebServiceResponse();
		}

		protected new WhsPalletIDNeutralWebServiceResponse Response
		{
			get { return (WhsPalletIDNeutralWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
