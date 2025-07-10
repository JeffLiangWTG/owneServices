using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefDocOrgCusCodeFilterBusinessObject))]
	sealed class RefDocOrgCusCodeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestRegulatingCountryFilter()
		{
			var code1 = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			var code2 = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			code1.DOC_RN_NKRegulatingCountry = "AU";
			code1.DOC_DocumentType = "ESI";
			code2.DOC_RN_NKRegulatingCountry = "CN";
			code2.DOC_DocumentType = "ESI";

			Factory.Save();

			var filter = new RefDocOrgCusCodeFilterBusinessObject();
			((ModuleNkFilter)filter[RefDocOrgCusCodeFilterBusinessObject.Descriptions.RegulatingCountry]).Property = "AU";
			((ModuleNkFilter)filter[RefDocOrgCusCodeFilterBusinessObject.Descriptions.RegulatingCountry]).IsActive = true;

			var codes = new RefDocOrgCusCodeCollection(Factory)
			{
				AdditionalFilter = filter.Filter
			};

			AssertCollectionContains("Should only contain code1", code1, codes);
			AssertCollectionNotContains("Should not contain code2", code2, codes);
		}

		public void TestDocumentTypeFilter()
		{
			var code1 = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			var code2 = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			code1.DOC_DocumentType = "ESI";
			code2.DOC_DocumentType = "AWB";

			Factory.Save();

			var filter = new RefDocOrgCusCodeFilterBusinessObject();
			((ModuleTextFilter)filter[RefDocOrgCusCodeFilterBusinessObject.Descriptions.DocumentType]).Property = "ESI";
			((ModuleTextFilter)filter[RefDocOrgCusCodeFilterBusinessObject.Descriptions.DocumentType]).IsActive = true;

			var codes = new RefDocOrgCusCodeCollection(Factory)
			{
				AdditionalFilter = filter.Filter
			};

			AssertCollectionContains("Should only contain code1", code1, codes);
			AssertCollectionNotContains("Should not contain code2", code2, codes);
		}

		public void TestDirectionFilter()
		{
			var code1 = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			var code2 = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			code1.DOC_DocumentType = "ESI";
			code1.DOC_Direction = "IMP";
			code2.DOC_DocumentType = "ESI";
			code2.DOC_Direction = "EXP";

			Factory.Save();

			var filter = new RefDocOrgCusCodeFilterBusinessObject();
			((ModuleTextFilter)filter[RefDocOrgCusCodeFilterBusinessObject.Descriptions.Direction]).Property = "IMP";
			((ModuleTextFilter)filter[RefDocOrgCusCodeFilterBusinessObject.Descriptions.Direction]).IsActive = true;

			var codes = new RefDocOrgCusCodeCollection(Factory)
			{
				AdditionalFilter = filter.Filter
			};

			AssertCollectionContains("Should only contain code1", code1, codes);
			AssertCollectionNotContains("Should not contain code2", code2, codes);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefDocOrgCusCodeFilterBusinessObject();
		}

		#endregion
	}
}
