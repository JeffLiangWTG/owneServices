using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignAttachmentItem))]
	sealed class GlbCompanyCampaignAttachmentItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPropertiesReadOnly()
		{
			GlbCompanyCampaignAttachmentItem item = new GlbCompanyCampaignAttachmentItem(Factory);
			AssertEquals(true, item.DescriptionInfo.ReadOnly);
			AssertEquals(true, item.FileNameInfo.ReadOnly);
			AssertEquals(false, item.SelectedInfo.ReadOnly);
		}

		public void TestMaxLengthsAreTheSameAsStorageDocs()
		{
			var item = new GlbCompanyCampaignAttachmentItem(Factory);
			AssertEquals(StorageDocsSchema.SC_Desc.MaxLength, item.DescriptionInfo.MaxLength);
			AssertEquals(StorageDocsSchema.SC_FileName.MaxLength, item.FileNameInfo.MaxLength);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSetLinkedDoc()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();

			using (var tmpFile = TempFile.NewWithExtension("txt"))
			{
				File.WriteAllText(tmpFile.Filename, "test 1");

				IeDoc newFile1 = campaign.DocManagerInfo.AddFileOrDocument(tmpFile.Filename, "MSC");
				newFile1.Description = "temp file 1";

				IeDoc newFile2 = campaign.DocManagerInfo.AddFileOrDocument(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "small.gif"), "MSC");
				newFile2.Description = "small image";

				var item1 = new GlbCompanyCampaignAttachmentItem(Factory);
				item1.SetLinkedDoc(newFile1, false);
				var item2 = new GlbCompanyCampaignAttachmentItem(Factory);
				item2.SetLinkedDoc(newFile2, true);

				AssertEquals("Text File Name incorrect", Path.GetFileName(tmpFile.Filename), item1.FileName);
				AssertEquals("Image File Name incorrect", "small.gif", item2.FileName);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GlbCompanyCampaignAttachmentItem(Factory);
		}
	}
}
