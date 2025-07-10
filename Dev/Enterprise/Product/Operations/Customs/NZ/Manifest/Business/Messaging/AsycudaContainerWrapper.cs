using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class AsycudaContainerWrapper : ITransportEquipment
	{
		public AsycudaContainerWrapper(AsycudaContainer container)
		{
			this.container = Argument.NotNull(container, "Container cannot be null");
		}
		readonly AsycudaContainer container;

		ZInt ITransportEquipment.MessageSequence { get; set; }

		ZString ITransportEquipment.ContainerNumber => container.ACN_ContainerNumber;

		ZString ITransportEquipment.ContainerMode => ZString.Empty;

		ZString ITransportEquipment.Status => container.ACN_EmptyFullIndicator;

		ZString ITransportEquipment.Size => container.ContainerType?.RC_ISOEquipmentSizeTypeCode ?? ZString.Empty;

		ZString ITransportEquipment.StowPosition => container.ACN_StowageLocation;

		IEnumerable<ZString> ITransportEquipment.SealNumbers => container.ACN_Seal1.IsEmpty ? Enumerable.Empty<ZString>() : new ZString[] { container.ACN_Seal1 };

		ZBool ITransportEquipment.IsPallet => false;

		ZString ITransportEquipment.SealingParty => ZString.Empty;

		ZString ITransportEquipment.AttachedEquipmentCode => ZString.Empty;

		IEnumerable<ZGuid> ITransportEquipment.RelatedPackages => Enumerable.Empty<ZGuid>();

		// Packing Location not yet implemented in ASYCUDA
		ZGuid ITransportEquipment.StuffingLocation => ZGuid.Empty;

		ZGuid ITransportEquipment.PK => ZGuid.Empty;
	}
}
