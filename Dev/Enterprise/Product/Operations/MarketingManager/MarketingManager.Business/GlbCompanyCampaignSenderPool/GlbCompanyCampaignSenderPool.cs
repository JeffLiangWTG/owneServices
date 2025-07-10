using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSenderPool : ActiveBusinessObjectCollection<GlbCompanyCampaignSenderPoolItem>
	{
		public GlbCompanyCampaignSenderPool(GlbCompanyCampaign parentCampaign)
			: base(parentCampaign.Factory, parentCampaign, new ZQuery(), GlbCompanyCampaignSenderPoolItemSchema.GCP_G0_Campaign)
		{
			if (parentCampaign == null)
			{
				throw new ArgumentNullException(nameof(parentCampaign));
			}

			SetNewTotalAmountForAllChildren(this, EventArgs.Empty);
			CountChanged += SetNewTotalAmountForAllChildren;
		}

		static MultilingualString ErrorMessageForUniqueSenders { get; } = ResString.GetMultilingualString("bdd2d172-787e-4c07-bfe4-62e8bad6677c", "Each staff member should be unique in Sender Pool.");

		protected override void SetHasChanges(bool hasChanges)
		{
			// Don't call base, we don't want the exception thrown in the Basher test
		}

		protected override void SetDefaultsForNewElementCore(GlbCompanyCampaignSenderPoolItem newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			SetNewTotalAmountForAllChildren(this, EventArgs.Empty);
			newElement.GCP_SendRatioInfo.ValueChanged += SetNewTotalAmountForAllChildren;
		}

		void SetNewTotalAmountForAllChildren(object sender, EventArgs eventArgs)
		{
			var totalAmount = this.Sum(item => item.GCP_SendRatio);
			this.ForEach(item => item.SetTotalAmount(totalAmount));
		}

		protected override void OnLoadedIntoCollectionCore(GlbCompanyCampaignSenderPoolItem loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			loadedObject.GCP_SendRatioInfo.ValueChanged -= SetNewTotalAmountForAllChildren;
			loadedObject.GCP_SendRatioInfo.ValueChanged += SetNewTotalAmountForAllChildren;
		}

		public void ValidateAll()
		{
			this.ForEach(senderPoolItem =>
			{
				senderPoolItem.RemoveRowError(ErrorMessageForUniqueSenders);
				senderPoolItem.Validation.ValidateAll();
			});

			var itemsWithTheSameSenderStaff = this
				.GroupBy(senderPoolItem => senderPoolItem.GCP_GS_NKSender)
				.Select(grouping => new { Count = grouping.Count(), grouping })
				.Where(arg => arg.Count > 1)
				.SelectMany(arg => arg.grouping);
			itemsWithTheSameSenderStaff.ForEach(senderPoolItem => senderPoolItem.AddRowError(ErrorMessageForUniqueSenders));
		}
	}
}
