using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.CmdLine;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS.Tests;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.CmdLine.Tests
{
	[TestFixture]
	class CmdLineTest
	{
		[Test]
		public void TestEmptyArgument()
		{
			AssertArgumentError(string.Empty, "No arguments entered.");
		}

		[Test]
		public void TestInvalidFunction()
		{
			AssertArgumentError("NoFUNCTION", "Invalid argument entered: NOFUNCTION");
		}

		[Test]
		public void TestAllCodeListProducersAreMapped()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var codeListAISPath = Path.Combine(binPath, @"CodeLists\TestFiles\Input\ais-cci-codelists_2025Mar.pdf");
			var codeListAISUCC5Path = Path.Combine(binPath, @"CodeLists\TestFiles\Input\ais-codelists_UCC5_2024December.xlsx");
			var codeListAESPath = Path.Combine(binPath, @"CodeLists\TestFiles\Input\aes-codelists_2022September.pdf");
			var codeListNCTSPath = Path.Combine(binPath, @"CodeLists\TestFiles\Input\ncts-codelists_2024October.pdf");

			var actualRunCodes = CodeListProgram.GetFunctionsToRun(
				string.Empty,
				GetAllCodeLists(),
				new[] {
					(ApplicationType.AIS, codeListAISPath),
					(ApplicationType.AISUCC5, codeListAISUCC5Path),
					(ApplicationType.AES, codeListAESPath),
					(ApplicationType.NCTS, codeListNCTSPath)
				},
				forceLoadFromFilePath: true
				).Keys.Select(key => key.Code).Distinct().OrderBy(x => x).ToArray();

			var expectedRunCodes = Constants.CodeListConstants.ValidCodeLists.Where(x => !Constants.CodeListConstants.UnpublishedCodeLists.Contains(x))
				.OrderBy(x => x).ToArray();

			Assert.That(actualRunCodes, Is.EquivalentTo(expectedRunCodes));
		}

		static IRevenueCodeListDetails[] GetAllCodeLists() => new IRevenueCodeListDetails[]
		{
			new AdditionalDeclarationTypesDetails(),
			new AdditionalProcedureDetails(),
			new AdditionalReference(),
			new AdditionalReferenceNational(),
			new AdditionalInformation(),
			new AuthorisationCodeTypesDetails(),
			new ErrorTypesDetails(),
			new LocationIdentificationQualifiersDetails(),
			new LegalBasisTypeDetails(),
			new LocationOfGoodsDetails(),
			new LocationTypesDetails(),
			new NatureOfTransactionsDetails(),
			new PreviousDocumentTypesDetails(),
			new SupportingDocumentTypesDetails(),
			new TransportDocumentTypeDetails(),
			new UnitsOfMeasurementDetailsForTesting(),
			new CountryCodeDetails(),

			new CodeLists.Services.AISUCC5.AdditionalDeclarationTypesDetails(),
			new CodeLists.Services.AISUCC5.AdditionalProcedureDetails(),
			new CodeLists.Services.AISUCC5.AdditionalInformation(),
			new CodeLists.Services.AISUCC5.AuthorisationCodeTypesDetails(),
			new CodeLists.Services.AISUCC5.ControlResultDetails(),
			new CodeLists.Services.AISUCC5.ErrorTypesDetails(),
			new CodeLists.Services.AISUCC5.KindOfPackagesDetails(),
			new CodeLists.Services.AISUCC5.LegalBasisTypeDetails(),
			new CodeLists.Services.AISUCC5.LocationIdentificationQualifiersDetails(),
			new CodeLists.Services.AISUCC5.LocationOfGoodsDetails(),
			new CodeLists.Services.AISUCC5.LocationTypesDetails(),
			new CodeLists.Services.AISUCC5.NatureOfTransactionsDetails(),
			new CodeLists.Services.AISUCC5.PreviousDocumentTypesDetails(),
			new CodeLists.Services.AISUCC5.SupportingDocumentTypesDetails(),
			new CodeLists.Services.AISUCC5.TransportDocumentTypeDetails(),
			new CodeLists.Services.AISUCC5.Tests.UnitsOfMeasurementDetailsForTesting(),

			new CodeLists.Services.AES.AdditionalProcedureDetails(),
			new CodeLists.Services.AES.AdditionalReference(),
			new CodeLists.Services.AES.AdditionalReferenceNational(),
			new CodeLists.Services.AES.AddtionalInformation(),
			new CodeLists.Services.AES.BusinessRejectionType(),
			new CodeLists.Services.AES.CalculationOfTaxes(),
			new CodeLists.Services.AES.CountryCodesCommonTransitOutsideCommunity(),
			new CodeLists.Services.AES.CountryCodesCountryRegimeOTH(),
			new CodeLists.Services.AES.DestinationCountry(),
			new CodeLists.Services.AES.DiversionRejectionCode(),
			new CodeLists.Services.AES.ExitControlResultCode(),
			new CodeLists.Services.AES.FunctionalErrorCode(),
			new CodeLists.Services.AES.MeasurementUnitAndQualifier(),
			new CodeLists.Services.AES.NotificationType(),
			new CodeLists.Services.AES.PreviousDocumentType(),
			new CodeLists.Services.AES.RejectionReasonType(),
			new CodeLists.Services.AES.SpecificCircumstanceIndicator(),
			new CodeLists.Services.AES.SupportingDocument(),
			new CodeLists.Services.AES.TransportCharges(),
			new CodeLists.Services.AES.TransportDocument(),
			new CodeLists.Services.AES.TypeOfAlternativeEvidence(),
			new CodeLists.Services.AES.TypeOfControls(),

			new CodeLists.Services.NCTS.AdditionalInformation(),
			new CodeLists.Services.NCTS.AdditionalReference(),
			new CodeLists.Services.NCTS.AuthorisationTypeDeparture(),
			new CodeLists.Services.NCTS.AuthorisationTypeDestination(),
			new CodeLists.Services.NCTS.ControlType(),
			new CodeLists.Services.NCTS.CountryCodeFullList(),
			new CodeLists.Services.NCTS.DeclarationTypeAdditional(),
			new CodeLists.Services.NCTS.DocumentType(),
			new CodeLists.Services.NCTS.FunctionalErrorCode(),
			new CodeLists.Services.NCTS.GuaranteeType(),
			new CodeLists.Services.NCTS.IncidentCode(),
			new CodeLists.Services.NCTS.NotificationType(),
			new CodeLists.Services.NCTS.PreviousDocument(),
			new CodeLists.Services.NCTS.PreviousDocumentExport(),
			new CodeLists.Services.NCTS.PreviousDocumentExcise(),
			new CodeLists.Services.NCTS.QualifierOfIdentificationIncident(),
			new CodeLists.Services.NCTS.QueryIdentifier(),
			new CodeLists.Services.NCTS.RejectionCodeDepartureExport(),
			new CodeLists.Services.NCTS.RejectionCodeDestinationExit(),
			new CodeLists.Services.NCTS.SpecificCircumstanceIndicatorCode(),
			new CodeLists.Services.NCTS.SupportingDocumentType(),
			new CodeLists.Services.NCTS.TransportChargesMethodOfPayment(),
			new CodeLists.Services.NCTS.UNDangerousGoodsCode(),
			new CodeLists.Services.NCTS.XmlErrorCodesCode(),
		};

		internal static void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var binFilesPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var processStartInfo = new ProcessStartInfo(Path.Combine(binFilesPath, "CargoWise.RefDbRepo.IEReferenceData.CmdLine.exe"), parameters)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");
			var message = ProcessRunner.RunProcess(processStartInfo, false);
			Assert.That(message, Does.Contain(expectedErrorMessage));
		}
	}
}
