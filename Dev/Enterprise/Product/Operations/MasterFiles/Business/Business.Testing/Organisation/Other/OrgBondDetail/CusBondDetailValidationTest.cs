using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CusBondDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPW_CPH_Guarantee()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "PW-CPH-OH";
			Factory.Save();

			var cusPermitHeaderPK = ZGuid.NewZGuid();
			string sql = $@"insert into dbo.CusPermitHeader (CPH_PK, CPH_OH_PermitHolder, CPH_StartDate, CPH_Number, CPH_QtyValIndicator, CPH_Type, CPH_RN_NKCountryCode, CPH_SystemCreateTimeUtc, CPH_SystemCreateUser, CPH_SystemLastEditTimeUtc, CPH_SystemLastEditUser) values
						('{cusPermitHeaderPK}', '{orgHeader.PK}', CURRENT_TIMESTAMP, '5678', 'BTH', 'BBB', 'NA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			TestConnection.ExecuteNonQuery(sql);

			var bondDetail = Factory.New<CusBondDetail>();
			bondDetail.Parent = orgHeader;
			bondDetail.PW_CPH_Guarantee = cusPermitHeaderPK;
			bondDetail.PW_BondType = GuaranteeBondTypeList.Codes.SingleTransaction;
			bondDetail.PW_ActivityCode = GuaranteeActivityCodeList.Codes.ConsumesGuarantee;
			Factory.Save();

			var errorMessage = "The Guarantee with same Type(STB) and Activity(CON or OAC) already exists.";
			var bondDetail2 = Factory.New<CusBondDetail>();
			bondDetail2.Parent = orgHeader;
			bondDetail2.PW_BondType = GuaranteeBondTypeList.Codes.SingleTransaction;
			bondDetail2.PW_ActivityCode = GuaranteeActivityCodeList.Codes.ConsumesGuarantee;
			bondDetail2.PW_CPH_Guarantee = cusPermitHeaderPK;

			CombineAssertions(() =>
			{
				AssertHasError("Activity-CON", bondDetail2.PW_CPH_GuaranteeInfo, errorMessage);

				bondDetail2.PW_ActivityCode = GuaranteeActivityCodeList.Codes.ConsumeAndRelease;
				bondDetail2.Validation.ValidatePW_CPH_Guarantee();
				AssertHasError("Activity-OAC", bondDetail2.PW_CPH_GuaranteeInfo, errorMessage);

				bondDetail2.PW_BondType = GuaranteeBondTypeList.Codes.Continuous;
				bondDetail2.Validation.ValidatePW_CPH_Guarantee();
				AssertNoError("Type-CON", bondDetail2.PW_CPH_GuaranteeInfo, errorMessage);
			});
		}
	}
}
