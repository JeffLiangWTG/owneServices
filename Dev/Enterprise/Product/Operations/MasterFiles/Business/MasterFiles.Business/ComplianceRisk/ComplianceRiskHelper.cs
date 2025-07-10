using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Agency;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business
{
	public static class ComplianceRiskHelper
	{
		/// <summary>
		/// Validation to check registry if compliance risk is enabled for this business object and implementing IComplianceItemRiskStatusProvider
		/// </summary>
		public static bool CheckIfComplianceRiskEnabled(BusinessObject parentBizO) => IsMasterEnabledComplianceWise && parentBizO is IComplianceItemRiskStatusProvider && CheckIfImplementedIComplianceItemRiskStatusProvider(parentBizO.GetType(), false);

		public static bool CheckIfComplianceRiskEnabled(Type bizoType, bool allowViewType) => IsMasterEnabledComplianceWise && CheckIfImplementedIComplianceItemRiskStatusProvider(bizoType, allowViewType);

		public static (bool IsCurrent, ZDateTime JobEndDate) GetJobEndDateAndIsCurrent(ZString? jobStatus, ZDateTime jsDepartTime, ZDateTime jsArriveTime, BusinessObject parentBizO, IEnumerable<ComplianceRouting> routings)
		{
			var isJobOpened = IsJobOpened(jobStatus);
			var jobTimeAllEmpty = jsDepartTime == ZDateTime.Empty && jsArriveTime == ZDateTime.Empty;
			(var isCurrent, var jobEndDate) = GetJobEndDateAndIsCurrentFromRoutings(jobTimeAllEmpty, parentBizO, routings, isJobOpened);

			isCurrent = isCurrent || isJobOpened && (IsDateWithinSevenDays(jsDepartTime) || IsDateWithinSevenDays(jsArriveTime));

			if (!isCurrent && isJobOpened && parentBizO is IComplianceItemRiskStatusProvider complianceItemRiskStatusProvider)
			{
				foreach (var subComplianceRiskStatusProvider in complianceItemRiskStatusProvider.SubComplianceRiskStatusProviders)
				{
					if (subComplianceRiskStatusProvider.JobTime.IsCurrent)
					{
						isCurrent = true;
					}
				}
			}

			if (!jobTimeAllEmpty)
			{
				jobEndDate = new[] { jsDepartTime, jsArriveTime, jobEndDate }.MaxOrDefault(d => d);
			}

			return (isCurrent, jobEndDate);
		}

		static (bool IsCurrentOfRoutings, ZDateTime JobEndDateOfRoutings) GetJobEndDateAndIsCurrentFromRoutings(bool jobTimeAllEmpty, BusinessObject parentBizO, IEnumerable<ComplianceRouting> routings, bool isJobOpened)
		{
			var isCurrentOfRoutings = false;
			var jobEndDateOfRoutings = ZDateTime.Empty;
			var hasOneLegAllEmpty = false;
			if (routings.IsNullOrEmpty())
			{
				hasOneLegAllEmpty = true;
			}
			else
			{
				foreach (var routing in routings)
				{
					if (!isCurrentOfRoutings && isJobOpened && (IsDateWithinSevenDays(routing.ETD) || IsDateWithinSevenDays(routing.ETA)
						|| IsDateWithinSevenDays(routing.ATD) || IsDateWithinSevenDays(routing.ATA)))
					{
						isCurrentOfRoutings = true;
					}

					if (routing.ETD == ZDateTime.Empty && routing.ETA == ZDateTime.Empty
						&& routing.ATD == ZDateTime.Empty && routing.ATA == ZDateTime.Empty)
					{
						hasOneLegAllEmpty = true;
					}

					jobEndDateOfRoutings = new[] { routing.ETD, routing.ETA, routing.ATD, routing.ATA, jobEndDateOfRoutings }.MaxOrDefault(d => d);
				}
			}

			var systemCreateDateTime = GetSystemCreateDateTime(parentBizO);

			if (jobTimeAllEmpty && hasOneLegAllEmpty)
			{
				isCurrentOfRoutings = isCurrentOfRoutings || isJobOpened && IsDateWithinJobUpdatePeriod(systemCreateDateTime);
				jobEndDateOfRoutings = new[] { systemCreateDateTime.AddMonths(OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value), jobEndDateOfRoutings }.MaxOrDefault(d => d);
			}

			return (isCurrentOfRoutings, jobEndDateOfRoutings);
		}

		static bool IsJobOpened(ZString? jobStatus)
		{
			return !jobStatus.HasValue || (jobStatus.Value != JobHeaderStatus.Closed.Code && jobStatus.Value != JobHeaderStatus.Complete.Code);
		}

		static ZDateTime GetSystemCreateDateTime(BusinessObject parentBizO)
		{
			return !parentBizO.IsInDatabase ? ZDateTime.Now : (parentBizO as IAuditDetails).SystemCreateTimeUtc;
		}

		static bool IsDateWithinSevenDays(ZDateTime referenceDateTime) => referenceDateTime.Date.ToZDateTime().IsInTheFuture(ZDateTime.Now.Date.AddDays(-7));

		static bool IsDateWithinJobUpdatePeriod(ZDateTime referenceDateTime) => referenceDateTime.IsInTheFuture(ZDateTime.UtcNow
			.AddMonths(-OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value).ToDateTime());

		static bool CheckIfImplementedIComplianceItemRiskStatusProvider(Type bizoType, bool allowViewType)
		{
			var interfaceImplemented = false;

			if (typeof(IComplianceItemRiskStatusProvider).IsAssignableFrom(bizoType))
			{
				if (typeof(IForwardingShipment).IsAssignableFrom(bizoType)
					|| typeof(IQuotedBooking).IsAssignableFrom(bizoType)
					|| typeof(IForwardingConsol).IsAssignableFrom(bizoType))
				{
					interfaceImplemented = IsFreightEnabledComplianceWise;
				}
				else if (typeof(Enterprise.Integration.Customs.IBaseJobDeclaration).IsAssignableFrom(bizoType))
				{
					interfaceImplemented = IsCustomsEnabledComplianceWise;
				}
				else if (typeof(IAgencyShipment).IsAssignableFrom(bizoType)
					|| typeof(IBillOfLading).IsAssignableFrom(bizoType))
				{
					interfaceImplemented = IsLinerAgencyEnabledComplianceWise;
				}
				else if (Globals.IsTest)
				{
					interfaceImplemented = true;
				}
			}
			else if (allowViewType && typeof(IViewComplianceRiskStatusProvider).IsAssignableFrom(bizoType))
			{
				var providerBizOType = bizoType.GetCustomAttribute<ViewComplianceRiskStatusProviderAttribute>()?.ProviderBusinessObjectType;
				if (providerBizOType != null)
				{
					interfaceImplemented = CheckIfComplianceRiskEnabled(providerBizOType, allowViewType);
				}
			}

			return interfaceImplemented;
		}

		public static ZString ExtractAllAlphanumericFromHsCode(ZString hsCode)
		{
			return Regex.Replace(hsCode, @"[^a-zA-Z0-9]", string.Empty);
		}

		public static string GetUnionKeyFromCommodity(ZString harmonizedCode, ZString groupingOrCountry, ZString origin, ZString description)
		{
			return $"{harmonizedCode.ToUpperInvariant()}$${groupingOrCountry}$${origin.ToUpperInvariant()}$${description.ToUpperInvariant()}";
		}

		public static bool IsComplianceWarningMessageEnabled => IsMasterEnabledComplianceWise && OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.Value;

		public static bool IsMasterEnabledComplianceWise => Env.Registry.RawRegistry.EnableComplianceRisk.Value;

		public static bool IsLinerAgencyEnabledComplianceWise => IsMasterEnabledComplianceWise && LinerAgencyDataRegistry.Instance.LinerAgencyEnableComplianceWise.Value.EnableComplianceWise;

		public static bool IsCustomsEnabledComplianceWise => IsMasterEnabledComplianceWise && ComplianceRiskFeatureControlHelper.HasIntegrateComplinaceWiseToCustomsDeclarationModuleEnabled();

		public static bool IsComplianceCommodityScreeningEnable => IsMasterEnabledComplianceWise && ComplianceRiskFeatureControlHelper.HasComplianceWiseCommodityScreeningEnable();

		public static bool IsCustomsEnabledManageRiskStatusOnCommercialInvoice => IsCustomsEnabledComplianceWise && IsComplianceCommodityScreeningEnable && ComplianceRiskFeatureControlHelper.HasIntegrateComplinaceWiseCustomsDeclarationManageRiskStatusOnCommercialInvoice();

		public static bool IsFreightEnabledComplianceWise => IsMasterEnabledComplianceWise && FreightDataRegistry.Instance.FreightEnableComplianceWise.Value.EnableComplianceWise;

		public static bool IsGlobalCommercialInvoiceEnabled => OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopmentEnabled(OrganisationsDataRegistry.ComplianceWiseFeatureCodes.GlobalCommercialInvoice);

		public static bool IsComplianceCommodityRiskAssessmentEnabled => OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.Value;

		public static bool IsJobEntitiesCachingEnabled => OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopmentEnabled(OrganisationsDataRegistry.ComplianceWiseFeatureCodes.JobEntitiesCaching);

		public static Dictionary<ZInt, ComplianceCommodity[]> GetCommercialInvoiceLines(IEnumerable<ZInt> clusteredKeys, ZString groupingOrCountry, ZString source, ZGuid parentID, ZString commoditySource, BusinessObjectFactory factory)
		{
			var keys = clusteredKeys.Distinct().ToArray();
			const string clusteredKeysParamName = "@clusteredKeys";
			const string sql = $@"
SELECT
	{JobComInvoiceLineSchema.Constants.JI_ClusterKey},
	{JobComInvoiceLineSchema.Constants.JI_Tariff},
	{JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin},
	{JobComInvoiceLineSchema.Constants.JI_NDescription},
	{JobComInvoiceLineSchema.Constants.JI_Description}
FROM {JobComInvoiceLineSchema.Constants.SqlSchemaName}.{JobComInvoiceLineSchema.Constants.TableName}
JOIN {clusteredKeysParamName} ON [Value] = {JobComInvoiceLineSchema.Constants.JI_ClusterKey}
WHERE {JobComInvoiceLineSchema.Constants.JI_Tariff} <> '';";

			var collection = new DynamicBusinessObjectCollection(factory);
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New(parameterName: clusteredKeysParamName, value: keys, schemaColumn: JobComInvoiceLineSchema.JI_ClusterKey, isTableValued: true)
			};
			collection.Load(sql, parameters);

			var results = collection
				.Where((x) => !((ZString)x[JobComInvoiceLineSchema.JI_Tariff]).IsEmpty)
				.GroupBy(
					keySelector: (x) => (ZInt)x[JobComInvoiceLineSchema.Constants.JI_ClusterKey],
					elementSelector: (x) => new ComplianceCommodity(groupingOrCountry, source, parentID, commoditySource, rawDbLine: x))
				.ToDictionary(
					keySelector: (x) => x.Key,
					elementSelector: (x) => x.ToArray());

			keys
				.Where((x) => !results.ContainsKey(x))
				.ForEach((x) => results.Add(x, Array.Empty<ComplianceCommodity>()));

			return results;
		}

		public static DeclarationInvoiceLineCache GetCommercialInvoiceLines(ZInt clusteredKey, ZString groupingOrCountry, ZString source, ZGuid parentID, ZString commoditySource, BusinessObjectFactory factory)
		{
			const string clusteredKeyParamName = "@clusteredKey";
			const string sql = $@"
SELECT
	{JobComInvoiceLineSchema.Constants.JI_ClusterKey},
	{JobComInvoiceLineSchema.Constants.JI_Tariff},
	{JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin},
	{JobComInvoiceLineSchema.Constants.JI_NDescription},
	{JobComInvoiceLineSchema.Constants.JI_Description}
FROM {JobComInvoiceLineSchema.Constants.SqlSchemaName}.{JobComInvoiceLineSchema.Constants.TableName}
WHERE {JobComInvoiceLineSchema.Constants.JI_ClusterKey} = {clusteredKeyParamName}
	AND
	(
		{JobComInvoiceLineSchema.Constants.JI_Tariff} <> ''
		OR
		{JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin} <> ''
	);";

			var collection = new DynamicBusinessObjectCollection(factory);
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New(parameterName: clusteredKeyParamName, value: clusteredKey, schemaColumn: JobComInvoiceLineSchema.JI_ClusterKey)
			};
			collection.Load(sql, parameters);

			var commodities = collection
				.Where((x) => !((ZString)x[JobComInvoiceLineSchema.JI_Tariff]).IsEmpty)
				.Select((x) => new ComplianceCommodity(groupingOrCountry, source, parentID, commoditySource, rawDbLine: x))
				.ToArray();

			var countriesOfOrigin = collection
				.Select((x) => (ZString)x[JobComInvoiceLineSchema.JI_CountryOfOrigin])
				.Where((x) => !x.IsEmpty)
				.ToArray();

			return new DeclarationInvoiceLineCache(commodities, countriesOfOrigin);
		}
	}

	public class ComplianceRouting
	{
		public ZDateTime ETD { get; set; }

		public ZDateTime ETA { get; set; }

		public ZDateTime ATD { get; set; }

		public ZDateTime ATA { get; set; }
	}
}
