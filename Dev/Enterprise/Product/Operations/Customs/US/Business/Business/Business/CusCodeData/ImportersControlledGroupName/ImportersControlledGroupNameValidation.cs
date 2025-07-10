namespace Enterprise.Customs.US.Business
{
	public class ImportersControlledGroupNameValidation : Customs.Business.CusCodeDataValidation
	{
		public ImportersControlledGroupNameValidation(ImportersControlledGroupName bizObj)
			: base(bizObj)
		{
		}

		protected override void CheckCY_Code()
		{
		}
	}
}
