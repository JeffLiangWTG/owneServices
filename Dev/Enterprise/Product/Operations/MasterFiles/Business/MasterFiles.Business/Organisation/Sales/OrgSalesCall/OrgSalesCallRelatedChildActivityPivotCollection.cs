using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	class OrgSalesCallRelatedChildActivityPivotCollection : RelatedChildActivityPivotCollection
	{
		public OrgSalesCallRelatedChildActivityPivotCollection(OrgSalesCall master)
			: base(master)
		{
		}

		protected new OrgSalesCall Master
		{
			get { return (OrgSalesCall)base.Master; }
		}

		protected override RelationValidationResult CheckIsValidChildCore(IRelatableActivity parentActivity, IRelatableActivity childActivity, bool childMustBeInDatabase)
		{
			var baseValidation = base.CheckIsValidChildCore(parentActivity, childActivity, childMustBeInDatabase);
			if (!baseValidation.IsValid)
			{
				return baseValidation;
			}

			var linkedInquiry = Master.LinkedInquiry;
			if (linkedInquiry != null)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("21889ba1-8260-4625-ae29-2b3d7bc0059f", "{0} cannot have any additional relationships until client intelligence is set on its related inquiry ({1}).", Master.HumanReadableName, linkedInquiry.HumanReadableName));
			}

			return new RelationValidationResult(true);
		}
	}
}
