using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class NctsHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCALCalculationMethod()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var calCalculationMethodInfo = header.CALCalculationMethodInfo;
		CombineAssertions(() =>
		{
			Assert(true);
			header.CALCalculationMethod = "";
			AssertNoMessageErrorContaining("Empty", calCalculationMethodInfo, "list");
			header.CALCalculationMethod = "DUT";
			AssertNoMessageErrorContaining("DUT", calCalculationMethodInfo, "list");
			header.CALCalculationMethod = "WGT";
			AssertNoMessageErrorContaining("WGT", calCalculationMethodInfo, "list");
			header.CALCalculationMethod = "DEF";
			AssertNoMessageErrorContaining("DEF", calCalculationMethodInfo, "list");
			header.CALCalculationMethod = "X";
			AssertHasMessageErrorContaining("X", calCalculationMethodInfo, "list");
		});
	}

	public void TestCheckCalCalculationMethod_WGT_SeaContainers()
	{
		var expectedMessageError = "When calculation is WGT and Transport mode is SEA, a container is mandatory.";
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		header.CALCalculationMethod = CalculationMethodList.Codes.WGT;
		CombineAssertions(() =>
		{
			AssertHasMessageError("CAL = WGT, Transport mode = SEA, no containers", header.CALCalculationMethodInfo, expectedMessageError);
			var container = header.DepartureHeaderContainers.AddNew();
			container.BC_Mode = "CNT";
			header.CALCalculationMethod = CalculationMethodList.Codes.WGT;
			AssertNoMessageError("CAL = WGT, Transport mode = SEA and container available", header.CALCalculationMethodInfo, expectedMessageError);
		});
	}
}
