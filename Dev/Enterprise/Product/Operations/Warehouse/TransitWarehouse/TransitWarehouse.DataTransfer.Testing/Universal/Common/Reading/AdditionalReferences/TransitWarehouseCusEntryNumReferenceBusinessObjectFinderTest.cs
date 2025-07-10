using System;
using CargoWise.Definitions;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<CusEntryNumber>))]
	public class TransitWarehouseCusEntryNumReferenceBusinessObjectFinderTest : MatchingBusinessObjectFinderTest
	{
		public override void TestFind()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var houseBillReference = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			var reference = new TransitAdditionalReferenceInfo
			{
				Type = WarehouseAdditionalReferenceTypes.Codes.HouseBill,
				Category = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				Value = "HSBReference"
			};

			var finder = new TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<CusEntryNumber>(reference);
			var matchingReference = finder.Find(parent);
			AssertEquals("Precondition - Only additional reference must be HSB.", 1, parent.CusEntryNumReferences.Count);
			AssertEquals(houseBillReference.PK, matchingReference.PK);
		}

		public void TestFindNull()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "PANReference", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			var reference = new TransitAdditionalReferenceInfo
			{
				Type = WarehouseAdditionalReferenceTypes.Codes.HouseBill,
				Category = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				Value = "HSBReference"
			};

			var finder = new TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<CusEntryNumber>(reference);
			AssertEquals("Precondition - Only additional reference must be HSB.", 1, parent.CusEntryNumReferences.Count);
			AssertExceptionThrown<ArgumentNullException>("Value cannot be null.", () => finder.Find(null));
		}

		public void TestFindByCategory()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var houseBillInMatchingCategory = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSBReference", "NAN");
			var reference = new TransitAdditionalReferenceInfo
			{
				Type = WarehouseAdditionalReferenceTypes.Codes.HouseBill,
				Category = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				Value = "HSBReference"
			};

			var finder = new TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<CusEntryNumber>(reference);
			var matchingReference = finder.Find(parent);
			AssertEquals("Precondition - Only additional reference must be HSB.", 1, parent.CusEntryNumReferences.Count);
			AssertEquals(houseBillInMatchingCategory.PK, matchingReference.PK);
		}

		public void TestFindByEntryType()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var houseBillReference = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "Reference", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, "Reference", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			var reference = new TransitAdditionalReferenceInfo
			{
				Type = WarehouseAdditionalReferenceTypes.Codes.HouseBill,
				Category = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				Value = "Reference"
			};

			var finder = new TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<CusEntryNumber>(reference);
			var matchingReference = finder.Find(parent);

			AssertEquals("Precondition - Only additional reference must be HSB.", 2, parent.CusEntryNumReferences.Count);
			AssertEquals(houseBillReference.PK, matchingReference.PK);
		}

		public void TestCannotFind()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.HouseBill, "Reference", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, "Reference", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			var reference = new TransitAdditionalReferenceInfo
			{
				Type = WarehouseAdditionalReferenceTypes.Codes.DestinationPort,
				Category = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				Value = "Reference"
			};

			var finder = new TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<CusEntryNumber>(reference);
			var matchingReference = finder.Find(parent);
			AssertEquals("Precondition - Additional References must remain the same.", 2, parent.CusEntryNumReferences.Count);
			AssertNull(matchingReference);
		}

		public void TestFindOther()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var otherReference = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.Other, "Other 1", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.Other, "Other 2", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			var reference = new TransitAdditionalReferenceInfo
			{
				Type = WarehouseAdditionalReferenceTypes.Codes.Other,
				Category = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				Value = "Other 1"
			};

			var finder = new TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<CusEntryNumber>(reference);
			var matchingReference = finder.Find(parent);
			AssertEquals("Precondition - Only additional reference must be OTH.", 2, parent.CusEntryNumReferences.Count);
			AssertEquals(otherReference.PK, matchingReference.PK);
		}

		public void TestCannotFindOther()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.Other, "Other 1", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.Other, "Other 2", TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			var reference = new TransitAdditionalReferenceInfo
			{
				Type = WarehouseAdditionalReferenceTypes.Codes.Other,
				Category = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				Value = "Other"
			};

			var finder = new TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<CusEntryNumber>(reference);
			var matchingReference = finder.Find(parent);
			AssertEquals("Precondition - Additional References must remain the same.", 2, parent.CusEntryNumReferences.Count);
			AssertNull(matchingReference);
		}

		public void TestFindCus()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var cusRef1 = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber, "CAN1001", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			var cusRef2 = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber, "CAN1002", TransitWarehouseReferenceCategories.Codes.CustomsReference);

			var reference = new TransitAdditionalReferenceInfo
			{
				Type = WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber,
				Category = TransitWarehouseReferenceCategories.Codes.CustomsReference,
				Value = "CAN1001"
			};

			var finder = new TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<CusEntryNumber>(reference);
			var matchingReference = finder.Find(parent);
			AssertEquals("There should have 2 Customs References.", 2, parent.CusEntryNumReferences.Count);
			AssertEquals("Precondition - The matching additional reference must be CAN1001.", cusRef1.PK, matchingReference.PK);
		}

		public void TestCannotFindCus()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectTransitWarehouseAdditionalReferences>();
			var cusRef1 = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber, "CAN1001", TransitWarehouseReferenceCategories.Codes.CustomsReference);
			var cusRef2 = Helper.CreateCustomsAdditionalReference(parent, WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber, "CAN1002", TransitWarehouseReferenceCategories.Codes.CustomsReference);

			var reference = new TransitAdditionalReferenceInfo
			{
				Type = WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber,
				Category = TransitWarehouseReferenceCategories.Codes.CustomsReference
			};

			var finder = new TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<CusEntryNumber>(reference);
			var matchingReference = finder.Find(parent);
			AssertEquals("There should have 2 Customs References.", 2, parent.CusEntryNumReferences.Count);
			AssertNull(matchingReference);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}
}
