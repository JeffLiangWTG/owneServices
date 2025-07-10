using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HTransportEquipment : ITransportEquipment
	{
		readonly AsycudaBillLinkAsycudaContainer container;

		public N5101HTransportEquipment(AsycudaBillLinkAsycudaContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}

		ZString ITransportEquipment.CharacteristicCode => container.RefContainer?.RC_Code ?? ZString.Empty;

		ZString ITransportEquipment.ID => container.ContainerNumber;

		ZString ITransportEquipment.UsedCapacityCode => container.ContainerMode;

		IEnumerable<ZString> ITransportEquipment.Seals => container.Seals;
	}
}
