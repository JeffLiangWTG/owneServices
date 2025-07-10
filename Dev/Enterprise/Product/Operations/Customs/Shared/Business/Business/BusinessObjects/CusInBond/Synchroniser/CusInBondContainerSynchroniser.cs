using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business
{
	public class CusInBondContainerSynchroniser : ContainerSynchroniser<CusInBondContainer>
	{
		public CusInBondContainerSynchroniser(CusInBondContainer destination, ForwardingContainer source, ForwardingShipment shipmentSource)
			: base(destination, source, shipmentSource)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			GetCusInBondCargoDescCollectionSynchroniser();
			Synchronisers.Add(GetUNDGDataItemCollectionSynchroniser(shipmentSource, Destination));
		}

		protected virtual void GetCusInBondCargoDescCollectionSynchroniser()
		{
			Synchronisers.Add(new CusInBondCargoDescCollectionSynchroniser(shipmentSource, Destination));
		}

		protected virtual UNDGDataItemCollectionSynchroniser GetUNDGDataItemCollectionSynchroniser(ForwardingShipment shipmentSource, CusInBondContainer destination)
		{
			return new UNDGDataItemCollectionSynchroniser(shipmentSource, destination);
		}
	}

	public abstract class ContainerSynchroniser<TDestContainer> : BusinessObjectSynchroniser
		where TDestContainer : BusinessObject, ISynchableContainer
	{
		protected ContainerSynchroniser(TDestContainer destination, ForwardingContainer source, ForwardingShipment shipmentSource)
			: base(destination, source)
		{
			this.shipmentSource = shipmentSource;
		}

		public new TDestContainer Destination
		{
			get { return (TDestContainer)base.Destination; }
		}

		public new ForwardingContainer Source
		{
			get { return (ForwardingContainer)base.Source; }
		}

		public ForwardingShipment ShipmentSource
		{
			get { return shipmentSource; }
		}
		protected readonly ForwardingShipment shipmentSource;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.ContainerNumInfo, Source.JC_ContainerNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.Seal1Info, Source.JC_SealNumInfo));
			if (Destination.Seal2Supported)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.Seal2Info, Source.JC_AdditionalSealNumInfo));
			}
			if (Destination.Seal3Supported)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.Seal3Info, Source.JC_Additional2SealNumInfo));
			}
		}
	}

	public interface ISynchableContainer
	{
		ZPropertyInfo ContainerNumInfo { get; }
		ZPropertyInfo Seal1Info { get; }
		bool Seal2Supported { get; }
		ZPropertyInfo Seal2Info { get; }
		bool Seal3Supported { get; }
		ZPropertyInfo Seal3Info { get; }
		bool IsNonContainerized { get; }
	}
}
