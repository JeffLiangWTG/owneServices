using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Constants;

namespace Enterprise.Rating.Business
{
	public class NotApplicableRateLineRemover
	{
		#region SuppressResourceStringsCheckRegion

		/// <summary>
		/// Ideally this would not require AutoRatingCalculatorParameters since it doesn't run calculators.
		/// It is required only for RemoveChargesThatDependOnInvalidOrMissingRequiredMeasures which only needs
		/// it to get the units for cost/tariff calculators. They get the units from the base calculator,
		/// which is found from AutoRatingCalculatorParameters.GetOriginalLines. Since the result from
		/// GetOriginalLines is cached, and the AutoRatingCalculatorParameters can be modified with filters
		/// after the caching the whole logic is suspect. The complexity of cost/tariff calculators and original lines
		/// should be isolated into a single place and managed more clearly.
		///
		/// WARNING: If aren't sure, put your remove methods last.
		/// i.e. RemoveCompanyTariffsIrrelevantLinesBasedOnClientRate need to be added after RemoveChargesWhereRateLineConditionIsNotMet.
		/// Otherwise we would remove CompanyTariff even the EXL line would be removed later on.
		/// </summary>
		public void RemoveNotApplicable(
			AutoRatingCalculatorParameters parameters,
			RateLinesRepository linesRepository,
			FilterOptions options,
			IDialogService dialogService)
		{
			var criteria = parameters.Criteria;
			var serviceRater = parameters.ServiceRater;
			var costSell = options.IsCosting ? CostSell.Cost : CostSell.Revenue;

			if (!options.DisableInvalidRatesFilter)
			{
				RemoveInvalidLines(linesRepository);
			}
			RemoveLinesWithInvalidCalculators(linesRepository);

			if (!options.DisableExcludedFromAutoRatingFilter)
			{
				RemoveExcludedFromAutoRatingEntries(linesRepository, options.IsCosting);
			}

			if (options.IsCosting)
			{
				if (!criteria.GatewayConfiguration.IsGatewayShipment)
				{
					RemoveIrrelevantCosts(criteria, linesRepository);
				}
			}
			else if (!options.DisableIrrelevantTariffsFilter)
			{
				RemoveIrrelevantCompanyTariffRateLinesBasedOnLevelAndOutOfDateRange(criteria, linesRepository);
				RemoveIrrelevantGroupClientRates(criteria, linesRepository);
			}

			if (!options.DisablePaymentTermsFilter)
			{
				RemoveRateLinesNotApplicableToPaymentTermOverride(criteria, linesRepository);
				RemoveChargesNotApplicableToPaymentTerm(criteria, linesRepository, costSell);
			}

			if (options.RemoveFreightCharge && options.IsCosting)
			{
				RemoveInternationalFreightChargeCodeWithSkipFreightCharge(criteria, linesRepository);
			}

			RemoveChargeCodesNotMatchingCurrentCompany(linesRepository);
			RemoveChargeCodesFromUnauthorizedDepartments(criteria, linesRepository);
			RemoveRebateLines(options.IsRebateCalculationMode, linesRepository);
			RemoveEquipmentTypeIrrelevantLines(criteria, linesRepository);
			RemoveMessageSubTypeIrrelevantLines(criteria, linesRepository);
			RemoveBillingTypeIrrelevantLines(linesRepository);
			RemoveRateLinesOutOfDateRange(criteria, linesRepository, options.IsCosting);
			RemoveFreightLinesOverriddenBySpotRate(criteria, linesRepository, options.IsCosting);
			serviceRater.RemoveRatesOverridenByServiceSpotRates(linesRepository);
			RemoveRatesOverridenByContainerSpotRates(criteria, linesRepository, options.IsCosting);

			if (!options.DisableSpotFilter && options.IsCosting)
			{
				RemoveItemsOverriddenBySpotCosts
					(
						criteria,
						linesRepository.GetLines().Select(l => new Tuple<AccChargeCode, FastLine>(l.ChargeCode, l)).ToList(),
						(line, log) =>
						{
							linesRepository.Remove(line, log);
						}
					);
			}

			RemoveChargesCodesAndLinesNotToBeChargedBasedOnGroupsAndServices(criteria, serviceRater, linesRepository, options.IsCosting);
			RemoveChargesForConsolLeadNoApportionment(criteria, linesRepository, costSell);
			RemoveChargesNotApplicableToASMShipments(criteria, linesRepository);
			RemoveConsumerSpecificCharges(criteria, linesRepository);
			RemoveCostRatesOverridenByIntercompanyTariff(criteria, linesRepository, options.IsCosting);

			var isRateEntryAdapter = criteria.AutoRating is RateEntryAdapter;

			if (!isRateEntryAdapter)
			{
				RemoveChargesThatDependOnInvalidOrMissingRequiredMeasures(parameters, linesRepository, criteria);
				RemoveChargesWhereRateLineConditionIsNotMet(criteria, linesRepository);
			}

			RemoveCompanyTariffsIrrelevantLinesBasedOnClientRate(linesRepository, costSell, criteria, options.IsCosting);
			RemoveMultipleClientContractNumber(criteria, options.IsCosting, linesRepository, dialogService);

			RemoveOverriddenRates(criteria, serviceRater, linesRepository, options);
		}

		#region Invalid Rates

		void RemoveInvalidLines(RateLinesRepository linesRepository)
		{
			linesRepository.Remove(l => !l.Line.IsValidRate(), l => l.Line.InvalidReason);
		}

		internal static void RemoveLinesWithInvalidCalculators(RateLinesRepository linesRepository)
		{
			linesRepository.Remove(l =>
			{
				if (l.Line.Uses(CalculatorType.HighestRate))
				{
					return l.Line.ChildRateLineItems.Any(l => string.IsNullOrEmpty(l.TM_BreakWeightVolume) && l.TM_Type == Calculator.Items.Operator.UNT);
				}
				return false;
			}, "Highest rate calculator missing required units.");
		}

		#endregion

		#region No Apportionment for BCN / SCN

