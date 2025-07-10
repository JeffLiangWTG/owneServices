using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(OrgHeaderWrapper))]
	sealed class OrgHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCustomDocumentLabels()
		{
			var customLabels = orgHeader.CustomLabels;
			var customLabel1 = customLabels.AddNew();
			customLabel1.OT_Type = OrgConstants.CustomLabelType.OverrideExportDoc;
			var customLabel2 = customLabels.AddNew();
			customLabel2.OT_Type = OrgConstants.CustomLabelType.OverrideImportDoc;
			var customLabel3 = customLabels.AddNew();
			customLabel3.OT_Type = OrgConstants.CustomLabelType.OverrideExportDoc;
			var customLabel4 = customLabels.AddNew();
			customLabel4.OT_Type = OrgConstants.CustomLabelType.OverrideImportDoc;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("ExportCustomDocumentLabels", new[] { customLabel1.PK, customLabel3.PK }, orgHeaderWrapper.ExportCustomDocumentLabels.Select(c => c.PK));
				AssertContainsExactElementsInAnyOrder("ImportCustomDocumentLabels", new[] { customLabel2.PK, customLabel4.PK }, orgHeaderWrapper.ImportCustomDocumentLabels.Select(c => c.PK));
			});
		}

		public void TestAddInfo()
		{
			AssertSame(TWOrgImpAddInfo.Get(orgHeader), orgHeaderWrapper.AddInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return OrgHeaderWrapper.New(Factory.New<OrgHeader>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderWrapper = OrgHeaderWrapper.New(orgHeader);
		}

		OrgHeader orgHeader;
		OrgHeaderWrapper orgHeaderWrapper;
	}
}
