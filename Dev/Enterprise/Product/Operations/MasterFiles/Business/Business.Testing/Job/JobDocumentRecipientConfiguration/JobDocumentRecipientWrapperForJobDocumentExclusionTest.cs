using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocumentRecipientWrapperForJobDocumentExclusion))]
	public class JobDocumentRecipientWrapperForJobDocumentExclusionTest : JobDocumentRecipientWrapperForOrgDocumentBaseTest<JobDocumentRecipientWrapperForJobDocumentExclusion>
	{
		public override void TestCanDelete()
		{
			var wrapper = (JobDocumentRecipientWrapperForJobDocumentExclusion)GetNewBusinessObject();

			Assert("JobDocumentExclusion wrapppers should be deletable.", wrapper.CanDelete);
		}

		public void TestDelete()
		{
			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			var exclusion = Factory.NewWithValidTestData<JobDocumentExclusion>();
			exclusion.JDE_OD_Document = orgDocument.PK;
			var wrapper = new JobDocumentRecipientWrapperForJobDocumentExclusion(exclusion, Configuration);
			Assert("Pre-condition", !exclusion.IsDeleted);
			Assert("Pre-condition", !orgDocument.IsDeleted);

			wrapper.Delete();
			Assert(exclusion.IsDeleted);
			Assert(!orgDocument.IsDeleted);
		}

		#region Implementation

		protected override JobDocumentRecipientWrapperForJobDocumentExclusion GetWrapper(OrgDocument orgDocument, JobDocumentRecipientConfiguration configuration)
		{
			var exclusion = Factory.NewWithValidTestData<JobDocumentExclusion>();
			exclusion.JDE_OD_Document = orgDocument.PK;
			return new JobDocumentRecipientWrapperForJobDocumentExclusion(exclusion, configuration);
		}

		protected override ZBool ExpectedValueForIsExclusion => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			var exclusion = Factory.NewWithValidTestData<JobDocumentExclusion>();
			exclusion.JDE_OD_Document = orgDocument.PK;
			orgDocument.OD_OC = orgContact.PK;
			return new JobDocumentRecipientWrapperForJobDocumentExclusion(exclusion, Configuration);
		}

		#endregion
	}
}
