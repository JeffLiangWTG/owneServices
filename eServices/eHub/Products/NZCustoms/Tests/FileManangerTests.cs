using System.IO;
using CargoWise.eHub.Products.NZCustoms.Common;
using CargoWise.eHub.Products.NZCustoms.PullService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.NZCustoms.Tests
{
	[TestClass]
	public class FileManangerTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FileManangerTests_SaveFailedToDeliverMessageTest()
		{
			var testConfigurationProvider = new TestConfigurationProvider();
			var fileMannager = new FileManagerTest("bla", testConfigurationProvider);

			var replyMessage = new NZCustomsReply
			{
				Reference = "C000101",
				Content = "This is content",
				Attachments = new[]
		        {
		            new Attachment() {Filename = "filename1.txt", ContentType = @"text\xml", Content = "Attachment 1"},
		            new Attachment() {Filename = "filename2.txt", ContentType = @"text\xml", Content = "Attachment 2"},
		            new Attachment() {Filename = "filename3.txt", ContentType = @"text\xml", Content = "Attachment 3"}
		        }
			};

			string filePath = fileMannager.SaveReceivedMessage(replyMessage, "MR1010110");

			var fileInfo = new FileInfo(filePath);
			Assert.IsTrue(fileInfo.Exists);

			try
			{
				using (var reader = new StreamReader(fileInfo.FullName))
				{
					string text = reader.ReadToEnd();
					Assert.AreEqual("<NZCustomsReply xmlns:ns0=\"http://cargowise.com/ehub/products/\"><Reference>C000101</Reference><Content>This is content</Content><Attachments><Attachment Filename=\"filename1.txt\" ContentType=\"text\\xml\">Attachment 1</Attachment><Attachment Filename=\"filename2.txt\" ContentType=\"text\\xml\">Attachment 2</Attachment><Attachment Filename=\"filename3.txt\" ContentType=\"text\\xml\">Attachment 3</Attachment></Attachments></NZCustomsReply>", text);
				}
			}
			finally
			{
				fileInfo.Delete();
			}
		}

		public class FileManagerTest : FileManager
		{
			string id;

			public FileManagerTest(string id, IConfigurationProvider configurationProvider)
				: base(configurationProvider)
			{
				this.id = id;
			}
			protected override string NewID
			{
				get
				{
					return id;
				}
			}
		}
	}
}
