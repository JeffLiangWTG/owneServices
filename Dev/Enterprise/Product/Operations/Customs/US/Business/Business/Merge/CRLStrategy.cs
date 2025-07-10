namespace Enterprise.Customs.US.Business
{
	public class CRLMergeStrategy : CargoReleaseMergeStrategy
	{
		public CRLMergeStrategy(JobDeclaration declaration)
			: base(declaration, CusEntryHeaderMessageTypeList.Codes.CargoRelease)
		{
		}

		protected override bool IsActiveCore
		{
			get
			{
				var declaration = (JobDeclaration)Declaration;
				return declaration.IsFormalImport && declaration.US_EnableCRL && !declaration.IsBorderMovement && !declaration.IsACE;
			}
		}
	}
}
