namespace Enterprise.Customs.TW.Business.Testing
{
	class BondedFactoryCollectionForTest : BondedFactoryCollection
	{
		public BondedFactoryCollectionForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		public new bool AllowNew => base.AllowNew;
	}
}
