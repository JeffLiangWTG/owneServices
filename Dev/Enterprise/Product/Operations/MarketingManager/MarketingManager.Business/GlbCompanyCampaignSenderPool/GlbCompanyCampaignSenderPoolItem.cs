using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	[DependentBusinessObject(typeof(GlbCompanyCampaign), "SenderPool")]
	public class GlbCompanyCampaignSenderPoolItem : AutoGlbCompanyCampaignSenderPoolItem
	{
		public GlbCompanyCampaignSenderPoolItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected bool GCP_G0_Campaign_ReadOnly => true;

		public ZString SendRatioPercentage
		{
			get { return sendRatioPercentage; }
			private set { SetNonPersistentPropertyValue(SendRatioPercentageInfo, ref sendRatioPercentage, value); }
		}

		public ZPropertyInfo SendRatioPercentageInfo => GetZPropertyInfo(nameof(SendRatioPercentage));

		public void SetTotalAmount(ZInt totalAmount)
		{
			using (SuspendSettingHasChanges())
			{
				if (totalAmount > 0)
				{
					var value = 100 * GCP_SendRatio / totalAmount;
					SendRatioPercentage = value > 0 ? value.ToString(CultureInfo.InvariantCulture) : "< 1";
				}
				else
				{
					SendRatioPercentage = "0";
				}
			}
		}

		ZString sendRatioPercentage;
	}
}
