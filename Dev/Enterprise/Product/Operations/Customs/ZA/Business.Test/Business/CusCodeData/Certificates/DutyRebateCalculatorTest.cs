using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DutyRebateCalculatorTest : RebateCalculatorAbstractTest<DutyRebateCertificate>
	{
		protected override void CreateCertificate(CusEntryInstruction entryInstruction, ZString code, ZShort order)
		{
			var certificate1 = entryInstruction.DutyRebateCertificates.AddNew();
			certificate1.CY_Code = code;
			certificate1.CY_Order = order;
		}

		protected override ZString PermitTypeCore => PermitTypeList.Codes.PRC;

		protected override PRVValueUnits PRVValueUnit => PRVValueUnits.CENTS;

		protected override RebateCalculator<DutyRebateCertificate> GetRebateCalculator(JobDeclaration declaration) => new DutyRebateCalculator(declaration);
	}
}
