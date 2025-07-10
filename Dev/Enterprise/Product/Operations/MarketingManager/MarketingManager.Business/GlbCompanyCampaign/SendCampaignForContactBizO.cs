using System.Linq;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class SendCampaignForContactBizO : NonPersistentBusinessObject
	{
		public SendCampaignForContactBizO(OrgContact contact)
			: base(contact.Factory)
		{
			this.contact = contact;
		}

		#region Property

		public OrgContact Contact
		{
			get { return contact; }
		}

		public CampaignContact CampaignContact
		{
			get { return Factory.Load<CampaignContact>(contact.PK); }
		}

		[List("CampaignList")]
		public ZGuid CampaignPK
		{
			get { return campaignPK; }
			set { SetNonPersistentPropertyValue(CampaignPKInfo, ref campaignPK, value); }
		}

		public ZPropertyInfo CampaignPKInfo
		{
			get { return GetZPropertyInfo(nameof(CampaignPK)); }
		}

		public GlbCompanyCampaign Campaign
		{
			get { return Factory.Load<GlbCompanyCampaign>(CampaignPK); }
		}

		readonly OrgContact contact;
		ZGuid campaignPK;

		#endregion

		#region Campaign List
#if DEBUG
		public
#endif
		class SendCampaignForContactBizOCampaignCollection : GlbCompanyCampaignCollection
		{
			public SendCampaignForContactBizOCampaignCollection(BusinessObjectFactory factory, ZQuery query)
				: base(factory, query)
			{
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
			protected override ZQuery CreateRelationshipFilter()
			{
				ZQuery query = new ZQuery();
				foreach (CodeDescriptionPair codePair in new GlbCompanyCampaignLookups(Factory).CampaignTypeList)
				{
					query.AddToFilter(JoinCondition.Or, GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam, codePair.Code);
				}
				return query;
			}
		}

		public GlbCompanyCampaignCollection CampaignList
		{
			get
			{
				if (campaignList == null)
				{
					ZGuid[] campaignPKs = Contact.Campaigns.Cast<GlbCompanyCampaignItem>().Select(c => c.G8_G0).ToArray();
					ZQuery query = new ZQuery(GlbCompanyCampaignSchema.PK, SQLComparisonOperator.NotEqual, campaignPKs);
					campaignList = new SendCampaignForContactBizOCampaignCollection(Factory, query);
				}
				return campaignList;
			}
		}
		GlbCompanyCampaignCollection campaignList;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public SendCampaignForContactBizOValidation Validation
		{
			get { return new SendCampaignForContactBizOValidation(this); }
		}

		#endregion
	}
}
