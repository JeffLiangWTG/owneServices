using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class OrgSalesCallRelatedParentActivityPivotCollection : RelatedParentActivityPivotCollection
	{
		public OrgSalesCallRelatedParentActivityPivotCollection(OrgSalesCall master)
			: base(master)
		{
			var tablePrefixesThatShouldBeLoadedWithSpecificElementType = RelatedActivityLinkLookups.GetTablePrefixesThatShouldBeLoadedWithSpecificElementType(Factory);

			foreach (var pivot in this)
			{
				if (tablePrefixesThatShouldBeLoadedWithSpecificElementType.TryGetValue(pivot.RAP_ParentActivityTableCode, out var elementTypeForLoad))
				{
					AddFetchHintForParentActivity(elementTypeForLoad, pivot);
				}
				else
				{
					var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(pivot.RAP_ParentActivityTableCode);
					AddFetchHintForParentActivity(type, pivot);
				}
			}
		}

		void AddFetchHintForParentActivity(Type type, ViewRelatedActivityPivot pivot)
		{
			if (!typeof(NonPersistentBusinessObject).IsAssignableFrom(type))
			{
				Factory.AddFetchHint(type, pivot.RAP_ParentActivityID);
			}
		}

		protected new OrgSalesCall Master
		{
			get { return (OrgSalesCall)base.Master; }
		}

		protected override RelationValidationResult CheckIsValidParentCore(IRelatableActivity childActivityInLocalFactory, IRelatableActivity parentActivity, bool parentMustBeInDatabase)
		{
			var baseValidation = base.CheckIsValidParentCore(childActivityInLocalFactory, parentActivity, parentMustBeInDatabase);
			if (!baseValidation.IsValid)
			{
				return baseValidation;
			}

			var linkedInquiry = Master.LinkedInquiry;
			if (linkedInquiry != null && linkedInquiry.PK != parentActivity.PK)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("ab041951-2410-4fa2-b627-9036024cfdc9", "{0} cannot have any additional relationships until client intelligence is set on its related inquiry ({1}).", Master.HumanReadableName, linkedInquiry.HumanReadableName));
			}

			return new RelationValidationResult(true);
		}

		protected override RelationValidationResult CheckCanRemoveActivityCore(IRelatableActivity parentActivity)
		{
			var baseValidation = base.CheckCanRemoveActivityCore(parentActivity);
			if (!baseValidation.IsValid)
			{
				return baseValidation;
			}

			if (parentActivity.TablePrefix == CrmOpportunitySchema.Constants.Prefix)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("e6622387-454a-4fe2-b0f7-55732690b709", "Communications cannot be detached from GLOW opportunities"));
			}

			var linkedInquiry = Master.LinkedInquiry;
			if (linkedInquiry != null)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("d2f5f776-88c9-4634-93e4-41377f209150", "Cannot remove the relationship between {0} and {1} until the client intelligence is set on {1}.", Master.HumanReadableName, linkedInquiry.HumanReadableName));
			}

			return new RelationValidationResult(true);
		}
	}
}
