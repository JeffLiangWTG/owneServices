using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class CommissionAgreementItemConflict : ICommissionAgreementItemConflict
	{
		public CommissionAgreementItemConflict(OrgCommissionAgreementItem winnerAgreementItem, OrgCommissionAgreementItem loserAgreementItem, bool displayLoserAgreementItem = false)
		{
			Argument.NotNull(winnerAgreementItem, "winnerAgreementItem");
			Argument.NotNull(loserAgreementItem, "loserAgreementItem");

			this.winnerAgreementItemWrapper = new CommissionAgreementRelatedLastestVersionWrapper<OrgCommissionAgreementItem>(winnerAgreementItem);
			this.loserAgreementItemWrapper = new CommissionAgreementRelatedLastestVersionWrapper<OrgCommissionAgreementItem>(loserAgreementItem);
			this.displayLoserAgreementItem = displayLoserAgreementItem;
		}

		readonly bool displayLoserAgreementItem;

		public OrgCommissionAgreementItem AgreementItem
		{
			get { return displayLoserAgreementItem ? LoserAgreementItem : WinnerAgreementItem; }
		}

		#region WinnerAgreementItem

		readonly CommissionAgreementRelatedLastestVersionWrapper<OrgCommissionAgreementItem> winnerAgreementItemWrapper;

		public OrgCommissionAgreementItem WinnerAgreementItem
		{
			get { return winnerAgreementItemWrapper.LastestVersion; }
		}

		public OrgCommissionAgreement WinnerCommissionAgreement
		{
			get { return WinnerAgreementItem.CommissionAgreement; }
		}

		#endregion

		#region LoserAgreementItem

		readonly CommissionAgreementRelatedLastestVersionWrapper<OrgCommissionAgreementItem> loserAgreementItemWrapper;

		public OrgCommissionAgreementItem LoserAgreementItem
		{
			get { return loserAgreementItemWrapper.LastestVersion; }
		}

		public OrgCommissionAgreement LoserCommissionAgreement
		{
			get { return LoserAgreementItem.CommissionAgreement; }
		}

		#endregion

		public ZBool IsDeleted
		{
			get
			{
				return
					WinnerAgreementItem.IsDeleted ||
					WinnerCommissionAgreement.IsDeleted ||
					LoserAgreementItem.IsDeleted ||
					LoserCommissionAgreement.IsDeleted;
			}
		}

		#region Equals

		public override bool Equals(object obj)
		{
			var otherConflict = obj as CommissionAgreementItemConflict;
			if (otherConflict == null)
			{
				return false;
			}

			var loserCommissionAgreementPk = GetMainVersionPk(LoserCommissionAgreement);
			var otherLoserCommissionAgreementPk = GetMainVersionPk(otherConflict.LoserCommissionAgreement);
			var winnerCommissionAgreementPk = GetMainVersionPk(WinnerCommissionAgreement);
			var otherWinnerCommissionAgreementPk = GetMainVersionPk(otherConflict.WinnerCommissionAgreement);

			return
				loserCommissionAgreementPk == otherLoserCommissionAgreementPk &&
				winnerCommissionAgreementPk == otherWinnerCommissionAgreementPk &&
				WinnerAgreementItem.GetItemPath().SequenceEqual(otherConflict.WinnerAgreementItem.GetItemPath());
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 13;

				hash = (hash * 7) + GetMainVersionPk(LoserCommissionAgreement).GetHashCode();
				hash = (hash * 7) + GetMainVersionPk(WinnerCommissionAgreement).GetHashCode();

				foreach (var item in WinnerAgreementItem.GetItemPath())
				{
					hash = (hash * 7) + item.GetHashCode();
				}

				return hash;
			}
		}

		ZGuid GetMainVersionPk(OrgCommissionAgreement commissionAgreement)
		{
			return commissionAgreement != null ? commissionAgreement.MainVersion.PK : ZGuid.Empty;
		}

		#endregion
	}

	public class CommissionAgreementRelatedLastestVersionWrapper<T>
		where T : BusinessObject, ICommissionAgreementRelated<T>
	{
		public CommissionAgreementRelatedLastestVersionWrapper(T initialVersion)
		{
			this.initialVersion = initialVersion;
			this.initialAgreement = initialVersion.CommissionAgreement;
			this.mainVersion = initialVersion.GetMainVersion();
		}

		public T LastestVersion
		{
			get { return initialAgreement == null || !initialAgreement.IsDeleted ? initialVersion : mainVersion; }
		}

		readonly T initialVersion;
		readonly OrgCommissionAgreement initialAgreement;
		readonly T mainVersion;
	}
}
