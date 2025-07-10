using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using BusinessObject = CargoWise.EntityFramework.BusinessObject;
using RefCusTradeGroupCodes = Enterprise.Core.Constants.Customs.Universal.RefCusTradeGroup.Codes;

namespace Enterprise.Customs.Business.Extensions
{
	public static class BusinessObjectExtensions
	{
		public static void LoadChildrenForDeletionIncludingCusAddInfoAndCusCodeData<B>(this IEnumerable<B> bizObjs, IBusiness[] childList = null)
			where B : BusinessObject
		{
			if (bizObjs != null)
			{
				bizObjs.ForEach(bizObj => bizObj.LoadChildrenForDeletionIncludingCusAddInfoAndCusCodeData(childList));
			}
		}

		public static void LoadChildrenForDeletionIncludingCusAddInfoAndCusCodeData(this BusinessObject bizObj, IBusiness[] childList = null)
		{
			if (bizObj != null)
			{
				bizObj.LoadChildrenForDeletion(childList, getAdditionalChildren: AdditionalChildrenHelper.GetAdditionalChildrenIfSupported);
			}
		}

		internal static IDisposable EnableUnknown<T>(this BusinessObjectFactory factory)
			where T : BusinessObject, IOldParentIDProvider
		{
			IDisposable result = null;
			if (factory == null)
			{
				result = DisposableAction.NoAction;
			}
			else
			{
				result = new UnknownEnabledIndexEnabler(GetEnableUnknownInTypeDeciderDictionary(factory), typeof(T));
			}
			return result;
		}

		static Dictionary<Type, int> GetEnableUnknownInTypeDeciderDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
		}

		class UnknownEnabledIndexEnabler : IDisposable
		{
			public UnknownEnabledIndexEnabler(Dictionary<Type, int> dictionary, Type key)
			{
				this.dictionary = dictionary;
				this.key = key;
				int index;
				if (dictionary.TryGetValue(key, out index))
				{
					dictionary[key] = ++index;
				}
				else
				{
					index = 1;
					dictionary.Add(key, index);
				}
			}
			readonly Dictionary<Type, int> dictionary;
			readonly Type key;

			public void Dispose()
			{
				int index;
				if (dictionary.TryGetValue(key, out index))
				{
					dictionary[key] = --index;
				}
			}
		}

		internal static bool IsUnknownEnabled<T>(this BusinessObjectFactory factory)
			where T : BusinessObject, IOldParentIDProvider
		{
			int index;
			return factory != null && GetEnableUnknownInTypeDeciderDictionary(factory).TryGetValue(typeof(T), out index) && index > 0;
		}

		static Dictionary<Guid, int> GetOldParentIDsForDeletion<T>(BusinessObjectFactory factory)
			where T : BusinessObject, IOldParentIDProvider
		{
			Dictionary<Guid, int> result = null;
			var dictionary = factory.GetCachedValue("EnableOldParentIDsForDeletion", () => new Dictionary<Type, Dictionary<Guid, int>>());
			var key = typeof(T);
			if (!dictionary.TryGetValue(key, out result))
			{
				result = new Dictionary<Guid, int>();
				dictionary.Add(key, result);
			}
			return result;
		}

		internal static DisposableAction EnableOldParentID<T>(this T bizObj)
			where T : BusinessObject, IOldParentIDProvider
		{
			DisposableAction result = null;
			if (bizObj != null)
			{
				var oldParentIDs = GetOldParentIDsForDeletion<T>(bizObj.Factory);
				var pk = bizObj.PK.ToGuid();
				result = new DisposableAction(() =>
				{
					int index = 0;
					oldParentIDs.TryGetValue(pk, out index);
					oldParentIDs[pk] = ++index;
				}, () =>
				{
					int index = 0;
					if (oldParentIDs.TryGetValue(pk, out index))
					{
						oldParentIDs[pk] = --index;
					}
				});
			}
			return result;
		}

