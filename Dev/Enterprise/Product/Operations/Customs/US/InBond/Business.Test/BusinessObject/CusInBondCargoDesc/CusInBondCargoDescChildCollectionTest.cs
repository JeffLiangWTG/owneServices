using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondCargoDescChildCollection))]
	sealed class CusInBondCargoDescChildCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHasACommodityWithPartNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew("APLU", "789654");
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			var childCommodities = commodity.ChildCommodities;
			AssertEquals(false, childCommodities.HasACommodityWithPartNumber);
			var childCommodity = childCommodities.AddNew();
			AssertEquals(false, childCommodities.HasACommodityWithPartNumber);
			childCommodity.BY_PartNumber = "SD";
			AssertEquals(true, childCommodities.HasACommodityWithPartNumber);
		}

		public void TestHasACommodityWithDifferentSupplier()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew("APLU", "789654");
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			var childCommodities = commodity.ChildCommodities;
			AssertEquals(false, childCommodities.HasACommodityWithDifferentSupplier(ZGuid.Empty));
			var childCommodity1 = childCommodities.AddNew();
			AssertEquals(false, childCommodities.HasACommodityWithDifferentSupplier(ZGuid.Empty));
			childCommodity1.BY_OH_Supplier = org2.PK;
			AssertEquals(false, childCommodities.HasACommodityWithDifferentSupplier(ZGuid.Empty));
			AssertEquals(true, childCommodities.HasACommodityWithDifferentSupplier(org1.PK));
			AssertEquals(false, childCommodities.HasACommodityWithDifferentSupplier(org2.PK));
			var childCommodity2 = childCommodities.AddNew();
			childCommodity2.BY_OH_Supplier = org2.PK;
			AssertEquals(false, childCommodities.HasACommodityWithDifferentSupplier(ZGuid.Empty));
			AssertEquals(true, childCommodities.HasACommodityWithDifferentSupplier(org1.PK));
			AssertEquals(false, childCommodities.HasACommodityWithDifferentSupplier(org2.PK));
			childCommodity2.BY_OH_Supplier = org1.PK;
			AssertEquals(false, childCommodities.HasACommodityWithDifferentSupplier(ZGuid.Empty));
			AssertEquals(true, childCommodities.HasACommodityWithDifferentSupplier(org1.PK));
			AssertEquals(true, childCommodities.HasACommodityWithDifferentSupplier(org2.PK));
		}

		public void TestClearParentDetailsWhenEnteredOnChild()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew("APLU", "789654");
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			commodity.BY_PartNumber = "PART1";
			commodity.BY_HarmonisedTariff = "1010101010";
			commodity.BY_GrossWeight = 130.2m;
			commodity.BY_GrossWeightUnit = "KG";
			commodity.BY_MonetaryValue = 1500m;
			var childCommodities = commodity.ChildCommodities;
			var childCommodity = (CusInBondCargoDesc)((IBindingList)childCommodities).AddNew();
			childCommodity.BY_GrossWeight = 1m;
			AssertEquals(true, childCommodities.IsNonCommittedCollectionElement(childCommodity));
			AssertEquals("PART1", commodity.BY_PartNumber);
			AssertEquals("1010101010", commodity.BY_HarmonisedTariff);
			AssertEquals(130.2m, commodity.BY_GrossWeight);
			AssertEquals("KG", commodity.BY_GrossWeightUnit);
			AssertEquals(1500m, commodity.BY_MonetaryValue);
			((ICancelAddNew)childCommodities).EndNew(0);
			AssertEquals(false, childCommodities.IsNonCommittedCollectionElement(childCommodity));
			AssertEquals("PART1", commodity.BY_PartNumber);
			AssertEquals("", commodity.BY_HarmonisedTariff);
			AssertEquals(0m, commodity.BY_GrossWeight);
			AssertEquals("", commodity.BY_GrossWeightUnit);
			AssertEquals(0m, commodity.BY_MonetaryValue);
			childCommodity = (CusInBondCargoDesc)((IBindingList)childCommodities).AddNew();
			childCommodity.BY_GrossWeight = 1m;
			AssertEquals(true, childCommodities.IsNonCommittedCollectionElement(childCommodity));
			AssertEquals("PART1", commodity.BY_PartNumber);
			AssertEquals("", commodity.BY_HarmonisedTariff);
			AssertEquals(0m, commodity.BY_GrossWeight);
			AssertEquals("", commodity.BY_GrossWeightUnit);
			AssertEquals(0m, commodity.BY_MonetaryValue);
			((IBusinessObjectInternals)childCommodity).Row[CusInBondCargoDesc.Schema.BY_PartNumber] = "PARE";
			AssertEquals("PART1", commodity.BY_PartNumber);
			AssertEquals("", commodity.BY_HarmonisedTariff);
			AssertEquals(0m, commodity.BY_GrossWeight);
			AssertEquals("", commodity.BY_GrossWeightUnit);
			AssertEquals(0m, commodity.BY_MonetaryValue);
			((ICancelAddNew)childCommodities).EndNew(1);
			AssertEquals(false, childCommodities.IsNonCommittedCollectionElement(childCommodity));
			AssertEquals("", commodity.BY_PartNumber);
			AssertEquals("", commodity.BY_HarmonisedTariff);
			AssertEquals(0m, commodity.BY_GrossWeight);
			AssertEquals("", commodity.BY_GrossWeightUnit);
			AssertEquals(0m, commodity.BY_MonetaryValue);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusInBondCargoDesc>();

		protected override System.Type GetExpectedCollectionType() => typeof(CusInBondCargoDescChildCollection);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			return new CusInBondCargoDescChildCollection(commodity);
		}
	}
}
