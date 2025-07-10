using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignItemLookups : AutoGlbCompanyCampaignItemLookups
	{
		public GlbCompanyCampaignItemLookups(AutoGlbCompanyCampaignItem parent) : base(parent)
		{
		}

		public new GlbCompanyCampaignItem Parent
		{
			get { return (GlbCompanyCampaignItem)base.Parent; }
		}

		#region Campaigns

		public GlbCompanyCampaignCollection Campaigns => Parent.RecipientType == CampaignContactTypeCodeList.Descriptions.GlbStaff.ToString() ||
			Parent.RecipientType == CampaignContactTypeCodeList.Descriptions.HRJobApplicant.ToString() ?
			new HRGlbCompanyCampaignCollection(Factory) :
			new GlbCompanyCampaignCollection(Factory);

		#endregion

		#region Organisations

		public OrganisationsFindBoxCollection ClientOrganisations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#endregion

		#region Delivery Methods

		public CodeDescriptionPairList DeliveryMethods
		{
			get
			{
				var deliveryMethods = new CodeDescriptionPairList();
				deliveryMethods.AddPair(DeliveryMethodsConstants.PrintCode, DeliveryMethodsConstants.PrintDescription);
				deliveryMethods.AddPair(DeliveryMethodsConstants.EmailCode, DeliveryMethodsConstants.EmailDescription);
				return deliveryMethods;
			}
		}

		public static class DeliveryMethodsConstants
		{
			public static MultilingualString PrintDescription => ResString.GetMultilingualString("3ee38a5d-32db-4388-aa45-83bb5f5c2006", "Print");
			public static MultilingualString TargetListDescription => ResString.GetMultilingualString("ce7697f7-8cce-4a06-92ca-96f3ec2a338b", "Target List");
			public static MultilingualString EmailDescription => ResString.GetMultilingualString("193d4ce2-4985-4cf2-bb4b-461c05d08c6f", "Email");

			public static ZString PrintCode { get; } = "PRN";
			public static ZString TargetListCode { get; } = "TGL";
			public static ZString EmailCode { get; } = "EML";
		}

		#endregion

		public static class StagesConstants
		{
			public const string Reset = "RST";
			public const string Taken = "TKN";
			public const string Submitted = "SBT";
		}
	}
}
