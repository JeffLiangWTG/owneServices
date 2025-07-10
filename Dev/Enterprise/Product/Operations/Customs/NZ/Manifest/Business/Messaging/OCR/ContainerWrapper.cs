using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class ContainerWrapper : ITransportEquipment
	{
		public ContainerWrapper(AsycudaContainer container)
		{
			this.container = Argument.NotNull(container, "Container cannot be null");
		}

		readonly AsycudaContainer container;

		public ZString ContainerNumber => container.ACN_ContainerNumber;

		public ZString ContainerMode => ZString.Empty;

		public ZString Status => ContainerStatusList.Codes.C7;

		public ZString Size => ZString.Empty;

		public IEnumerable<ZString> SealNumbers
		{
			get
			{
				if (!container.ACN_Seal1.IsEmpty)
				{
					yield return container.ACN_Seal1;
				}

				if (!container.ACN_Seal2.IsEmpty)
				{
					yield return container.ACN_Seal2;
				}

				if (!container.ACN_Seal3.IsEmpty)
				{
					yield return container.ACN_Seal3;
				}
			}
		}

		public ZString SealingParty => container.ACN_SealingPartyName;

		public ZString AttachedEquipmentCode => ZString.Empty;

		public ZString StowPosition => container.ACN_StowageLocation;

		public ZBool IsPallet => false;

		ZInt ITransportEquipment.MessageSequence
		{
			get { return fMessageSequence; }
			set { fMessageSequence = value; }
		}
		ZInt fMessageSequence;

		public IEnumerable<ZGuid> RelatedPackages => Enumerable.Empty<ZGuid>();

		public ZGuid StuffingLocation => ZGuid.Empty;

		public ZGuid PK => container.PK;
	}
}
