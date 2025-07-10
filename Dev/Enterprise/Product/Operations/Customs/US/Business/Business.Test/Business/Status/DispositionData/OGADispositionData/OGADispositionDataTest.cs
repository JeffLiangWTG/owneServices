using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OGADispositionData))]
	sealed class OGADispositionDataTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<OGADispositionData>
	{
		public void TestDefault()
		{
			OGADispositionData data = Factory.New<OGADispositionData>();
			AssertEquals(CusAddInfoTypeAttribute.Codes.USOGADisposition, data.B7_Type);
		}

		public void TestPGAAgencyCodeDesc()
		{
			var code = Declaration.OGADispositionCodes.AddNew();
			code.US_ProgramCode = PGAAgencyProgramCodeList.Codes.Biologics;
			AssertEquals(PGAAgencyProgramCodeList.Descriptions.Biologics, code.AgencyCodeDesc);

			code.US_ProgramCode = PGAAgencyProgramCodeList.Codes.LaceyAct;
			AssertEquals(PGAAgencyProgramCodeList.Descriptions.LaceyAct, code.AgencyCodeDesc);
		}

		public void TestReviewReasonCodeDesc()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineReviewReason, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineReviewReason);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineReviewReason, "11", "GENERAL", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineReviewReason, "31", "PN REG HOLD MANUF NOT RGSTRD", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var code = Declaration.OGADispositionCodes.AddNew();
			code.US_Source = OGADispositionSourceList.Codes.PGA;
			code.US_ReviewReasonCode = "11";
			AssertEquals("GENERAL", code.ReviewReasonCodeDesc);
			code.US_ReviewReasonCode = "31";
			AssertEquals("Description should come from dbo.ZZRefCusCodeList", "PN REG HOLD MANUF NOT RGSTRD", code.ReviewReasonCodeDesc);
		}

		public void TestDispositionCodeDesc()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "10", "DOCUMENT REQUIRED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var code = Declaration.OGADispositionCodes.AddNew();
			code.US_Code = FDALineLevelDispositionCodeList.Codes._10;

			AssertEquals(FDALineLevelDispositionCodeList.Descriptions._10, code.DispositionCodeDesc);
			AssertEquals("Parent Loading", Declaration, code.Parent);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(FDALineLevelDispositionCodeList.Descriptions._10, code.DispositionCodeDesc);

			code.US_Source = OGADispositionSourceList.Codes.PGA;
			AssertEquals("Description should come from dbo.ZZRefCusCodeList", "DOCUMENT REQUIRED", code.DispositionCodeDesc);
		}

		public void TestDocumentTypeDesc()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "DIS Form List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "AMS01", "AMS_FOREIGN_GOVT_EXPORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var code = Declaration.OGADispositionCodes.AddNew();
			code.US_DocumentType = DocumentTypeCodeList.Codes._01;
			AssertEquals(DocumentTypeCodeList.Descriptions._01, code.DocumentTypeDesc);
			code.US_DocumentType = "AMS01";
			AssertEquals("AMS_FOREIGN_GOVT_EXPORT", code.DocumentTypeDesc);
			code.US_DocumentType = "ABCDE";
			AssertEquals(ZString.Empty, code.DocumentTypeDesc);
		}

		public JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Declaration.OGADispositionCodes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var data = factory.New<JobDeclaration>().OGADispositionCodes.AddNew();
			data.US_Code = FDALineLevelDispositionCodeList.Codes._02;
			data.US_DispositionDate = ZDateTime.Today;
			return data;
		}
	}
}
