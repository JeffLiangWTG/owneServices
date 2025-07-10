using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolShipmentCollection))]
	sealed class ForwardingConsolShipmentCollectionTest : ConsolShipmentCollectionBOCollectionTest
	{
		public void TestSetCurrentConsol_OuterPackLinesChanged_Add()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.Consols.Add(consol1);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.CurrentConsolChanged = (oldValue, newValue) => { shipment.OuterPackLines.AddNew(); };

			shipment.Consols.Add(consol2);
			shipment.OuterPackLines.CurrentConsol = consol2;

			AssertEquals(true, shipment.OuterPackLines.ToList<ForwardingPackLine>().All(packLine => packLine.CurrentConsol == consol2));
		}

		public void TestSetCurrentConsol_OuterPackLinesChanged_Delete()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.Consols.Add(consol1);

			var packLine = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine.CurrentConsolChanged = (oldValue, newValue) => { shipment.OuterPackLines.Remove(packLine2); };

			shipment.Consols.Add(consol2);
			shipment.OuterPackLines.CurrentConsol = consol2;

			AssertEquals(true, shipment.OuterPackLines.ToList<ForwardingPackLine>().All(packLine => packLine.CurrentConsol == consol2));
		}

		public void TestAdd_ShouldSynchronizeGateways_ForNewShipment_WhenLoadingIsTrue()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			consol.Shipments.Load();

			shipment.Consols.AddFromDatabase(consol.PK);

			AssertEquals(2, shipment.Gateways.Count);
			AssertEquals(sendingForwarder.MainAddress.PK, shipment.Gateways[0].JSG_OA_ForwarderAddress);
			AssertEquals(receivingForwarder.MainAddress.PK, shipment.Gateways[1].JSG_OA_ForwarderAddress);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ForwardingConsolShipmentCollection(Factory.New<ForwardingConsol>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ForwardingShipment>();
		}
	}
}
