using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using ClassificationTypeList = Enterprise.Customs.US.Business.ClassificationTypeList;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartCollection))]
	public class OrgSupplierPartCollectionTest : Customs.Business.Testing.OrgSupplierPartCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSupplierPartCollection(Factory);
		}

		public override void TestAddingNewPart()
		{
			var collection1 = new OrgSupplierPartCollection(Factory);
			var part1 = collection1.AddNew();
			AssertEquals("When collection is not linked to item, pivot should not be created", 0, part1.PivotsForBinding.Count);

			var item = Factory.NewWithValidTestData<CusUSLVItem>();
			var collection2 = new OrgSupplierPartCollection(Factory, item);
			var part2 = collection2.AddNew();
			AssertEquals("A pivot should have been created for the new part", 1, part2.PivotsForBinding.Count);
			AssertEquals("Pivot ChildType for LVS should be HTI", ClassificationTypeList.Codes.HTI, ((ICusClassPartPivotCollection<CusClassPartPivot>)part2.PivotsForBinding).Single().CI_ChildType);
		}

		#region TestAddNewPartFromItem

		public void TestAddNewPartFromItem_SetsConsigneeAsRelatedOrganization()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment.ConsigneeOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			consignment.SellerOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var item = consignment.CusUSLVItems.AddNew();
			var collection = new OrgSupplierPartCollection(Factory, item);
			var part = collection.AddNew();

			var orgRelation = part.RelatedOrganisations.SingleOrDefault() as OrgPartRelation;
			AssertNotNull("New relationship was created", orgRelation);
			AssertEquals("Consignee should be set as a related organisation", consignment.ConsigneeOrgPK, orgRelation.OU_OH);
			AssertEquals("Relationship Type should be Owner", OrgPartRelation.RelationshipTypes.Owner, orgRelation.OU_Relationship);
		}

		public void TestAddNewPartFromItem_SetsPivotTariffNum()
		{
			var item = Factory.NewWithValidTestData<CusUSLVItem>();
			item.ULI_Tariff = "666778";

			var collection = new OrgSupplierPartCollection(Factory, item);
			var part = collection.AddNew();
			var pivot = ((ICusClassPartPivotCollection<CusClassPartPivot>)part.PivotsForBinding).Single();

			AssertEquals("Pivot Tariff number should be set from Item", item.ULI_Tariff, pivot.CI_TariffNum);
		}

		public void TestAddNewPartFromItem_SetsCountryOfOrigin()
		{
			var item = Factory.NewWithValidTestData<CusUSLVItem>();
			item.ULI_RN_NKCountryOfOrigin = CountryCodes.Austria;

			var collection = new OrgSupplierPartCollection(Factory, item);
			var part = collection.AddNew();
			var pivot = ((ICusClassPartPivotCollection<CusClassPartPivot>)part.PivotsForBinding).Single();

			AssertEquals("Pivot Country of Origin should be set from Item", item.ULI_RN_NKCountryOfOrigin, pivot.CD_UC_NKCountryOfOrigin);
		}

		public void TestAddNewPartFromItem_SetsPivotADDApplicable()
		{
			var item = Factory.NewWithValidTestData<CusUSLVItem>();
			item.ULI_AntiDumping = true;

			var collection = new OrgSupplierPartCollection(Factory, item);
			var part = collection.AddNew();
			var pivot = ((ICusClassPartPivotCollection<CusClassPartPivot>)part.PivotsForBinding).Single();

			AssertEquals("Pivot ADD Applicable should be set from Item Anti-Dumping", item.ULI_AntiDumping, pivot.CD_ADDApplicable);
		}

		public void TestAddNewPartFromItem_SetsPivotCVDApplicable()
		{
			var item = Factory.NewWithValidTestData<CusUSLVItem>();
			item.ULI_Countervailing = true;

			var collection = new OrgSupplierPartCollection(Factory, item);
			var part = collection.AddNew();
			var pivot = ((ICusClassPartPivotCollection<CusClassPartPivot>)part.PivotsForBinding).Single();

			AssertEquals("Pivot CVD Applicable should be set from Item Countervailing", item.ULI_Countervailing, pivot.CD_CVDApplicable);
		}

		#region TestAddNewPartFromItem_SetsDisclaimReason

		void TestAddNewPartFromItem_SetsDisclaimReason(Action<CusUSLVItem, string> setItemDisclaimReason, Func<CusClassPartPivot, string> getPivotDisclaimReason)
		{
			const string PGADisclaimReason = "A";
			var item = Factory.NewWithValidTestData<CusUSLVItem>();
			setItemDisclaimReason(item, PGADisclaimReason);

			var collection = new OrgSupplierPartCollection(Factory, item);
			var part = collection.AddNew();
			var pivot = ((ICusClassPartPivotCollection<CusClassPartPivot>)part.PivotsForBinding).Single();

			AssertEquals("Pivot CVD Applicable should be set from Item Countervailing", PGADisclaimReason, getPivotDisclaimReason(pivot));
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_ACEFDA()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.ACEFDAWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_ACEFDADisclaimReason);
		}
		public void TestAddNewPartFromItem_SetsDisclaimReason_AMS()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.AMSWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_AMSDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_NOP()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.NOPWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_NOPDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_APHIS()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.APHISWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_APHISDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_CPSC()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.CPSCWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_CPSCDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_DEA()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.DEAWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_DEADisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_FWS()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.FWSWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_FWSDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_Lacey()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.LaceyActWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_LaceyActDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_NHTSA()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.NHTSAWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_NHTSADisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_NMFS370()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.NMFS370Wrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_NMFS370DisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_NMFSAMR()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.NMFSAMRWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_NMFSAMRDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_NMFSHMS()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.NMFSHMSWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_NMFSHMSDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_ODS()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.ODSWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_ODSDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_OMC()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.OMCWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_OMCDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReasonPST()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.PSTWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_PSTDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_TSCA()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.TSCAWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_TSCADisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_TTB()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.TTBWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_TTBDisclaimReason);
		}

		public void TestAddNewPartFromItem_SetsDisclaimReason_VNE()
		{
			TestAddNewPartFromItem_SetsDisclaimReason(
				(item, reason) => item.VNEWrapper.DisclaimReason = reason,
				(pivot) => pivot.CD_VNEDisclaimReason);
		}

		#endregion

		#endregion
	}
}
