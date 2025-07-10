using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CommissionAgreementConflictsFinder
	{
		#region New

		public static CommissionAgreementConflictsFinder New()
		{
			var type = TypeDecider.GetTypeForBinding(typeof(CommissionAgreementConflictsFinder));
			var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
			return (CommissionAgreementConflictsFinder)constructor.Invoke(null);
		}

		protected CommissionAgreementConflictsFinder()
		{
		}

		#endregion

		#region GetMoreGenericCommissionAgreementConflicts

		public ICollection<ICommissionAgreementConflict> GetMoreGenericCommissionAgreementConflicts(OrgOpportunity opportunity, bool onlyForModifiedAgreements)
		{
			Argument.NotNull(opportunity, "opportunity");

			var result = new List<ICommissionAgreementConflict>();
			foreach (var commissionAgreement in opportunity.CommissionAgreementsForEdit.Where(x => (!onlyForModifiedAgreements || x.HasChanges) && !x.IsReversed))
			{
				result.AddRange(GetMoreGenericCommissionAgreementConflicts(commissionAgreement));
			}

			return result;
		}

		protected virtual IEnumerable<ICommissionAgreementConflict> GetMoreGenericCommissionAgreementConflicts(OrgCommissionAgreement commissionAgreement)
		{
			foreach (var productItem in commissionAgreement.ProductItems)
			{
				foreach (var serviceItem in productItem.ChildServiceItems)
				{
					foreach (var subModuleItem in serviceItem.ChildSubModuleItems)
					{
						var itemArgs = GetItemPathCodesForConflictChecking(subModuleItem);
						if (itemArgs != null)
						{
							var moreGenericItem = FindMoreGenericItem(subModuleItem.CommissionAgreement, itemArgs);
							if (moreGenericItem != null)
							{
								yield return new CommissionAgreementItemConflict(subModuleItem, moreGenericItem);
							}
						}
					}
				}
			}
		}

		protected CommissionItemArgs GetItemPathCodesForConflictChecking(OrgCommissionAgreementItem item)
		{
			if (!item.CAI_IsInclude || item.CAI_Type != OrgCommissionAgreementItemTypes.Codes.SubModule)
			{
				return null;
			}

			var commissionAgreement = item.CommissionAgreement;
			if (commissionAgreement == null || commissionAgreement.CA0_OH_Customer.IsEmpty)
			{
				return null;
			}

			var itemPath = item.GetItemPath().ToArray();
			if (itemPath.Length == 3)
			{
				var product = itemPath[0].Item2;
				var service = itemPath[1].Item2;
				var subModule = itemPath[2].Item2;
				var mode = OrgCommissionAgreementItemLookups.AllModesCode;
				var origin = ZString.Empty;
				var destination = ZString.Empty;

				return new CommissionItemArgs(ZGuid.Empty, product, service, subModule, ZDate.Empty, mode, origin, destination);
			}
			else if (itemPath.Length == 4)
			{
				var product = itemPath[0].Item2;
				var service = OrgCommissionAgreementItemLookups.AllServicesCode;
				var subModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;
				var origin = itemPath[1].Item2;
				var destination = itemPath[2].Item2;
				var mode = itemPath[3].Item2;

				return new CommissionItemArgs(ZGuid.Empty, product, service, subModule, ZDate.Empty, mode, origin, destination);
			}

			return null;
		}

		OrgCommissionAgreementItem FindMoreGenericItem(OrgCommissionAgreement commissionAgreement, CommissionItemArgs itemArgs)
		{
			var coveringItems = new List<OrgCommissionAgreementItem>();
			foreach (var agreement in GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true))
			{
				coveringItems.AddRange(agreement.GetItemsThatCover(itemArgs));
			}

			if (coveringItems.Count == 0)
			{
				return null;
			}

			return GetMostSpecificCommissionAgreementItem(coveringItems);
		}

		#endregion

		#region GetNewCommissionAgreementConflicts

		public ICollection<ICommissionAgreementConflict> GetNewCommissionAgreementConflicts(OrgOpportunity opportunity, IEnumerable<ICommissionAgreementConflict> previousConflicts)
		{
			Argument.NotNull(opportunity, "opportunity");

			var result = new List<ICommissionAgreementConflict>();
			var nonDeletedPreviousConflicts = previousConflicts.Where(x => !x.IsDeleted);
			var previousConflictLookup = new HashSet<ICommissionAgreementConflict>(nonDeletedPreviousConflicts);

			var conflictsOfModifiedAgreements = GetMoreGenericCommissionAgreementConflicts(opportunity, true);
			foreach (var modifiedAgreementConflict in conflictsOfModifiedAgreements)
			{
				if (!previousConflictLookup.Contains(modifiedAgreementConflict))
				{
					result.Add(modifiedAgreementConflict);
				}
				else
				{
					var originalEffectiveDate = modifiedAgreementConflict.WinnerCommissionAgreement.GetEffectiveDateOriginalValue();
					if (modifiedAgreementConflict.WinnerCommissionAgreement.EffectiveDate < originalEffectiveDate)
					{
						result.Add(modifiedAgreementConflict);
					}
				}
			}

			return result;
		}

		#endregion

		#region FindDuplicates

		public virtual ICollection<ICommissionAgreementDuplication> FindDuplicates(OrgCommissionAgreementItem item)
		{
			var duplicateItems = new List<ICommissionAgreementDuplication>();
			var commissionAgreement = item.CommissionAgreement;
			foreach (var otherAgreement in GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, false))
			{
				var otherAgreementDuplicateItem = otherAgreement.FindDuplicate(item);
				if (otherAgreementDuplicateItem != null)
				{
					var product = GetProduct(item);
					var otherProduct = GetProduct(otherAgreementDuplicateItem);
					if (product.ConditionCollection.Count != otherProduct.ConditionCollection.Count)
					{
						continue;
					}

					bool isUnique = false;
					foreach (var condition in otherProduct.ConditionCollection)
					{
						if (!product.HasCondition(condition.CIC_Mode, condition.CIC_RL_NKOrigin, condition.CIC_RL_NKDestination))
						{
							isUnique = true;
							break;
						}
					}

					if (!isUnique)
					{
						duplicateItems.Add(new CommissionAgreementItemDuplication(otherAgreementDuplicateItem));
					}
				}
			}

			return duplicateItems;
		}

		OrgCommissionAgreementItem GetProduct(OrgCommissionAgreementItem item)
		{
			OrgCommissionAgreementItem result = item;
			while (result.ParentItem != null)
			{
				result = result.ParentItem;
			}

			return result;
		}

		#endregion

		#region Helper Methods

		protected OrgCommissionAgreementItem GetMostSpecificCommissionAgreementItem(IEnumerable<OrgCommissionAgreementItem> items)
		{
			return items.MinBy(x =>
			{
				var typeRank =
					(x.CAI_Type == OrgCommissionAgreementItemTypes.Codes.Product) ? 2 :
					(x.CAI_Type == OrgCommissionAgreementItemTypes.Codes.Service) ? 1 :
					(x.CAI_Type == OrgCommissionAgreementItemTypes.Codes.SubModule) ? 0 :
					0;

				var codeRank = x.CAI_Code == OrgCommissionAgreementItem.AllItemCode ? 1 : 0;

				var tradeLaneRank = x.ConditionCollection.Count > 0
					? x.ConditionCollection.Sum(c => c.CIC_RL_NKOrigin.IsEmpty ? 2 : 0) + x.ConditionCollection.Sum(c => c.CIC_RL_NKDestination.IsEmpty ? 2 : 0)
					: 0;

				var modeRank = x.ConditionCollection.Count > 0
					? x.ConditionCollection.Sum(c => c.CIC_Mode == OrgCommissionAgreementItem.AllItemCode ? 1 : 0)
					: 0;

				var isDraftRank = x.IsDraft() ? 0 : 1;

				return ((typeRank + 1) * 100) + ((codeRank + 1) * 10) + ((tradeLaneRank + 1) * 10) + ((modeRank + 1) * 10) + isDraftRank;
			});
		}

		protected static IEnumerable<OrgCommissionAgreement> GetOtherAgreementsEffectiveAtSameTime(OrgCommissionAgreement commissionAgreement, bool onlyIncludeAgreementsInDatabase)
		{
			if (commissionAgreement == null || commissionAgreement.CA0_OH_Customer.IsEmpty)
			{
				return Enumerable.Empty<OrgCommissionAgreement>();
			}

			var fromDate = commissionAgreement.EffectiveDate;
			var toDate = commissionAgreement.CA0_ExpiredDate;

			var otherAgreementsForCustomerQuery = new ZQuery(OrgCommissionAgreementSchema.CA0_OH_Customer, commissionAgreement.CA0_OH_Customer);
			otherAgreementsForCustomerQuery.AddToFilter(OrgCommissionAgreementSchema.CA0_CommissionStream, commissionAgreement.CA0_CommissionStream);
			otherAgreementsForCustomerQuery.AddToFilter(OrgCommissionAgreementSchema.CA0_ReversedDateUtc, null);
			otherAgreementsForCustomerQuery.AddToFilter(OrgCommissionAgreementSchema.PK, SQLComparisonOperator.NotEqual, commissionAgreement.PK);
			var notDraftOfAgreementFilter = new ZQuery(OrgCommissionAgreementSchema.CA0_CA0_ParentVersion, SQLComparisonOperator.NotEqual, commissionAgreement.PK);
			notDraftOfAgreementFilter.AddToFilter(JoinCondition.Or, OrgCommissionAgreementSchema.CA0_CA0_ParentVersion, null);
			otherAgreementsForCustomerQuery.AddToFilter(notDraftOfAgreementFilter);
			if (commissionAgreement.ParentVersion != null)
			{
				otherAgreementsForCustomerQuery.AddToFilter(OrgCommissionAgreementSchema.PK, SQLComparisonOperator.NotEqual, commissionAgreement.ParentVersion.PK);
			}

			var factory = commissionAgreement.Factory;
			var otherAgreementsForCustomer = factory.Load<OrgCommissionAgreement>(otherAgreementsForCustomerQuery).Where(x => onlyIncludeAgreementsInDatabase ? x.IsInDatabase : !x.IsUncommittedDraft);

			return
				otherAgreementsForCustomer
					.Where(x => fromDate < x.CA0_ExpiredDate || fromDate.IsEmpty || x.CA0_ExpiredDate.IsEmpty)
					.Where(x => x.EffectiveDate < toDate || toDate.IsEmpty || x.EffectiveDate.IsEmpty)
					.Where(x => !(x.HasDraft && x.Draft.IsReversed));
		}

		#endregion

		#region GetMoreSpecificCommissionAgreementConflicts

		public IEnumerable<ICommissionAgreementConflict> GetMoreSpecificCommissionAgreementConflicts(OrgOpportunity opportunity, bool onlyForModifiedAgreements)
		{
			Argument.NotNull(opportunity, "opportunity");

			var result = new List<ICommissionAgreementConflict>();
			foreach (var commissionAgreement in opportunity.CommissionAgreementsForEdit.Where(x => (!onlyForModifiedAgreements || x.HasChanges) && !x.IsReversed))
			{
				result.AddRange(GetMoreSpecificCommissionAgreementConflicts(commissionAgreement));
			}

			return result;
		}

		protected virtual IEnumerable<ICommissionAgreementConflict> GetMoreSpecificCommissionAgreementConflicts(OrgCommissionAgreement commissionAgreement)
		{
			foreach (var productItem in commissionAgreement.ProductItems)
			{
				foreach (var serviceItem in productItem.ChildServiceItems)
				{
					foreach (var subModuleItem in serviceItem.ChildSubModuleItems)
					{
						foreach (var conflict in GetMoreSpecificCommissionAgreementConflicts(subModuleItem))
						{
							yield return conflict;
						}
					}
				}
			}
		}

		public virtual IEnumerable<ICommissionAgreementConflict> GetMoreSpecificCommissionAgreementConflicts(OrgCommissionAgreementItem commissionAgreementItem)
		{
			var itemArgs = GetItemPathCodesForConflictChecking(commissionAgreementItem);
			if (itemArgs != null)
			{
				var moreSpecificItems = FindMoreSpecificItemConflicts(commissionAgreementItem.CommissionAgreement, itemArgs);
				foreach (var item in moreSpecificItems)
				{
					yield return new CommissionAgreementItemConflict(item, commissionAgreementItem);
				}
			}
		}

		IEnumerable<OrgCommissionAgreementItem> FindMoreSpecificItemConflicts(OrgCommissionAgreement commissionAgreement, CommissionItemArgs itemArgs)
		{
			var coveredItems = new List<OrgCommissionAgreementItem>();
			foreach (var agreement in GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true))
			{
				coveredItems.AddRange(agreement.GetItemsThatAreCoveredBy(itemArgs));
			}

			return coveredItems;
		}

		#endregion
	}
}
