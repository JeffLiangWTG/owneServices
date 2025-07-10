using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetProductOwners

		[WebMethod(Description = "Gets list of product owners")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public OrgHeadersWebServiceResponse GetProductOwners(string productCodeOrBarcode, string palletId = null)
		{
			return HandleWebServiceRequest<OrgHeadersWebServiceResponse>(result => GetProductOwnersCore(result, productCodeOrBarcode, palletId));
		}

		#region GetProductOwnersCore

		void GetProductOwnersCore(OrgHeadersWebServiceResponse response, string productCodeOrBarcode, string palletId)
		{
			if (!productCodeOrBarcode.IsNullOrEmpty())
			{
				var allParts = new List<OrgSupplierPart>();
				allParts.AddRange(WebServiceHelper.GetPartsByPartNum(Factory, productCodeOrBarcode));
				allParts.AddRange(WebServiceHelper.GetPartsByBarcode(Factory, productCodeOrBarcode));

				if (allParts.Count != 0)
				{
					response.Organisations = GetUniqueProductOwners(allParts, palletId);
				}
				else
				{
					response.LogBusinessValidationError(Res.GetString("d21efa54-e6c6-45a1-954b-57f94ddc6d33", "Please provide a valid product code or barcode."));
				}
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("d21efa54-e6c6-45a1-954b-57f94ddc6d33", "Please provide a valid product code or barcode."));
			}
		}

		#endregion

		#region GetUniqueProductOwners

		OrgHeaderInfo[] GetUniqueProductOwners(List<OrgSupplierPart> allParts, string palletId)
		{
			var allRelatedOrgs = new Dictionary<ZGuid, OrgHeaderInfo>();
			if (!palletId.IsNullOrEmpty())
			{
				var query = new ZQuery(WhsInventoryViewSchema.WI_PalletID, palletId);
				query.AddToFilter(WhsInventoryViewSchema.WI_OP, allParts.Select(p => p.PK).ToArray());

				var inventoryLines = Factory.Load<WhsInventoryView>(query);

				var orgPKs = inventoryLines.Where(i => i.WI_AvailableToTransferQuantity > 0).Select(x => x.WI_OH_Client).Distinct().ToList();

				if (orgPKs.Count == 1)
				{
					var org = Factory.Load<OrgHeader>(orgPKs[0]);
					allRelatedOrgs.Add(org.PK, new OrgHeaderInfo(org));
				}
			}

			if (allRelatedOrgs.Count != 1)
			{
				var relatedOrganisations = allParts.SelectMany(part => part.RelatedOrganisations).Cast<OrgPartRelation>();
				var partRelations = relatedOrganisations.Where(organisation => organisation.IsOwner || organisation.IsBoth);

				foreach (var partRelation in partRelations)
				{
					var org = partRelation.Organisation;
					if (!allRelatedOrgs.ContainsKey(partRelation.OU_OH) && org.OH_IsActive)
					{
						allRelatedOrgs.Add(partRelation.OU_OH, new OrgHeaderInfo(org));
					}
				}
			}

			return allRelatedOrgs.Values.ToArray();
		}

		#endregion

		#endregion
	}
}
