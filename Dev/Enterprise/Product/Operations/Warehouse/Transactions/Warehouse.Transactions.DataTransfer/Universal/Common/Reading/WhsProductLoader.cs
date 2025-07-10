using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public static class WhsProductLoader
	{
		#region GetMatchedProductOrMaybeCreateNew

		public static OrgSupplierPart GetMatchedProductOrMaybeCreateNew(string lineType, ZInt? lineNumber, ZString productCode, OrderLine orderLineDO, OrgHeader client, bool allowNew, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var result = new OrgSupplierPart.Loader(factory.BOFactory).LoadAndReturnMatchingCount(productCode, client, null, false);
			OrgSupplierPart product = null;
			if (result.TotalMatchCount > 1)
			{
				throw new DataObjectReadFailureException(Res.GetString("4334CC4C-40C8-4CF3-8A66-72E582409802", "Cannot Import {0} Line {1}.\r\nMultiple Product matched: {2} for Client {3}.", lineType, lineNumber.GetValueOrDefault(), productCode, client.OH_Code));
			}
			else
			{
				if (result.TotalMatchCount == 1)
				{
					product = result.BestMatchingProduct;
				}

				if (product == null && allowNew)
				{
					product = factory.New<OrgSupplierPart>();
					PopulateProduct(product, productCode, orderLineDO, client);

					logger.Log(LogType.Information, Res.GetString("WhsDocketLineDataObjectReader|UnableToMatchProductMessage", "{0} New Product created.", GetUnableToMatchProductMessage(orderLineDO)));
				}
			}
			return product;
		}

		#endregion

		#region PopulateProduct

		static void PopulateProduct(OrgSupplierPart product, ZString productCode, OrderLine dataObject, OrgHeader client)
		{
			product.OP_PartNum = productCode;

			var productDescription = dataObject.Product != null ? dataObject.Product.Description.GetValueOrDefault() : productCode;
			product.OP_Desc = !productDescription.IsEmpty ? productDescription : productCode;

			PopulateProductClientRelationship(dataObject, product, client);

			var stockKeepingUnit = dataObject.OrderedQtyUnit.GetCodeAsUpperCase();
			if (!stockKeepingUnit.IsEmpty) // don't override default with ""
			{
				product.OP_StockKeepingUnit = stockKeepingUnit;
			}
		}

		#endregion

		#region PopulateProductClientRelationship

		static void PopulateProductClientRelationship(OrderLine dataObject, OrgSupplierPart product, OrgHeader client)
		{
			var relation = product.RelatedOrganisations.AddOwner(client);
			relation.OU_UsePartAttrib1 = !dataObject.PartAttribute1.GetValueOrDefault().IsEmpty;
			relation.OU_UsePartAttrib2 = !dataObject.PartAttribute2.GetValueOrDefault().IsEmpty;
			relation.OU_UsePartAttrib3 = !dataObject.PartAttribute3.GetValueOrDefault().IsEmpty;
			relation.OU_UsePackingDate = !dataObject.PackingDate.GetValueOrDefault().IsEmpty;
			relation.OU_UseExpiryDate = !dataObject.ExpiryDate.GetValueOrDefault().IsEmpty;
			relation.OU_UseSerialNumber = !dataObject.SerialNumber.GetValueOrDefault().IsEmpty;
		}

		#endregion

		#region GetUnableToMatchProductMessage

		public static string GetUnableToMatchProductMessage(OrderLine dataObject)
		{
			return Res.GetString("WhsProductLoader|UnableToMatchProductMessage", "Unable to match Product: {0}.", dataObject.Product.ToStringContents());
		}

		#endregion
	}
}
