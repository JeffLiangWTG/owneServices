using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(StowPlanMessageIssueCollectionView))]
	class StowPlanMessageIssueCollectionViewTest : BusinessObjectCollectionViewTestCase<StowPlanMessageIssueCollectionView>
	{
		protected override StowPlanMessageIssueCollectionView GetCollectionToTest()
		{
			return SailingData.IssueCollectionView;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return SailingData.IssueCollection.AddNew(ZGuid.Empty, "", "", "", "", NotificationType.MessageError);
		}

		StowPlanSailingData SailingData
		{
			get { return fSailingData ?? (fSailingData = new StowPlanSailingData(Factory.New<JobVoyage>())); }
		}
		StowPlanSailingData fSailingData;
	}
}
