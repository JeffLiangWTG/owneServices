using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class DocketIEnumerableExtensions
	{
		#region Find Docket

		public static T FindDocket<T>(this IEnumerable<T> elements, WhsWarehouse whs, OrgHeader org)
			where T : WhsDocket
		{
			return FindDocket(elements, whs.PK, org.PK, ZString.Empty);
		}

		public static T FindDocket<T>(this IEnumerable<T> elements, ZGuid whsPK, ZGuid orgPK)
			where T : WhsDocket
		{
			return FindDocket(elements, whsPK, orgPK, "");
		}

		public static T FindDocket<T>(this IEnumerable<T> elements, ZGuid whsPK, ZGuid orgPK, ZString reference)
			where T : WhsDocket
		{
			return FindDocket(elements, whsPK, orgPK, reference, null);
		}

		public static T FindDocket<T>(this IEnumerable<T> elements, ZGuid whsPK, ZGuid orgPK, ZString reference, Func<T, bool> customPredicate)
			where T : WhsDocket
		{
			return FindDocket(elements, whsPK, orgPK, reference, customPredicate, ZDateTimeOffset.Empty);
		}

		public static T FindDocket<T>(this IEnumerable<T> elements, ZGuid whsPK, ZGuid orgPK, ZString reference, Func<T, bool> customPredicate, ZDateTimeOffset requiredDate)
			where T : WhsDocket
		{
			return FindDocket(elements, whsPK, orgPK, reference, customPredicate, requiredDate, false);
		}

		public static T FindDocket<T>(this IEnumerable<T> elements, ZGuid whsPK, ZGuid orgPK, ZString reference, Func<T, bool> customPredicate, ZDateTimeOffset requiredDate, bool useStrictComparisonRules)
			where T : WhsDocket
		{
			foreach (var docket in elements)
			{
				if (docket.WD_WW_Whs == whsPK && docket.WD_OH_Client == orgPK)
				{
					if (IsTheSameIZTypeValue(reference, docket.WD_ExternalReference, useStrictComparisonRules) &&
						IsTheSameIZTypeValue(requiredDate.Date, docket.WD_RequiredDate.Date, useStrictComparisonRules) &&
						(customPredicate == null || customPredicate(docket)))
					{
						return docket;
					}
				}
			}
			return null;
		}

		static bool IsTheSameIZTypeValue(IZType nonStrictValue, IZType strictValue, bool useStrictComparisonRules)
		{
			return (!useStrictComparisonRules && nonStrictValue.IsEmpty) || nonStrictValue.Equals(strictValue);
		}

		#endregion
	}
}
