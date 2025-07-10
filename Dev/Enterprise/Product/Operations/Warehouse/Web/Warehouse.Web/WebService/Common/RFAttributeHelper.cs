using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Common
{
	public static class RFAttributeHelper
	{
		public static int GetRFAttributeConfirm(OrgSupplierPart supplierPart, OrgHeader client)
		{
			var rfAttributeConfirm = 0;
			if (supplierPart != null && client != null)
			{
				var partRelation = supplierPart.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);
				if (partRelation != null)
				{
					switch (partRelation.OU_RFAttributeConfirm)
					{
						case RFAttributeConfirmCode.Codes.PartAttribute1:
							rfAttributeConfirm = 1;
							break;
						case RFAttributeConfirmCode.Codes.PartAttribute2:
							rfAttributeConfirm = 2;
							break;
						case RFAttributeConfirmCode.Codes.PartAttribute3:
							rfAttributeConfirm = 3;
							break;
						case RFAttributeConfirmCode.Codes.SerialNumber:
							rfAttributeConfirm = 4;
							break;
					}
				}
			}
			return rfAttributeConfirm;
		}

		#region AddScannedReleaseCapturedSerialNumbers

		public static void AddScannedReleaseCapturedSerialNumbers(List<ScannedRCASerialNumbersPerProductInfo> scannedSerialNumbers, IEnumerable<WhsPickLine> pickLines)
		{
			if (pickLines.Any())
			{
				AddFetchHintsForAddScannedReleaseCapturedSerialNumbers(pickLines);

				var pickLinesWithRCAs = pickLines.Where(p => p.HasReleaseCapturedAttribs);

				foreach (var pickLine in pickLinesWithRCAs)
				{
					var product = pickLine.Product;
					var productPK = product.Parent.PK.ToGuid();
					var client = pickLine.Inventory.Client;
					var clientPK = client.PK.ToGuid();
					var matchedScannedSerialNumberByProduct = scannedSerialNumbers.SingleOrDefault(s => s.ProductPK == productPK && s.ClientPK == clientPK);

					var partAttributes = Enumerable.Empty<string>();

					if (!pickLine.WZ_ReleaseCapturedSerialNumber.IsEmpty)
					{
						partAttributes = new[] { pickLine.WZ_ReleaseCapturedSerialNumber.ToString() };
					}

					if (matchedScannedSerialNumberByProduct != null)
					{
						matchedScannedSerialNumberByProduct.ScannedRCASerialNumbers.AddRange(partAttributes);
					}
					else
					{
						scannedSerialNumbers.Add(new ScannedRCASerialNumbersPerProductInfo(productPK, clientPK, new List<string>(partAttributes)));
					}
				}
			}
		}

		static void AddFetchHintsForAddScannedReleaseCapturedSerialNumbers(IEnumerable<WhsPickLine> pickLines)
		{
			var factory = pickLines.First().Factory;

			var pickLinesWithRCAs = pickLines.Where(p => p.HasReleaseCapturedAttribs);
			if (pickLinesWithRCAs.Any())
			{
				foreach (var pickLine in pickLinesWithRCAs)
				{
					factory.AddFetchHint(WhsDocketLineSchema.PK, pickLine.WZ_WE_InventoryLine);
					factory.AddFetchHint(WhsInventoryViewSchema.PK, pickLine.WZ_WE_InventoryLine);
				}

				var inventories = pickLinesWithRCAs.DistinctBy(pl => pl.WZ_WE_InventoryLine).Select(pl => pl.Inventory);
				foreach (var inventory in inventories)
				{
					factory.AddFetchHint(OrgHeaderSchema.PK, inventory.WI_OH_Client);
					factory.AddFetchHint(OrgMiscServSchema.OM_OH, inventory.WI_OH_Client);
					factory.AddFetchHint(OrgSupplierPartSchema.PK, inventory.WI_OP);
					factory.AddFetchHint(OrgPartRelationSchema.OU_OP, inventory.WI_OP);
				}
			}
		}

		#endregion
	}
}
