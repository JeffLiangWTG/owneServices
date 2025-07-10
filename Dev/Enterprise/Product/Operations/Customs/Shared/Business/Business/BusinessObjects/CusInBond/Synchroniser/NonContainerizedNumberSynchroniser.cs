using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business
{
	public class NonContainerizedNumberSynchroniser : GenericNonContainerizedNumberSynchroniser<CusInBondContainer>
	{
		public NonContainerizedNumberSynchroniser(CusInBondContainer destination, ForwardingShipment source)
			: base(destination, source)
		{ }

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_ContainerNumInfo, () => new ZString(CusInBondContainer.NonContainerizedNumber), () => new[] { Destination.BC_ParentIDInfo }));
			HookCargoDescCollSynchroniser();
		}

		protected virtual void HookCargoDescCollSynchroniser()
		{
			Synchronisers.Add(new CusInBondCargoDescCollectionSynchroniser(Source, Destination));
		}
	}

	public class GenericNonContainerizedNumberSynchroniser<TDestContainer> : BusinessObjectSynchroniser
		where TDestContainer : BusinessObject, ISynchableContainer
	{
		public GenericNonContainerizedNumberSynchroniser(TDestContainer destination, ForwardingShipment source)
			: base(destination, source)
		{
		}

		public new TDestContainer Destination
		{
			get { return (TDestContainer)base.Destination; }
		}

		public new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}
	}
}
