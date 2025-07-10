namespace Enterprise.Customs.US.Business
{
	public class BCRMergeStrategy : CargoReleaseMergeStrategy
	{
		public BCRMergeStrategy(JobDeclaration declaration)
			: base(declaration, CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease)
		{
		}

		protected override bool IsActiveCore
		{
			get
			{
				var declaration = (JobDeclaration)Declaration;
				return declaration.IsFormalImport && declaration.US_EnableCRL && declaration.IsBorderMovement;
			}
		}
	}
}
