using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(JobRequiredDocumentAddInfoFilterBusinessObject))]
	public class JobRequiredDocumentAddInfoFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region HiddentFilterTest

		public void TestCompanyFilter()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();

			var addInfo1 = Factory.NewWithValidTestData<JobRequiredDocumentAddInfo>();
			addInfo1.EX_ReferenceNumber = "REF1";
			addInfo1.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo1.EX_GC_Company = GlbCompany.CurrentCompany.PK;

			var addInfo2 = Factory.NewWithValidTestData<JobRequiredDocumentAddInfo>();
			addInfo2.EX_ReferenceNumber = "REF2";
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo2.EX_GC_Company = company.PK;
			Factory.Save();

			var glbCompanyFilter = new JobRequiredDocumentAddInfoFilterBusinessObject();
			var collection = new JobRequiredDocumentAddInfoCollection(Factory, glbCompanyFilter.Filter);
			collection.Load();

			AssertCollectionContains(addInfo1, collection);
			AssertCollectionNotContains(addInfo2, collection);
		}

		public void TestCategoryFilter()
		{
			var addInfo1 = Factory.NewWithValidTestData<JobRequiredDocumentAddInfo>();
			addInfo1.EX_ReferenceNumber = "REF1";
			addInfo1.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;

			var addInfo2 = Factory.NewWithValidTestData<JobRequiredDocumentAddInfo>();
			addInfo2.EX_ReferenceNumber = "REF2";
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			Factory.Save();

			var categoryFilter = new JobRequiredDocumentAddInfoFilterBusinessObject();
			((ModuleTextFilter)categoryFilter["Application Code"]).Property = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			((ModuleTextFilter)categoryFilter["Application Code"]).IsActive = true;

			var collection = new JobRequiredDocumentAddInfoCollection(Factory, categoryFilter.Filter);
			collection.Load();

			AssertCollectionContains(addInfo1, collection);
			AssertCollectionNotContains(addInfo2, collection);
		}
		#endregion

		public void TestDocumentNumberFilterQuery()
		{
			var document = Factory.NewWithValidTestData<JobRequiredDocument>();
			document.EQ_DocNumber = "DOC00001";
			var addInfo1 = document.AddInfos.AddNew();
			addInfo1.EX_ReferenceNumber = "REF1";
			addInfo1.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;

			var addInfo2 = document.AddInfos.AddNew();
			addInfo2.EX_ReferenceNumber = "REF2";
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;

			var document2 = Factory.NewWithValidTestData<JobRequiredDocument>();
			document2.EQ_DocNumber = "DOC00002";

			var addInfo3 = document2.AddInfos.AddNew();
			addInfo3.EX_ReferenceNumber = "REF3";
			addInfo3.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			Factory.Save();

			var documentNumberFilter = new JobRequiredDocumentAddInfoFilterBusinessObject();
			((ModuleTextFilter)documentNumberFilter["Document Number"]).Property = "DOC00001";
			((ModuleTextFilter)documentNumberFilter["Document Number"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			((ModuleTextFilter)documentNumberFilter["Document Number"]).IsActive = true;

			var collection = new JobRequiredDocumentAddInfoCollection(Factory, documentNumberFilter.Filter);
			collection.Load();

			AssertCollectionContains(addInfo1, collection);
			AssertCollectionContains(addInfo2, collection);
			AssertCollectionNotContains(addInfo3, collection);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobRequiredDocumentAddInfoFilterBusinessObject();
		}
		#endregion
	}
}
