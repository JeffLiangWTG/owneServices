using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondContainerDeclarationSynchroniser : CusInBondContainerCommonDeclarationSynchroniser
	{
		internal CusInBondContainerDeclarationSynchroniser(CusInBondContainer destination, CusContainer source, US.Business.Bill billSource)
			: base(destination, source)
		{
			this.billSource = Argument.NotNull(billSource, "billSource");
		}
		readonly US.Business.Bill billSource;

		public new CusContainer Source
		{
			get { return (CusContainer)base.Source; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.BC_ContainerNumInfo, Source.CO_ContainerNumberInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BC_Seal1Info, Source.CO_SealInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BC_Seal2Info, Source.CO_SecondSealInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BC_RCInfo, Source.CO_RCInfo));
			}
		}

		protected override CusInBondCargoDescCollectionDeclarationSynchroniser GetNewCusInBondCargoDescCollectionDeclarationSynchroniser()
		{
			return new CusInBondCargoDescCollectionDeclarationSynchroniser(billSource, Destination, Source.PK);
		}
	}
}
