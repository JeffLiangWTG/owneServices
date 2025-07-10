using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.DataMapping.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class ExamSurveyQuestionImporterBizOValidationTest : BusinessObjectValidationTestCase
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateFileLocation()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			ExamSurveyQuestionImporterBizO bizO = new ExamSurveyQuestionImporterBizO(campaign, new FileMapperForTest());
			bizO.FileLocation = "meh";
			AssertEquals(1, bizO.FileLocationInfo.GetErrors().Count());
			AssertEquals("File 'meh' not found or accessible", bizO.FileLocationInfo.GetErrors().GetFirstMessage());

			bizO.FileLocation = "";
			AssertEquals(1, bizO.FileLocationInfo.GetErrors().Count());
			AssertMandatoryValidationError(bizO.FileLocationInfo, true);

			bizO.FileLocation = Path.Combine(BaseSourcePath, "build.xml");
			AssertEquals(0, bizO.FileLocationInfo.GetErrors().Count());
		}

		public void TestValidateStartingRowIndex()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			ExamSurveyQuestionImporterBizO bizO = new ExamSurveyQuestionImporterBizO(campaign, new FileMapperForTest());
			bizO.StartingRowIndex = -1;
			AssertEquals(1, bizO.StartingRowIndexInfo.GetErrors().Count());
			AssertContains(MandatoryValidation.ValueCannotBeNegative, bizO.StartingRowIndexInfo.GetErrors().GetFirstMessage());

			bizO.StartingRowIndex = 0;
			AssertEquals(1, bizO.StartingRowIndexInfo.GetErrors().Count());
			AssertMandatoryValidationError(bizO.StartingRowIndexInfo, true);

			bizO.StartingRowIndex = 1;
			AssertEquals(0, bizO.StartingRowIndexInfo.GetErrors().Count());
		}

		public void TestDoNotValidateStartingRowIndexIfXmlFile()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			ExamSurveyQuestionImporterBizO bizO = new ExamSurveyQuestionImporterBizO(campaign, new FileMapperForTest());
			bizO.FileLocation = "import.xml";
			bizO.StartingRowIndex = -1;
			AssertEquals(0, bizO.StartingRowIndexInfo.GetErrors().Count());

			bizO.StartingRowIndex = 0;
			AssertEquals(0, bizO.StartingRowIndexInfo.GetErrors().Count());
		}
	}
}
