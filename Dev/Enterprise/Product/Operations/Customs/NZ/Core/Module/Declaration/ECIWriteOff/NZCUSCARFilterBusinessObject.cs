namespace Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff
{
	public class CUSCARFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
	{
		public new ECIWriteOffFilterLookups Lookups
		{
			get { return (ECIWriteOffFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new ECIWriteOffFilterLookups(this);
		}
	}
}
