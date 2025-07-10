using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(NOExportProcedureCodeList))]
sealed class NOExportProcedureCodeListTest : TestCaseWithFactory
{
	public void TestAllCodesAndDescriptions() => CombineAssertions(() =>
	{
		AssertCodeDescriptionPairList(new NOExportProcedureCodeList(),
			("TRA", "Transit"),
			("EXP", "Export"));
	});
}
