using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignAttachmentItemCollection))]
	sealed class GlbCompanyCampaignAttachmentItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GlbCompanyCampaignAttachmentItemCollection>
	{
		protected override GlbCompanyCampaignAttachmentItemCollection GetCollectionToTest()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			return new GlbCompanyCampaignAttachmentItemCollection(campaign);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GlbCompanyCampaignAttachmentItem(Factory);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSynchronise()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();

			using (TempFile tmpFile = TempFile.NewWithExtension("txt"))
			{
				File.WriteAllText(tmpFile.Filename, "test 1");

				IeDoc newFile1 = campaign.DocManagerInfo.AddFileOrDocument(tmpFile.Filename, "MSC");
				newFile1.Description = "temp file 1";

				AssertEquals(1, campaign.DocManagerInfo.Files.Count);
				AssertEquals(0, campaign.DocManagerInfo.Documents.Count);
				AssertEquals(1, campaign.CampaignAttachments.Count);

				IeDoc newFile2 = campaign.DocManagerInfo.AddFileOrDocument(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "small.gif"), "MSC");
				newFile2.Description = "small image";
				((BusinessObject)newFile2)["SC_FileName"] = "small.gif"; // DocManagerInfo.AddFileOrDocument() does not correctly set all properties

				campaign.CampaignAttachments.Synchronise();

				AssertEquals(1, campaign.DocManagerInfo.Files.Count);
				AssertEquals(1, campaign.DocManagerInfo.Documents.Count);
				AssertEquals(2, campaign.CampaignAttachments.Count);

				campaign.CampaignAttachments[1].Selected = true;
				campaign.DocManagerInfo.Files.Remove(campaign.DocManagerInfo.Files[0]);
				campaign.CampaignAttachments.Synchronise();

				AssertEquals(0, campaign.DocManagerInfo.Files.Count);
				AssertEquals(1, campaign.DocManagerInfo.Documents.Count);
				AssertEquals(1, campaign.CampaignAttachments.Count);
				AssertEquals("Selected flag remains set", true, campaign.CampaignAttachments[0].Selected);

				campaign.CampaignAttachments.Synchronise();

				AssertEquals(0, campaign.DocManagerInfo.Files.Count);
				AssertEquals(1, campaign.DocManagerInfo.Documents.Count);
				AssertEquals(1, campaign.CampaignAttachments.Count);
				AssertEquals("Selected flag remains set", true, campaign.CampaignAttachments[0].Selected);

				campaign.DocManagerInfo.Documents.Remove(campaign.DocManagerInfo.Documents[0]);
				campaign.CampaignAttachments.Synchronise();

				AssertEquals(0, campaign.DocManagerInfo.Files.Count);
				AssertEquals(0, campaign.DocManagerInfo.Documents.Count);
				AssertEquals(0, campaign.CampaignAttachments.Count);
			}
		}

		public void TestLoad()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			using (var tempDir = new TempDirectory())
			{
				var filePath1 = Path.Combine(tempDir.DirectoryName, "file 1.txt");
				var filePath2 = Path.Combine(tempDir.DirectoryName, "file 2, with comma.txt");
				var filePath3 = Path.Combine(tempDir.DirectoryName, "file 3.txt");
				var filePath4 = Path.Combine(tempDir.DirectoryName, "file 4, with comma.txt");

				try
				{
					File.WriteAllText(filePath1, "test 1 file text");
					File.WriteAllText(filePath2, "test 2 file text");
					File.WriteAllText(filePath3, "test 3 file text");
					File.WriteAllText(filePath4, "test 4 file text");

					campaign.G0_AttachmentList = "file 1.txt,\"file 2, with comma.txt\",crap.txt";

					campaign.DocManagerInfo.AddFileOrDocument(filePath1, "MSC");
					campaign.DocManagerInfo.AddFileOrDocument(filePath2, "MSC");
					campaign.DocManagerInfo.AddFileOrDocument(filePath3, "MSC");
					campaign.DocManagerInfo.AddFileOrDocument(filePath4, "MSC");

					Factory.Save();

					GlbCompanyCampaign loadedCampaign = Factory.Load<GlbCompanyCampaign>(campaign.PK);

					AssertEquals(4, loadedCampaign.CampaignAttachments.Count);
					AssertEquals(true, loadedCampaign.CampaignAttachments[0].Selected);
					AssertEquals(true, loadedCampaign.CampaignAttachments[1].Selected);
					AssertEquals(false, loadedCampaign.CampaignAttachments[2].Selected);
					AssertEquals(false, loadedCampaign.CampaignAttachments[3].Selected);

					AssertEquals(false, loadedCampaign.HasChanges);
				}
				finally
				{
					File.Delete(filePath1);
					File.Delete(filePath2);
					File.Delete(filePath3);
					File.Delete(filePath4);
				}
			}
		}

		public void TestSave()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			using (var tempDir = new TempDirectory())
			{
				var filePath1 = Path.Combine(tempDir.DirectoryName, "file 1.txt");
				var filePath2 = Path.Combine(tempDir.DirectoryName, "file 2, with comma.txt");
				var filePath3 = Path.Combine(tempDir.DirectoryName, "file 3.txt");
				var filePath4 = Path.Combine(tempDir.DirectoryName, "file 4, with comma.txt");

				try
				{
					File.WriteAllText(filePath1, "test 1 file text");
					File.WriteAllText(filePath2, "test 2 file text");
					File.WriteAllText(filePath3, "test 3 file text");
					File.WriteAllText(filePath4, "test 4 file text");

					campaign.DocManagerInfo.AddFileOrDocument(filePath1, "MSC");
					campaign.DocManagerInfo.AddFileOrDocument(filePath2, "MSC");
					campaign.DocManagerInfo.AddFileOrDocument(filePath3, "MSC");
					campaign.DocManagerInfo.AddFileOrDocument(filePath4, "MSC");

					AssertEquals(4, campaign.CampaignAttachments.Count);

					campaign.CampaignAttachments[0].Selected = true;
					campaign.CampaignAttachments[3].Selected = true;

					Factory.Save();

					AssertEquals("\"file 1.txt\",\"file 4, with comma.txt\"", campaign.G0_AttachmentList);
				}
				finally
				{
					File.Delete(filePath1);
					File.Delete(filePath2);
					File.Delete(filePath3);
					File.Delete(filePath4);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestSave_OnCampaignDelete()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (var tempDir = new TempDirectory())
			{
				var filePath1 = Path.Combine(tempDir.DirectoryName, "file 1.txt");
				try
				{
					File.WriteAllText(filePath1, "test 1 file text");
					campaign.G0_AttachmentList = "file 1.txt";
					campaign.DocManagerInfo.AddFileOrDocument(filePath1, "MSC");
					Factory.Save();

					var loadedCampaign = Factory.Load<GlbCompanyCampaign>(campaign.PK);
					AssertEquals(1, loadedCampaign.CampaignAttachments.Count);
					AssertEquals(true, loadedCampaign.CampaignAttachments[0].Selected);
					loadedCampaign.Delete();
					Factory.Save();
				}
				finally
				{
					File.Delete(filePath1);
				}
			}
		}

		public void TestG0_AttachmentListMaxLength()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			using (var tempDir = new TempDirectory())
			{
				var filePath1 = Path.Combine(tempDir.DirectoryName, ZString.Replicate('A', 128) + ".txt");
				var filePath2 = Path.Combine(tempDir.DirectoryName, ZString.Replicate('B', 128) + ".txt");

				try
				{
					File.WriteAllText(filePath1, "test 1 file text");
					File.WriteAllText(filePath2, "test 2 file text");

					campaign.DocManagerInfo.AddFileOrDocument(filePath1, "MSC");
					campaign.DocManagerInfo.AddFileOrDocument(filePath2, "MSC");

					AssertEquals(2, campaign.CampaignAttachments.Count);

					campaign.CampaignAttachments[0].Selected = true;
					campaign.CampaignAttachments[1].Selected = true;

					Factory.Save(); // Length of selected Filenames exceeds original MaxLength of G0_AttachmentList which is 256 thus, throwing a 'Max Length exceeds' exception.

					AssertEquals("\"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA.txt\",\"BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB.txt\"", campaign.G0_AttachmentList);
				}
				finally
				{
					File.Delete(filePath1);
					File.Delete(filePath2);
				}
			}
		}
	}
}
