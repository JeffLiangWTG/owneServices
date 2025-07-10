using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruiter.GUI.Testing
{
	sealed class ResumeDragDropHelperTest : TestCaseWithFactory
	{
		public void TestProcessResumeDragDropWithoutDaxtra()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG_TEST1";
			Factory.Save();
			var configs = new ReferringPartyConfigurationCollection();
			var config1 = configs.AddNew();
			config1.Domain = "@wisetechglobal.com";
			config1.ReferringParty = "OH";
			config1.DefaultReferringSource = "AGT";
			config1.OrganizationPK = org.PK;
			RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configs);
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			ResumeDragDropHelper.ProcessResumeDragDropWithoutDaxtra(new DataObject(DataFormats.FileDrop, new string[] { EmptyMsgPath }), application);
			AssertEquals("AGT", application.HP_SourceType);
			AssertEquals(org.PK, application.HP_OH_ReferringOrganisation);
		}

		public void TestHP_SubmissionTimeUtcWithEmailInDifferentFormat()
		{
			using (RecruiterDataRegistry.Instance.DocTypeCV.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid())) // See Work Item WI00256361
			{
				var application = Factory.NewWithValidTestData<HRJobApplication>();
				using (var form = new HRJobApplicationForm(application))
				{
					ResumeDragDropHelper.ProcessResumeDragDrop(new DataObject(DataFormats.FileDrop, new string[] { EmptyMsgPath }), form);
					AssertEquals(new DateTime(2019, 03, 13, 00, 05, 09), application.HP_SubmissionTimeUtc);
				}

				using (var form = new HRJobApplicationForm(application))
				{
					ResumeDragDropHelper.ProcessResumeDragDrop(new DataObject(DataFormats.FileDrop, new string[] { CantReadEmailAddressPath }), form);
					AssertEquals(new DateTime(2019, 08, 07, 00, 48, 20), application.HP_SubmissionTimeUtc);
				}
			}
		}

		public void TestDocManagerInfoUseBusinessEntityFactoryAsInternalWhenDoingDragDrop()
		{
			ExecuteDragDropTest(saveApplicationToDatabase: false);
		}

		public void TestDocManagerInfoUseBusinessEntityFactoryAsInternalWhenDoingDragDrop_ApplicationIsInDatabase()
		{
			ExecuteDragDropTest(saveApplicationToDatabase: true);
		}

		void ExecuteDragDropTest(bool saveApplicationToDatabase)
		{
			using (RecruiterDataRegistry.Instance.DocTypeCV.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid())) // See Work Item WI00256361
			{
				var application = Factory.NewWithValidTestData<HRJobApplication>();

				if (saveApplicationToDatabase)
				{
					Factory.Save();
				}

				using (var form = new HRJobApplicationForm(application))
				{
					ResumeDragDropHelper.ProcessResumeDragDrop(new DataObject(DataFormats.FileDrop, new string[] { EmptyMsgPath }), form);
					Assert(application.DocManagerInfo.UseBusinessEntityFactoryAsInternal);
					var applicationReloaded = ((BusinessObjectFactory)application.DocManagerInfo.MasterFactory).Load<HRJobApplication>(application.PK);
					AssertNotNull(applicationReloaded);
				}
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(Business.Testing.EmailParsingRuleTest).Assembly));

		string emptyMsgPath;
		string EmptyMsgPath
		{
			get
			{
				if (string.IsNullOrEmpty(emptyMsgPath))
				{
					emptyMsgPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.msg", "empty.msg");
				}
				return emptyMsgPath;
			}
		}

		string cantReadEmailAddressPath;
		string CantReadEmailAddressPath
		{
			get
			{
				if (string.IsNullOrEmpty(cantReadEmailAddressPath))
				{
					cantReadEmailAddressPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.cant-read-email-address.eml", "cant-read-email-address.eml");
				}
				return cantReadEmailAddressPath;
			}
		}
	}
}
