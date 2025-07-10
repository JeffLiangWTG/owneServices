using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class ContainerWrapper : ITransportEquipment
	{
		public ContainerWrapper(PackingGroup packGroup)
		{
			this.packGroup = packGroup;
			container = packGroup.Container;
		}

		readonly PackingGroup packGroup;
		readonly CusContainer container;

		#region ITransportEquipment Methods

		public ZString ContainerNumber
		{
			get { return container.CO_ContainerNumber; }
		}

		public ZString ContainerMode => ZString.Empty;

		public ZString Status
		{
			get
			{
				var result = ZString.Empty;

				if (container.CO_FCL_LCL_AIR == ContainerModeList.Codes.FCL)
				{
					result = ContainerStatusList.Codes.C5;
				}
				else if (container.CO_FCL_LCL_AIR == ContainerModeList.Codes.LCL)
				{
					result = ContainerStatusList.Codes.C7;
				}
				else if (container.CO_FCL_LCL_AIR == ContainerModeList.Codes.Bulk)
				{
					result = ContainerStatusList.Codes.C8;
				}
				else if (container.CO_FCL_LCL_AIR == ContainerModeList.Codes.Empty)
				{
					result = ContainerStatusList.Codes.C4;
				}

				return result;
			}
		}

		public ZString Size
		{
			//Code value: TSW uses UN/EDIFACT 8155 Equipment size and type description codes
			get { return container.CO_MAF_ContainerType; }
		}

		public IEnumerable<ZString> SealNumbers
		{
			get
			{
				if (!container.CO_Seal.IsEmpty)
				{
					yield return container.CO_Seal;
				}

				if (!container.CO_SecondSeal.IsEmpty)
				{
					yield return container.CO_SecondSeal;
				}
			}
		}

		public ZString SealingParty
		{
			get { return container.CO_SealingParty; }
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

		public ZBool IsPallet
		{
			get { return container.ContainerNumberIsValidPalletNumber(); }
		}

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
				foreach (Package package in packGroup.Packages)
				{
					yield return package.PK;
				}
			}
		}

		public ZGuid StuffingLocation
		{
			get { return container.PackingLocationOrgPK; }
		}

		public ZGuid PK
		{
			get { return packGroup.PK; }
		}

		#endregion
	}
}
