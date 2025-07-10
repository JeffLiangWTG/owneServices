using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaUniversalEventMessageFailureProcessorTest : SGAsycudaUniversalEventMessageProcessorTest
	{
		public void TestCreateLogEntryFromErrors()
		{
			// Add a user description
			const string userCommentaryText = "USER GENERATED MESSAGE";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GlobalManifestErrorCommentary, "Global Manifest Error Commentary");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GlobalManifestErrorCommentary, "R01", userCommentaryText, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var errorPairs = new CodeDescriptionPairList();
			errorPairs.AddPair("ERR", "");
			errorPairs.AddPair("R01", "COUNTRY CODE/REGION/ORIGIN OF GOODS IS INVALID");
			errorPairs.AddPair("N14", "HSCODE IS MANDATORY,PLEASE CHECK CIF/FOB AND GOODS TYPE/FLIGHT NO");

			var logEntry = SGAsycudaUniversalEventMessageFailureProcessor.CreateLogEntryFromErrors(Factory, "ERR", errorPairs);
			AssertEquals("Multiple Entries", "ERR - ERR  - R01 USER GENERATED MESSAGE - N14 HSCODE IS MANDATORY,PLEASE CHECK CIF/FOB AND GOODS TYPE/FLIGHT NO", logEntry);

			var noCodeList = new CodeDescriptionPairList();
			noCodeList.AddPair("", "");

			var logEntry2 = SGAsycudaUniversalEventMessageFailureProcessor.CreateLogEntryFromErrors(Factory, "ERR", noCodeList);
			AssertEquals("Empty Code", "", logEntry2);

			var logEntry3 = SGAsycudaUniversalEventMessageFailureProcessor.CreateLogEntryFromErrors(Factory, "ERR", new CodeDescriptionPairList());
			AssertEquals("No Entries", "", logEntry3);

			var logEntry4 = SGAsycudaUniversalEventMessageFailureProcessor.CreateLogEntryFromErrors(Factory, "ERR", null);
			AssertEquals("null list", "", logEntry4);
		}
	}
}
