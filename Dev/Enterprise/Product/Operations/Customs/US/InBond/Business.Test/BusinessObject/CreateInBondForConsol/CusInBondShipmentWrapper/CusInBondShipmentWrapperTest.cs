using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondShipmentWrapper))]
	sealed class CusInBondShipmentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShipmentNumber()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(wrapper.ShipmentNumberInfo);
			AssertEquals("Shipment Number", resourceStringData.Caption);

			AssertEquals("Shipment.JS_UniqueConsignRef", "S00000001", wrapper.ShipmentNumber);
		}

		public void TestHouseBillNumber()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(wrapper.HouseBillNumberInfo);
			AssertEquals("House Bill Number", resourceStringData.Caption);

			AssertEquals("Shipment.JS_HouseBill", "HB10000001", wrapper.HouseBillNumber);
		}

		public void TestConsigneeOrganization()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TSTCONSIGNEE";
			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;

			wrapper.ConsigneeOrganizationAddress = address.PK;
			AssertEquals(wrapper.ConsigneeOrgPK, org.PK);
		}

		public void TestConsignorPK()
		{
			var guid = ZGuid.NewZGuid();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = guid;

			AssertEquals(wrapper.ConsignorPK, guid);
		}

		public void TestGoodValue()
		{
			shipment.JS_GoodsValue = 100;
			AssertEquals(100, wrapper.GoodsValue.ToZInt());
		}

		public void TestActualWeight()
		{
			shipment.JS_ActualWeight = 100;
			AssertEquals(100, wrapper.ActualWeight.ToZInt());
		}

		public void TestWeightUnit()
		{
			shipment.JS_UnitOfWeight = "KG";
			AssertEquals("KG", wrapper.WeightUnit);
		}

		public void TestActualVolume()
		{
			shipment.JS_ActualVolume = 100;
			AssertEquals(100, wrapper.ActualVolume.ToZInt());
		}

		public void TestVolumeUnit()
		{
			shipment.JS_UnitOfVolume = "M3";
			AssertEquals("M3", wrapper.VolumeUnit);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return wrapper;
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000001";
			shipment.JS_HouseBill = "HB10000001";
			wrapper = new CusInBondShipmentWrapper(shipment);
		}
		ForwardingShipment shipment;
		CusInBondShipmentWrapper wrapper;
	}
}
