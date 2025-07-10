namespace Enterprise.Customs.US.Business.Testing
{
	public class ACEDrawbackJobDeclarationValidationTest : CommonDrawbackJobDeclarationValidationTest
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
		}

		#endregion
	}
}
