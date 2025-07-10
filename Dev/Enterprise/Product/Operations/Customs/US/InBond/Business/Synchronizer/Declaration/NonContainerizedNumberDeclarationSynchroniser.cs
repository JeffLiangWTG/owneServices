using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	abstract class CusInBondContainerCommonDeclarationSynchroniser : Customs.Business.BusinessObjectSynchroniser
	{
		protected CusInBondContainerCommonDeclarationSynchroniser(CusInBondContainer destination, BusinessObject source)
			: base(destination, source)
		{
		}

		public new CusInBondContainer Destination
		{
			get { return (CusInBondContainer)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				commoditiesSynchroniser = GetNewCusInBondCargoDescCollectionDeclarationSynchroniser();
				Synchronisers.Add(commoditiesSynchroniser);
			}
		}
		CusInBondCargoDescCollectionDeclarationSynchroniser commoditiesSynchroniser;

		internal void MarkCommoditySynchroniserAsOphant()
		{
			commoditiesSynchroniser.MarkCommoditySynchroniserAsOphant();
		}

		protected abstract CusInBondCargoDescCollectionDeclarationSynchroniser GetNewCusInBondCargoDescCollectionDeclarationSynchroniser();
	}

	class NonContainerizedNumberDeclarationSynchroniser : CusInBondContainerCommonDeclarationSynchroniser
	{
		internal NonContainerizedNumberDeclarationSynchroniser(CusInBondContainer destination, Bill source)
			: base(destination, source)
		{
		}

		public new Bill Source
		{
			get { return (Bill)base.Source; }
		}

		#region Implementation

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				Synchronisers.Add(new Customs.Business.FieldSynchroniser(Destination.BC_ContainerNumInfo, () => new ZString(CusInBondContainer.NonContainerizedNumber), () => new[] { Destination.BC_ParentIDInfo }));
			}
		}

		protected override CusInBondCargoDescCollectionDeclarationSynchroniser GetNewCusInBondCargoDescCollectionDeclarationSynchroniser()
		{
			return new CusInBondCargoDescCollectionDeclarationSynchroniser(Source, Destination, ZGuid.Empty);
		}

		#endregion
	}
}
