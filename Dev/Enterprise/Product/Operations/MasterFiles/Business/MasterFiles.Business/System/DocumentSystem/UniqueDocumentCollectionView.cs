using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.StmMenuItem)]
	public class UniqueDocumentCollectionView : ActiveBusinessObjectCollection<StmMenuItem>
	{
		public UniqueDocumentCollectionView(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public UniqueDocumentCollectionView(BusinessObjectFactory factory, List<ZGuid> applicableCommandPKs, string businessContext)
			: base(factory)
		{
			if (applicableCommandPKs != null)
			{
				this.applicableCommandPKs.AddRange(applicableCommandPKs);
			}
			this.businessContext = businessContext;
		}
		readonly List<ZGuid> applicableCommandPKs = new List<ZGuid>();
		readonly string businessContext;

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new StmMenuItemFindBoxListProvider(this); }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			result.AddToFilter(StmMenuItemSchema.SU_PreventAutoDelivery, "N");
			result.AddToFilter(StmMenuItemSchema.SU_ContactType, SQLComparisonOperator.NotEqual, ContactType.NoContactType.Code);
			result.AddToFilter(StmMenuItemSchema.SU_ContactType, SQLComparisonOperator.NotEqual, string.Empty);

			if (!string.IsNullOrEmpty(businessContext))
			{
				result.AddToFilter(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.Equal, businessContext);
			}

			if (applicableCommandPKs.Count > 0)
			{
				var applicableCommandPKsQuery = new ZQuery() { AllowTableValuedParameters = true };
				applicableCommandPKsQuery.AddToFilter(StmMenuItemSchema.PK, applicableCommandPKs);
				result.AddToFilter(applicableCommandPKsQuery);
			}

			return result;
		}

		protected override bool MatchesFilterCore(StmMenuItem element, bool fetchOnlyFromLocalCache)
		{
			var result = base.MatchesFilterCore(element, fetchOnlyFromLocalCache);

			if (result)
			{
				result = IsUniqueCode(element) && (applicableCommandPKs.Count == 0 || applicableCommandPKs.Contains(element.PK));
			}

			return result;
		}

		bool IsUniqueCode(StmMenuItem menuItem)
		{
			ZGuid uniqueValue;

			if (!UniqueCodes.TryGetValue(menuItem.MenuItemUniqueCode, out uniqueValue))
			{
				UniqueCodes[menuItem.MenuItemUniqueCode] = uniqueValue = menuItem.PK;
			}

			return uniqueValue == menuItem.PK;
		}

		readonly Dictionary<ZString, ZGuid> UniqueCodes = new Dictionary<ZString, ZGuid>();
	}
}
