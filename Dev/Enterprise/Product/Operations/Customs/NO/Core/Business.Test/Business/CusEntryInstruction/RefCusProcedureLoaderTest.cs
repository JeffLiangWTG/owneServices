using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(RefCusProcedureLoader))]
sealed class RefCusProcedureLoaderTest : TestCaseWithFactory
{
	public void TestParameters() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertExceptionThrown<ArgumentNullException>("When factory is null", () => RefCusProcedureLoader.GetAllApplicableProcedures(null, entryInstruction));
		AssertExceptionThrown<ArgumentNullException>("When entryInstruction is null", () => RefCusProcedureLoader.GetAllApplicableProcedures(Factory, null));
		AssertExceptionThrown<ArgumentNullException>("When entryInstruction.JobDeclaration is null", () => RefCusProcedureLoader.GetAllApplicableProcedures(Factory, Factory.New<CusEntryInstruction>()));
	});

	public void TestGetAllApplicableProcedures()
	{
		CreateRefCusProcedureCodeDataForTest();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "A";

		var procedures = RefCusProcedureLoader.GetAllApplicableProcedures(Factory, entryInstruction);

		CombineAssertions(() =>
		{
			AssertEquals("Total Items", 3, procedures.Count);
			Assert("Contains Code: 0471", procedures.ContainsKey("0471"));
			Assert("Contains Code: 5710", procedures.ContainsKey("5710"));
			Assert("Contains Code: 0571", procedures.ContainsKey("0571"));
			Assert("All Values are not null", procedures.Values.All(v => v is not null));
		});
	}

	void CreateRefCusProcedureCodeDataForTest()
	{
		var referenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
		_ = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "04",
			previousProcedureCode: "71",
			concession: ZString.Empty,
			description: "Description1",
			shipmentType: "IMP",
			outOfWarehouse: true,
			group: "A");

		_ = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "57",
			previousProcedureCode: "10",
			concession: ZString.Empty,
			description: "Description 2",
			shipmentType: "IMP",
			outOfWarehouse: false,
			group: "A");

		_ = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "05",
			previousProcedureCode: "71",
			concession: ZString.Empty,
			description: "Description3",
			shipmentType: "IMP",
			outOfWarehouse: true,
			group: "A");
	}
}
