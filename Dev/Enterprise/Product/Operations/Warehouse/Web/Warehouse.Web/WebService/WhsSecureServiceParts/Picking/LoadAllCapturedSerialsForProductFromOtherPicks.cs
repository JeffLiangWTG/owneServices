using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Load all captured serial numbers from other unfinalised picks.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsReleaseCapturedSerialsWebServiceResponse LoadAllCapturedSerialsForProductFromOtherPicks(Guid clientPK, Guid pickPK, Guid productPK)
		{
			return HandleWebServiceRequest<WhsReleaseCapturedSerialsWebServiceResponse>(response => LoadAllCapturedSerialsForProductFromOtherPicksCore(response, clientPK, pickPK, productPK));
		}

		void LoadAllCapturedSerialsForProductFromOtherPicksCore(WhsReleaseCapturedSerialsWebServiceResponse response, Guid clientPK, Guid pickPK, Guid productPK)
		{
			var product = WhsProduct.GetWhsProduct(Factory, productPK);
			var client = Factory.Load<OrgHeader>(clientPK);
			if (product != null && client != null)
			{
				response.ReleaseCapturedSerials = GetReleaseCapturedSerialsFromOtherPicks(pickPK, client.PK, productPK).ToArray();
			}
		}

		IEnumerable<string> GetReleaseCapturedSerialsFromOtherPicks(ZGuid pickPK, ZGuid clientPK, ZGuid productPK)
		{
			var releaseCapturedQuery = WhsReleaseLine.GetOtherReleaseCapturedSerialNumbersQuery(clientPK, productPK, pickPK, ZGuid.Empty, null);
			var releaseCapturedAttributes = Factory.Load<WhsPickLine>(releaseCapturedQuery);
			return releaseCapturedAttributes.Select(pl => pl.WZ_ReleaseCapturedSerialNumber.ToString());
		}
	}
}
