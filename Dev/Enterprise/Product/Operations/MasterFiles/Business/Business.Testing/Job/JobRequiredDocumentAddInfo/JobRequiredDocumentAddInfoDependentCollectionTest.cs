using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobRequiredDocumentAddInfoDependentCollection))]
	sealed class JobRequiredDocumentAddInfoDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetDistinctCompanies()
		{
			var company = Factory.New<GlbCompany>();
			var company2 = Factory.New<GlbCompany>();

			var requiredDocumentAddInfo = RequiredDocument.AddInfos.AddNew();
			AssertEquals(1, RequiredDocument.AddInfos.GetDistinctCompanies().Count());

			requiredDocumentAddInfo.EX_GC_Company = company.PK;
			Assert(RequiredDocument.AddInfos.GetDistinctCompanies().Contains(company));

			var requiredDocumentAddInfo2 = RequiredDocument.AddInfos.AddNew();
			AssertEquals(2, RequiredDocument.AddInfos.GetDistinctCompanies().Count());

			requiredDocumentAddInfo2.EX_GC_Company = company.PK;
			AssertEquals(1, RequiredDocument.AddInfos.GetDistinctCompanies().Count());
			Assert(RequiredDocument.AddInfos.GetDistinctCompanies().Contains(company));

			var requiredDocumentAddInfo3 = RequiredDocument.AddInfos.AddNew();
			requiredDocumentAddInfo3.EX_GC_Company = company2.PK;
			AssertEquals(2, RequiredDocument.AddInfos.GetDistinctCompanies().Count());
		}

		public void TestGetInstructions()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "US";

			AssertEquals("", RequiredDocument.AddInfos.GetInstructionsOnHowToDeleteAddInfos(company));

			var addInfo = RequiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_GC_Company = company.PK;
			AssertEquals("go to eDocs and click 'View/Edit DIS Data'. Then delete the related records in the first grid.", RequiredDocument.AddInfos.GetInstructionsOnHowToDeleteAddInfos(company));

			addInfo = RequiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_GC_Company = company.PK;
			AssertEquals("go to eDocs and click 'View/Edit DIS Data'. Then delete the related records in the first grid.", RequiredDocument.AddInfos.GetInstructionsOnHowToDeleteAddInfos(company));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobRequiredDocumentAddInfoDependentCollection(RequiredDocument);
		}

		JobRequiredDocument RequiredDocument
		{
			get { return requiredDocument ?? (requiredDocument = Factory.New<JobRequiredDocument>()); }
		}
		JobRequiredDocument requiredDocument;
	}
}
