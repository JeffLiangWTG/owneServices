using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.GUI
{
	public partial class CampaignTrackingSalesRelationControl : SalesRelationControl
	{
		public CampaignTrackingSalesRelationControl()
		{
			InitializeComponent();
		}

		protected override void CreateRelationshipAndShowFormForNewEntity(ZArchitecture.Modules.ZController controller, IBusiness newEntity)
		{
			var newCommunication = newEntity as OrgSalesCall;
			if (newCommunication != null)
			{
				var campaignItemAsMaster = Model.Master as IGlbCompanyCampaignItem;
				SalesEnquiry inquiry = campaignItemAsMaster != null ? ((BusinessObject)CurrentDataItem).Factory.Load<SalesEnquiry>(campaignItemAsMaster.G8_RecipientID) : null;
				if (inquiry != null && inquiry.Header == null)
				{
					newCommunication.LinkedInquiry = inquiry;
					newCommunication.RelatedParentActivityPivotCollection.DeleteAll();
					newCommunication.RelatedParentActivityPivotCollection.AddNewPivot(Model.Master);
				}
			}

			base.CreateRelationshipAndShowFormForNewEntity(controller, newEntity);
		}

		#region RelatableTypeButtonCaptionPairs

		protected override IEnumerable<KeyValuePair<string, ResourceStringData>> RelatableTypeButtonCaptionPairs
		{
			get
			{
				foreach (var pair in additionalRelatableTypeButtonCaptionPairs)
				{
					yield return pair;
				}

				foreach (var pair in base.RelatableTypeButtonCaptionPairs)
				{
					yield return pair;
				}
			}
		}

		readonly KeyValuePair<string, ResourceStringData>[] additionalRelatableTypeButtonCaptionPairs = new KeyValuePair<string, ResourceStringData>[]
		{
			new KeyValuePair<string, ResourceStringData>(RelatableActivityTypeList.Codes.Communication, Res.GetData("34de09ac-dc8b-4d83-97da-c5d5f1385460", "Communication"))
		};

		#endregion
	}
}
