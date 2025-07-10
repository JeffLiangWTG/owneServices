using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CustomDocumentsCollection))]
	sealed class CustomDocumentsCollectionTest : MasterFiles.Business.Testing.OrgCustomLabelsCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CustomDocumentsCollection(OrgConstants.CustomLabelType.OverrideExportDoc, Organisation, Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => GetCollectionToTest().AddNew();

		public void TestISequenceNumberHeader()
		{
			var collection = GetCollectionToTest();
			var customDocument1 = collection.AddNew();
			var customDocument2 = collection.AddNew();
			var customDocument3 = collection.AddNew();
			AssertContainsExactElementsInExactOrder(new[] { customDocument1, customDocument2, customDocument3 }, ((ISequenceNumberHeader)collection).Lines);
		}

		public void TestCreateAdditionalFilter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var overrideExportDoc = org.CustomLabels.AddNew();
			overrideExportDoc.OT_Type = OrgConstants.CustomLabelType.OverrideExportDoc;
			var overrideImportDoc = org.CustomLabels.AddNew();
			overrideImportDoc.OT_Type = OrgConstants.CustomLabelType.OverrideImportDoc;

			var overrideExportDocCollection = new CustomDocumentsCollection(OrgConstants.CustomLabelType.OverrideExportDoc, org, Factory);
			var overrideImportDocCollection = new CustomDocumentsCollection(OrgConstants.CustomLabelType.OverrideImportDoc, org, Factory);
			overrideExportDocCollection.Load();
			overrideImportDocCollection.Load();

			CombineAssertions(() =>
			{
				AssertEquals("overrideExportDocCollection.Count", 1, overrideExportDocCollection.Count);
				AssertEquals("overrideImportDocCollection.Count", 1, overrideImportDocCollection.Count);
				AssertEquals("overrideExportDocCollection[0].PK", overrideExportDoc.PK, overrideExportDocCollection[0].PK);
				AssertEquals("overrideImportDocCollection[0].PK", overrideImportDoc.PK, overrideImportDocCollection[0].PK);
			});
		}

		public void TestSetDefaultsForNewChild()
		{
			var overrideExportDoc = new CustomDocumentsCollection(OrgConstants.CustomLabelType.OverrideExportDoc, Organisation, Factory).AddNew();
			AssertEquals(OrgConstants.CustomLabelType.OverrideExportDoc, overrideExportDoc.OT_Type);

			var overrideImportDoc = new CustomDocumentsCollection(OrgConstants.CustomLabelType.OverrideImportDoc, Organisation, Factory).AddNew();
			AssertEquals(OrgConstants.CustomLabelType.OverrideImportDoc, overrideImportDoc.OT_Type);
		}

		OrgHeader Organisation
		{
			get
			{
				if (organisation == null)
				{
					organisation = Factory.NewWithValidTestData<OrgHeader>();
				}

				return organisation;
			}
		}

		OrgHeader organisation;
	}
}
