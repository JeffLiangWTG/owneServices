using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(OrgCustomLabels))]
	sealed class OrgCustomLabelsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOT_Position()
		{
			customDocumentLabels.RemoveAndDeleteAll();
			var customDocumentLabels1 = customDocumentLabels.AddNew();
			var customDocumentLabels2 = customDocumentLabels.AddNew();
			var customDocumentLabels3 = customDocumentLabels.AddNew();
			AssertEquals((ZShort)1, customDocumentLabels1.OT_Position);
			AssertEquals((ZShort)2, customDocumentLabels2.OT_Position);
			AssertEquals((ZShort)3, customDocumentLabels3.OT_Position);

			customDocumentLabels1.OT_Position = 0;
			AssertEquals((ZShort)1, customDocumentLabels1.OT_Position);

			customDocumentLabels1.OT_Position = 2;
			AssertEquals((ZShort)2, customDocumentLabels1.OT_Position);
			AssertEquals((ZShort)1, customDocumentLabels2.OT_Position);
			AssertEquals((ZShort)3, customDocumentLabels3.OT_Position);

			customDocumentLabels3.OT_Position = 1;
			AssertEquals((ZShort)3, customDocumentLabels1.OT_Position);
			AssertEquals((ZShort)2, customDocumentLabels2.OT_Position);
			AssertEquals((ZShort)1, customDocumentLabels3.OT_Position);

			customDocumentLabels.RemoveAndDelete(customDocumentLabels3);
			AssertEquals((ZShort)2, customDocumentLabels1.OT_Position);
			AssertEquals((ZShort)1, customDocumentLabels2.OT_Position);

			var customDocumentLabels4 = customDocumentLabels.AddNew();
			AssertEquals((ZShort)3, customDocumentLabels4.OT_Position);
		}

		public void TestLookups()
		{
			AssertType<OrgCustomLabelsLookups>(customLabels.Lookups);
		}

		public void TestValidation()
		{
			AssertType<OrgCustomLabelsValidation>(customLabels.Validation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgWrapper = OrgHeaderWrapper.New(org);
			customDocumentLabels = orgWrapper.ExportCustomDocumentLabels;
			customLabels = customDocumentLabels.AddNew();
		}

		CustomDocumentsCollection customDocumentLabels;
		OrgCustomLabels customLabels;
	}
}