		static void RemoveChargesForConsolLeadNoApportionment(RatingCriteria criteria, RateLinesRepository linesRepository, CostSell costOrSell)
		{
			if (criteria.AutoRating is AutoRatingProxy autoRatingProxy && autoRatingProxy.AutoRating is ConsolLeadShipmentRatingAdapter consolLeadAdapter)
			{
				var shipment = consolLeadAdapter.ThisShipment;
				if (shipment != null)
				{
					var fastLines = linesRepository
						.GetLines()
						.Where(line => line.ChargeCode != null);
					foreach (var fastLine in fastLines)
					{
						var line = fastLine.Line;
						if ((line.TL_UnitFactor == UnitFactorList.Codes.BCN
							|| line.TL_UnitFactor == UnitFactorList.Codes.SCN)
							&& line.Uses(CalculatorType.Unit))
						{
							if (line.TL_WeightVolume == QuantityUnit.CN)
							{
								var message = Res.GetString("E7A3BA16-FA0C-469A-8FE9-F6ADB4AD1934", "'CN' unit is NOT supported by Unit Factor '{0} - No Apportionment'", line.TL_UnitFactor);
								linesRepository.Remove(fastLine, message);
							}
							else if (line.TL_WeightVolume == QuantityUnit.HB && IsSubShipment(shipment))
							{
								var message = Res.GetString("9C5B402D-FC5D-492A-8B92-1DE0F49AFC05", "'HB' unit is calculated only for Lead Shipments but NOT Sub-Shipments in {0}", line.TL_UnitFactor);
								linesRepository.Remove(fastLine, message);
							}
							else if (line.TL_WeightVolume == QuantityUnit.LW && IsLeadShipment(shipment))
							{
								var message = Res.GetString("E268A3CA-B8FE-42D7-901E-2EA7DD39FEB4", "'LW' unit is calculated only for Sub-Shipments but NOT Lead Shipments in {0}", line.TL_UnitFactor);
								linesRepository.Remove(fastLine, message);
							}
						}
					}
				}
			}
		}

		static bool IsSubShipment(CommonShipment shipment) => shipment != null && !shipment.JS_JS_ColoadMasterShipment.IsEmpty;

		static bool IsLeadShipment(CommonShipment shipment) =>
			shipment != null
			&&
			(
				shipment.JS_ShipmentType == Constants.ShipmentTypes.BuyersConsolLead
				||
				shipment.JS_ShipmentType == Constants.ShipmentTypes.ShippersConsolLead
			);

		#endregion

		#region Charges Not Applicable For ASM Shipments

		static void RemoveChargesNotApplicableToASMShipments(RatingCriteria criteria, RateLinesRepository linesRepository)
		{
			if (criteria.AutoRatedFor?.FirstOrDefault() is CommonShipment commonShipment)
			{
				if (IsSubShipmentOfASMShipment(commonShipment))
				{
					linesRepository.Remove(x => x.IsIntercompanyTariff() && x.Line.TL_UnitFactor != UnitFactorList.Codes.SAM, Res.GetString("855af8d3-4bab-45d7-b73d-fe657d54a2fa", "Assembly Master sub-shipment(s) are not applicable for Unit Factor not setting as SAM."));
				}
				else if (IsAssemblyMasterLeadShipment(commonShipment))
				{
					linesRepository.Remove(x => x.Line.TL_UnitFactor == UnitFactorList.Codes.SAM, Res.GetString("a5265579-83b4-4fc7-9443-85a1ba600706", "'top-level' Assembly Master lead shipment(s) are not applicable for Unit Factor of SAM."));
				}
			}
		}

		static bool IsSubShipmentOfASMShipment(CommonShipment shipment) => shipment != null && !shipment.JS_JS_ColoadMasterShipment.IsEmpty && shipment.CoLoadMasterShipment.IsAssemblyMaster;

		static bool IsAssemblyMasterLeadShipment(CommonShipment shipment) => shipment != null && shipment.JS_JS_ColoadMasterShipment.IsEmpty && shipment.IsAssemblyMaster;

		#endregion

		#region Exluded from AutoRating

		void RemoveExcludedFromAutoRatingEntries(RateLinesRepository linesRepository, bool isCost)
		{
			if (isCost)
			{
				linesRepository.Remove(x => x.ParentRateEntry.TI_IsExcludedFromAutoRating, LogMessages.ExcludedFromAutocosting);
			}
		}

		#endregion

		#region Charge Codes

		void RemoveChargeCodesNotMatchingCurrentCompany(RateLinesRepository linesRepository) =>
			linesRepository.Remove(x => x.ChargeCode.AC_GC != Env.CurrentCompanyPK, LogMessages.ChargeCodeDoesNotBelongToCompany, withWarning: true);

		internal static void RemoveChargeCodesFromUnauthorizedDepartments(RatingCriteria criteria, RateLinesRepository linesRepository)
		{
			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges);
			var departmentAccessCache = new Dictionary<string, bool>();

			if (checkpoint.IsAllowed)
			{
				return;
			}

