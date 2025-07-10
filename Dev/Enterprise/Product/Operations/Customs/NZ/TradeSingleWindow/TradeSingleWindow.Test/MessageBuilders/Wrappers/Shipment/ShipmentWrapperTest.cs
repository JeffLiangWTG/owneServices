using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class ShipmentWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new ShipmentWrapper(null);
		}

		public void TestICRConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "ICR CONSIGNEE";
			testShipment.ConsigneePK = consignee.PK;
			AssertEquals("ICR CONSIGNEE", icrWrappedShipment.Consignee.Name);
		}

		public void TestICRConsignor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "ICR CONSIGNOR";
			testShipment.ConsignorPK = consignor.PK;
			AssertEquals("ICR CONSIGNOR", icrWrappedShipment.Consignor.Name);
		}

		public void TestCREConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CRE CONSIGNEE";
			testShipment.ConsigneePK = consignee.PK;
			AssertEquals("CRE CONSIGNEE", creWrappedShipment.Consignee.Name);
		}

		public void TestCREConsignor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "CRE CONSIGNOR";
			testShipment.ConsignorPK = consignor.PK;
			AssertEquals("CRE CONSIGNOR", creWrappedShipment.Consignor.Name);
		}

		public void TestCREConsignmentItems()
		{
			AssertEquals(Enumerable.Empty<ICREConsignmentItem>(), creWrappedShipment.ConsignmentItems);
		}

		public void TestCRESequenceNumber()
		{
			AssertEquals("SequenceNumber - Each shipment is the consignment - i.e. only 1 sequence number", (ZShort)0, creWrappedShipment.SequenceNumber);
		}

		public void TestICRSequenceNumber()
		{
			AssertEquals("SequenceNumber - Each shipment is the consignment - i.e. only 1 sequence number", (ZShort)0, icrWrappedShipment.SequenceNumber);
		}

		public void TestDeliveryNotifyParties()
		{
			AssertEquals(Enumerable.Empty<IOrganisationSimple>(), icrWrappedShipment.DeliveryNotifyParties);
		}

		public void TestSupportingDocuments()
		{
			AssertEquals(0, icrWrappedShipment.SupportingDocuments.Count());
		}

		protected override void SetUp()
		{
			base.SetUp();
			testShipment = Factory.New<ForwardingShipment>();
			var wrappedShipment = new ShipmentWrapper(testShipment);
			creWrappedShipment = wrappedShipment;
			icrWrappedShipment = wrappedShipment;
		}

		ForwardingShipment testShipment;
		ICREConsignment creWrappedShipment;
		IICRConsignment icrWrappedShipment;
	}
}