		internal static bool IsOldParentIDEnabled<T>(this BusinessObjectFactory factory, Guid pk)
			where T : BusinessObject, IOldParentIDProvider
		{
			var result = false;
			if (factory != null)
			{
				var oldParentIDs = GetOldParentIDsForDeletion<T>(factory);
				int index = 0;
				result = oldParentIDs != null && oldParentIDs.TryGetValue(pk, out index) && index > 0;
			}
			return result;
		}

		internal static ZGuid GetOldParentIDIfNeeded<T>(this BusinessObjectFactory factory, ZGuid parentID, Func<Guid> getPK)
			where T : BusinessObject, IOldParentIDProvider
		{
			if (factory != null && parentID.IsEmpty)
			{
				var pk = getPK();
				if (IsOldParentIDEnabled<T>(factory, pk))
				{
					var bizObjs = factory.GetBizOsForPK(pk).OfType<T>().ToArray();
					if (bizObjs.Length > 1)
					{
						ErrorReporter.ReportOnce("Multiple Business Object against one row", new ZStringBuilder(bizObjs.Select(x => x.GetType()).Distinct().Select(x => x.FullName)).ToStringWithNewLineBetweenAppends());
					}
					else if (bizObjs.Length == 1)
					{
						parentID = bizObjs[0].OldParentID;
					}
				}
			}
			return parentID;
		}

