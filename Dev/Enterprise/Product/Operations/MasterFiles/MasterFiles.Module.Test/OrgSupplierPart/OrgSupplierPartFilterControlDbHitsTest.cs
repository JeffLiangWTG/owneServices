using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	class OrgSupplierPartFilterControlDbHitsTest : FilterControlDBHitsTestCase<OrgSupplierPartCollection, OrgSupplierPartFilterStripBusinessObject>
	{
		protected override Dictionary<string, int> GetBaseHits()
		{
			var dict = new Dictionary<string, int>
			{
				{ OrgSupplierPartSchema.Constants.TableName, 1 }
			};
			return dict;
		}

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var baseHits = GetBaseHits();
			var hitsDictionary = new Dictionary<string, Dictionary<string, int>>();

			var allOwnersHits = new Dictionary<string, int>(baseHits);
			allOwnersHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			allOwnersHits.Add(OrgPartRelationSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(OrgSupplierPart.AllOwners), allOwnersHits);

			var allSuppliersHits = new Dictionary<string, int>(baseHits);
			allSuppliersHits.Add(OrgPartRelationSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(OrgSupplierPart.AllSuppliers), allSuppliersHits);

			var allProductCategoriesHits = new Dictionary<string, int>(baseHits);
			allProductCategoriesHits.Add(OrgPartRelationSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(OrgSupplierPart.AllProductCategories), allProductCategoriesHits);

			var allLocalPartsHits = new Dictionary<string, int>(baseHits);
			allLocalPartsHits.Add(OrgPartRelationSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(OrgSupplierPart.AllLocalParts), allLocalPartsHits);

			var allLocalPartDescHits = new Dictionary<string, int>(baseHits);
			allLocalPartDescHits.Add(OrgPartRelationSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(OrgSupplierPart.AllLocalPartDescriptions), allLocalPartDescHits);

			var isBOMProductHits = new Dictionary<string, int>(baseHits);
			isBOMProductHits.Add(OrgPartBOMSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(OrgSupplierPart.IsBOMProduct), isBOMProductHits);

			return hitsDictionary;
		}

		protected override OrgSupplierPartCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new OrgSupplierPartCollection(factory);
		}

		protected override OrgSupplierPartFilterStripBusinessObject GetNewFilterBusinessObject() => new OrgSupplierPartFilterStripBusinessObject();

		protected override ZFilterStripControl GetNewFilterControl(OrgSupplierPartCollection collection, OrgSupplierPartFilterStripBusinessObject filterBizO) => new OrgSupplierPartFilterStripControl(collection, filterBizO);

		protected override void SetupData()
		{
			for (int i = 0; i < 10; i++)
			{
				var org = Factory.New<OrgHeader>();
				org.FillWithValidTestData();
				var part = CreateNewPart("Test Product " + i);
				var relation = part.RelatedOrganisations.AddNew();
				relation.OU_OH = org.PK;
				relation.OU_OP = part.PK;

				if (i > 4)
				{
					var bom = part.BillOfMaterials.AddNew();
					bom.OE_OP_Component = CreateNewPart("Test BOM " + i).PK;
				}
			}
		}

		protected override bool ShouldCheckForUnusedFetchHints(string columnName) => false;

		OrgSupplierPart CreateNewPart(string partNum)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partNum;
			return part;
		}
	}
}
