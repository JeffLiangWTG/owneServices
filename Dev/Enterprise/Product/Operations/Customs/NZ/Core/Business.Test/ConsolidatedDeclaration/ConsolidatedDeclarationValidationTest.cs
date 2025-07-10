using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Test
{
	sealed class ConsolidatedDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCongruency_NonPeriodic()
		{
			const string message = "Consolidated Entries must be of the same Importer, Transport Mode, Entry Style, Discharge ETA, Vessel and Flight/Voyage.";
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: non-periodic", "NOR", consolidatedDeclaration.JobDeclarations[1].JE_MessageSubType);
				consolidatedDeclaration.Validation.ValidateAll();
				AssertNoRowError("No error for congruent declarations", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_OH_Importer = Factory.New<OrgHeader>().PK;
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Importer", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_OH_Importer = consolidatedDeclaration.JobDeclarations[0].JE_OH_Importer;
				consolidatedDeclaration.JobDeclarations[1].JE_TransportMode = "@@@";
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Transport Mode", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_TransportMode = consolidatedDeclaration.JobDeclarations[0].JE_TransportMode;
				consolidatedDeclaration.JobDeclarations[1].JE_MessageSubType = "@@@";
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Entry Style", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_MessageSubType = consolidatedDeclaration.JobDeclarations[0].JE_MessageSubType;
				consolidatedDeclaration.JobDeclarations[1].JE_DateOfArrival = new ZDateTime(2024, 10, 21);
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Discharge ETA", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_DateOfArrival = consolidatedDeclaration.JobDeclarations[0].JE_DateOfArrival;
				consolidatedDeclaration.JobDeclarations[1].JE_VesselName = "CSCL Melbourne";
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different VesselName", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_VesselName = consolidatedDeclaration.JobDeclarations[0].JE_VesselName;
				consolidatedDeclaration.JobDeclarations[1].JE_VoyageFlightNo = "0BOADN1MA";
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different VoyageNo", consolidatedDeclaration, message);

				consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
				consolidatedDeclaration.Validation.ValidateAll();
				AssertNoRowError("No error for empty selection", consolidatedDeclaration, message);
				consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
				consolidatedDeclaration.Validation.ValidateAll();
				AssertNoRowError("No error for single selection", consolidatedDeclaration, message);
			});
		}

		public void TestCongruency_Periodic()
		{
			const string message = "Periodic Consolidated Entries must be of the same Importer, Transport Mode, Entry Style, Vessel and Flight/Voyage.";
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			foreach (var dec in consolidatedDeclaration.JobDeclarations)
			{
				dec.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			}

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: periodic", "PER", consolidatedDeclaration.JobDeclarations[1].JE_MessageSubType);
				consolidatedDeclaration.Validation.ValidateAll();
				AssertNoRowError("No error for congruent declarations", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_OH_Importer = Factory.New<OrgHeader>().PK;
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Importer", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_OH_Importer = consolidatedDeclaration.JobDeclarations[0].JE_OH_Importer;
				consolidatedDeclaration.JobDeclarations[1].JE_TransportMode = "@@@";
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Transport Mode", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_TransportMode = consolidatedDeclaration.JobDeclarations[0].JE_TransportMode;
				consolidatedDeclaration.JobDeclarations[1].JE_MessageSubType = "@@@";
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different Entry Style", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_MessageSubType = consolidatedDeclaration.JobDeclarations[0].JE_MessageSubType;
				consolidatedDeclaration.JobDeclarations[1].JE_DateOfArrival = new ZDateTime(2024, 10, 21);
				consolidatedDeclaration.Validation.ValidateAll();
				AssertNoRowError("Periodic Entries may have different Discharge ETA", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_VesselName = "CSCL Melbourne";
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different VesselName", consolidatedDeclaration, message);
				consolidatedDeclaration.JobDeclarations[1].JE_VesselName = consolidatedDeclaration.JobDeclarations[0].JE_VesselName;
				consolidatedDeclaration.JobDeclarations[1].JE_VoyageFlightNo = "0BOADN1MA";
				consolidatedDeclaration.Validation.ValidateAll();
				AssertHasRowError("Different VoyageNo", consolidatedDeclaration, message);

				consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
				consolidatedDeclaration.Validation.ValidateAll();
				AssertNoRowError("No error for empty selection", consolidatedDeclaration, message);
				consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
				consolidatedDeclaration.Validation.ValidateAll();
				AssertNoRowError("No error for single selection", consolidatedDeclaration, message);
			});
		}

		public void TestCheckCRD_PeriodTo()
		{
			const string Message = "You have not entered an Entry Period Date.";
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var declaration = consolidatedDeclaration.JobDeclarations[0];

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			consolidatedDeclaration.Validation.ValidateAll();
			AssertNoMessageError("No error for Normal declaration", consolidatedDeclaration.CRD_PeriodToInfo, Message);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			consolidatedDeclaration.Validation.ValidateAll();
			AssertHasMessageError("Message error for Periodic declaration", consolidatedDeclaration.CRD_PeriodToInfo, Message);

			consolidatedDeclaration.CRD_PeriodTo = ZDate.Today;
			AssertNoMessageError("No error when Entry Period Date is provided", consolidatedDeclaration.CRD_PeriodToInfo, Message);
		}
	}
}
