using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Interop.OutlookIntegration;
using Enterprise.Interop.OutlookIntegration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OutlookEmailSenderTest : TestCaseWithFactory
	{
		public void TestDontSaveToEDocs()
		{
			TestOutlookEmailSender contactSender = GetNewContactSender();
			ISendEmailSource sup = Factory.New<OrgHeader>();
			EmailSenderConfiguration selection = new EmailSenderConfiguration(sup);
			selection.HtmlEmail.Subject = "email subject";
			contactSender.DisplayEmailAndSave(
				new MockOutlookApplication(),
				selection);

			AssertEquals(0, contactSource.DocManagerInfo.Files.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestSaveToEdocs()
		{
			UnitTestUserNotification.Instance.ThrowOnWarningAndError = true;

			TestOutlookEmailSender contactSender = GetNewContactSender();
			ISendEmailSource sup = Factory.NewWithValidTestData<OrgHeader>();
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = documentFactoryProvider.GetFactory(Factory);
			contactSource.DocManagerInfo.MasterFactory = documentFactory;
			AssertEquals("Precondition", contactSource.DocManagerInfo.MasterFactory, documentFactory);
			var documentFactoryAsBizoFactory = (BusinessObjectFactory)documentFactory;
			AssertEquals("Precondition", 0, documentFactoryAsBizoFactory.SaveCount);

			EmailSenderConfiguration selection = new EmailSenderConfiguration(sup);
			selection.HtmlEmail.Subject = "email subject";
			selection.SaveToEDocsDocumentType = "MSC";
			contactSender.DisplayEmailAndSave(
				new MockOutlookApplication(),
				selection);
			AssertEquals("Should have saved once", 1, documentFactoryAsBizoFactory.SaveCount);

			if (contactSource.DocManagerInfo.Files.Count != 1)
			{
				Fail(string.Format(
					"There should be 1 file added, but were {0}.{1}\r\nError messages:\r\n{2}",
					contactSource.DocManagerInfo.Files.Count,
					contactSource.DocManagerInfo.Documents.Count > 0 ? "\r\nUnexpected image document was added with name " + contactSource.DocManagerInfo.Documents[0].FileName + "." : "",
					string.Join("\r\n", UnitTestUserNotification.Instance.PreviousMessages)));
			}

			AssertEquals("Doc type", "MSC", contactSource.DocManagerInfo.Files[0].DocType);
			AssertEquals("email subject", contactSource.DocManagerInfo.Files[0].Description);
			AssertEquals("email subject.msg", contactSource.DocManagerInfo.Files[0].FileName);

			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var tempFilePath = resourceRetriever.SaveResourceToFile(@"Enterprise.MasterFiles.GUI.Test.TestOutlookMsg.msg");
				AssertFileSameAsBytes(tempFilePath, contactSource.DocManagerInfo.Files[0].ImageData);
			}
		}

		#region Implementation

		ZForm form;
		MockContactSource contactSource;
		new BusinessObjectFactory Factory;

		protected override void SetUp()
		{
			base.SetUp();
			form = new ZForm();
			form.PlugIns.Add(Enterprise.ZArchitecture.Modules.ControllerIDs.eDocsPlugIn);
			form.Show();
			Application.DoEvents();
			Factory = new BusinessObjectFactory();
			contactSource = new MockContactSource(Factory);
		}

		TestOutlookEmailSender GetNewContactSender()
		{
			return new TestOutlookEmailSender(form, contactSource);
		}

		protected override void TearDown()
		{
			base.TearDown();
			form.Dispose();
		}

		class TestOutlookEmailSender : OutlookEmailSender
		{
			public TestOutlookEmailSender(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource)
				: base(uIThreadSyncInvoke, contactSource)
			{
			}

			public new void DisplayEmailAndSave(
				OutlookApplication outlook, EmailSenderConfiguration senderConfiguration)
			{
				base.DisplayEmailAndSave(outlook, senderConfiguration);
			}
		}

		class MockOutlookApplication : OutlookApplication
		{
			public override OutlookMailItem CreateMailItem(object customData)
			{
				return new MockOutlookMailItem(customData);
			}
		}

		internal class MockContactSource : NonPersistentBusinessObject, ISendEmailSource
		{
			public MockContactSource(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			#region ISendEmailSource Members

			public AddressBookSelection GetAddressBookSelection()
			{
				return new AddressBookSelection();
			}

			public string EmailSubject
			{
				get { return ""; }
			}

			public string TemplateCategory
			{
				get { return MailTemplateCategoryList.Codes.None; }
			}

			string ISendEmailSource.DefaultFromDisplayName
			{
				get { return GlbStaff.CurrentUser.GS_FullName; }
			}

			string ISendEmailSource.OverridingDefaultFromEmailAddress
			{
				get { return null; }
			}

			Type ISendEmailSource.DocWrapperType
			{
				get { return null; }
			}

			Logs ISendEmailSource.Logs
			{
				get { return null; }
			}

			#endregion

			#region IDocManagerSupport Members

			public DocManagerInfo DocManagerInfo
			{
				get
				{
					if (docManagerInfo == null)
					{
						docManagerInfo = new DocManagerInfo(this, "INC");
					}
					return docManagerInfo;
				}
			}
			DocManagerInfo docManagerInfo;

			#endregion
		}

		#endregion
	}
}
