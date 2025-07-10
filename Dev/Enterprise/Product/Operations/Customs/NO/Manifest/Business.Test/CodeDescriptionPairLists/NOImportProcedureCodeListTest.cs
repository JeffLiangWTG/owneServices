using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(NOImportProcedureCodeList))]
sealed class NOImportProcedureCodeListTest : TestCaseWithFactory
{
	public void TestAllCodesAndDescriptions() => CombineAssertions(() =>
	{
		AssertCodeDescriptionPairList(new NOImportProcedureCodeList(),
			("COLLECTIVE_RELEASE", "Collective clearance at unload"),
			("IMMEDIATE_RELEASE_IMPORT", "Into free circulation"),
			("IMMEDIATE_RELEASE_VOEC", "VOEC shipments"),
			("TRANSIT_IMPORT", "Transit from sender"),
			("TRANSIT_RELEASE", "Transit started at border"),
			("WAREHOUSE_RELEASE", "Entry into customs warehouse"),
			("DOCUMENTS_NOT_OBLIGED_RELEASE", "Documents exempt from the obligation to declare"),
			("ATA_CARNET", "Pre-cleared acc. to ATA convention"),
			("TIR_CARNET", "Sent according to TIR convention"));
	});
}
