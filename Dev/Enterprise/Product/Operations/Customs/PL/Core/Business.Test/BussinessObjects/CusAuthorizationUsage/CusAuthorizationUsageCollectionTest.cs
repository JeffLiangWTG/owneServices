using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>))]
sealed class CusAuthorizationUsageCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestMaxCountForValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var authorisationUsages = entryInstruction.CusAuthorizationUsages;

		var exportMessageError = "Only 9 authorizations are allowed.";
		var importMessageError = "Only 1 authorizations are allowed.";
		CombineAssertions(() =>
		{
			var authorisation = authorisationUsages.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertNoRowError("Import 1 authorisation", authorisation, exportMessageError);
			AssertNoRowError("Import 1 authorisation", authorisation, importMessageError);

			authorisation = authorisationUsages.AddNew();
			AssertNoRowError("Import 2 authorisations", authorisation, exportMessageError);
			AssertHasRowError("Import 2 authorisations", authorisation, importMessageError);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertNoRowError("Export 2 authorisations", authorisation, importMessageError);
			AssertNoRowError("Export 2 authorisations", authorisation, exportMessageError);

			for (int i = 0; i < 7; i++)
			{
				authorisation = authorisationUsages.AddNew();
				AssertNoRowError($"Export {i} authorisations", authorisation, exportMessageError);
			}

			authorisation = authorisationUsages.AddNew();
			AssertHasRowError("Export 10 authorisations", authorisation, exportMessageError);
		});
	}

	protected override Type GetExpectedCollectionType() => typeof(CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>);

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var entryInstruction = Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew();
		return (BusinessObjectCollection)entryInstruction.CusAuthorizationUsages;
	}
}