		public static CusAddInfo[] GetCusAddInfoChildren(this ICusAddInfoTypeSupporter supporter)
		{
			CusAddInfo[] result = null;
			var bizObj = supporter as BusinessObject;
			if (bizObj != null)
			{
				var query = new ZQuery(CusAddInfoSchema.B7_ParentID, bizObj.PK);
				query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, bizObj.TablePrefix);
				query.AddToFilter(CusAddInfoSchema.B7_Type, supporter.GetCusAddInfoTypes().Keys);
				query.FetchOnlyFromLocalCache = !bizObj.IsInDatabase;
				result = bizObj.Factory.Load<CusAddInfo>(query);
			}
			return result ?? Array.Empty<CusAddInfo>();
		}

		public static CusCodeData[] GetCusCodeDataChildren(this ICusCodeDataTypeSupporter supporter)
		{
			CusCodeData[] result = null;
			var bizObj = supporter as BusinessObject;
			if (bizObj != null)
			{
				var query = new ZQuery(CusCodeDataSchema.CY_ParentID, bizObj.PK);
				query.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, bizObj.TablePrefix);
				query.AddToFilter(CusCodeDataSchema.CY_Type, supporter.GetCusCodeDataTypes().Keys);
				query.FetchOnlyFromLocalCache = !bizObj.IsInDatabase;
				result = bizObj.Factory.Load<CusCodeData>(query);
			}
			return result ?? Array.Empty<CusCodeData>();
		}

		public static void DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported(this BusinessObject bizObj)
		{
			if (bizObj != null)
			{
				var factory = bizObj.Factory;
				using (factory.EnableUnknown<CusAddInfo>())
				using (factory.EnableUnknown<CusCodeData>())
				using (factory.EnableUnknown<CusSupportingInfo>())
				{
					var children = new List<BusinessObject>();
					children.AddRange(bizObj.GetAllCusAddInfoChildrenIfSupported());
					children.AddRange(bizObj.GetAllCusCodeDataChildrenIfSupported());
					children.AddRange(bizObj.GetAllCusSupportingInfoChildrenIfSupported());
					if (children.Count > 0)
					{
						foreach (var child in children)
						{
							child.FetchStrategy.FetchForDelete();
						}
						children.DeleteAll();
					}
				}
			}
		}

		static CusAddInfo[] GetAllCusAddInfoChildrenIfSupported(this BusinessObject bizObj)
		{
			CusAddInfo[] result = null;
			var supporter = bizObj as ICusAddInfoTypeSupporter;
			if (supporter != null)
			{
				var query = new ZQuery(CusAddInfoSchema.B7_ParentID, bizObj.PK);
				query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, bizObj.TablePrefix);
				query.FetchOnlyFromLocalCache = !bizObj.IsInDatabase;
				var factory = bizObj.Factory;
				result = factory.Load<CusAddInfo>(query);
				result.ForEach(x => x.FetchForLoadChildEditableObjectsIfNeeded());
			}
			return result ?? Array.Empty<CusAddInfo>();
		}

		static CusCodeData[] GetAllCusCodeDataChildrenIfSupported(this BusinessObject bizObj)
		{
			CusCodeData[] result = null;
			var supporter = bizObj as ICusCodeDataTypeSupporter;
			if (supporter != null)
			{
				var query = new ZQuery(CusCodeDataSchema.CY_ParentID, bizObj.PK);
				query.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, bizObj.TablePrefix);
				query.FetchOnlyFromLocalCache = !bizObj.IsInDatabase;
				var factory = bizObj.Factory;
				result = factory.Load<CusCodeData>(query);
				result.ForEach(x => x.FetchForLoadChildEditableObjectsIfNeeded());
			}
			return result ?? Array.Empty<CusCodeData>();
		}

		static CusSupportingInfo[] GetAllCusSupportingInfoChildrenIfSupported(this BusinessObject bizObj)
		{
			CusSupportingInfo[] result = null;
			var supporter = bizObj as ICusSupportingInfoTypeSupporter;
			if (supporter != null)
			{
				var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, bizObj.PK);
				query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, bizObj.TablePrefix);
				query.FetchOnlyFromLocalCache = !bizObj.IsInDatabase;
				var factory = bizObj.Factory;
				result = factory.Load<CusSupportingInfo>(query);
				result.ForEach(x => x.FetchForLoadChildEditableObjectsIfNeeded());
			}
			return result ?? Array.Empty<CusSupportingInfo>();
		}

		//
		// Summary:
		//     Loads the most recent log based on SL_EventTime for a given Event.  Cancelled or Estimated
		//     events are not retrieved by this method
		public static StmALog MostRecentLogByEventTimeExcludingEstimated(this Logs logs, Event @event)
		{
			StmALog result = null;
			if (logs != null && @event != null)
			{
				result = logs.MostRecentLogByEventTime(@event, new ZQuery(StmALogSchema.SL_IsEstimate, ZBool.False));
			}

			return result;
		}

		//
		// Summary:
		//     Loads the most recent log based on SL_EventTime for a given Event.  Cancelled or Estimated
		//     events are not retrieved by this method
		public static StmALog MostRecentLogByEventTimeExcludingEstimated(this Logs logs, Event @event, ZQuery extraQuery)
		{
			StmALog result = null;
			if (logs != null && @event != null)
			{
				var query = new ZQuery(StmALogSchema.SL_IsEstimate, ZBool.False);
				query.AddToFilter(extraQuery);
				result = logs.MostRecentLogByEventTime(@event, query);
			}

			return result;
		}

		//
		// Summary:
		//     Loads the most recent log based on SL_EventTime and SL_Reference for a given
		//     Event.  Cancelled or Estimated events are not retrieved by this method
		public static StmALog MostRecentLogByEventTimeExcludingEstimated(this Logs logs, Event @event, ZString reference)
		{
			StmALog result = null;
			if (logs != null && @event != null)
			{
				var query = new ZQuery(StmALogSchema.SL_Reference, reference);
				query.AddToFilter(StmALogSchema.SL_IsEstimate, ZBool.False);
				result = logs.MostRecentLogByEventTime(@event, query);
			}

			return result;
		}

		//
		// Summary:
		//     Loads the most recent log based on SL_EventTime and SL_Reference for a given
		//     Event.  Cancelled or Estimated events are not retrieved by this method
		public static StmALog MostRecentLogByEventTimeExcludingEstimated(this Logs logs, Event @event, ZString reference, ZQuery extraQuery)
		{
			StmALog result = null;
			if (logs != null && @event != null)
			{
				var query = new ZQuery(StmALogSchema.SL_Reference, reference);
				query.AddToFilter(StmALogSchema.SL_IsEstimate, ZBool.False);
				query.AddToFilter(extraQuery);
				result = logs.MostRecentLogByEventTime(@event, query);
			}

			return result;
		}

		public static bool IsCountryConsideredInEuForSafetyAndSecurity(this BusinessObjectFactory factory, string countryCode) => factory.GetEuropeanUnionAndSsCountries().Contains(countryCode);

		public static IEnumerable<string> GetEuropeanUnionAndSsCountries(this BusinessObjectFactory factory)
			=> factory.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().Concat(factory.SafetyAndSecurityButNotEUCountries()).ToArray();

		public static IEnumerable<string> SafetyAndSecurityButNotEUCountries(this BusinessObjectFactory factory)
		{
			var result = factory.LoadEunTradeGroupMembersButNotInEU(RefCusTradeGroupCodes.EUForSafetyAndSecurity);
			return result.Any() ? result : new List<string>
			{
				Core.Constants.CountryCodes.Switzerland
			};
		}

		public static bool IsMemberOfEU(this BusinessObjectFactory factory, string countryCode) => factory.GetEuropeanUnionForCustomsMembers().Contains(countryCode);

		public static bool IsMemberOfICS2(this BusinessObjectFactory factory, string countryCode) => factory.GetEuropeanUnionForCustomsMembers()
			.Concat(new[] { Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Norway })
			.Contains(countryCode);

		public static bool IsInEuropeanCustomsUnion(this BusinessObjectFactory factory, string countryCode) => factory.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().Contains(countryCode);

		public static bool IsCountryEuOrCtCountry(this BusinessObjectFactory factory, string countryCode) => factory.GetEuropeanUnionAndCtCountries().Contains(countryCode);

		public static IEnumerable<string> GetEuropeanUnionAndCtCountries(this BusinessObjectFactory factory)
			=> factory.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().Concat(factory.CommonTransitButNotEUCountries()).ToArray();

		public static IEnumerable<string> CommonTransitButNotEUCountries(this BusinessObjectFactory factory)
		{
			var result = factory.LoadEunTradeGroupMembersButNotInEU(RefCusTradeGroupCodes.EUCommonTransitProcedure);
			return result.Any() ? result : new List<string>
			{
				Core.Constants.CountryCodes.Switzerland
			};
		}

		static IEnumerable<string> LoadEunTradeGroupMembersButNotInEU(this BusinessObjectFactory factory, ZString tradeGroup) => factory.GetCachedValue("LoadEunTradeGroupMembersButNotInEU_" + tradeGroup, () =>
		{
			var subEUC = new ZDBOnlySubQuery(typeof(CusRefTradeGroupCountryView), RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode, true);
			subEUC.AddSubQuery(RefCusTradeGroupCountrySchema.ZZB_ZZA_TradeGroup, GetEUTradeGroupSubQuery(RefCusTradeGroupCodes.EuropeanUnionForCustoms), JoinCondition.And);

			var qry = new ZDBOnlyQuery(typeof(CusRefTradeGroupCountryView));
			qry.AddSubQuery(RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode, subEUC, JoinCondition.And);
			qry.AddSubQuery(RefCusTradeGroupCountrySchema.ZZB_ZZA_TradeGroup, GetEUTradeGroupSubQuery(tradeGroup), JoinCondition.And);

			var resList = new List<string>();
			foreach (var c in factory.Load<CusRefTradeGroupCountryView>(qry))
			{
				if (!resList.Contains(c.ZZB_RN_NKTradeGroupCountryCode))
				{
					resList.Add(c.ZZB_RN_NKTradeGroupCountryCode);
				}
			}
			return resList;
		});

		static ZDBOnlySubQuery GetEUTradeGroupSubQuery(ZString tradeGroup)
		{
			var result = new ZDBOnlySubQuery(typeof(CusRefTradeGroupView), RefCusTradeGroupSchema.PK);
			result.AddToFilter(RefCusTradeGroupSchema.ZZA_TradeGroup, tradeGroup);
			result.AddToFilter(RefCusTradeGroupSchema.ZZA_ZZZ_NKDataGrouping, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			result.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now);
			result.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now);
			return result;
		}

		// If you edit this list please also edit Enterprise\Product\Operations\Customs\EU\Core\Business\MasterFiles\OrgSupplierPart\EuOrgSupplierParts.cs
		// and EuOrgSupplierParts.cs and any others that may have been modified in the same changeset as changes to that first file. Ask DJC if unsure.
		public static IEnumerable<string> GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers(this BusinessObjectFactory factory)
			=> factory.GetCachedValue("EuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers", () =>
			{
				var countries = factory.GetEuropeanUnionForCustomsTradeGroup();

				if (countries.Count > 0)
				{
					var groupLoader = new CusRefTradeGroupView.Loader(factory);
					var customsUnionAdditionalMembersTradeGroup = groupLoader.Load(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusTradeGroupCodes.CustomsUnionAdditionalMembers, ZDateTime.Now);

					if (customsUnionAdditionalMembersTradeGroup != null)
					{
						foreach (var additionalCountry in customsUnionAdditionalMembersTradeGroup.GetApplicableTradeGroupCountries(ZDateTime.Now).Select(x => x.ZZB_RN_NKTradeGroupCountryCode.ToString()))
						{
							if (!countries.Contains(additionalCountry))
							{
								countries.Add(additionalCountry);
							}
						}
					}
				}
				else
				{
					countries = new List<string>
					{
						Core.Constants.CountryCodes.Austria,
						Core.Constants.CountryCodes.Belgium,
						Core.Constants.CountryCodes.Bulgaria,
						Core.Constants.CountryCodes.Croatia,
						Core.Constants.CountryCodes.Cyprus,
						Core.Constants.CountryCodes.CzechRepublic,
						Core.Constants.CountryCodes.Denmark,
						Core.Constants.CountryCodes.Estonia,
						Core.Constants.CountryCodes.Finland,
						Core.Constants.CountryCodes.France,
						Core.Constants.CountryCodes.Germany,
						Core.Constants.CountryCodes.Greece,
						Core.Constants.CountryCodes.Hungary,
						Core.Constants.CountryCodes.Ireland,
						Core.Constants.CountryCodes.Italy,
						Core.Constants.CountryCodes.Latvia,
						Core.Constants.CountryCodes.Lithuania,
						Core.Constants.CountryCodes.Luxembourg,
						Core.Constants.CountryCodes.Malta,
						Core.Constants.CountryCodes.Netherlands,
						Core.Constants.CountryCodes.Poland,
						Core.Constants.CountryCodes.Portugal,
						Core.Constants.CountryCodes.Romania,
						Core.Constants.CountryCodes.Slovakia,
						Core.Constants.CountryCodes.Slovenia,
						Core.Constants.CountryCodes.Spain,
						Core.Constants.CountryCodes.Sweden,
						Core.Constants.CountryCodes.UnitedKingdom
					};
				}
				return countries;
			});

		public static IEnumerable<string> GetEuropeanUnionForCustomsMembers(this BusinessObjectFactory factory) => factory.GetCachedValue("EuropeanUnionEconomicGroupingMembersList", () =>
		{
			var result = factory.GetEuropeanUnionForCustomsTradeGroup();

			return result.Any() ? result : new List<string>
			{
				Core.Constants.CountryCodes.Austria,
				Core.Constants.CountryCodes.Belgium,
				Core.Constants.CountryCodes.Bulgaria,
				Core.Constants.CountryCodes.Croatia,
				Core.Constants.CountryCodes.Cyprus,
				Core.Constants.CountryCodes.CzechRepublic,
				Core.Constants.CountryCodes.Denmark,
				Core.Constants.CountryCodes.Estonia,
				Core.Constants.CountryCodes.Finland,
				Core.Constants.CountryCodes.France,
				Core.Constants.CountryCodes.Germany,
				Core.Constants.CountryCodes.Greece,
				Core.Constants.CountryCodes.Hungary,
				Core.Constants.CountryCodes.Ireland,
				Core.Constants.CountryCodes.Italy,
				Core.Constants.CountryCodes.Latvia,
				Core.Constants.CountryCodes.Lithuania,
				Core.Constants.CountryCodes.Luxembourg,
				Core.Constants.CountryCodes.Malta,
				Core.Constants.CountryCodes.Netherlands,
				Core.Constants.CountryCodes.Poland,
				Core.Constants.CountryCodes.Portugal,
				Core.Constants.CountryCodes.Romania,
				Core.Constants.CountryCodes.Slovakia,
				Core.Constants.CountryCodes.Slovenia,
				Core.Constants.CountryCodes.Spain,
				Core.Constants.CountryCodes.Sweden
			};
		});

		static List<string> GetEuropeanUnionForCustomsTradeGroup(this BusinessObjectFactory factory) => factory.GetCachedValue("EuropeanUnionForCustomsTradeGroup", () =>
		{
			var groupLoader = new CusRefTradeGroupView.Loader(factory);
			var europeanUnionForCustomsTradeGroup = groupLoader.Load(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusTradeGroupCodes.EuropeanUnionForCustoms, ZDateTime.Now);
			return europeanUnionForCustomsTradeGroup?.GetApplicableTradeGroupCountries(ZDateTime.Now)
				.Select(x => x.ZZB_RN_NKTradeGroupCountryCode.ToString())
				.ToList() ?? new List<string>();
		});

		public static CodeDescriptionPairList GetEuropeanUnionCountryList(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("EuropeanUnionCountryMembersCodeDescriptionPairList", () =>
			{
				var result = new CodeDescriptionPairList();

				var countryQuery = new ZQuery(RefCountrySchema.RN_EconomicGrouping, EconomicGroupList.Codes.EuropeanUnion);
				countryQuery.AddToFilter(JoinCondition.Or, RefCountrySchema.RN_Code, factory.GetEuropeanUnionForCustomsMembers());
				countryQuery.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, new[] { Core.Constants.CountryCodes.Monaco, Core.Constants.CountryCodes.IsleOfMan });

				foreach (var country in factory.Load<RefCountry>(countryQuery))
				{
					var countryCode = country.RN_Code == Core.Constants.CountryCodes.Greece ? (ZString)GreeceMemberStateCode : country.RN_Code;
					result.AddPairIfNotExist(countryCode, country.RN_DescMultilingual);
				}

				result.Sort();
				return result;
			});
		}

		public static IEnumerable<string> GetEuropeanUnionForSafetyAndSecurityCountries(this BusinessObjectFactory factory) => factory.GetCachedValue(nameof(GetEuropeanUnionForSafetyAndSecurityCountries), () =>
		{
			var groupLoader = new CusRefTradeGroupView.Loader(factory);
			var europeanUnionForSafetyAndSecurityTradeGroup = groupLoader.Load(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusTradeGroupCodes.EUForSafetyAndSecurity, ZDateTime.Now);
			return europeanUnionForSafetyAndSecurityTradeGroup?.GetApplicableTradeGroupCountries(ZDateTime.Now)
				.Select(x => x.ZZB_RN_NKTradeGroupCountryCode.ToString())
				.ToArray() ?? Enumerable.Empty<string>();
		});

		public static bool IsEuropeanUnionForSafetyAndSecurityCountry(this BusinessObjectFactory factory, string countryCode) => factory.GetEuropeanUnionForSafetyAndSecurityCountries().Contains(countryCode);

		const string GreeceMemberStateCode = "EL";
	}
}
