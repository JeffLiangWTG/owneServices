using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IAutoRatingFreightConditionsSupportable
	{
		RateLineConditionsSupporter ConditionsSupporter { get; }
	}

	public abstract class RateLineConditionsSupporter
	{
		protected RateLineConditionsSupporter(BusinessObject objectToWrap)
		{
			this.objectToWrap = objectToWrap;
		}

		public OrgHeader ExportBroker
		{
			get { return Get(ref exportBroker, GetExportBroker); }
		}

		CachedProperty<OrgHeader> exportBroker;
		protected abstract OrgHeader GetExportBroker();

		public OrgHeader ImportBroker
		{
			get { return Get(ref importBroker, GetImportBroker); }
		}

		CachedProperty<OrgHeader> importBroker;
		protected abstract OrgHeader GetImportBroker();

		public OrgHeader SendingAgent
		{
			get { return Get(ref sendingAgent, GetSendingAgent); }
		}

		CachedProperty<OrgHeader> sendingAgent;
		protected abstract OrgHeader GetSendingAgent();

		public OrgHeader ReceivingAgent
		{
			get { return Get(ref receivingAgent, GetReceivingAgent); }
		}

		CachedProperty<OrgHeader> receivingAgent;
		protected abstract OrgHeader GetReceivingAgent();

		public OrgHeader ControllingAgent
		{
			get { return Get(ref controllingAgent, GetControllingAgent); }
		}

		CachedProperty<OrgHeader> controllingAgent;
		protected abstract OrgHeader GetControllingAgent();

		public OrgHeader DepartureCFS
		{
			get { return Get(ref departureCFS, GetDepartureCFS); }
		}

		CachedProperty<OrgHeader> departureCFS;
		protected abstract OrgHeader GetDepartureCFS();

		public OrgHeader ArrivalCFS
		{
			get { return Get(ref arrivalCFS, GetArrivalCFS); }
		}

		CachedProperty<OrgHeader> arrivalCFS;
		protected abstract OrgHeader GetArrivalCFS();

		public bool HasDangerousGoods
		{
			get { return Get(ref hasDangerousGoods, GetHasDangerousGoods); }
		}

		CachedProperty<bool> hasDangerousGoods;
		protected abstract bool GetHasDangerousGoods();

		public BusinessObject ObjectToWrap
		{
			get { return objectToWrap; }
		}

		readonly BusinessObject objectToWrap;

		T Get<T>(ref CachedProperty<T> cachedProperty, GetValueDelegate<T> getValueDelegate)
		{
			return objectToWrap.Factory.GetCached(ref cachedProperty, getValueDelegate);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Method determines rate line conditions on multiple variants")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Parameter is required for evaluating UserDefinedCondition only if necessary")]
		public virtual bool? MeetsCondition(ZString condition, ZString conditionalExpression, Func<ZString, RateLineConditionsSupporter, bool?> isUserDefinedConditionMet, bool isOrgFrt, bool isDstFrt)
		{
			switch (condition)
			{
				case RateLineConditions.OwnBrokerage:
					return (isOrgFrt && IsOrgProxy(ExportBroker)) || (isDstFrt && IsOrgProxy(ImportBroker));

				case RateLineConditions.HandOver:
					return (isOrgFrt && !IsOrgProxy(ExportBroker)) || (isDstFrt && !IsOrgProxy(ImportBroker));

				case RateLineConditions.ForwardingAndBrokerage:
					return (isOrgFrt && IsOrgProxy(ExportBroker) && IsOrgProxy(SendingAgent)) || (isDstFrt && IsOrgProxy(ImportBroker) && IsOrgProxy(ReceivingAgent));

				case RateLineConditions.OwnCFS:
					return isOrgFrt && IsOrgProxy(DepartureCFS) || isDstFrt && IsOrgProxy(ArrivalCFS);

				case RateLineConditions.OwnGateway:
					return IsOrgProxy(SendingAgent);

				case RateLineConditions.DangerousGoods:
					return HasDangerousGoods;

				case RateLineConditions.OwnControllingAgent:
					return IsOrgProxy(ControllingAgent);

				case RateLineConditions.UserDefined:
					return isUserDefinedConditionMet(conditionalExpression, this);
			}

			return false;
		}

		bool IsOrgProxy(OrgHeader org)
		{
			return org != null && org.IsProxyOrg(GlbCompany.CurrentCompany);
		}
	}

	public static class RateLineConditions
	{
		public const string OwnBrokerage = "BRK";
		public const string HandOver = "HDO";
		public const string ForwardingAndBrokerage = "FBR";
		public const string OwnCFS = "CFS";
		public const string OwnGateway = "GTW";
		public const string UserDefined = "USR";
		public const string DangerousGoods = "DAG";
		public const string OwnControllingAgent = "OCA";

		public static class Descriptions
		{
			public static string OwnBrokerage { get { return Res.GetString("0045cb84-07b2-4e69-ada8-4bae64d40e1f", "Own Brokerage"); } }
			public static string HandOver { get { return Res.GetString("cc3be7eb-8e40-4356-9ffa-3107fde3c684", "Hand Over"); } }
			public static string ForwardingAndBrokerage { get { return Res.GetString("4cc4b977-73aa-494e-a6b0-b5fb233c5146", "Forwarding and Brokerage"); } }
			public static string OwnCFS { get { return Res.GetString("7ac72724-41d4-49eb-bd21-944749b0db65", "Own CFS"); } }
			public static string OwnGateway { get { return Res.GetString("5a5514e6-c673-48d9-8620-8b9a0b9492ca", "Own Gateway"); } }
			public static string UserDefined { get { return Res.GetString("eed19f87-5b73-4e9a-a5db-eb495d97734d", "User Defined"); } }
			public static string DangerousGoods { get { return Res.GetString("71dc845a-4b5a-4393-aa18-2b902e3339b2", "Job contains Dangerous Goods"); } }
			public static string OwnControllingAgent { get { return Res.GetString("38e0c176-bb81-4a2e-b52a-a72ab5c5ca19", "Own Controlling Agent"); } }
		}
	}
}