			var message = Res.GetString("61908605-7d11-48af-ba8b-676706c18f23", "Charge code is not accessible from this department.");
			linesRepository.Remove(x =>
			{
				if (x.ChargeCode.AC_DepartmentFilterList == "ALL" || x.ChargeCode.AC_DepartmentFilterList.Contains(Env.CurrentDepartment.Code))
				{
					return false;
				}

				var departmentCodes = x.ChargeCode.AC_DepartmentFilterList.Split(',').Select(code => code.Trim());

				foreach (var departmentCode in departmentCodes)
				{
					bool allowedLoginToDepartment = false;

					if (departmentAccessCache.TryGetValue(departmentCode, out var value))
					{
						allowedLoginToDepartment = value;
					}
					else
					{
						var department = criteria.Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode);
						if (department != null)
						{
							var security = new SecurityCore(GlbStaff.CurrentUser.StaffSecurityPermissionsCollection, GlbStaff.CurrentUser, Env.CurrentBranchPK, department.PK.ToGuid(), Env.CurrentCompanyPK, false);
							departmentAccessCache[departmentCode] = security.Login.IsAllowed;
							allowedLoginToDepartment = security.Login.IsAllowed;
						}
					}

					if (allowedLoginToDepartment)
					{
						return false;
					}
				}

				return true;
			}, message, withWarning: false);
		}

		#endregion

		#region Rebate

		static void RemoveRebateLines(bool isInRebateMode, RateLinesRepository linesRepository)
		{
			string getReason()
			{
				return isInRebateMode ? LogMessages.NotApplicableInRebateMode : LogMessages.OnlyApplicableInRebateMode;
			}

			linesRepository.Remove(x => x.Line.Uses(CalculatorType.ProfitShareRebate) != isInRebateMode, getReason());
		}

		#endregion

		#region Equipment Type

		void RemoveEquipmentTypeIrrelevantLines(RatingCriteria criteria, RateLinesRepository linesRepository)
		{
			foreach (var line in linesRepository.GetLines())
			{
				var criteriaEquipment = string.Empty;
				var entry = line.ParentRateEntry;
				var partyCartageEquipment = "cartage equipment";

				if (entry.IsOriginEntry())
				{
					partyCartageEquipment = "consignor " + partyCartageEquipment;
					criteriaEquipment = criteria.PickupCartageEquipment;
				}
				else if (entry.IsDestinationEntry())
				{
					partyCartageEquipment = "consignee " + partyCartageEquipment;
					criteriaEquipment = criteria.DeliveryCartageEquipment;
				}

				var calculator = line.Calculator;
				if (calculator.ShowEquipmentType)
				{
					var lineEquipment = calculator.EquipmentType;
					if (lineEquipment != criteriaEquipment
						&& lineEquipment != Constants.EquipmentNeeded.Any
						&& !string.IsNullOrEmpty(lineEquipment)) // for old data
					{
						linesRepository.Remove(line, LogMessages.EquipmentTypeIrrelevant(lineEquipment, partyCartageEquipment, criteriaEquipment));
					}
				}
			}
		}

		#endregion

		#region Message Sub Type

		static void RemoveMessageSubTypeIrrelevantLines(RatingCriteria criteria, RateLinesRepository linesRepository)
		{
			foreach (var line in linesRepository.GetLines())
			{
				var calculator = line.Calculator;
				if (calculator.ShowMessageTypeSubType)
				{
					var lineMessageType = calculator.MessageType;
					var criteriaMessageType = criteria.MessageType;
					var lineMessageSubType = calculator.MessageSubType;
					var criteriaMessageSubType = criteria.MessageSubType;

					var addLine = (lineMessageType.IsEmpty || criteriaMessageType.IsEmpty || lineMessageType == criteriaMessageType) && (lineMessageSubType.IsEmpty || criteriaMessageSubType.IsEmpty || lineMessageSubType == criteriaMessageSubType);

					if (!addLine)
					{
						linesRepository.Remove(line, LogMessages.MessageSubTypeIrrelevant(lineMessageType, lineMessageSubType, criteriaMessageType, criteriaMessageSubType));
					}
				}
			}
		}

		#endregion

		#region Out of Date Range

		internal static void RemoveRateLinesOutOfDateRange(RatingCriteria criteria, RateLinesRepository linesRepository, bool isCosting)
		{
			if (criteria == null)
			{
				throw new ArgumentNullException(nameof(criteria));
			}

			foreach (var line in linesRepository.GetLines())
			{
				var (rateDateType, jobDate, dateType, standardFilteringDesc) = criteria.GetEffectiveDateWithFallback(line.Line, isCosting: isCosting);

				if (rateDateType.IsFallbackDisabled && !jobDate.IsValid)
				{
					var reason = ZString.Format("{0} was empty and no fallback filtering was applied.", JobDateTypes.JobDateTypeList.GetDescriptionFromCode(dateType));
					linesRepository.Remove(line, reason);
					continue;
				}

				var startDate = line.Line.TL_RateStartDate;
				if (startDate.IsEmpty)
				{
					startDate = line.ParentRateEntry.TI_RateStartDate;
				}
				var endDate = line.Line.TL_RateEndDate;
				if (endDate.IsEmpty)
				{
					endDate = line.ParentRateEntry.TI_RateEndDate;
				}

				var outOfRange = jobDate < startDate || !endDate.IsEmpty && jobDate > endDate;
				if (outOfRange)
				{
					var jdStr = jobDate.ToShortDateString();
					var startStr = startDate.ToShortDateString();
					var endStr = endDate.IsEmpty ? "empty" : endDate.ToShortDateString();

					var reason = dateType.IsEmpty
						? ZString.Format("Job Date Type could not be determined from the registry. Default value is Today's date ({0}) which is outside of outside of the date range of this rate ({1} to {2})", jdStr, startStr, endStr)
						: ZString.Format("{0} ({1}) is outside of the date range of this rate ({2} to {3})", standardFilteringDesc + JobDateTypes.JobDateTypeList.GetDescriptionFromCode(dateType), jdStr, startStr, endStr);

					linesRepository.Remove(line, reason);
				}
			}
		}

		internal static void RemoveIrrelevantCompanyTariffRateLinesBasedOnLevelAndOutOfDateRange(RatingCriteria criteria, RateLinesRepository linesRepository)
		{
			if (criteria == null)
			{
				throw new ArgumentNullException(nameof(criteria));
			}

			if (criteria.LocalClient == null && criteria.OverseasAgent == null && criteria.Consignee == null && criteria.Consignor == null)
			{
				return;
			}

			foreach (var line in linesRepository.GetLines())
			{
				if (!line.ParentRateEntry.IsCompanyTariff())
				{
					continue;
				}

				if (!line.TL_FeeChargeType.IsEmpty)
				{
					continue;
				}

				var (rateDateType, jobDate, dateType, standardFilteringDesc) = criteria.GetEffectiveDateWithFallback(line.Line, isCosting: false);

				if (rateDateType.IsFallbackDisabled && !jobDate.IsValid)
				{
					var reason = ZString.Format("{0} was empty and no fallback filtering was applied.", JobDateTypes.JobDateTypeList.GetDescriptionFromCode(dateType));
					linesRepository.Remove(line, reason);
					continue;
				}

				var shouldFilterLine = true;
				var rateLineLevel = line.ParentRateEntry.ParentRatingHeader.TH_GlobalRateLevel;
				if (rateLineLevel > 0)
				{
					if (rateLineLevel == criteria.TariffLevel)
					{
						continue;
					}

					var chargedOrgs = line.GetChargedOrgs(forCompanyTariff: true);
					string rateMode = RatingHelper.GetRateMode(criteria, line.ParentRateEntry.TI_RateCategory);

					foreach (var chargedOrg in chargedOrgs)
					{
						var level = RatingCache.GetCompanyTariffLevel(chargedOrg, criteria.Company, line.ParentRateEntry.TI_RateCategory, rateMode, criteria.Direction, jobDate).level;
						if (level == rateLineLevel)
						{
							shouldFilterLine = false;
							break;
						}
					}
				}

				if (shouldFilterLine)
				{
					var jobDateString = jobDate.ToShortDateString();
					var levelString = line.ParentRateEntry.ParentRatingHeader.TH_GlobalRateLevel.ToString();
					var reason = dateType.IsEmpty
					? ZString.Format("Job Date Type could not be determined from the registry, defaulting to Today's date. Associated company tariff level ({1}) is not the most specific or ({0}) is outside of the date range", jobDateString, levelString)
					: ZString.Format("{0} Associated company tariff level ({2}) is not the most specific or ({1}) is outside of the date range", standardFilteringDesc + JobDateTypes.JobDateTypeList.GetDescriptionFromCode(dateType), jobDateString, levelString);
					linesRepository.Remove(line, reason);
				}
			}
		}

		#endregion

		#region Overriden By Spot Rate

		static void RemoveFreightLinesOverriddenBySpotRate(RatingCriteria criteria, RateLinesRepository linesRepository, bool isCosting)
		{
			var autoratingMode = isCosting ? criteria.CostSpotRateInfo.AutoratedMode : criteria.SellSpotRateInfo.AutoratedMode;

			if (autoratingMode == Constants.FreightRateAutoratingModes.Code.FreightPlusRate
				|| autoratingMode == Constants.FreightRateAutoratingModes.Code.AllInRate)
			{
				var company = criteria.Company ?? GlbCompany.CurrentCompany;
				var freightChargeCode = Env.Registry.GetFreightChargeCode(company.PK.ToGuid());
				var shouldFilterIntercompanyTariff = isCosting
					? criteria.CostSpotRateInfo.AutoratedValueType == AutoratedValueType.GatewaySell
					: criteria.SellSpotRateInfo.AutoratedValueType == AutoratedValueType.GatewaySell;

				var costSell = isCosting ? CostSell.Cost : CostSell.Revenue;

				if (shouldFilterIntercompanyTariff && !criteria.IsGatewaySellApplicableToGatewayConsol(costSell))
				{
					shouldFilterIntercompanyTariff = false;
				}

				bool IsFreightLineOverridenBySpotRate(FastLine raterLine)
				{
					bool isFreightChargeCode = raterLine.ChargeCode != null && raterLine.ChargeCode.PK == freightChargeCode;
					bool isFreightChargeGroup = raterLine.ChargeCode != null && raterLine.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight;

					var rateLine = raterLine.Line;

					if (!rateLine.ParentRateEntry.IsSpotEntry &&
						((autoratingMode == Constants.FreightRateAutoratingModes.Code.FreightPlusRate && isFreightChargeCode)
							|| (autoratingMode == Constants.FreightRateAutoratingModes.Code.AllInRate && isFreightChargeGroup)))
					{
						return !rateLine.IsIntercompanyTariff() || shouldFilterIntercompanyTariff;
					}

					return false;
				}

				foreach (var line in linesRepository.GetLines().Where(IsFreightLineOverridenBySpotRate))
				{
					linesRepository.Remove(line, Res.GetString("faabd617-22ba-407c-82a8-33b1ab89d794", "replaced by Job Negotiated Cost/Gateway Sell/One Off Freight Rate"));
				}
			}
		}

		#endregion

		#region Overriden By Container Spot Rates

		static void RemoveRatesOverridenByContainerSpotRates(RatingCriteria criteria, RateLinesRepository linesRepository, bool isCosting)
		{
			var rateLines = linesRepository.GetLines();

			var spotRateLines = rateLines.Where(x => x.Line.IsContainerSpotRate()).ToList();
			if (spotRateLines.Any())
			{
				var containerSpotRates = criteria.JobMeasures.GetContainerSpotRates();
				var company = criteria.Company ?? GlbCompany.CurrentCompany;
				var freightChargeCode = Env.Registry.GetFreightChargeCode(company.PK.ToGuid());

				foreach (var spotRateLine in spotRateLines)
				{
					var spotRate = containerSpotRates
						.Where(x => x.ContainerPK == spotRateLine.ParentRateEntry.ContainerPKForSpotEntry)
						.Select(x => isCosting ? x.CostSpotRate : x.SellSpotRate).SingleOrDefault();

					if (spotRate != null)
					{
						bool IsOverridenByContainerSpotRate(FastLine raterLine)
						{
							var rateLine = raterLine.Line;
							if (!rateLine.ParentRateEntry.IsSpotEntry && rateLine.ParentRateEntry.TI_RC == spotRateLine.ParentRateEntry.TI_RC && rateLine.ChargeCode != null)
							{
								return !rateLine.IsIntercompanyTariff() || spotRate.AutoratedValueType == AutoratedValueType.GatewaySell;
							}

							return false;
						}

						foreach (var nonSpotRateLine in rateLines.Where(IsOverridenByContainerSpotRate))
						{
							bool isFreightChargeCode = nonSpotRateLine.ChargeCode != null && nonSpotRateLine.ChargeCode.PK == freightChargeCode;
							bool isFreightChargeGroup = nonSpotRateLine.ChargeCode != null && nonSpotRateLine.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight;

							if ((spotRate.AutoratedMode == Constants.FreightRateAutoratingModes.Code.FreightPlusRate && isFreightChargeCode)
								|| (spotRate.AutoratedMode == Constants.FreightRateAutoratingModes.Code.AllInRate && isFreightChargeGroup))
							{
								linesRepository.Remove(nonSpotRateLine, Res.GetString("54dd34cb-1f5b-48a6-bbc9-d1316b182d43", "replaced by Container {0}", (spotRateLine.Line as RateLine).SpotRateDescription));
							}
						}
					}
				}
			}

			LogInvalidContainerSpotRates(criteria, isCosting, linesRepository);
		}

		static void LogInvalidContainerSpotRates(RatingCriteria criteria, bool isCosting, RateLinesRepository linesRepository)
		{
			var measures = criteria.JobMeasures;
			if (measures.HasContainerMeasure)
			{
				var invalidSpotRates = measures.GetContainerSpotRates()
					.Where(x => (isCosting && x.CostSpotRate != null && !x.CostSpotRateIsValid)
							|| (!isCosting && x.SellSpotRate != null && !x.SellSpotRateIsValid));
				foreach (var invalidSpotRate in invalidSpotRates)
				{
					var spotRateType = isCosting ? invalidSpotRate.CostSpotRate.GetAutoratedValueTypeDescription() : invalidSpotRate.SellSpotRate.GetAutoratedValueTypeDescription();
					linesRepository.LogAfterSearch().Information(Res.GetString("ec86865f-972d-4ef7-8d6f-889d24deac1f", "Container {0} has {1} which was ignored due to invalid configuration.", invalidSpotRate.ContainerNumber, spotRateType));
				}
			}
		}

		#endregion

		#region Overriden by Spot Existing Costs On Job
		public static void RemoveItemsOverriddenBySpotCosts<T>(RatingCriteria criteria, IEnumerable<Tuple<AccChargeCode, T>> lines, Action<T, string> remove)
		{
			var jobRelatedCharges = (criteria.GetRatingAdapter() as IAutoRatingSpotChargeInfo)?.GetJobSpotCharges();
			if (jobRelatedCharges?.Any() == true)
			{
				var shouldRemoveLinesWithFreightChargeGroup = jobRelatedCharges.Keys.Any(behaviour => RatingBehaviours.IsFreightChargeGroupOverriderBehaviour(behaviour));

				var otherChargesWithSpotBehaviour =
					jobRelatedCharges
					.Where(c => RatingBehaviours.IsSpecificChargeOverriderSpotBehaviour(c.Key))
					.SelectMany(c => c.Value)
					.ToHashSet();

				foreach (var line in lines)
				{
					bool isFreightChargeGroup = line.Item1 != null && line.Item1.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight;

					if (shouldRemoveLinesWithFreightChargeGroup && isFreightChargeGroup)
					{
						remove(line.Item2, Res.GetString("45E1B97D-3939-4C7D-9B83-2AAF4BF75FE9", "removed due to existing Costs with {0}/{1} Rating Behavior", RatingBehaviours.AllInAdhocOverridingSpotRate, RatingBehaviours.AllInBSAOverridingSpotRate));
					}

					if (otherChargesWithSpotBehaviour.Contains(line.Item1.AC_Code))
					{
						remove(line.Item2, Res.GetString("2AC58F9A-8B71-4C5C-A5BB-EC07FD2EB235", "removed due to existing Costs with exact charge code and {0}/{1} Rating Behavior", RatingBehaviours.FreightAdhocOverridingSpotRate, RatingBehaviours.FreightBSAOverridingSpotRate));
					}
				}
			}
		}
		#endregion

		#region Groups and Services

		static void RemoveChargesCodesAndLinesNotToBeChargedBasedOnGroupsAndServices(RatingCriteria criteria, ServiceAutoRater serviceRater, RateLinesRepository linesRepository, bool isCosting)
		{
			var chargeFilter = isCosting ? criteria.ChargeCodeGroups.CostChargesFilter : criteria.ChargeCodeGroups.SellChargesFilter;

			foreach (var fastLine in linesRepository.GetLines())
			{
				var chargeCode = fastLine.ChargeCode;
				if (chargeCode == null)
				{
					continue;
				}

				// This filter is not applicable for Freight Inclusive charges. They are included in Freight charge, so, if Freight is not
				// filtered out, the FreightInclusiveCalculator will add the charge to Freight charge. If Freight charge is filtered out,
				// then FreightInclusiveCalculator will do nothing as there is no Freight charge to add the charge to.
				if (fastLine.Line.Uses(CalculatorType.FreightInclusive))
				{
					continue;
				}

				if (criteria.IsServicesOnly && chargeCode.AC_ChargeSubGroup.IsEmpty)
				{
					linesRepository.Remove(chargeCode, Res.GetString("8701C57B-A853-499F-A04F-B244E12B28A9", "Only Services are eligible with this adapter"));
					continue;
				}

				if (!criteria.ChargeCodeGroups.Contains(chargeCode.AC_ChargeGroup))
				{
					linesRepository.Remove(chargeCode, ZString.Format("{0} charge code group is not listed among applicable on the rating criteria", chargeCode.AC_ChargeGroup));
					continue;
				}

				if (isCosting && _Rating.ExcludeConsolLevelChargesOnCosting && chargeCode.AC_IsGroupageCharge)
				{
					linesRepository.Remove(chargeCode, GetConsolLevelReasonMessage(chargeCode, criteria));
					continue;
				}

				if (isCosting
					&& fastLine.IsIntercompanyTariff()
					&& chargeCode.AC_IsGroupageCharge
					&& criteria.GatewayConfiguration.IsForwardingShipment)
				{
					linesRepository.Remove(chargeCode, GetConsolLevelReasonMessage(chargeCode, criteria));
					continue;
				}

				if (chargeCode.AC_IsGroupageCharge && (chargeFilter & ChargeCodeFilter.AutorateConsolLevelOnly) == 0)
				{
					linesRepository.Remove(chargeCode, GetConsolLevelReasonMessage(chargeCode, criteria));
					continue;
				}

				if (!chargeCode.AC_IsGroupageCharge && (chargeFilter & ChargeCodeFilter.AutorateNonConsolLevelOnly) == 0)
				{
					linesRepository.Remove(chargeCode, GetConsolLevelReasonMessage(chargeCode, criteria));
					continue;
				}

				serviceRater.RemoveUnmatchedOrInvalidLines(linesRepository, chargeCode);
			}

			serviceRater.RemoveOtherOriginAndDestinationChargesWithInvalidLocation(linesRepository);
		}

		static string GetConsolLevelReasonMessage(AccChargeCode chargeCode, RatingCriteria criteria) =>
			chargeCode.AC_IsGroupageCharge
				? Res.GetString(
					"012a7128-79a5-11e8-9603-1c1b0d09faa1",
					"{0} charge code is flagged as Consol Level. Consol Level Costs don't apply to {1}.",
					chargeCode.AC_Code,
					criteria.OperationalJobCode)
				: Res.GetString(
					"32602362-79a5-11e8-b80d-1c1b0d09faa1",
					"{0} charge code is not flagged as Consol Level. Non-Consol Level Costs don't apply to {1}.",
					chargeCode.AC_Code,
					criteria.OperationalJobCode);

		#endregion

		#region Cost Rates overriden by Intercompany Tariff

		static void RemoveCostRatesOverridenByIntercompanyTariff(RatingCriteria criteria, RateLinesRepository linesRepository, bool isCosting)
		{
			if (!isCosting || !criteria.ShouldRemoveNonIntercompanyTariffFRTEntries(_Rating.BillingType))
			{
				return;
			}

			var allcompaniesOrgProxies = GlbCompany.GetActiveCompanies().SelectMany(x => x.GetAllOrgProxiesIncludingBranches()).ToHashSet();

			bool IsCostingFreightEntry(FastLine fastLine)
			{
				var rateLine = fastLine.Line;
				if (rateLine != null
					&& !rateLine.ParentRateEntry.IsIntercompanyTariff()
					&& !rateLine.ParentRateEntry.IsSpotEntry
					&& !rateLine.TL_AC.IsEmpty
					&& rateLine.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight
					&& !allcompaniesOrgProxies.Contains(rateLine.ParentRateEntry.ParentRatingHeader.TH_OH))
				{
					return true;
				}

				return false;
			}

			var rateLines = linesRepository.GetLines();
			var costingFreightEntries = rateLines.Where(x => IsCostingFreightEntry(x)).ToList();
			if (costingFreightEntries.Any())
			{
				foreach (var line in costingFreightEntries)
				{
					linesRepository.Remove(line, Res.GetString("b48249c4-8ded-4770-9cbf-ca12aa52d46b", "replaced by Intercompany tariff"));
				}
			}
		}

		#endregion

		#region Multiple Client Contract Number

		void RemoveMultipleClientContractNumber(RatingCriteria criteria, bool isCosting, RateLinesRepository repository, IDialogService dialogService)
		{
			if (isCosting || criteria.IsMultipleClientContractNumberSupported)
			{
				return;
			}

			var contractNumbers = repository.GetLines()
				.Where(l => !l.ParentRateEntry.IsSpotEntry)
				.Select(l => l.ParentRateEntry.TI_ContractNumber.ToString())
				.Distinct()
				.ToArray();
			var nonBlankContractNumbers = contractNumbers
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.ToArray();

			string singleClientContractNumber = nonBlankContractNumbers.Length <= 1
				? nonBlankContractNumbers.FirstOrDefault()
				: criteria.GetSingleClientContractNumber(contractNumbers, dialogService);

			if (singleClientContractNumber != null)
			{
				var linesToRemove = GetLinesToRemoveBySelectedClientContractNumber(repository.GetLines(), singleClientContractNumber);

				repository.Remove(line =>
					new RemoveResult(line)
					{
						ShouldRemove = linesToRemove.Contains(line),
						RemovalReason = FormattableString.Invariant($"Rate with Contract Number '{line.ParentRateEntry.TI_ContractNumber}' wasn't matched with the selected Client Contract Number '{singleClientContractNumber}'."),
					});
			}
		}

		/// <summary>
		/// Compares lines (grouped by charge code) for contract numbers in the given list and removes weaker matches.
		/// In each charge code group:
		/// - removes those with a non-blank number that is not in the given list.
		/// - removes those with a blank number if there is a rate with a specified number
		/// </summary>
		static IEnumerable<FastLine> GetLinesToRemoveBySelectedClientContractNumber(IEnumerable<FastLine> lines, string selectedClientContractNumber)
		{
			var linesToRemove = new List<FastLine>();

			foreach (var linesByChargeCode in lines.GroupBy(l => l.Line.TL_AC))
			{
				var hasSelected =
					!string.IsNullOrWhiteSpace(selectedClientContractNumber) &&
					linesByChargeCode.Any(l => l.ParentRateEntry.TI_ContractNumber.EqualsIgnoringCase(selectedClientContractNumber));

				linesToRemove.AddRange(linesByChargeCode.Where(l =>
					(l.ParentRateEntry.TI_ContractNumber.IsEmpty && hasSelected) ||
					(!l.ParentRateEntry.TI_ContractNumber.IsEmpty && !l.ParentRateEntry.TI_ContractNumber.EqualsIgnoringCase(selectedClientContractNumber))));
			}

			return linesToRemove;
		}

		#endregion

		#region Overriden

		static void RemoveOverriddenRates(
			RatingCriteria criteria,
			ServiceAutoRater serviceRater,
			RateLinesRepository linesRepository,
			FilterOptions options)
		{
			var remover = new OverriddenRateLineRemover(criteria, options);
			foreach (var chargeCode in linesRepository.GetChargeCodes())
			{
				var groups = linesRepository.GetSimilarLineGroups(chargeCode);

				foreach (var group in groups)
				{
					remover.Remove(group, linesRepository);
				}

				serviceRater.RemoveOverridden(linesRepository, chargeCode, options.IsCosting);
			}

			UpdateJobNamedAccountAndOrFilterRatesWithNamedAccount(criteria, linesRepository);
		}

		static void UpdateJobNamedAccountAndOrFilterRatesWithNamedAccount(RatingCriteria criteria, RateLinesRepository rateLinesRepository)
		{
			if (criteria.IsManualCostSelectMode)
			{
				return;
			}

			if (criteria == null)
			{
				return;
			}

			var host = FindHostSupportingNamedAccountPlugin(criteria);
			if (host == null)
			{
				return;
			}

			var costLines = rateLinesRepository.GetLines().Where(l => l.IsCostRate()).ToArray();
			var namedAccountsFromRates = costLines
				.Where(l => l.ParentRateEntry.NamedAccounts != null)
				.SelectMany(l => l.ParentRateEntry.NamedAccounts
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.Select(x => x.Trim().ToUpper(CultureInfo.InvariantCulture)))
				.Distinct().ToArray();

			var jobNamedAccount = criteria.NamedAccount.Trim().ToUpper();
			if (namedAccountsFromRates.Length == 0 || namedAccountsFromRates.Any(x => x == jobNamedAccount))
			{
				return;
			}

			var namedAccountFromRatesToUpdateToJob = GetNamedAccountFromRatesToUpdateToJob(
				jobNamedAccount,
				namedAccountsFromRates,
				LinesRemoval);

			if (!string.IsNullOrWhiteSpace(namedAccountFromRatesToUpdateToJob))
			{
				host.AddOrUpdateNamedAccount(namedAccountFromRatesToUpdateToJob);
			}

			void LinesRemoval(string namedAccountToKeep)
			{
				foreach (var costLine in costLines)
				{
					var lineNamedAccounts = NamedAccountComparer.GetNamedAccountsFromLine(costLine);
					if (!lineNamedAccounts.Contains(namedAccountToKeep))
					{
						var reasonMessage = FormattableString.Invariant($"Rate with Named Account '{string.Join(", ", lineNamedAccounts)}' not applied.");
						rateLinesRepository.Remove(costLine, reasonMessage);
					}
				}
			}
		}

		static IAutoRatingNamedAccountPlugin FindHostSupportingNamedAccountPlugin(RatingCriteria criteria)
		{
			IAutoRatingNamedAccountPlugin host = null;
			AutoRatingProxy autoRatingProxy = criteria;
			while (autoRatingProxy?.AutoRating != null && host == null)
			{
				var autoRating = autoRatingProxy.AutoRating;
				host = autoRating as IAutoRatingNamedAccountPlugin;
				autoRatingProxy = autoRating as AutoRatingProxy;
			}

			return host;
		}

		static string GetNamedAccountFromRatesToUpdateToJob(ZString jobNamedAccount, string[] namedAccountsFromRates, Action<string> linesRemoval)
		{
			var namedAccountFromRatesToUpdateToJob = namedAccountsFromRates.OrderBy(x => x).First();

			if (!jobNamedAccount.IsEmpty)
			{
				var dialogResult = _Rating.Interactor.ShowNamedAccountMessageBox(jobNamedAccount, namedAccountFromRatesToUpdateToJob);

				if (dialogResult == ZDialogResult.Ignore)
				{
					linesRemoval.Invoke(string.Empty);
				}

				if (dialogResult == ZDialogResult.Yes)
				{
					_Rating.Interactor.Log(LogType.Information, FormattableString.Invariant($"Job Named Account '{jobNamedAccount}' is replaced with rate Named Account '{namedAccountFromRatesToUpdateToJob}'"));
					linesRemoval.Invoke(namedAccountFromRatesToUpdateToJob);
				}
				else
				{
					namedAccountFromRatesToUpdateToJob = string.Empty;
				}
			}
			else if (namedAccountsFromRates.Length != 1)
			{
				namedAccountFromRatesToUpdateToJob = string.Empty;
			}

			return namedAccountFromRatesToUpdateToJob;
		}

		#endregion

		#region Irrelevant Costs

		internal static void RemoveIrrelevantCosts(RatingCriteria criteria, RateLinesRepository linesRepository)
		{
			foreach (var rateLine in linesRepository.GetLines())
			{
				if (rateLine.Line.Uses(CalculatorType.Note))
				{
					linesRepository.Remove(rateLine, "notes are not supported for costs");
					continue;
				}

				if (rateLine.ParentRateEntry.IsSpotEntry || rateLine.Line.IsIntercompanyTariff())
				{
					continue;
				}

				var org = rateLine.ParentRateEntry.ParentRatingHeader.Header;
				var chargeCode = rateLine.ChargeCode;
				var providersAndContractors = criteria.GetTransportProvidersAndContractorPKs(chargeCode);

				if (org == null)
				{
					if (!rateLine.TL_FeeChargeType.IsEmpty && !criteria.AllDistinctCarriers.Any(carrier => rateLine.Line.IsFeeChargeApplicable(carrier)))
					{
						linesRepository.Remove(rateLine, ZString.Format("{0} Fee/Charge type not applicable to the Carrier(s)", rateLine.TL_FeeChargeType));
					}
				}
				else if (!providersAndContractors.Contains(org.PK) && (criteria.SelectedServiceProviderFromRateSelector.IsEmpty || criteria.SelectedServiceProviderFromRateSelector != org.PK))
				{
					linesRepository.Remove(rateLine, ZString.Format("{0} is not among creditors/contractors on a job for {1} charge group", org.OH_Code, chargeCode.AC_ChargeGroup));
				}
			}
		}

		#endregion

		#region Irrelevant Group Client Rates

		static void RemoveIrrelevantGroupClientRates(RatingCriteria criteria, RateLinesRepository linesRepository)
		{
			if (criteria.LocalClient == null && criteria.OverseasAgent == null && criteria.Consignee == null && criteria.Consignor == null)
			{
				return;
			}

			foreach (var rateLine in linesRepository.GetLines())
			{
				if (RateLineHelper.IsRateCategoryUnknownForJobServiceSpotEntry(rateLine.Line)
					|| !rateLine.ParentRateEntry.IsClientRateHavingSubsidiaryRelations())
				{
					continue;
				}

				var chargedOrgs = rateLine.GetChargedOrgs(forCompanyTariff: false);
				string rateMode = RatingHelper.GetRateMode(criteria, rateLine.ParentRateEntry.TI_RateCategory);
				bool saveLine = false;
				var lineOrg = rateLine.ParentRateEntry.ParentRatingHeader.Header;
				var effectiveDate = criteria.GetEffectiveDateWithFallback(rateLine.Line, isCosting: false).date;

				foreach (var org in chargedOrgs)
				{
					saveLine = lineOrg.PKEquals(org) || RatingCache.GetCompanyTariffLevel(org, criteria.Company, rateLine.ParentRateEntry.TI_RateCategory, rateMode, criteria.Direction, effectiveDate).applyGroupRate;
					if (saveLine)
					{
						break;
					}
				}

				if (!saveLine)
				{
					linesRepository.Remove(rateLine, LogMessages.OrgDoesntUseGroupClientRates(lineOrg.OH_Code));
				}
			}
		}

		#endregion

		#region Not Applicable to Payment Term Override

		void RemoveRateLinesNotApplicableToPaymentTermOverride(RatingCriteria criteria, RateLinesRepository linesRepository)
		{
			foreach (var line in linesRepository.GetLines())
			{
				if (line.ParentRateEntry.IsCosting() || line.ParentRateEntry.IsWiseCost())
				{
					continue;
				}

				if (criteria.PaymentTermOverride.IsEmpty)
				{
					if (!line.ParentRateEntry.TI_PaymentTerm.IsEmpty)
					{
						var message = ZString.Format("Job has not specified Payment Term Override", criteria.PaymentTermOverride);
						linesRepository.Remove(line, message);
					}
				}
				else
				{
					if (!line.ParentRateEntry.TI_PaymentTerm.IsEmpty && line.ParentRateEntry.TI_PaymentTerm != criteria.PaymentTermOverride)
					{
						var message = ZString.Format("Payment Term Override didn't match job {0}", criteria.PaymentTermOverride);
						linesRepository.Remove(line, message);
					}
				}
			}
		}

		#endregion

		#region Not Applicable to Payment Term

		void RemoveChargesNotApplicableToPaymentTerm(RatingCriteria criteria, RateLinesRepository linesRepository, CostSell costOrSell)
		{
			var shouldSkip = criteria.PaymentTerm.IsEmpty() || costOrSell == CostSell.Cost && criteria.IsDomestic();
			if (shouldSkip)
			{
				return;
			}

			foreach (var line in linesRepository.GetLines())
			{
				var chargeCode = line.ChargeCode;
				if (chargeCode == null)
				{
					continue;
				}

				var alwaysChargedMessage = LogMessages.AlwaysCharged(chargeCode, criteria.ConsumerType);
				if (!string.IsNullOrEmpty(alwaysChargedMessage))
				{
					var shouldNotFilterMessage = ZString.Format("{0} {1}\treason:\t{2}.", LogEventTypes.RateLineNotFiltered, line.DisplayInfo(), alwaysChargedMessage);
					linesRepository.LogAfterSearch().Information(shouldNotFilterMessage);
					continue;
				}

				var entry = line.ParentRateEntry;
				var chargeGroup = chargeCode.AC_ChargeGroup;
				var isApplicableToPaymentTermFiltering = criteria.IsApplicableToPaymentTermFiltering(chargeGroup, costOrSell);

				if (isApplicableToPaymentTermFiltering && !entry.IsIntercompanyTariff())
				{
					// filters the lines by incoterm/direction
					var chargedParty = criteria.ChargesPaidBy(entry.Origin(), entry.Destination(), chargeGroup, costOrSell);

					if (chargedParty == ChargedParty.None)
					{
						var message = ZString.Format("{0} charge group is not applicable for {1} {2}", chargeGroup, criteria.GetLeg(entry), criteria.GetDirectionPaymentTermString(costOrSell, chargeGroup));
						linesRepository.Remove(line, message);
						continue;
					}
				}

				if (costOrSell == CostSell.Revenue && !entry.IsSpotEntry && line.GetOrgImportance() == IncotermsRanker.NotApplicable)
				{
					var incotermsRanker = line.GetIncotermsRanker();
					var orgTypes = incotermsRanker.OrgTypes;
					string reason;

					if (orgTypes.Any())
					{
						var registryItemCaption = incotermsRanker.GetRatesPrioritiesRegistryItemCaption();
						if (registryItemCaption == null)
						{
							reason = "could not establish sell rates priority registry";
							if (line.IsCollect() == null)
							{
								reason += " due to empty PaymentTerm";
							}
							else if (criteria.JobDirection == Directions.Unknown)
							{
								reason += " for Unknown job direction";
							}
						}
						else
						{
							var orgsStr = new ZStringBuilder(orgTypes.Select(x => x.ToString())).ToStringWithDelimiterBetweenAppends(", ");
							reason = ZString.Format("{0} organization is not listed in sell rates priority registry among {1} applicable organizations", orgsStr, registryItemCaption);
						}
					}
					else
					{
						reason = "not linked to any organisation";
					}

					linesRepository.Remove(line, reason);
				}

				if (costOrSell == CostSell.Cost && !entry.TI_PaymentTerm.IsEmpty && !entry.IsIntercompanyTariff())
				{
					var jobPaymentTerm = criteria.GetPaymentTermString(CostSell.Cost, chargeGroup);

					var checkPaymentTermNotApplicable = (entry.TI_PaymentTerm == WRConstants.PaymentTerm.Collect && jobPaymentTerm == WRConstants.PaymentTerm.Prepaid)
						|| (entry.TI_PaymentTerm == WRConstants.PaymentTerm.Prepaid && jobPaymentTerm == WRConstants.PaymentTerm.Collect);

					if (checkPaymentTermNotApplicable)
					{
						var message = ZString.Format("Payment Terms didn't match job {0}", jobPaymentTerm);
						linesRepository.Remove(line, message);
					}
				}
			}
		}

		#endregion

		#region Remove CompanyTariff ChargeCode If Exists In ClientRate

		void RemoveCompanyTariffsIrrelevantLinesBasedOnClientRate(RateLinesRepository linesRepository, CostSell costOrSell, RatingCriteria criteria, bool isCosting)
		{
			if (costOrSell == CostSell.Revenue)
			{
				var exlCalculatorRateLines = linesRepository
					.GetLines()
					.Where(rateLine => rateLine.Line.Uses(CalculatorType.ExcludeCompanyTariffs)
						&& rateLine.ParentRateEntry.IsClientRate());

				foreach (var exlCalculatorRateLine in exlCalculatorRateLines)
				{
					foreach (var rateLine in linesRepository.GetLines())
					{
						if (
							rateLine.ParentRateEntry.IsCompanyTariff()
							&& rateLine.ChargeCode.AC_Code == exlCalculatorRateLine.ChargeCode.AC_Code
							&& rateLine.ParentRateEntry.ParentRatingHeader.TH_GlobalRateLevel != criteria.TariffLevel)
						{
							linesRepository.Remove(rateLine, Res.GetString("6d967197-bb42-487e-81a8-8830069d4afb", "Company Tariff {0} is found but filtered because rate comparer", exlCalculatorRateLine.ChargeCode.AC_Code));
						}
					}
					linesRepository.Remove
					(
						exlCalculatorRateLine,
						Res.GetString("810e8ddd-8b62-48c2-875e-084bb149b54e", "Client Rate {0} is found but not displayed because 'Exclude from Company Tariffs' Calculator used", exlCalculatorRateLine.ChargeCode.AC_Code)
					);
				}
			}
		}

		#endregion

		#region Consumer Specific Charges

		void RemoveConsumerSpecificCharges(RatingCriteria criteria, RateLinesRepository rateLinesRepository)
		{
			rateLinesRepository.Remove(criteria.ShouldRemoveCharge, "charge code is not applicable to criteria");
		}

		#endregion

		#region Rate Line Condition is not met

		void RemoveChargesWhereRateLineConditionIsNotMet(RatingCriteria criteria, RateLinesRepository rateLinesRepository)
		{
			using (var evaluator = new UserExpressionEvaluator(criteria.ConditionsSupporter?.ObjectToWrap))
			{
				rateLinesRepository.Remove(fastLine =>
				{
					var result = new RemoveResult(fastLine);
					var rateLine = fastLine.Line;

					if (rateLine.TL_Condition.IsEmpty)
					{
						return result;
					}

					var conditionMet = MeetsConditionOnRateLine(fastLine, criteria.ConditionsSupporter, evaluator);

					if (conditionMet == null)
					{
						result.ShouldRemove = true;
						result.RemovalReason = "RateLine condition " + GetCondition(rateLine) + " cannot be evaluated as boolean value. Please rewrite it.";
					}
					else if (!conditionMet.Value)
					{
						result.ShouldRemove = true;
						result.RemovalReason = "RateLine condition " + GetCondition(rateLine) + " not met";
					}
					else
					{
						result.ShouldRemove = false;
						result.RetainedReason = "RateLine condition " + GetCondition(rateLine) + " met";
					}

					return result;
				});
			}
		}

		static ZString GetCondition(IRateLine rateLine)
		{
			return rateLine.TL_Condition == RateLineConditions.UserDefined ? rateLine.TL_ConditionalExpression : rateLine.TL_Condition;
		}

		bool? MeetsConditionOnRateLine(FastLine fastLine, RateLineConditionsSupporter supporter, IUserExpressionEvaluator evaluator)
		{
			var rateLine = fastLine.Line;

			if (supporter != null)
			{
				var isOrgFrt = rateLine.ParentRateEntry.IsOriginEntry() || rateLine.ParentRateEntry.IsFreightEntry();
				var isDstFrt = rateLine.ParentRateEntry.IsDestinationEntry() || rateLine.ParentRateEntry.IsFreightEntry();

				bool? IsUserDefinedConditionMet(ZString conditionExpression, RateLineConditionsSupporter conditionSupporter) => evaluator.IsUserDefinedConditionMet(conditionExpression);

				return supporter.MeetsCondition(rateLine.TL_Condition, rateLine.TL_ConditionalExpression, IsUserDefinedConditionMet, isOrgFrt, isDstFrt);
			}

			return false;
		}

		#endregion

		#region Billing Type

		void RemoveBillingTypeIrrelevantLines(RateLinesRepository rateLinesRepository)
		{
			rateLinesRepository.Remove(x => _Rating.IsEqualization && !x.Line.Uses(CalculatorType.Equalization), "Only lines with Volume Equalization Discount calculator are applicable during equalization autorating");
			rateLinesRepository.Remove(x => !_Rating.IsEqualization && x.Line.Uses(CalculatorType.Equalization), "Volume Equalization Discount calculator is only applicable during volume equalization discount rating");
		}

		#endregion

		#region Measures

		void RemoveChargesThatDependOnInvalidOrMissingRequiredMeasures(AutoRatingCalculatorParameters parameters, RateLinesRepository rateLinesRepository, RatingCriteria criteria)
		{
			LineMeasureMatcher.RemoveChargesThatDependOnInvalidOrMissingRequiredMeasures(parameters, rateLinesRepository, criteria);
		}

		#endregion

		#region Remove International Freight Charge Code With SkipFreightCharge

		internal static void RemoveInternationalFreightChargeCodeWithSkipFreightCharge(RatingCriteria criteria, RateLinesRepository rateLinesRepository)
		{
			if (!criteria.SkipFreightCharge)
			{
				return;
			}

			var company = criteria.Company ?? GlbCompany.CurrentCompany;
			var chargeCodePK = Env.Registry.GetFreightChargeCode(company.PK.ToGuid());

			rateLinesRepository.Remove(line => line.ChargeCode.PK == chargeCodePK, string.Format("{0} consumes an allocation with spot rate offered by the carrier to be manually input.", criteria.AdapterType.ToString()));
		}

		#endregion

		#endregion

		public struct FilterOptions
		{
			public bool DisableExcludedFromAutoRatingFilter { get; set; }
			public bool DisablePaymentTermsFilter { get; set; }
			public bool DisableIrrelevantTariffsFilter { get; set; }
			public bool DisableInvalidRatesFilter { get; set; }
			public bool IsCosting { get; set; }
			public bool IsRebateCalculationMode { get; set; }
			public bool DisableSpotFilter { get; set; }
			public bool RemoveFreightCharge { get; set; }
		}
	}
}
