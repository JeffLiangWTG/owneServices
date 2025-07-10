namespace Enterprise.Customs.ZA.Business
{
	public class BondedWarehousingHelper : Customs.Business.BondedWarehousingHelper
	{
		public BondedWarehousingHelper(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new static class Constants
		{
			public const string OriginalProcedureCode = "OriginalProcedureCode";
			public const string ROOCert = "ROOCert";
		}

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}
	}
}
