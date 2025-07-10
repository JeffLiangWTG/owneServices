using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Common.Testing
{
	[TestedType(typeof(WhsOrgSupplierPartCollection))]
	public class WhsOrgSupplierPartCollectionTest : OrgSupplierPartCollectionTest
	{
		#region TestConstructor_DefaultSupplierAndOwner

		public void TestConstructor_DefaultSupplierAndOwner()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "INTHEMSYD";
			org1.FillWithValidTestData();
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "INTHEMCAC";
			org2.FillWithValidTestData();
			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Supplier = org1.PK;
			link.OL_OH_Buyer = org2.PK;
			link.OL_ProductRelation = OrgRelationTypeList.Codes.Supplier;
			link.OL_RN_NKImporterCountry = "AU";
			Factory.Save();

			TestDefaultSupplierAndOwner(org1, org2, (s, o) => new WhsOrgSupplierPartCollection(Factory, s, o, false).AddNew());
			TestDefaultSupplierAndOwner(org1, org2, (s, o) => new WhsOrgSupplierPartCollection(Factory, s, o, "", "", false).AddNew());
			TestDefaultSupplierAndOwner(org1, org2, (s, o) => new WhsOrgSupplierPartCollection(Factory, s, o, false, PartFilterOptions.None).AddNew());
		}

		void TestDefaultSupplierAndOwner(OrgHeader supplier, OrgHeader owner, Func<OrgHeader, OrgHeader, OrgSupplierPart> addNew)
		{
			var newPart = addNew(supplier, owner);
			AssertNotNull("Should default the owner.", newPart.RelatedOrganisations.FindByOrganisationAndRelationship(owner, OrgPartRelation.RelationshipTypes.Owner));
		}

		#endregion

		#region GetCollectionToTest

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsOrgSupplierPartCollection(Factory);
		}

		#endregion
	}
}
