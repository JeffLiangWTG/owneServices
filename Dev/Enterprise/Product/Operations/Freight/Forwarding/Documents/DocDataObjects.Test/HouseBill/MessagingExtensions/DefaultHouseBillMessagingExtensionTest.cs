using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Moq;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class DefaultHouseBillMessagingExtensionTest : TestCaseWithFactory
	{
		public void TestGetXmlNamespace()
		{
			var template = new Mock<IHouseBillTemplate>();
			template.SetupGet(d => d.DataContext).Returns("BillOfLading");

			var documentPivot = new Mock<IDocumentPivot>();
			documentPivot.SetupGet(d => d.TemplatePK).Returns(Guid.Parse("40e652ce-ea13-4897-bbad-a4954921b1f3"));

			var shipment = Factory.New<ForwardingShipment>();
			shipment.IsEditingElectronicBOL = false;

			AssertNull(new DefaultHouseBillMessagingExtension(shipment).GetXmlNamespace());
			AssertNullOrEmpty(new DefaultHouseBillMessageInstructions(documentPivot.Object, template.Object).XmlNamespace);

			shipment.IsEditingElectronicBOL = true;

			AssertEquals("/eHBL/1", new DefaultHouseBillMessagingExtension(shipment).GetXmlNamespace());
			AssertEquals("/eHBL/1", new ElectronicBOLMessageInstructions(template.Object).XmlNamespace);
		}

		public void TestGetAdditionalParametersForEvent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var extension = new DefaultHouseBillMessagingExtension(shipment);
			var parameters = extension.GetAdditionalParametersForEvent();

			AssertEquals(2, parameters.Length);
			AssertEquals(Core.Constants.BillStatusUpdatedTypes.OriginalBillSentForPublication, parameters.FirstOrDefault(x => x.Key == EventConstants.EventReferenceParameters.Codes.Type).Value);
			AssertEquals("Original", parameters.FirstOrDefault(x => x.Key == EventConstants.EventReferenceParameters.Codes.Action).Value);

			shipment.JS_ElectronicBillOfLadingStatus = Freight.Business.FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
			extension = new DefaultHouseBillMessagingExtension(shipment);
			parameters = extension.GetAdditionalParametersForEvent();

			AssertEquals(2, parameters.Length);
			AssertEquals(Core.Constants.BillStatusUpdatedTypes.OriginalBillSentForPublication, parameters.FirstOrDefault(x => x.Key == EventConstants.EventReferenceParameters.Codes.Type).Value);
			AssertEquals("Amendment", parameters.FirstOrDefault(x => x.Key == EventConstants.EventReferenceParameters.Codes.Action).Value);
		}

		public void TestIsSendingAmendment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var extension = new DefaultHouseBillMessagingExtension(shipment);

			Assert(!extension.IsSendingAmendment().Value);

			shipment.JS_ElectronicBillOfLadingStatus = Freight.Business.FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
			extension = new DefaultHouseBillMessagingExtension(shipment);

			Assert(extension.IsSendingAmendment().Value);
		}

		public void TestShowEvents()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.IsEditingElectronicBOL = false;
			var extension = new DefaultHouseBillMessagingExtension(shipment);

			Assert(!extension.ShowEvents().Value);

			shipment.IsEditingElectronicBOL = true;
			extension = new DefaultHouseBillMessagingExtension(shipment);

			Assert(extension.ShowEvents().Value);
		}

		public void TestShowLastEventDetails()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var extension = new DefaultHouseBillMessagingExtension(shipment);

			Assert(!extension.ShowLastEventDetails().Value);
		}
	}
}
