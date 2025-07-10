using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.GUI
{
	public class EstimateValueExpiryAction : NonPersistentBusinessObject, IObsoleteValidation
	{
		public EstimateValueExpiryAction(IEnumerable<OrgTradeDetail> tradeDetails, ActionType actionType)
			: base()
		{
			this.tradeDetails = tradeDetails;
			this.actionType = actionType;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.expiryDate = ZDate.Today;
		}

		readonly IEnumerable<OrgTradeDetail> tradeDetails;
		readonly ActionType actionType;

		public enum ActionType
		{
			SetExpiry,
			UndoExpiry
		}

		static class Schema
		{
			public const string ExpiryDate = "ExpiryDate";
			public const string ExpiryReason = "ExpiryReason";
		}

		#region Properties

		#region Expiry Date

		public ZDate ExpiryDate
		{
			get => expiryDate;
			set
			{
				SetNonPersistentPropertyValue(ExpiryDateInfo, ref expiryDate, value);
				ValidateExpiryDate();
			}
		}
		ZDate expiryDate;
		public ZPropertyInfo ExpiryDateInfo => GetZPropertyInfo(Schema.ExpiryDate);

		#endregion

		#region Expiry Reason

		[List("ExpiryReasonList")]
		public ZString ExpiryReason
		{
			get => expiryReason;
			set
			{
				SetNonPersistentPropertyValue(ExpiryReasonInfo, ref expiryReason, value);
				ValidateExpiryReason();
			}
		}
		ZString expiryReason;
		public ZPropertyInfo ExpiryReasonInfo => GetZPropertyInfo(Schema.ExpiryReason);

		public CodeDescriptionPairList ExpiryReasonList => OrganisationsDataRegistry.Instance.EstimateExpiryReasons.Value.GetActiveCodeDescriptionPairList();

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateExpiryDate();
			ValidateExpiryReason();
		}

		void ValidateExpiryDate()
		{
			ExpiryDateInfo.ClearAllNotifications();
			if (actionType == ActionType.SetExpiry)
			{
				MandatoryValidation.CheckEntered(ExpiryDateInfo);
			}
		}

		void ValidateExpiryReason()
		{
			ExpiryReasonInfo.ClearAllNotifications();
			if (actionType == ActionType.SetExpiry)
			{
				MandatoryValidation.CheckEntered(ExpiryReasonInfo);
				ListValidation.ErrorIfInvalidCode(ExpiryReasonInfo);
			}
		}

		#endregion

		#region Apply Action

		public void Apply()
		{
			bool hasChanges = false;

			if (actionType == ActionType.SetExpiry)
			{
				foreach (var tradeDetail in tradeDetails.Where(x => x.IsCommitted))
				{
					tradeDetail.SetProspectExpiry(ExpiryDate, ExpiryReason);
					hasChanges = true;
				}
			}
			else if (actionType == ActionType.UndoExpiry)
			{
				foreach (var tradeDetail in tradeDetails.Where(x => x.IsExpired))
				{
					tradeDetail.UndoProspectExpiry();
					hasChanges = true;
				}
			}

			if (hasChanges)
			{
				var parentOrg = tradeDetails.FirstOrDefault()?.Sales?.Primary;
				if (parentOrg != null)
				{
					parentOrg.HasChanges = true;
				}
			}
		}

		#endregion
	}
}
