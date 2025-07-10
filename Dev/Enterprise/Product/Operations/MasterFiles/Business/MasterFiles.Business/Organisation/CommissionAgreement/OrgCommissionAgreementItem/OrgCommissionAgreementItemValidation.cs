//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCommissionAgreementItemValidation
//
//    This class should be used for overriding validation in AutoOrgCommissionAgreementItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementItemValidation : AutoOrgCommissionAgreementItemValidation
	{
		public OrgCommissionAgreementItemValidation(AutoOrgCommissionAgreementItem parent)
			: base(parent)
		{
		}

		new OrgCommissionAgreementItem Parent
		{
			get { return (OrgCommissionAgreementItem)base.Parent; }
		}

		#region Properties

		protected override void CheckCAI_Code()
		{
			base.CheckCAI_Code();

			MandatoryValidation.CheckEntered(Parent.CAI_CodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CAI_CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.CAI_CodeInfo, Parent.SiblingItems);

			var lowestLevelItemType = CommissionLookups.ShouldShowServicesAndSubModules ? OrgCommissionAgreementItemTypes.Codes.SubModule : OrgCommissionAgreementItemTypes.Codes.Product;
			var itemForConflicts = CommissionLookups.ShouldShowServicesAndSubModules ? Parent : Parent.ChildServiceItems.FirstOrDefault()?.ChildSubModuleItems.FirstOrDefault();
			var isCommissionAgreementReversed = (Parent.CommissionAgreement?.IsReversed).GetValueOrDefault(false);

			if (itemForConflicts != null && Parent.CAI_Type == lowestLevelItemType && !isCommissionAgreementReversed)
			{
				var hasDupicateError = false;
				if (Parent.CAI_IsInclude)
				{
					var duplicateItems = ConflictsFinder.FindDuplicates(itemForConflicts);
					if (duplicateItems.Count > 0)
					{
						var duplicateItemsErrorMessage = ConflictsTextProvider.ToErrorMessage(duplicateItems);
						Parent.CAI_CodeInfo.AddError(duplicateItemsErrorMessage);
						hasDupicateError = true;
					}
				}

				if (!hasDupicateError)
				{
					var newSpecificConflicts = GetMoreSpecificCommissionAgreementConflicts(itemForConflicts);
					if (!newSpecificConflicts.IsEmpty)
					{
						Parent.CAI_CodeInfo.AddWarning(newSpecificConflicts);
					}
				}
			}

			if (Parent.CAI_Type == OrgCommissionAgreementItemTypes.Codes.Product)
			{
				ValidateMultipleRates();
			}
		}

		protected override void CheckCAI_IsInclude()
		{
			base.CheckCAI_IsInclude();

			if (Parent.IsAllItem)
			{
				if (!Parent.CAI_IsInclude)
				{
					Parent.CAI_IsIncludeInfo.AddError(Res.GetString("08e982f5-1618-458e-8a42-aadf75cda3d6", "'{0}' can not be an exclusion.", OrgCommissionAgreementItem.AllItemCode));
				}
			}
			else
			{
				if (!Parent.CAI_IsInclude)
				{
					if (Parent.SiblingAllItem == null)
					{
						Parent.CAI_IsIncludeInfo.AddError(Res.GetString("dc490946-0a4d-4f0a-84f9-13f8fade329f", "Must include '{0}' before exclusions can be added.", OrgCommissionAgreementItem.AllItemCode));
					}
				}
			}
		}

		#endregion

		#region Row Notifications

		protected void CheckChildItems()
		{
			if (Parent.CAI_IsInclude)
			{
				CheckAtLeastOneChildItemIsIncluded();
			}
			else
			{
				CheckNoChildItems();
			}
		}

		void CheckAtLeastOneChildItemIsIncluded()
		{
			foreach (var childItemCollection in Parent.ChildItemCollections)
			{
				AddRowErrorIfNoItemIsIncluded(Parent, childItemCollection);
			}
		}

		void CheckNoChildItems()
		{
			foreach (var childItemCollection in Parent.ChildItemCollections)
			{
				AddRowErrorIfHasAnyItems(Parent, Parent.CAI_Type, childItemCollection);
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckChildItems();
			ValidateMultipleRates();
		}

		#endregion

		#region Messages

		static ZString GetAtLeastOneItemIsIncludedErrorMessage(ZString itemTypeDescription)
		{
			return Res.GetString("4391ff18-2d66-42d3-b772-d4a34cbaa425", "There must be at least one {0} item that is included.", itemTypeDescription);
		}

		static ZString GetCanNotHaveAnyItemsErrorMessage(ZString parentItemTypeDescription, ZString itemTypeDescription)
		{
			return Res.GetString("6f9b725c-a3cc-43a8-be89-0ebf07a627e8", "Excluded {0} items can not contain any child {1} items.", parentItemTypeDescription, itemTypeDescription);
		}

		#endregion

		#region Implementation

		internal static void AddRowErrorIfNoItemIsIncluded(BusinessObject bizObj, OrgCommissionAgreementItemCollection items)
		{
			var itemTypeDescription = new OrgCommissionAgreementItemTypes().GetDescriptionFromCode(items.Type);
			var errorMessage = GetAtLeastOneItemIsIncludedErrorMessage(itemTypeDescription);
			bizObj.RemoveRowError(errorMessage);
			if (!items.Any(x => x.CAI_IsInclude))
			{
				bizObj.AddRowError(errorMessage);
			}
		}

		internal static void AddRowErrorIfHasAnyItems(BusinessObject bizObj, ZString parentItemType, OrgCommissionAgreementItemCollection items)
		{
			var itemTypes = new OrgCommissionAgreementItemTypes();
			var parentItemTypeDescription = itemTypes.GetDescriptionFromCode(parentItemType);
			var itemTypeDescription = itemTypes.GetDescriptionFromCode(items.Type);
			var errorMessage = GetCanNotHaveAnyItemsErrorMessage(parentItemTypeDescription, itemTypeDescription);
			bizObj.RemoveRowError(errorMessage);
			if (items.Any())
			{
				bizObj.AddRowError(errorMessage);
			}
		}

		CommissionAgreementConflictsFinder ConflictsFinder
		{
			get { return conflictsFinder ?? (conflictsFinder = CommissionAgreementConflictsFinder.New()); }
		}
		CommissionAgreementConflictsFinder conflictsFinder;

		CommissionAgreementConflictsTextProvider ConflictsTextProvider
		{
			get { return conflictsTextProvider ?? (conflictsTextProvider = CommissionAgreementConflictsTextProvider.New()); }
		}
		CommissionAgreementConflictsTextProvider conflictsTextProvider;

		ZString GetMoreSpecificCommissionAgreementConflicts(OrgCommissionAgreementItem commissionAgreementItem)
		{
			var result = ZString.Empty;

			if (commissionAgreementItem != null)
			{
				var conflicts = ConflictsFinder.GetMoreSpecificCommissionAgreementConflicts(commissionAgreementItem).ToArray();
				if (conflicts.Length > 0)
				{
					result = CommissionAgreementConflictsTextProvider.New().ToConflictWarningMessage(conflicts);
				}
			}

			return result;
		}

		#endregion

		public void ValidateMultipleRates()
		{
			if (Parent == null || Parent.CommissionAgreement == null)
			{
				return;
			}

			foreach (var recipient in Parent.CommissionAgreement.Recipients)
			{
				foreach (var productItem in Parent.CommissionAgreement.ProductItems)
				{
					var product = productItem.CAI_Code;
					var service = CommissionRuleLookups.AnyServicesCode;
					var subModule = CommissionRuleLookups.AnySubModulesCode;
					var mode = OrgCommissionAgreementItemLookups.AllModesCode;
					var origin = "";
					var destination = "";

					if (productItem.ChildServiceItems.Count == 1)
					{
						var serviceItem = productItem.ChildServiceItems[0];
						service = serviceItem.CAI_Code;

						if (serviceItem.ChildSubModuleItems.Count == 0)
						{
							subModule = ZString.Empty;
						}
						else if (serviceItem.ChildSubModuleItems.Count == 1)
						{
							var subModuleItem = serviceItem.ChildSubModuleItems[0];
							subModule = subModuleItem.CAI_Code;
						}
					}

					foreach (var condition in productItem.ConditionCollection)
					{
						mode = condition.CIC_Mode;
						origin = condition.CIC_RL_NKOrigin;
						destination = condition.CIC_RL_NKDestination;

						var itemArgs = new CommissionItemArgs(ZGuid.Empty, product, service, subModule, ZDate.Empty, mode, origin, destination);

						string error = CheckRule(recipient, itemArgs);
						if (!string.IsNullOrEmpty(error))
						{
							condition.AddRowWarning(error);
							productItem.AddRowWarning(error);
						}
					}

					if (productItem.ConditionCollection.Count == 0)
					{
						var itemArgs = new CommissionItemArgs(ZGuid.Empty, product, service, subModule, ZDate.Empty, mode, origin, destination);

						string error = CheckRule(recipient, itemArgs);
						if (!string.IsNullOrEmpty(error))
						{
							productItem.AddRowWarning(error);
						}
					}
				}
			}
		}

		string CheckRule(OrgCommissionAgreementRecipient recipient, CommissionItemArgs itemArgs)
		{
			var rule = recipient.GetResponsibleStaffCommissionRule(itemArgs);
			if (rule == null)
			{
				return null;
			}

			foreach (var rate in rule.Rates)
			{
				bool found = false;
				foreach (var currentRate in recipient.Rates)
				{
					if ((rate.ACT_CommissionAmount != 0 && rate.ACT_CommissionAmount == currentRate.CAT_CommissionAmount)
						|| (rate.ACT_CommissionPercentage != 0 && rate.ACT_CommissionPercentage == currentRate.CAT_CommissionPercentage))
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					string tradeLane = string.Format(CultureInfo.InvariantCulture, "| {0} > {1} | {2}",
						string.IsNullOrEmpty(itemArgs.Origin) ? OrgCommissionAgreementItem.AllItemCode : (string)itemArgs.Origin,
						string.IsNullOrEmpty(itemArgs.Destination) ? OrgCommissionAgreementItem.AllItemCode : (string)itemArgs.Destination,
						itemArgs.Mode);

					string amount = rate.ACT_CommissionAmount != 0 ? rate.ACT_CommissionAmount.ToString() : rate.ACT_CommissionPercentage + "%";

					return ResString.GetMultilingualString("815C8818-47AA-4DEB-84B0-AD05DAF13D4F", "{0} {1} = Staff Entitlement has {2}. Create a new Opportunity and commission agreement or create a sub agreement to ensure correct entitlement rates are configured.", itemArgs.Product, tradeLane, amount);
				}
			}

			return null;
		}
	}
}
