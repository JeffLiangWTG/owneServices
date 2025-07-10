using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer
{
	public class HVLVConvertShipmentToCustomsJobProcessor : IProcessor
	{
		public HVLVConvertShipmentToCustomsJobProcessor(ForwardingShipment shipment, string triggerCode)
		{
			this.Shipment = shipment;
			this.Command = GetCommandFromTriggerCode(triggerCode);
		}

		ForwardingShipment Shipment { get; }
		BaseHVLVRelatedJobCommand Command { get; }

		BaseHVLVRelatedJobCommand GetCommandFromTriggerCode(string triggerCode)
		{
			switch (triggerCode)
			{
				case WorkflowTriggerActionTypeConstants.Codes.AUAirCargoReport:
					return new AUAirCargoReportCommand(Shipment);
				case WorkflowTriggerActionTypeConstants.Codes.AUSeaCargoReport:
					return new AUSeaCargoReportCommand(Shipment);
				case WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS:
					return new USAirAMSCommand(Shipment);
				case WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS:
					return new USSeaAMSCommand(Shipment);
				case WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest:
					return new USRoadEManifestCommand(Shipment);
				case WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration:
					return CreateH7DeclarationCommand(Shipment);
				case WorkflowTriggerActionTypeConstants.Codes.NZAirCargoReport:
					return new NZAirCargoReportCommand(Shipment);
				case WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport:
					return new NZSeaCargoReportCommand(Shipment);
				default:
					return null;
			}
		}

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			if (Command != null)
			{
				if (Command.CheckDataIsReadyForConvertToCustomsJob(out var errorMessage))
				{
					if (!Command.Converter.TryAcquireApplicationLock(
					() =>
					{
						if (!Command.Converter.TryConvert(out errorMessage, Shipment.Factory))
						{
							notifications.AddError(ResString.GetMultilingualString("f20cc9b9-cba0-450e-9c0f-02fb21530c45", "{0} could not be created for Shipment ({1}): {2}", Command.RelatedJobName, Shipment.JS_UniqueConsignRef, errorMessage));
						}
					},
					out var shipmentLockedErrorMessage))
					{
						notifications.AddError(shipmentLockedErrorMessage);
					}
				}
				else
				{
					notifications.AddError(errorMessage);
				}
			}
		}

		BaseHVLVRelatedJobCommand CreateH7DeclarationCommand(ForwardingShipment shipment)
		{
			switch (shipment.Destination?.Country?.Code)
			{
				case CountryCodes.France:
					return new FRH7DeclarationCommand(shipment);
				case CountryCodes.Ireland:
					return new IEH7DeclarationCommand(shipment);
				case CountryCodes.Spain:
					return new ESH7DeclarationCommand(shipment);
				case CountryCodes.UnitedKingdom:
					return new GBH7DeclarationCommand(shipment);
				case CountryCodes.Italy:
					return new ITH7DeclarationCommand(shipment);
				default:
					return null;
			}
		}
	}
}
