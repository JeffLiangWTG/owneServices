using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ValaValueRebateCalculatorTest : RebateCalculatorAbstractTest<RCCCertificate>
	{
		protected override void CreateCertificate(CusEntryInstruction entryInstruction, ZString code, ZShort order)
		{
			var certificate1 = entryInstruction.RCCCertificates.AddNew();
			certificate1.CY_Code = code;
			certificate1.CY_Order = order;
		}

		protected override ZString PermitTypeCore => PermitTypeList.Codes.VALA;

		protected override PRVValueUnits PRVValueUnit => PRVValueUnits.RANDS;

		protected override RebateCalculator<RCCCertificate> GetRebateCalculator(JobDeclaration declaration) => new ValaValueRebateCalculator(declaration);
	}
}
