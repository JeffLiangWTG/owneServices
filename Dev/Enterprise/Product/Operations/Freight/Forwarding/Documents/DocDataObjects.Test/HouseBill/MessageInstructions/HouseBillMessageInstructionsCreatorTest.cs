using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class HouseBillMessageInstructionsCreatorTest : TestCaseWithFactory
	{
		public void TestGetMessageInstructions()
		{
			var creator = new HouseBillMessageInstructionsCreator();
			var template = new Mock<IHouseBillTemplate>();
			var shipment = Factory.New<ForwardingShipment>();

			var documentPivot = new Mock<IDocumentPivot>();
			documentPivot.SetupGet(d => d.TemplatePK).Returns(Guid.Parse("40e652ce-ea13-4897-bbad-a4954921b1f3"));
			AssertType<FIATAHouseBillMessageInstructions>(creator.GetMessageInstructions(shipment, documentPivot.Object, template.Object));

			documentPivot = new Mock<IDocumentPivot>();
			documentPivot.SetupGet(d => d.TemplatePK).Returns(Guid.Parse("bf695095-380c-4241-bd71-084b68120e75"));
			AssertType<CarrierHouseBillMessageInstructions>(creator.GetMessageInstructions(shipment, documentPivot.Object, template.Object));

			documentPivot = new Mock<IDocumentPivot>();
			documentPivot.SetupGet(d => d.TemplatePK).Returns(Guid.NewGuid);
			shipment.IsEditingElectronicBOL = true;
			AssertType<ElectronicBOLMessageInstructions>(creator.GetMessageInstructions(shipment, documentPivot.Object, template.Object));

			shipment.IsEditingElectronicBOL = false;
			AssertType<DefaultHouseBillMessageInstructions>(creator.GetMessageInstructions(shipment, documentPivot.Object, template.Object));
		}
	}
}
