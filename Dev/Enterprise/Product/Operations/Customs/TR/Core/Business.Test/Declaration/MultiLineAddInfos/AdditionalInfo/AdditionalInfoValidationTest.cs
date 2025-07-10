using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	public class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCodeIsNotMandatory()
		{
			additionalInfo.CSI_Code = ZString.Empty;
			AssertNoMessageErrors("CSI_Code is not mandatory for TR anymore, because the field is removed", additionalInfo.CSI_CodeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			additionalInfo = declaration.AdditionalInfos.AddNew();
		}
		AdditionalInfo additionalInfo;
	}
}
