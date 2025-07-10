using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public static class OpportunityCommissionChangeUtils
	{
		public const string ExpectedOpportunityCommissionChangeCaption = "Opportunity Commission Change";
		public const string ExpectedOpportunityCommissionChangeCaptionText = @"The opportunity's current status of '{0}' indicates it is applicable for new commission agreements.
By changing the status to '{1}', this will no longer be allowed. Existing commission agreements on this opportunity will also be marked for reversal.

Are you sure you want to continue?";

		public static ZString[] ExpectedOpportunityCommissionChangeLogs => new ZString[] { "Opportunity status changed from effective to non-effective." };

		public static OpportunityStatusCollection GetOpportunityStatusCollection()
		{
			return new OpportunityStatusCollection
			{
				{ "NON", (NoResString)"Non-Effective", false, true, true, "" },
				{ "EFF", (NoResString)"Effective", true, true, true, "" }
			};
		}
	}
}
