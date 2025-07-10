using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentRecipientWrapperForJobDocumentExclusion : JobDocumentRecipientWrapperForOrgDocument
	{
		public JobDocumentRecipientWrapperForJobDocumentExclusion(JobDocumentExclusion exclusion, JobDocumentRecipientConfiguration configuration)
			: base(exclusion.Document, configuration)
		{
			Exclusion = exclusion;
		}

		public JobDocumentExclusion Exclusion { get; }

		public override bool CanDelete => true;

		public override void Delete()
		{
			Exclusion.Delete();
			base.Delete();
		}

		public override ZBool IsExclusion => true;
	}
}
