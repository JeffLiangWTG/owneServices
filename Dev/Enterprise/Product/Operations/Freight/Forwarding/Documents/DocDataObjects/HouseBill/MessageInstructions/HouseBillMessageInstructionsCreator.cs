using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents
{
	public sealed class HouseBillMessageInstructionsCreator : IHouseBillMessageInstructionsCreator
	{
		public IMessageInstructions GetMessageInstructions(BusinessObject parent, IDocumentPivot pivot, IHouseBillTemplate template)
		{
			if (pivot.TemplatePK == Guid.Parse("40e652ce-ea13-4897-bbad-a4954921b1f3")) // FIATA template
			{
				return new FIATAHouseBillMessageInstructions(pivot, template);
			}

			if (pivot.TemplatePK == Guid.Parse("bf695095-380c-4241-bd71-084b68120e75")) // Carrier Bill Of Lading template
			{
				return new CarrierHouseBillMessageInstructions(pivot, template);
			}

			if (parent is ForwardingShipment shipment && shipment.IsEditingElectronicBOL)
			{
				return new ElectronicBOLMessageInstructions(template);
			}

			return new DefaultHouseBillMessageInstructions(pivot, template);
		}
	}
}
