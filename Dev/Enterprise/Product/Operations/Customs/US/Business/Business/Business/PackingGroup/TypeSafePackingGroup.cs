namespace Enterprise.Customs.US.Business
{
	partial class PackingGroup : AutoPackingGroup
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		public new PackageCollection Packages
		{
			get { return (PackageCollection)base.Packages; }
		}

		public new PackingGroupLookups Lookups
		{
			get { return (PackingGroupLookups)base.Lookups; }
		}

		public new PackingGroupValidation Validation
		{
			get { return (PackingGroupValidation)base.Validation; }
		}

		protected override Customs.Business.CusDecHouseContainerPivotLookups GetNewLookups()
		{
			return new PackingGroupLookups(this);
		}

		protected override Customs.Business.CusDecHouseContainerPivotValidation GetNewValidation()
		{
			return new PackingGroupValidation(this);
		}

		protected override Customs.Business.BasePackageCollection CreateNewPackageCollection()
		{
			return new PackageCollection(this);
		}
	}
}
