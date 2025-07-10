using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class ContainerWrapper : ITransportEquipment
	{
		public ContainerWrapper(ZGuid packingLinkPK, ForwardingContainer container)
		{
			this.packingLinkPK = packingLinkPK;
			this.container = container;
		}

		readonly ZGuid packingLinkPK;
		readonly ForwardingContainer container;

		#region ITransportEquipment Methods

		public ZString ContainerNumber
		{
			get { return container.JC_ContainerNum; }
		}

		public ZString ContainerMode
		{
			get { return container.JC_ContainerMode; }
		}

		public ZString Status
		{
			get
			{
				var result = ZString.Empty;
				if (container.JC_ContainerMode == "GRP" || container.JC_ContainerMode == ContainerModeList.Codes.LCL)
				{
					result = ContainerStatusList.Codes.C7;
				}
				else if (container.JC_ContainerMode == ContainerModeList.Codes.FCL)
				{
					result = ContainerStatusList.Codes.C5;
				}
				else if (container.JC_ContainerMode == ContainerModeList.Codes.Bulk)
				{
					result = ContainerStatusList.Codes.C8;
				}
				else if (container.JC_ContainerMode == ContainerModeList.Codes.Empty)
				{
					result = ContainerStatusList.Codes.C4;
				}

				return result;
			}
		}

		public ZString Size
		{
			//Code value: TSW uses UN/EDIFACT 8155 Equipment size and type description codes
			get { return container.RefContainer.RC_Code; }
		}

		public IEnumerable<ZString> SealNumbers
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
			}
		}

		public ZString SealingParty
		{
			get { return container.JC_SealParty; }
		}

		public ZString AttachedEquipmentCode
		{
			//TODO: where from?
			get { return ZString.Empty; }
		}

		public ZString StowPosition
		{
			//Use format BayRowTier (e.g. BBBRRTT)
			get { return container.JobContainer != null && !container.JobContainer.JC_StowagePosition.IsEmpty ? container.JobContainer.JC_StowagePosition.PadLeft(7, '0') : ZString.Empty; }
		}

		public ZBool IsPallet => false;

		public ZInt MessageSequence
		{
			get { return fMessageSequence; }
			set { fMessageSequence = value; }
		}
		ZInt fMessageSequence;

		public IEnumerable<ZGuid> RelatedPackages
		{
			get
			{
				foreach (ForwardingShipment shipment in container.Consol.Shipments)
				{
					if (shipment.JS_ShipmentType != Core.Constants.ShipmentTypes.AssemblyMaster)
					{
						yield return shipment.PK;
					}
				}
			}
		}

		public ZGuid StuffingLocation => ZGuid.Empty;

		public ZGuid PK
		{
			get { return packingLinkPK; }
		}

		#endregion
	}
}
