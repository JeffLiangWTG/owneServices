using System.Text;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using Enterprise.Edifact.D96B.Segments;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses
{
	internal class ProdatValidatorForTest
	{
		public static bool ValidateMessage_Exposed(StringBuilder errorCollector, string[] secData) => ProdatValidator.ValidateMessage(EdifactValidationTestHelper.PrepareSecGroup<Enterprise.Edifact.D96B.Messages.PRODAT.PRODATMessage>(secData, errorCollector), errorCollector);

		public static bool Validate_Group0_Exposed(StringBuilder errorCollector, string[] secData) => ProdatValidator.Validate_Group0(EdifactValidationTestHelper.PrepareSecGroup<Enterprise.Edifact.D96B.Messages.PRODAT.PRODATMessage>(secData, errorCollector), errorCollector);
		public static bool Validate_Group0_UNH_Exposed(StringBuilder errorCollector, string[] segData) => ProdatValidator.Validate_Group0_UNH(EdifactValidationTestHelper.PrepareMsgSection(segData, new UNHSegmentMessageSection(9999), errorCollector), errorCollector);
		public static bool Validate_Group0_BGM_Exposed(StringBuilder errorCollector, string[] segData) => ProdatValidator.Validate_Group0_BGM(EdifactValidationTestHelper.PrepareMsgSection(segData, new BGMSegmentMessageSection(9999), errorCollector), errorCollector);
		public static bool Validate_Group0_DTM_Exposed(StringBuilder errorCollector, string[] segData) => ProdatValidator.Validate_Group0_DTM(EdifactValidationTestHelper.PrepareMsgSection(segData, new DTMSegmentMessageSection(9999), errorCollector), errorCollector);

		public static bool Validate_Group8_Exposed(StringBuilder errorCollector, string[] secData) => ProdatValidator.Validate_Group8(EdifactValidationTestHelper.PrepareSecGroup<Enterprise.Edifact.D96B.Messages.PRODAT.SegmentGroup8>(secData, errorCollector), errorCollector);
		public static bool Validate_Group8_LIN_Exposed(StringBuilder errorCollector, string[] segData, out string lineNumber) => ProdatValidator.Validate_Group8_LIN(EdifactValidationTestHelper.PrepareMsgSection(segData, new LINSegmentMessageSection(9999), errorCollector), errorCollector, out lineNumber);
		public static bool Validate_Group8_PIA_Exposed(StringBuilder errorCollector, string[] segData, string lineNumber) => ProdatValidator.Validate_Group8_PIA(EdifactValidationTestHelper.PrepareMsgSection(segData, new PIASegmentMessageSection(9999), errorCollector), errorCollector, lineNumber);
		public static bool Validate_Group8_DTM_Exposed(StringBuilder errorCollector, string[] segData, string lineNumber) => ProdatValidator.Validate_Group8_DTM(EdifactValidationTestHelper.PrepareMsgSection(segData, new DTMSegmentMessageSection(9999), errorCollector), errorCollector, lineNumber);
		public static bool Validate_Group8_MEA_Exposed(StringBuilder errorCollector, string[] segData, string lineNumber) => ProdatValidator.Validate_Group8_MEA(EdifactValidationTestHelper.PrepareMsgSection(segData, new MEASegmentMessageSection(9999), errorCollector), errorCollector, lineNumber);
		public static bool Validate_Group8_FTX_Exposed(StringBuilder errorCollector, string[] segData, string lineNumber) => ProdatValidator.Validate_Group8_FTX(EdifactValidationTestHelper.PrepareMsgSection(segData, new FTXSegmentMessageSection(9999), errorCollector), errorCollector, lineNumber);
		public static bool Validate_Group8_PGI_Exposed(StringBuilder errorCollector, string[] segData, string lineNumber) => ProdatValidator.Validate_Group8_PGI(EdifactValidationTestHelper.PrepareMsgSection(segData, new PGISegmentMessageSection(9999), errorCollector), errorCollector, lineNumber);
	}
}
