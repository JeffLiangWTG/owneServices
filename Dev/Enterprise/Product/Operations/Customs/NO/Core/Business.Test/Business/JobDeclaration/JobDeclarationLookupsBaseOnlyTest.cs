using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(JobComInvoiceLineLookups))]
sealed class JobDeclarationLookupsBaseOnlyTest : JobDeclarationLookupsAbstractTest<JobDeclarationLookups>
{
	public void TestShipmentType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Codes from list", "EU, IM", lookups.MessageSubTypeList.CodesAsString);
			AssertSame("Cached", lookups.MessageSubTypeList, lookups.MessageSubTypeList);
		});
	}

	public void TestLocationOfGoods()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Codes from list", "A, A8, B, C, D, X", lookups.LocationOfGoodsCollection.CodesAsString);
			AssertSame("Cached", lookups.LocationOfGoodsCollection, lookups.LocationOfGoodsCollection);
		});
	}

	public void TestMergeByList()
	{
		var list = lookups.MergeByList;
		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, list.Count);
			AssertEquals("Codes from list", "MAX, NON", list.CodesAsString);
			AssertSame("cached", list, declaration.Lookups.MergeByList);
			AssertEquals("MAX", "Maximum", list.GetDescriptionFromCode(MergeInvoiceLinesConstants.Maximum));
			list.AssertCodeListIsOrdered();
		});
	}

	public void TestMessageTypeList()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Codes from list", "IMP, EXP", lookups.MessageTypeList.CodesAsString);
			AssertSame("Cached", lookups.MessageTypeList, lookups.MessageTypeList);
		});
	}

	public void TestCargoIdTypeList()
	{
		CombineAssertions(() =>
		{
			AssertSame("Cached", lookups.CargoIdTypeList, lookups.CargoIdTypeList);

			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
			AssertEquals("Air - Codes from list", "LSE, ULD, CON, BCN, SCN, CNT, NCT", lookups.CargoIdTypeList.CodesAsString);
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			AssertEquals("Iwt - Codes from list", "CNT, NCT", lookups.CargoIdTypeList.CodesAsString);
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.OwnPropulsion;
			AssertEquals("Own - Codes from list", "CNT, NCT", lookups.CargoIdTypeList.CodesAsString);
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Mail;
			AssertEquals("Mai - Codes from list", "CNT, NCT", lookups.CargoIdTypeList.CodesAsString);
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Rail;
			AssertEquals("Rai - Codes from list", "FCL, LCL, BLK, LQD, BBK, BCN, SCN, CNT, NCT", lookups.CargoIdTypeList.CodesAsString);
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Road;
			AssertEquals("Roa - Codes from list", "FCL, FTL, LCL, LTL, BCN, SCN, CNT, NCT", lookups.CargoIdTypeList.CodesAsString);
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			AssertEquals("Sea - Codes from list", "FCL, LCL, BLK, LQD, BBK, BCN, SCN, ROR, CNT, NCT", lookups.CargoIdTypeList.CodesAsString);

			AssertEquals("CNT description", "Containerized", lookups.CargoIdTypeList.GetDescriptionFromCode("CNT"));
			AssertEquals("NCT description", "Non-Containerized", lookups.CargoIdTypeList.GetDescriptionFromCode("NCT"));
		});
	}

	public void TestImportersList()
	{
		CombineAssertions(() =>
		{
			AssertType<ConsigneeCollection>("ImportersListType", declaration.Lookups.ImportersList);
			var containsDefaultForName = lookups.ImportersList.FilterBusinessObjectDefaults.ContainsDefaultFor("Name:Property");
			AssertEquals("Contains default for Name property", expected: true, containsDefaultForName);
		});
	}

	public void TestEntryPhaseStatusList() => CombineAssertions(() =>
	{
		var codeList = lookups.EntryPhaseStatusList;
		AssertType<CustomsEntryPhaseStatusList>(codeList);
		AssertSame("Cached", lookups.EntryPhaseStatusList, codeList);
		AssertEquals("Codes from list", "REM, FIN", codeList.CodesAsString);
	});

	protected override string MessageType => JobMessageTypeList.Codes.Import;
}
