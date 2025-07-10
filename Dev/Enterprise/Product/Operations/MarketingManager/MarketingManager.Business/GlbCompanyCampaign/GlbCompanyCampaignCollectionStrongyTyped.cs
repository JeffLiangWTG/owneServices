using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignCollectionStrongyTyped : ActiveBusinessObjectCollection<GlbCompanyCampaign>
	{
		public GlbCompanyCampaignCollectionStrongyTyped(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbCompanyCampaignCollectionStrongyTyped(GlbCompanyCampaign masterCampaign, ZQuery filter, ZByte? horizontalId = null)
			: base(masterCampaign.Factory, masterCampaign, filter, GlbCompanyCampaignSchema.G0_G0_Master)
		{
			this.masterCampaign = masterCampaign;
			this.HorizontalId = horizontalId;
		}

		readonly GlbCompanyCampaign masterCampaign;

		public ZByte? HorizontalId;

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();

			if (HorizontalId.HasValue)
			{
				filter.AddToFilter(GlbCompanyCampaignSchema.G0_HorizontalId, HorizontalId.Value);
			}

			return filter;
		}

		protected override object[] GetCollectionState()
		{
			return new object[]
			{
				HorizontalId
			};
		}

		protected override void SetDefaultsForNewElementCore(GlbCompanyCampaign newCampaign)
		{
			base.SetDefaultsForNewElementCore(newCampaign);

			if (masterCampaign != null && HorizontalId.HasValue)
			{
				newCampaign.G0_G0_Master = masterCampaign.PK;
				newCampaign.G0_CampaignName = masterCampaign.G0_CampaignName;
				newCampaign.G0_HorizontalId = HorizontalId.Value;
				newCampaign.G0_UseLastEmailSenderAddress = newCampaign.G0_HorizontalId > 1;
				newCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				newCampaign.G0_IsSalesAndMarketing = masterCampaign.G0_IsSalesAndMarketing;
				newCampaign.G0_Category = masterCampaign.G0_Category;
				newCampaign.G0_Type = masterCampaign.G0_Type;
				newCampaign.G0_GS_NKCampaignManager = masterCampaign.G0_GS_NKCampaignManager;
				newCampaign.G0_GS_NKCampaignCoordinator = masterCampaign.G0_GS_NKCampaignCoordinator;
				newCampaign.G0_Stage = masterCampaign.G0_Stage;
				newCampaign.G0_EstimatedStartedDate = masterCampaign.G0_EstimatedStartedDate;
				newCampaign.G0_EstimatedCompletedDate = masterCampaign.G0_EstimatedCompletedDate;
				newCampaign.G0_ActualStartedDate = masterCampaign.G0_ActualStartedDate;
				newCampaign.G0_ActualCompletedDate = masterCampaign.G0_ActualCompletedDate;
				newCampaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
				newCampaign.SendToAll = true;

				var currentHorizontal = masterCampaign.Horizontals.FirstOrDefault(h => h.Id == HorizontalId);

				var verticalIds = System.Array.Empty<ZString>();
				if (currentHorizontal != null)
				{
					verticalIds = currentHorizontal.Campaigns.Select(c => c.G0_VerticalId).ToArray();
				}

				foreach (var character in ZString.AlphabeticCharacters)
				{
					if (!verticalIds.Any(id => id.EqualsIgnoringCase(character.ToString())))
					{
						newCampaign.G0_VerticalId = character.ToString();
						break;
					}
				}

				newCampaign.MasterCampaign.AllTouches.Add(newCampaign);
			}
		}
	}
}
