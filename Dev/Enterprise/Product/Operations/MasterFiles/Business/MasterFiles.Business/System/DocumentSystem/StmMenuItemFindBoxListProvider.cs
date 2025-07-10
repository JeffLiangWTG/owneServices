using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class StmMenuItemFindBoxListProvider : FindBoxListProvider
	{
		public StmMenuItemFindBoxListProvider(IBusinessObjectCollection collection)
			: base(collection)
		{
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			var result = Enumerable.Empty<BusinessObject>();

			if (!string.IsNullOrEmpty(code))
			{
				result = List.Factory.Load(GetTypeOfElements(code), GetCompleteFilterForCode(code));
				if (!result.Any())
				{
					result = List.Factory.Load(GetTypeOfElements(code), GetCompleteFilterForCode(code, true));
				}
			}

			return result;
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
		{
			var result = Enumerable.Empty<BusinessObject>();

			if (!string.IsNullOrEmpty(code))
			{
				result = List.Factory.Load(GetTypeOfElements(code), GetFilterForCode(code));
				if (!result.Any())
				{
					result = List.Factory.Load(GetTypeOfElements(code), GetFilterForCode(code, true));
				}
			}

			return result;
		}

		public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
		{
			var result = code;
			var success = false;
			var query = GetFilterForCode(code, SQLComparisonOperator.StartsWith);
			query.OrderBy = StmMenuItem.Schema.SU_MenuName;

			var completeFilter = CompleteFilter;
			if (completeFilter != null)
			{
				query.AddToFilter(completeFilter);
			}

			var menuItem = List.Factory.LoadTop1<StmMenuItem>(query);
			if (menuItem != null)
			{
				result = menuItem.DocumentId;
				success = true;
			}

			return (result, success);
		}

		ZQuery GetCompleteFilterForCode(string code, bool useLegacyPath = false)
		{
			var query = GetFilterForCode(code, useLegacyPath);
			var completeFilter = CompleteFilter;

			if (completeFilter != null)
			{
				query.AddToFilter(completeFilter);
			}

			return query;
		}

		ZQuery GetFilterForCode(string code, bool useLegacyPath = false)
		{
			return GetFilterForCode(code, SQLComparisonOperator.Equal, useLegacyPath);
		}

		ZQuery GetFilterForCode(string code, SQLComparisonOperator comparisonOperator, bool useLegacyPath = false)
		{
			var result = new DocumentZQuery();

			var codes = code.Split(new char[] { ':' }, 4);

			if (codes.Length > 0)
			{
				var businessContext = codes[0].Trim();
				result.AddToFilter(StmMenuItemSchema.SU_BusinessContext, comparisonOperator, businessContext);

				if (codes.Length > 1)
				{
					var menuName = codes[1].Trim();
					result.AddToFilter(StmMenuItemSchema.SU_MenuName, comparisonOperator, menuName);

					if (codes.Length > 2)
					{
						var menuPath = codes[2].Trim();
						var menuPathQuery = new ZQuery(StmMenuItemSchema.SU_MenuPath, comparisonOperator, (menuPath == ".") ? string.Empty : (useLegacyPath ? LegacyPath : string.Empty) + menuPath);

						result.AddToFilter(menuPathQuery);

						if (codes.Length > 3)
						{
							var contactType = codes[3].Trim();
							if (!string.IsNullOrEmpty(contactType))
							{
								result.AddToFilter(StmMenuItemSchema.SU_ContactType, comparisonOperator, contactType);
							}
						}
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant legacy document path")]
		const string LegacyPath = "Legacy Documents/";
	}
}
