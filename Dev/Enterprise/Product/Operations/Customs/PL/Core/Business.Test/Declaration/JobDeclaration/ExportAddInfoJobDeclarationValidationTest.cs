using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class ExportAddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_BorderTransportMeansC0890_EntryStyle_CO()
	{
		const string transportMeans = "10";
		const string messageWarning = "[C0890] Active Border Transport Means, Border T.O.ID. will be skipped in declaration XML.";
		const string messageError = "Please enter a Type of Identification";

		var modesDisallowedForCO = new[] { TransportModes.Mail, TransportModes.FixedTransportInstallations };

		var codesAllowedForCO = new[] { ProcedureCodes._76, ProcedureCodes._77 };

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = ZString.Empty;
			declaration.ZG_BorderTransportMeans = transportMeans;
			AssertHasWarning("Mode is empty", declaration.ZG_BorderTransportMeansInfo, messageWarning);

			foreach (var code in codesAllowedForCO)
			{
				entryInstruction.CEI_Procedure = code;
				foreach (var mode in modesDisallowedForCO)
				{
					declaration.JE_TransportMode = mode;

					declaration.ZG_BorderTransportMeans = ZString.Empty;
					AssertNoWarning($"mode:{mode}, code:{code}, the Border Transport Means is not active", declaration.ZG_BorderTransportMeansInfo, messageWarning);

					declaration.ZG_BorderTransportMeans = transportMeans;
					AssertHasWarning($"mode:{mode}, code:{code}, the Border Transport Means is active", declaration.ZG_BorderTransportMeansInfo, messageWarning);
				}

				declaration.JE_TransportMode = TransportModes.Air;
				declaration.ZG_BorderTransportMeans = ZString.Empty;
				AssertHasMessageError($"Mandatory check will be executed", declaration.ZG_BorderTransportMeansInfo, messageError);
			}
		});
	}

	public void TestCheckZG_BorderTransportMeansC0890_EntryStyle_EX()
	{
		const string transportMeans = "10";
		const string messageWarning = "[C0890] Active Border Transport Means, Border T.O.ID. will be skipped in declaration XML.";
		const string messageError = "Please enter a Type of Identification";

		var modesAll = new[] { TransportModes.Air, TransportModes.FixedTransportInstallations, TransportModes.InlandWaterwayTransport,
			TransportModes.Mail, TransportModes.OwnPropulsion, TransportModes.Rail, TransportModes.Road, TransportModes.Sea };

		var codesAllowedForEX = new[] { ProcedureCodes._10, ProcedureCodes._11, ProcedureCodes._23, ProcedureCodes._31 };

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = ZString.Empty;
			declaration.ZG_BorderTransportMeans = transportMeans;
			AssertHasWarning("Mode is empty", declaration.ZG_BorderTransportMeansInfo, messageWarning);

			foreach (var mode in modesAll)
			{
				declaration.JE_TransportMode = mode;
				foreach (var code in codesAllowedForEX)
				{
					entryInstruction.CEI_Procedure = code;

					declaration.ZG_BorderTransportMeans = ZString.Empty;
					AssertHasMessageError($"mode:{mode}, code:{code}, mandatory check will be shown in the code (10, 11, 23, 31)", declaration.ZG_BorderTransportMeansInfo, messageError);

					declaration.ZG_BorderTransportMeans = transportMeans;
					AssertNoMessageError($"mode:{mode}, code:{code}, the transport means has value", declaration.ZG_BorderTransportMeansInfo, messageError);
				}
			}

			entryInstruction.CEI_Procedure = ProcedureCodes._42;
			declaration.ZG_BorderTransportMeans = transportMeans;
			AssertHasWarning("The warning will be shown when the style is EX with codes not in {10, 11, 23, 31}", declaration.ZG_BorderTransportMeansInfo, messageWarning);
			AssertNoMessageError($"The transport means has value", declaration.ZG_BorderTransportMeansInfo, messageError);
		});
	}

	public void TestCheckZG_BorderTransportMeans_ListValidation()
	{
		var messageError = "The code you have selected is not in the list.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.ZG_BorderTransportMeans = "10";
			declaration.AddInfoValidation.ValidateZG_BorderTransportMeans();
			AssertNoMessageError("No message error", declaration.ZG_BorderTransportMeansInfo, messageError);
			declaration.ZG_BorderTransportMeans = "12";
			declaration.AddInfoValidation.ValidateZG_BorderTransportMeans();
			AssertHasMessageError("No message error", declaration.ZG_BorderTransportMeansInfo, messageError);
		});
	}

	public void TestCheckZG_PresentationStartDate_RuleR0052E()
	{
		const string errorMessage = "[R0052E] You have not entered a Presentation Start Date.";

		var exportDeclaration = Factory.New<JobDeclaration>();
		var entryInstruction = exportDeclaration.CustomsEntryInstructions.AddNew();

		var propertyInfo = exportDeclaration.ZG_PresentationStartDateInfo;
		exportDeclaration.ZG_PresentationStartDate = ZDateTime.Empty;

		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = "R";
			exportDeclaration.AddInfoValidation.ValidateZG_PresentationStartDate();
			AssertHasMessageError("The instruction has Sub Style = R.", propertyInfo, errorMessage);

			entryInstruction.CEI_SubStyle = "A";
			exportDeclaration.AddInfoValidation.ValidateZG_PresentationStartDate();
			AssertNoMessageError("The instruction has Sub Style <> R.", propertyInfo, errorMessage);

			entryInstruction = exportDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = "A";
			exportDeclaration.AddInfoValidation.ValidateZG_PresentationStartDate();
			AssertNoMessageError("Two instructions have Sub Style <> R.", propertyInfo, errorMessage);

			entryInstruction.CEI_SubStyle = "R";
			exportDeclaration.AddInfoValidation.ValidateZG_PresentationStartDate();
			AssertHasMessageError("One of two instructions has Sub Style = R.", propertyInfo, errorMessage);

			exportDeclaration.ZG_PresentationStartDate = ZDateTime.Now;
			AssertNoMessageError("Presentation Start Date is not empty.", propertyInfo, errorMessage);
		});
	}
}
