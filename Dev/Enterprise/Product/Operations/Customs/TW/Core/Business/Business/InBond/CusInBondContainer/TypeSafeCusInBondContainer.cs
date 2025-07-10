namespace Enterprise.Customs.TW.Business
{
	public partial class CusInBondContainer : AutoTWCusInBondContainer
	{
		public new CusInBondContainer Clone()
		{
			return (CusInBondContainer)base.Clone();
		}

		public new CusInBondContainerValidation Validation => (CusInBondContainerValidation)base.Validation;

		protected override Customs.Business.CusInBondContainerValidation GetNewValidation()
		{
			return new CusInBondContainerValidation(this);
		}

		public new CusInBondContainerLookups Lookups => (CusInBondContainerLookups)base.Lookups;

		protected override Customs.Business.CusInBondContainerLookups GetNewLookups()
		{
			return new CusInBondContainerLookups(this);
		}
	}
}
