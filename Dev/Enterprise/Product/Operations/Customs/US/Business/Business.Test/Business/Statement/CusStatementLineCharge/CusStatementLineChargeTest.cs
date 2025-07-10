using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusStatementLineCharge))]
	sealed class CusStatementLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestChargeTypeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AccountingClassFeeCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AccountingClassFeeCode);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.DutiableMail, "DutiableMail Mail Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();
			CusStatementLineCharge cusStatementLineCharge = Factory.New<CusStatementLineCharge>();
			cusStatementLineCharge.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.DutiableMail;

			AssertEquals("DutiableMail Mail Fee", cusStatementLineCharge.ChargeTypeDescription);
		}
	}
}
