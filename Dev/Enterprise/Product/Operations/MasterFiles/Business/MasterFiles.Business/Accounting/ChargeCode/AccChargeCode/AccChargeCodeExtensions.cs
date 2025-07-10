using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class AccChargeCodeExtensions
	{
		public static bool IsGatewayRelated(this AccChargeCode chargeCode)
		{
			var isGatewayRelated = false;
			var gatewayChargeCodePks = AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.GetAsGuidArray().ToHashSet();
			if (gatewayChargeCodePks.IsNullOrEmpty())
			{
				isGatewayRelated = true;
			}
			else if (chargeCode != null)
			{
				isGatewayRelated = gatewayChargeCodePks.Contains(chargeCode.PK.ToGuid()) || (chargeCode.IsLinkedToGlobalChargeCode && gatewayChargeCodePks.Contains(chargeCode.GlobalChargeCode.PK.ToGuid()));
			}
			return isGatewayRelated;
		}

		/// <summary>
		/// If the ChargeCode is Local, first load the AC_Code, then try to load from the Intercompany Mapped Global Charge Code.
		/// If unsuccessful, return the original Charge Code.
		/// </summary>
		public static AccChargeCode GetGlobalChargeCode(this AccChargeCode chargeCode, string ledgerType, ZGuid? clientPK)
		{
			if (chargeCode == null || chargeCode.IsGlobal)
			{
				return chargeCode;
			}

			var result = chargeCode.GlobalChargeCode
				?? GetGlobalChargeCodeFromIntercompanyLocalChargeCodeMapping(chargeCode, ledgerType, clientPK)
				?? chargeCode;

			return result;
		}

		static AccChargeCode GetGlobalChargeCodeFromIntercompanyLocalChargeCodeMapping(AccChargeCode localChargeCode, string ledgerType, ZGuid? clientPK)
		{
			if (string.IsNullOrEmpty(ledgerType))
			{
				return null;
			}

			AccChargeCode result = null;
			if (clientPK != null)
			{
				result = GetGlobalChargeCodeFromIntercompanyMapping(localChargeCode, ledgerType, clientPK);
			}

			return result ?? GetGlobalChargeCodeFromIntercompanyMapping(localChargeCode, ledgerType, null);
		}

		static AccChargeCode GetGlobalChargeCodeFromIntercompanyMapping(AccChargeCode localChargeCode, string ledgerType, ZGuid? clientPK)
		{
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany), AccGlobalChargeCodeMapPivotSchema.YP_AC);
			pivotSubQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_AC, localChargeCode.PK);
			pivotSubQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_TYPE, ledgerType);
			pivotSubQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride, clientPK);

			var mapSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Accounting.IGlobalChargeCodeMapIntercompany), AccGlobalChargeCodeMapSchema.PK);
			mapSubQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_IsActive, true);
			mapSubQuery.AddSubQuery(AccGlobalChargeCodeMapSchema.PK, AccGlobalChargeCodeMapPivotSchema.YP_YG, pivotSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(AccChargeCode));
			query.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, null);
			query.AddSubQuery(AccChargeCodeSchema.AC_Code, AccGlobalChargeCodeMapSchema.YG_Code, mapSubQuery, JoinCondition.And);

			var results = localChargeCode.Factory.Load<AccChargeCode>(query);

			return results.Length == 1 ? results[0] : null;
		}

		/// <summary>
		/// If the ChargeCode is Global, first load the Intercompany Mapped Local Charge Code, then try to load from the AC_Code.
		/// If unsuccessful, return the original Charge Code.
		/// </summary>
		public static AccChargeCode GetLocalChargeCode(this AccChargeCode chargeCode, string ledgerType, ZGuid? clientPK)
		{
			if (chargeCode == null || !chargeCode.IsGlobal)
			{
				return chargeCode;
			}

			var result = GetLocalChargeCodeFromIntercompanyChargeCodeMapping(chargeCode.AC_Code, ledgerType, clientPK, chargeCode.Factory)
				?? chargeCode.ChildChargeCodes.SingleOrDefault(c => c.AC_GC == Env.CurrentCompanyPK)
				?? chargeCode;

			return result;
		}

		public static AccChargeCode GetLocalChargeCodeFromIntercompanyChargeCodeMapping(string localChargeCode, string ledgerType, ZGuid? clientPK, BusinessObjectFactory factory)
		{
			if (string.IsNullOrEmpty(ledgerType))
			{
				return null;
			}

			AccChargeCode result = null;
			if (clientPK != null)
			{
				result = GetLocalChargeCodeFromIntercompanyMapping(localChargeCode, ledgerType, clientPK, factory);
			}

			return result ?? GetLocalChargeCodeFromIntercompanyMapping(localChargeCode, ledgerType, null, factory);
		}

		static AccChargeCode GetLocalChargeCodeFromIntercompanyMapping(string chargeCodeToMap, string ledgerType, ZGuid? clientPK, BusinessObjectFactory factory)
		{
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany), AccGlobalChargeCodeMapPivotSchema.YP_AC);
			pivotSubQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_TYPE, ledgerType);
			pivotSubQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride, clientPK);

			var mapSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Accounting.IGlobalChargeCodeMapIntercompany), AccGlobalChargeCodeMapSchema.PK);
			mapSubQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_Code, chargeCodeToMap);
			mapSubQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_IsActive, true);

			pivotSubQuery.AddSubQuery(AccGlobalChargeCodeMapPivotSchema.YP_YG, AccGlobalChargeCodeMapSchema.PK, mapSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(AccChargeCode));
			query.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);
			query.AddSubQuery(AccChargeCodeSchema.PK, AccGlobalChargeCodeMapPivotSchema.YP_AC, pivotSubQuery, JoinCondition.And);

			var results = factory.Load<AccChargeCode>(query);

			return results.Length == 1 ? results[0] : null;
		}
	}
}
