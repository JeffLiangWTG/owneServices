using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ContainerDocumentSupporterShipment))]
	sealed class ContainerDocumentSupporterShipmentTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<ForwardingContainer>();
		}

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var container = Factory.New<ForwardingContainer>();
			container.LinkedShipment = shipment;

			base.DoSetupForDocument(command, container);
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			// only want to run the HIDDEN ones that point direct to a template, not the ones that point to other menus
			return !documentCommand.SU_MenuName.Contains("Cartage Advice") ||
				!documentCommand.SU_FilterList.Equals("\"<CurrentCompany.GC_Code>\"!=\"<CurrentCompany.GC_Code>\"") ||
				documentCommand.SU_MenuName.Contains("Combined Cartage Advice") ||
				documentCommand.SU_MenuName.Contains("Transhipment Cartage Advice");
		}

		public void TestGetDocBusinessObjects()
		{
			ForwardingContainer container = Factory.New<ForwardingContainer>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			container.LinkedShipment = shipment;
			DocumentWrapper[] wrappers = container.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.CartageAdvice, null);
			AssertEquals("Wrapper created for container with linked shipment", 1, wrappers.Length);
			AssertEquals("Linked CommonShipment should be same shipment", ((ForwardingContainer)wrappers[0].WrappedObject).LinkedShipment.PK, shipment.PK);
		}

		public void TestGetContactOrganisationToCartage()
		{
			var shipmentDeliveryCartage = Factory.NewWithValidTestData<OrgHeader>();
			var shipmentPickupCartage = Factory.NewWithValidTestData<OrgHeader>();
			shipmentDeliveryCartage.OH_FullName = "ShipmentDeliveryCartage";
			shipmentPickupCartage.OH_FullName = "ShipmentPickupCartage";

			var commonContainer = Factory.New<ForwardingContainer>();
			var contact = commonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);
			AssertNull("Attentioned to: NULL", contact);

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			commonContainer.LinkedShipment = shipment;
			shipment.DocsAndCartage.DeliveryCartageCoPK = shipmentDeliveryCartage.PK;
			shipment.DocsAndCartage.PickupCartageCoPK = shipmentPickupCartage.PK;

			contact = commonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);
			AssertEquals("Attentioned to: ShipmentDeliveryCartage", shipmentDeliveryCartage.OH_FullName, contact.OrgHeader.FullName);
			contact = commonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.DEP);
			AssertEquals("Attentioned to: ShipmentPickupCartage", shipmentPickupCartage.OH_FullName, contact.OrgHeader.FullName);
		}

		public void TestSupportedDataContext()
		{
			ForwardingContainer commonContainer = Factory.New<ForwardingContainer>();
			AssertEquals("Core.Constants.DataContext.CartageAdvice is Supported", true, commonContainer.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CartageAdvice)));
			AssertEquals("Core.Constants.DataContext.Service is Supported", true, commonContainer.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Service)));
		}

		public void TestBusinessContext()
		{
			ForwardingContainer commonContainer = Factory.New<ForwardingContainer>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			commonContainer.LinkedShipment = shipment;
			AssertEquals(BusinessContext.ForwardingContainer, commonContainer.DocumentSupporter.BusinessContext);
		}

		public void TestLocalPort()
		{
			ForwardingContainer commonContainer = Factory.New<ForwardingContainer>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			commonContainer.LinkedShipment = shipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKA";
			AssertEquals("Destination port expected.", "NZAKA", commonContainer.DocumentSupporter.LocalPort(ContactType.LocalTransport, DocumentDirection.ARV));
			AssertEquals("Destination port expected.", "AUSYD", commonContainer.DocumentSupporter.LocalPort(ContactType.LocalTransport, DocumentDirection.DEP));
		}

		public void TestForeignPort()
		{
			ForwardingContainer commonContainer = Factory.New<ForwardingContainer>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			commonContainer.LinkedShipment = shipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKA";
			AssertEquals("Origin port expected.", "NZAKA", commonContainer.DocumentSupporter.ForeignPort(ContactType.LocalTransport, DocumentDirection.DEP));
			AssertEquals("Origin port expected.", "AUSYD", commonContainer.DocumentSupporter.ForeignPort(ContactType.LocalTransport, DocumentDirection.ARV));
		}
	}
}
