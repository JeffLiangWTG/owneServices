using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public abstract class RatingController<TRatingHeader> : ZController
		where TRatingHeader : RatingHeader
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(TRatingHeader);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var entry = sourceEntity as RateEntry;

			if (entry != null)
			{
				SetInitialTabPageNameToSelectWhenAFormIsShown(GetTabPageName(entry.TI_RateCategory));
				return base.GetLoadedBusinessEntityInLocalFactory(entry.Parent) as RatingHeader;
			}

			return base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var ratingHeader = base.GetNewBusinessEntityInLocalFactory();
			if (isGlobalRate)
			{
				((RatingHeader)ratingHeader).TH_GC = ZGuid.Empty;
			}

			return ratingHeader;
		}

		public void SetNewRatingHeaderAsGlobal()
		{
			isGlobalRate = true;
		}

		protected bool isGlobalRate;

		string GetTabPageName(string category)
		{
			var innerTab = BaseTabControl.GetTabPageName(category);
			string groupName = Groups.GetGroupNameByCategory(category);

			return string.IsNullOrEmpty(groupName)
						? innerTab
						: groupName + "+" + innerTab;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			var form = base.ShowEditForm(sourceEntity) as RatingForm;
			var entry = sourceEntity as RateEntry;

			if (form != null && entry != null && entry.Parent != null)
			{
				form.SelectSingleEntry(entry);
			}

			return form;
		}

		#region Security Override

		protected abstract CRMSecurityProvider<TRatingHeader> SecurityProvider { get; }

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject) =>
			GetSecurityCheckpoint(bizObject, FormAction.View, base.GetCheckPointForView);

		public override SecurityCheckpoint GetCheckPointForNew(BusinessObject bizObject) =>
			GetSecurityCheckpoint(bizObject, base.GetCheckPointForNew);

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject) =>
			GetSecurityCheckpoint(bizObject, FormAction.Delete, base.GetCheckPointForDelete);

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject) =>
			GetSecurityCheckpoint(bizObject, FormAction.Edit, base.GetCheckPointForEdit);

		protected override SecurityCheckpoint GetCheckPointForCopy(BusinessObject inMemorySourceEntity) =>
			GetSecurityCheckpoint(inMemorySourceEntity, base.GetCheckPointForCopy);

		SecurityCheckpoint GetSecurityCheckpoint(BusinessObject bizObject, Func<BusinessObject, SecurityCheckpoint> getDefaultCheckpoint) =>
			GetRateSecurityCheckpoint(bizObject) ?? getDefaultCheckpoint(bizObject);

		SecurityCheckpoint GetSecurityCheckpoint(BusinessObject bizObject, FormAction formAction, Func<BusinessObject, SecurityCheckpoint> getDefaultCheckpoint)
		{
			var rateSecurityCheckpoint = GetRateSecurityCheckpoint(bizObject);
			if (rateSecurityCheckpoint != null)
			{
				return rateSecurityCheckpoint;
			}

			var defaultCheckpoint = getDefaultCheckpoint(bizObject);

			return
				SecurityProvider?.GetSecurityCheckpoint(bizObject as TRatingHeader, formAction, defaultCheckpoint)
				??
				defaultCheckpoint;
		}

		SecurityCheckpoint GetRateSecurityCheckpoint(BusinessObject bizObject)
		{
			var ratingHeader = bizObject as RatingHeader ?? (bizObject as RateEntry)?.Parent;
			if (ratingHeader == null)
			{
				return null;
			}

			var deniedSecurityCheck = RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(
				rateEntryPKs: Array.Empty<ZGuid>(),
				ratingHeaderPKs: new[] { ratingHeader.PK },
				factory: ratingHeader.Factory);

			return deniedSecurityCheck?.SecurityCheckPoint;
		}

		protected bool IsGlobal(BusinessObject bizObject)
		{
			var ratingHeader = bizObject as RatingHeader ?? (bizObject as RateEntry)?.Parent;

			return ratingHeader.IsGlobal();
		}

		#endregion
	}
}

