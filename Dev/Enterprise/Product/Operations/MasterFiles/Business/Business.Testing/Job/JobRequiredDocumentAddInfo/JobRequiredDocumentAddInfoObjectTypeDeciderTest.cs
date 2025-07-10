using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobRequiredDocumentAddInfoObjectTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForNewAddInfo()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var addInfo = Factory.New<JobRequiredDocumentAddInfo>();
			addInfo.EX_GC_Company = company.PK;
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.DIS.IDISDocument>(), new JobRequiredDocumentAddInfoObjectTypeDecider().GetTypeForNewAddInfo(addInfo));

			company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			addInfo = Factory.New<JobRequiredDocumentAddInfo>();
			addInfo.EX_GC_Company = company.PK;
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.DIF.IDIFDocument>(), new JobRequiredDocumentAddInfoObjectTypeDecider().GetTypeForNewAddInfo(addInfo));
		}

		public void TestGetInstructions()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "US";
			AssertEquals("go to eDocs and click 'View/Edit DIS Data'. Then delete the related records in the first grid.", JobRequiredDocumentAddInfoObjectTypeDecider.GetInstructionsToDeleteDISMessagingRecords(company, Core.Constants.Customs.DocumentImageSystemIDs.US_DIS));

			company.GC_RN_NKCountryCode = "PR";
			AssertEquals("go to eDocs and click 'View/Edit DIS Data'. Then delete the related records in the first grid.", JobRequiredDocumentAddInfoObjectTypeDecider.GetInstructionsToDeleteDISMessagingRecords(company, Core.Constants.Customs.DocumentImageSystemIDs.US_DIS));

			company.GC_RN_NKCountryCode = "CA";
			AssertEquals("Please click 'View/Edit DIS Data'. Then delete the related records in the first grid.", JobRequiredDocumentAddInfoObjectTypeDecider.GetInstructionsToDeleteDISMessagingRecords(company, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF));
		}
	}
}
