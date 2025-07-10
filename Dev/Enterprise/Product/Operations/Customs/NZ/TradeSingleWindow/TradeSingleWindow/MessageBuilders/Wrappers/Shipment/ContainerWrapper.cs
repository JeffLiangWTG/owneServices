using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class ContainerWrapper : ITransportEquipment
	{
		public ContainerWrapper(ForwardingContainer container)
		{
			this.container = Argument.NotNull(container, "Container cannot be null");
		}

		readonly ForwardingContainer container;

		ZString ITransportEquipment.ContainerNumber
		{
			get { return container.JC_ContainerNum; }
		}

		ZString ITransportEquipment.ContainerMode
		{
			get { return container.JC_ContainerMode; }
		}

		ZString ITransportEquipment.Status
		{
			get { return ContainerStatusList.Codes.C7; }
		}

		ZString ITransportEquipment.Size
		{
			//TODO: where from?
			get { return ZString.Empty; }
		}

		IEnumerable<ZString> ITransportEquipment.SealNumbers
		{
			get
			{
				if (!container.JC_SealNum.IsEmpty)
				{
					yield return container.JC_SealNum;
				}

				if (!container.JC_AdditionalSealNum.IsEmpty)
				{
					yield return container.JC_AdditionalSealNum;
				}

				if (!container.JC_Additional2SealNum.IsEmpty)
				{
					yield return container.JC_Additional2SealNum;
				}
			}
		}

		public ZString SealingParty
		{
			get { return ZString.Empty; } // Not required here - CusContainer value not Forwarding container
		}

		ZString ITransportEquipment.AttachedEquipmentCode
		{
			//TODO: where from?
			get { return ZString.Empty; }
		}

		ZString ITransportEquipment.StowPosition
		{
			get { return container.JC_StowagePosition; }
		}

		ZBool ITransportEquipment.IsPallet
		{
			get { return false; }
		}

		ZInt ITransportEquipment.MessageSequence
		{
			get { return fMessageSequence; }
			set { fMessageSequence = value; }
		}
		ZInt fMessageSequence;

		IEnumerable<ZGuid> ITransportEquipment.RelatedPackages
		{
			get
			{
				foreach (ForwardingShipment shipment in container.Consol.Shipments)
				{
					yield return shipment.PK;
				}
			}
		}

		ZGuid ITransportEquipment.StuffingLocation
		{
			get { return ZGuid.Empty; }     // Not required here - CusContainer value not Forwarding container
		}

		ZGuid ITransportEquipment.PK
		{
			get { return container.PK; }
		}
	}
}
