using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC013CTransitOperationProvider))]
sealed class CC013CTransitOperationProviderTest : TransitOperationProviderAbstractTest<CC013CTransitOperationProvider>
{
	public override void TestLRN() => CombineAssertions(() =>
	{
		header.MovementReferenceEntryNumber.CE_EntryNum = "MRN";
		header.MovementHeader.BM_PaperlessInbondNum = "LRNOfTheNCT";
		AssertNullOrEmpty("When MRN is filled, LRN must be empty", Provider.LRN);
		header.ArrivalMrnFromUser = ZString.Empty;
		AssertEquals("When MRN is empty, LRN must be filled", "LRNOfTheNCT", Provider.LRN);
	});
}
